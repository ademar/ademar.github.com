--- 
mt_id: 20
layout: post
title: Look and say sequence
date: 2009-10-16 09:45:36 -04:00
tags:
- conway
- fsharp
- mathematics
- programming
---
 Also known as the <a href="http://www.research.att.com/~njas/sequences/A005150">Morris Number Sequence</a> 

{% highlight fsharp %}
let morris list =
    let rec morris' p' counter = function
        |[] -> [counter;p']
        |p::tail -> if (p<>p') 
                    then counter::p'::(morris' p 1 tail) 
                    else morris' p (counter+1) tail
    match list with
    |[] -> []
    |a::b ->  morris' a 1 b

{% endhighlight %}

p.. 
The sequence looks like this
{% highlight fsharp %}
> let rec applyN f n x = 
    if n = 0 then x else f (applyN f (n-1) x);;
val applyN : ('a -> 'a) -> int -> 'a -> 'a
> applyN morris 0 [1];;
val it : int list = [1]
> applyN morris 1 [1];;
val it : int list = [1; 1]
> applyN morris 2 [1];;
val it : int list = [2; 1]
> applyN morris 3 [1];;
val it : int list = [1; 2; 1; 1]
> applyN morris 4 [1];;
val it : int list = [1; 1; 1; 2; 2; 1]
> applyN morris 5 [1];;
val it : int list = [3; 1; 2; 2; 1; 1]
> applyN morris 6 [1];;
val it : int list = [1; 3; 1; 1; 2; 2; 2; 1]
{% endhighlight %}

p.. 
The ratio between the length of consecutive terms converges and is called Conway's constant.

{% highlight fsharp %}
> let S n = List.length (applyN morris n [1]);;
val S : int -> int
> let epsilon = 10E-5 ;;
val epsilon : float = 0.0001
> let limit f =
    let rec limit' n =
        if abs(f (n+1) - (f n))<epsilon then f (n+1) else limit' (n+1)
    limit' 1;;
val limit : (int -> float) -> float
> let conways_constant = limit (fun n -> float(S (n+1)) / float(S n));;
val conways_constant : float = 1.308504399 
{% endhighlight %}