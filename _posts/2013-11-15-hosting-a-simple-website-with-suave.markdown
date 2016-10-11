--- 
layout: post
title: Hosting a static website with Suave
tags:
- suave
- web
- fsharp
- async
- server
- mono
- linux
---
What follows is the code for the webserver running at [Suave.IO](http://suave.io)

```fsharp 
#light(*
exec fsharpi --exec $0 --quiet
*)

#r "suave.dll"

open Suave.Types
open Suave.Web
open Suave.Http
open System
open System.Net

let app : WebPart = 
    choose [
       Console.OpenStandardOutput() |> log >>= never;
       url_regex "(.*?)\.(fsx|dll|mdb|log)$" >>= FORBIDDEN "Access denied.";
       GET >>= choose [ url "/" >>= file "index.html"; browse ];
       NOT_FOUND "Found no handlers." 
    ]

let config = 
    { default_config with
       bindings = [ { scheme = HTTP ; ip = IPAddress.Parse "10.0.0.5" ; port   = 80us } ]
       timeout = TimeSpan.FromMilliseconds 3000.
    }

web_server config app

```

On Linux I lauch the script with something like this

```text

nohup fsharpi WebApp.fsx > web.log &

```

I ran a load test against Suave.IO with [Load Impact](http://loadimpact.com); this is the resulting graph. Note how the response time stays constant as the number of client connections increases.

![Load Impact Chart](https://pbs.twimg.com/media/BZM7IPvCYAEssAg.png:large)

And the corresponding resource usage graph of the Extra Small VM (Shared Core, 768 MB memory) on Azure running it.

![Resource Usage Graph](https://pbs.twimg.com/media/BZNCk5yCYAIwu3H.png:large)
