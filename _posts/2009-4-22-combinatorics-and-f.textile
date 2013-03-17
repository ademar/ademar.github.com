--- 
mt_id: 14
layout: post
title: Combinatorics and F#
date: 2009-04-22 10:23:35 -04:00
tags:
- combinatorics
- fsharp
---

Calculates non-repeating k-combinations of a list.

bc.. 
> let rec choices = function
  | []      -> []
  | p::tail -> (p,tail) :: [ for (y,l) in choices tail -> (y,l) ];;


val choices : 'a list -> ('a * 'a list) list

> choices [1..6];;
val it : (int * int list) list
= [(1, [2; 3; 4; 5; 6]); (2, [3; 4; 5; 6]); (3, [4; 5; 6]); (4, [5; 6]);
   (5, [6]); (6, [])]

> let rec combinations S k =
    [ if k=0 then yield [] else
            for (e,r) in choices S do
                for o in combinations r (k-1) do yield e::o  ];;


val combinations : 'a list -> int -> 'a list list

> combinations [1..6] 3;;
val it : int list list
= [[1; 2; 3]; [1; 2; 4]; [1; 2; 5]; [1; 2; 6]; [1; 3; 4]; [1; 3; 5]; [1; 3; 6];
   [1; 4; 5]; [1; 4; 6]; [1; 5; 6]; [2; 3; 4]; [2; 3; 5]; [2; 3; 6]; [2; 4; 5];
   [2; 4; 6]; [2; 5; 6]; [3; 4; 5]; [3; 4; 6]; [3; 5; 6]; [4; 5; 6]]

> let binomial n k = List.length (combinations [1..n] k);; 


val binomial : int -> int -> int

> binomial 52 5;;
val it : int = 2598960 
