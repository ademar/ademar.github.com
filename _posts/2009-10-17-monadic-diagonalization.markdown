--- 
mt_id: 21
layout: post
title: Monadic diagonalization
date: 2009-10-17 11:03:59 -04:00
tags:
- diagonalization
- fsharp
- mathematics
- monad
- programming
---
Suppose we want to find all two factors decompositions of a positive integer. We could try the following naive solution.

```fsharp
> let rec inf_seq n = seq { yield n; yield! inf_seq (n+1) };;

val inf_seq : int -> seq<int>

> let omega = inf_seq 1;;

val omega : seq<int>

> for i in omega do
       for j in omega do 
        if (i*j=24) then printf "(%d,%d)" i j;;

- Interrupt
(1,24)

``` 

This goes on ad infinitum and prints only one solution after interrupting the evaluation on the F# console.The reason is the following, not all pairs are treated fairly equal. Notice the generated output on the following expression:

```fsharp
> seq { for i in (inf_seq 1) do
                for j in (inf_seq 1) do yield (i,j)};;
val it : seq<int * int> = seq [(1, 1); (1, 2); (1, 3); (1, 4); ...]

```

The sequence only generates (1,x) pairs, because the first inner loop will never end iterating.

To address this issue we need a mechanism where the pairs are generated in a fair order. This mechanism is known as <a href="http://en.wikipedia.org/wiki/Cantor%27s_diagonal_argument">diagonalization</a> (because of Cantor's diagonalization proof). Our diagonalization process will take a lazy list of lazy lists and will rearrange its items following Cantor's method. The implementation of the function diagonal is given at the end of the post.

```fsharp
> let x = diag (LazyList.of_list [omega;omega;omega]);;

val x : LazyList<LazyList<int>>

> x;;
val it : LazyList<LazyList<int>> =
  seq [seq [1]; seq [2; 1]; seq [3; 2; 1]; seq [4; 3; 2]; ...]

```

We can embed this process into a work-flow (monad)

```fsharp
type DiagBuilder () = 
    member b.Return(x)  = LazyList.of_list [x]
    member b.Bind(x, rest) =  LazyList.concat (diag (LazyList.map rest x))
    member b.Let(p, rest)  = rest p
    member b.Delay(f ) = f ()
    member b.Zero() = LazyList.empty()

let diagonal = new DiagBuilder ()

```
 
Using the diagonal monad, our initial factorization problem can be solved like this

```fsharp 
> let all_pairs = diagonal { 
    let! n = LazyList.of_seq(inf_seq 1)
    let! m = LazyList.of_seq(inf_seq 1)
    return (n,m)
    } ;;

val all_pairs : LazyList<int * int>

> let factors = seq { for (n,m) in all_pairs do if (n*m=24) then yield (n,m) } ;;

val factors : seq<int * int>

> factors;;
val it : seq<int * int> = seq [(4, 6); (6, 4); (3, 8); (8, 3); ...]

```

There is some small caveat thought, our algorithm does not stop. Also notice that diagonal is not a real monad because our bind operator is not associative.

Here is the function diagonal which completes the program.

```fsharp 
let rec lzw f l1 l2 =
    LazyList.delayed ( fun () -> 
    match l1,l2 with
    |LazyList.Nil, _ -> l2
    |_, LazyList.Nil -> l1
    |LazyList.Cons(p1,tail1),LazyList.Cons(p2,tail2)
         -> LazyList.consf (f p1 p2) (fun () -> lzw f tail1 tail2))
         
let rec diag input = 
    LazyList.delayed ( fun () -> 
        match input with 
        |LazyList.Nil -> LazyList.empty()
        |LazyList.Cons(p,tail) 
            -> lzw (LazyList.append) 
                (LazyList.of_seq (seq {for x in p do yield LazyList.of_list [x]})) 
                (LazyList.consf (LazyList.empty()) (fun () -> diag tail)))

```
 
<b>References</b>
- Combinators for logic programming, Michael Spivey and Silvija Seres
- Enumerating a context-free language, Luke Palmer
 
