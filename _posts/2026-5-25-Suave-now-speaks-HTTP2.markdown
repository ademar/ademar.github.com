---
title: "Suave now speaks HTTP/2"
date: 2026-05-25
tags: [fsharp, suave, http2, dotnet]
---

# Suave now speaks HTTP/2

I just shipped HTTP/2 support in [Suave](https://github.com/SuaveIO/suave),
the lightweight F# web server. It is in-tree (no proxy, no Kestrel in front)
and turned on by default: every Suave application built against the new
release accepts HTTP/2 connections without any code change.

## What's in the box

The new `Suave.Http2` module implements the binary framing layer from
[RFC 9113](https://www.rfc-editor.org/rfc/rfc9113) end to end:

- **HPACK** header compression (RFC 7541).
- **Stream multiplexing** — many concurrent requests on one TCP connection.
- **Flow control** with `WINDOW_UPDATE` frames.
- **Prioritization** — frame ordering is driven by stream readiness, so a
  slow handler doesn't block a fast one on the same connection.
- **Server push** via `PUSH_PROMISE`.
- **Trailers** (trailing `HEADERS` blocks), with the RFC 7540 §8.1.2
  validation baked in.
- **Three negotiation paths**: TLS + ALPN (`h2`), `h2c` upgrade from
  HTTP/1.1, and `h2c` prior-knowledge.

The implementation is verified against
[h2spec](https://github.com/summerwind/h2spec).

## Zero config

The same `WebPart` serves HTTP/1.1 and HTTP/2 clients. Nothing to wire up:

```fsharp
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

let app =
  choose [
    GET >=> path "/" >=> OK "hello over HTTP/2 (or 1.1)"
  ]

[<EntryPoint>]
let main _ =
  startWebServer defaultConfig app
  0
```

Hit it three ways:

```bash
# HTTP/1.1
curl -v http://localhost:8080/

# h2c prior knowledge
curl --http2-prior-knowledge -v http://localhost:8080/

# h2c Upgrade negotiation
curl --http2 -v http://localhost:8080/
```

Bind a certificate and you also get ALPN negotiation, which is what
browsers actually use:

```fsharp
let cfg =
  { defaultConfig with
      bindings =
        [ HttpBinding.create HTTP         IPAddress.Loopback 8080us
          HttpBinding.create (HTTPS cert) IPAddress.Loopback 8443us ] }

startWebServer cfg app
```

Chrome / Firefox DevTools will show `h2` in the protocol column.

## Server push, the easy way

```fsharp
let index : WebPart =
  Http2.Push.push "/style.css" [ "accept", "text/css" ]
  >=> Http2.Push.push "/app.js"   [ "accept", "application/javascript" ]
  >=> setMimeType "text/html; charset=utf-8"
  >=> OK indexHtml
```

On an HTTP/2 connection the writer emits `PUSH_PROMISE` frames on the parent
stream and delivers the synthesised responses before the client asks for
them. On HTTP/1.1 the push intents are silently ignored, so the same
handler works in both worlds.

## A runnable demo

The repo ships a small end-to-end example,
[`examples/Http2Demo`](https://github.com/SuaveIO/suave/tree/master/examples/Http2Demo),
that exercises every feature in one place — multiplexing, push, HPACK,
prioritization (`/slow` vs `/fast`), and flow control (a 256 KiB body
that forces `WINDOW_UPDATE`):

```bash
dotnet run --project examples/Http2Demo/Http2Demo.fsproj -- \
  --http-port 8080 --https-port 8443

curl --http2-prior-knowledge -v http://127.0.0.1:8080/
nghttp -nv http://127.0.0.1:8080/
h2load -n 100 -c 1 -m 10 https://127.0.0.1:8443/tile/1
```

## Caveats

- WebSocket handshakes still go over HTTP/1.1 — RFC 8441 extended
  `CONNECT` is on the list but not implemented yet. Your WebSocket
  endpoints continue to work on the same port without any change.
- Server push is deprecated by major browsers in practice. It is still
  valid in the spec and useful for non-browser clients (and for h2spec
  conformance).
- Browsers refuse plain-text HTTP/2; use TLS or a terminating proxy if
  you want to test from a browser. Command-line clients (`curl`,
  `nghttp`, `h2load`) speak h2c directly.

## Try it

```bash
dotnet add package Suave
```

Docs for the new module live at
[suave.io/docs/http2.html](https://suave.io/docs/http2.html), and the
source is in
`src/Suave/Http2.fs`.
Bug reports and PRs welcome.
