---
layout: post
title: "IronKernel, and a chessboard problem I worked on in 2009"
date: 2026-07-25 10:00:00 -04:00
tags:
- ironkernel
- kernel
- lisp
- dotnet
- continuations
- combinatorics
- n-queens
---

Seventeen years ago I posted a [backtracking search for figures of constant
width on a chessboard](/blog/2009/04/finding-figures-of-constant-wi.html).
It was F#, it was fast, and enumerating the width-3 figures on an 11×11 board
still took about **31 days** on a quad-core.

I have been building [IronKernel](https://github.com/ironkernel-lang/IronKernel)
for a while now, and I wanted a problem I was familiar with to try it on.
This one turned out to be a good choice, because it broke in three directions I
did not expect.

## What IronKernel is

IronKernel is a dialect of [John Shutt's
Kernel](https://web.cs.wpi.edu/~jshutt/kernel.html) targeting Microsoft .NET. Kernel
looks like Scheme but is more radical about homoiconicity: there are no macros,
because there is something better. The core abstraction is the **operative**,
built with `vau`, which receives its operands *unevaluated* along with the
caller's environment:

```scheme
(define my-if
  (vau (test then else) env
    (if (eval env test)
      (eval env then)
      (eval env else))))
```

That is a first-class value. You can pass it around, store it, return it from a
function. `lambda` is not primitive — it is `vau` plus `wrap`:

```scheme
(define lambda
  (vau (params & body) static-env
    (wrap (eval static-env (list* vau params '_ body)))))
```

Environments are first-class too, so the whole macro/runtime distinction just
disappears. Underneath, programs are analyzed to a Core IR and compiled to CLR
delegates, with a trampolined CPS interpreter behind it for the parts that need
full Kernel semantics — `call/cc`, `shift`/`reset`, first-class `eval`.

Recapping the problem: a set of squares on an n×n board has **constant width w**
if every row, column and diagonal meets it in either 0 or exactly *w* squares.
Such a figure with *kw* squares is of type (n,k,w). Figures of type (n,n,1) are
exactly the n-queens solutions. It is from a lovely
[American Mathematical Monthly paper](https://userweb.ucs.louisiana.edu/~C00254569/hernandez-robert.pdf)
by Hernández and Robert.

## One: backtracking wants to be a library

My 2009 `find` was continuation-passing by hand — a pipeline of combinators
threading a solution, a frontier and a candidate list. I ported it faithfully
to IronKernel first, and it works, but it is F# control flow transliterated into
parentheses.

IronKernel's `shift`/`reset` are multi-shot and re-delimit when resumed. That is
exactly what McCarthy's `amb` needs: a choice point invokes its captured
continuation once per alternative.

```scheme
(define amb  (lambda (xs) (shift (lambda (k) (for-each xs k)))))
(define fail (lambda ()   (shift (lambda (k) #inert))))
```

Two lines. Now search code is *direct style* — it reads as straight-line code
that happens to run once per candidate:

```scheme
(collect
  (let ((a (amb (list 1 2))))
    (let ((b (amb (list 10 20))))
      (emit (cons a b)))))
; => ((1 . 10) (1 . 20) (2 . 10) (2 . 20))
```

The interesting wrinkle is mutable state. A search that marks up a board has to
unmark on the way out, and you cannot do it at the call site: by the time a
choice expression "returns", the rest of the search has already run inside the
continuation. The undo belongs to the *choice point*:

```scheme
(define amb-bracket
  (lambda (xs ok? enter! leave!)
    (shift (lambda (k)
      (for-each xs (lambda (x)
        (if (ok? x)
          (begin (enter! x) (k x) (leave! x))
          #inert)))))))
```

`enter!` runs before the continuation, `leave!` after it has been explored to
exhaustion. So placing a queen becomes:

```scheme
(let ((y (amb-bracket-range 0 n
           (lambda (c) (free? x c))
           (lambda (c) (mark! x c 1))
           (lambda (c) (mark! x c -1)))))
  (place-queens (+ x 1)))
```

The board is always consistent with the caller's view of it, and the caller
never sees a continuation. There is not one explicit continuation argument in
my new solver.

## Two: the search strategy is a value

Nondeterministic code only ever calls `emit`. What that *means* is decided by
whatever operative wraps it — and because operatives are first-class, the
strategy is genuinely swappable:

```scheme
(define collect
  (vau body env
    (let ((acc (make-vector 1 ())))
      (with-sink
        (lambda (s) (vector-set! acc 0 (cons s (vector-ref acc 0))))
        (lambda () (eval env (cons sequence body))))
      (list-reverse (vector-ref acc 0)))))
```

`first-of` installs a handler that escapes through `call/cc`, so the rest of the
tree is never explored. `count-of` tallies without retaining anything. Same
solver:

```
(6,6,2), all figures  : 2 figures    3.0 s
(6,6,2), first only   : 12 squares   0.8 s
```

A happy side effect of rewriting the algorithm rather than porting it: my 2009
program grows a figure outward from a seed square, so it only ever finds
*connected* figures. As I noted back then, that means it structurally cannot
solve the n-queens problem, which is the w=1 case. The new one can, and that is
the cheapest possible sanity check:

| | figures | time |
|---|---|---|
| `(8,8,1)` — 8 queens | **92** | 19 s |
| `(6,6,1)` — 6 queens | **4** | 1.1 s |
| `(4,4,2)` | 1 | 0.4 s |
| `(5,4,2)` | 5 | 1.0 s |
| `(6,6,2)` | 2 | 3.0 s |
| `(7,6,2)` | 24 | 97 s |
| `(8,8,2)` | 30 | 374 s |

All cross-checked against an independent exhaustive search.

## Three: stop searching, start building

The Hernández–Robert paper's Lemma 1 is not an existence proof, it is a
*recipe*. Composing an extended-constant-width figure with any constant width
figure multiplies the widths. And a figure that decomposes into transversals
stays decomposable, so you can *peel* a transversal off and drop the width by
one. Both operations are a few lines each — and `transversal` is itself written
against the same `amb` layer.

```scheme
(define w4 (compose fig3 fig2))   ; (5,4,2) ∘ (4,4,2)
(define w3 (peel w4))
(define w2 (peel w3))
```

```
  Figure 3            : type (5,4,2), extended
  fig3 . fig2         : type (20,16,4)
    peeled once       : type (20,16,3)
    peeled twice      : type (20,16,2)
    peeled thrice     : type (20,16,1)
```

A width-3 figure, in **milliseconds**, against 31 days of search. That is not a
language comparison — it is search versus construction, and construction wins by
a margin that makes the search look silly.

It keeps going. One more composition doubles the width:

```
  fig3 . (fig3 . fig2): type (100,64,8)
    peeled once       : type (100,64,7)
    peeled twice      : type (100,64,6)
```

That last line is my favourite. The paper says, of its own computational
section: *"we were not able to find a figure of width six with the computer."*
Here is one, on a 100×100 board, built and verified in under a second. Not
found — built.

Everything printed is checked by an independent `fig-type` routine that shares
no code with either the searcher or the builders.

## Where the code is

The `amb` layer turned out to be the reusable part, so I pulled it out into
[`IronKernel.Amb`](https://github.com/ironkernel-lang/IronKernel/tree/master/lib/IronKernel.Amb),
the first library in the IronKernel tree: `amb`, `amb-range`, `require`,
bracketed choice, and pluggable strategies. It performs no host I/O and it is
about seventy lines of code.

The two solutions live in
[`Examples/constant-width.ikr`](https://github.com/ironkernel-lang/IronKernel/blob/master/Examples/constant-width.ikr)
(the faithful port of the 2009 algorithm) and
[`Examples/constant-width-amb.ikr`](https://github.com/ironkernel-lang/IronKernel/blob/master/Examples/constant-width-amb.ikr)
(the one described here).

```bash
dotnet tool install -g IronKernel.Tool
ik Examples/constant-width-amb.ikr
```

I did not expect a seventeen-year-old chessboard problem to be the thing that
convinced me operatives and delimited continuations earn their keep. But
watching backtracking collapse into two lines, and then watching the whole
search become unnecessary, was a good afternoon.
