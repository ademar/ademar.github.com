--- 
mt_id: 27
layout: post
title: Combinators for logic programming
date: 2011-06-09 11:01:52 -04:00
tags:
- combinators
- fsharp
- logic
- mathematics
- programming
- prolog
---
Digging in my hard drive found this piece of code from my functional programming studies that implements
the logic unification algorithm described in the article 'Combinators for logic programming' by Michael Spivey and Silvija Seres.

Here are some samples of what kind of problems it can solve.

<b>Sample #1 - append</b>

```fsharp
let X = var "x"
let Y = var "y"

let problem1 = append X Y (list [1;2;3;4;5])

run problem1 |> printSolutions

```

returns:
<pre>{x=Nil,y=[1, 2, 3, 4, 5]}
{x=[1],y=[2, 3, 4, 5]}
{x=[1, 2],y=[3, 4, 5]}
{x=[1, 2, 3],y=[4, 5]}
{x=[1, 2, 3, 4],y=[5]}
{x=[1, 2, 3, 4, 5],y=Nil}
</pre>
<b>Sample #2 - good sequence</b>

```fsharp
let rec good s = 
    step( (s == cons (Int 0) Nil) 
            ||| exists (fun t -> 
                    exists(fun q -> 
                        exists(fun r -> 
                            (s == cons (Int 1) t) &&& append q r t  &&& (good q) &&& (good r)))) )

let problem2 = good  (var "s")

run problem2 |> printSolutions

```

returns:
<pre>
{s=[0]}
{s=[1, 0, 0]}
{s=[1, 0, 1, 0, 0]}
{s=[1, 1, 0, 0, 0]}
{s=[1, 0, 1, 0, 1, 0, 0]}
{s=[1, 1, 0, 0, 1, 0, 0]}
{s=[1, 1, 0, 1, 0, 0, 0]}
{s=[1, 0, 1, 1, 0, 0, 0]}
{s=[1, 1, 0, 0, 1, 0, 1, 0, 0]}
{s=[1, 1, 0, 1, 0, 0, 1, 0, 0]}
... ad infinitum
</pre>

<b>Sample #3 - Difference lists & Grammars</b>

```fsharp
let append' (a1,a2) (b1,b2) (c1,c2) = 
    (a1 == c1) &&& (a2 == b1) &&& (b2 == c2) // -- difference list axiom

let list3 xs  ys = List.foldBack (cons) xs ys

let (++) f1 f2  = 
    fun (list1,r) -> exists( fun x -> append' (list1,x) (x,r) (list1,r) &&& f1(list1,x) &&& f2(x,r))

let final s (list,rest) = exists(fun x -> ((list == list3  ([String(s)]) x)) &&& (x == rest))
let or' f g  (list,rest) = f  (list,rest) ||| g  (list,rest)

let noun =  final "cat" |>  or' <| final "mouse"
let determiner =  final "the" |>  or' <| final "a" 
let verb =  final "scares" |> or' <| final "hates" 

let noun_phrase = determiner ++ noun 
let verb_phrase = verb ++ noun_phrase

let sentence = noun_phrase ++ verb_phrase

run ( sentence (var "q", Nil)) |> printSolutions

```
 
returns:
<pre>
{q=[the, cat, scares, the, cat]}
{q=[the, cat, scares, the, mouse]}
{q=[the, mouse, scares, the, cat]}
{q=[the, cat, hates, the, cat]}
{q=[the, mouse, scares, the, mouse]}
{q=[a, cat, scares, the, cat]}
{q=[the, cat, scares, a, cat]}
{q=[the, mouse, hates, the, cat]}
{q=[a, cat, scares, the, mouse]}
{q=[a, mouse, scares, the, cat]}
{q=[the, cat, hates, the, mouse]}
{q=[the, mouse, scares, a, cat]}
{q=[a, cat, hates, the, cat]}
{q=[a, mouse, scares, the, mouse]}
{q=[the, cat, scares, a, mouse]}
{q=[the, mouse, hates, the, mouse]}
{q=[a, cat, scares, a, cat]}
{q=[a, mouse, hates, the, cat]}
{q=[the, cat, hates, a, cat]}
{q=[the, mouse, scares, a, mouse]}
{q=[a, cat, hates, the, mouse]}
{q=[a, mouse, scares, a, cat]}
{q=[the, cat, hates, a, mouse]}
{q=[the, mouse, hates, a, cat]}
{q=[a, cat, scares, a, mouse]}
{q=[a, mouse, hates, the, mouse]}
{q=[the, mouse, hates, a, mouse]}
{q=[a, cat, hates, a, cat]}
{q=[a, mouse, scares, a, mouse]}
{q=[a, cat, hates, a, mouse]}
{q=[a, mouse, hates, a, cat]}
{q=[a, mouse, hates, a, mouse]}
</pre>

The code is <a href="https://gist.github.com/1016874">here</a>
 
