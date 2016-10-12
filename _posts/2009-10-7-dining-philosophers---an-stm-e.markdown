--- 
mt_id: 18
layout: post
title: Dining philosophers - An STM example in F#
date: 2009-10-07 17:52:54 -04:00
tags:
- concurrency
- fsharp
- programming
- stm
---
The <a href="http://en.wikipedia.org/wiki/Dining_philosophers_problem">dining philosophers problem</a> is a well known problem that illustrate the complexities of managing shared state in a concurrent scenario. The following is a sample solution utilizing <a href="http://cs.hubfs.net/blogs/hell_is_other_languages/archive/2008/01/16/4565.aspx">Gregory Neverov</a>'s implementation of Shared Transactional Memory for F#.

We start be defining some functions to deal with our shared resources, in this case the forks. Here we use computation expressions syntax to code our transactional operations.

bc.. 
let check = function
  | true -> stm.Return(())
  | false -> retry ()

let take fork = 
    stm {
        let! ref_fork = readTVar (fork)
        do! check ref_fork
        do! writeTVar (fork) false } 

let acquire left_fork right_fork  =  
    stm { 
        do! take left_fork
        do! take right_fork } |> atomically    

let release left_fork right_fork = 
    stm { 
        do! writeTVar left_fork true
        do! writeTVar right_fork true
    } |> atomically

p.. 
The function @take fork@ checks if a fork is available, if that is the case it marks it as taken and returns, otherwise it retries the transaction. Notice that the retry statement discard the whole enclosing transaction and starts again from the beginning. In this little bit lies the reason why STM programs do not lock out.

For example; lets suppose philosopher A takes the left fork and when he reaches for the second fork finds out it is already taken. In this case we call retry and start from the beginning again. As a result you will never find a philosopher waiting to obtain a second fork while he is holding one.

The remaining of the program is pretty straightforward.

bc..    
let forkIO (f : unit -> unit) = (new Thread(f)).Start()

let rec forever act : unit = act (); forever act

let rng = new Random()

let randomDelay () = Thread.Sleep(rng.Next(1000))   

let eatOrThink i left_fork right_fork = 
    if ((rng.Next(100)) > 50) 
    then 
        acquire left_fork right_fork
        printf "philosopher [%d] is eating.\n" i
        randomDelay ()
        release left_fork right_fork
    else 
        printf "philosopher [%d] is thinking.\n" i
        randomDelay ()

let philosopher i leftfork rightfork = 
    fun () -> forever (fun () -> eatOrThink i leftfork rightfork)

let main n = 
    let forks = Array.map (fun x -> newTVar(true)) [| 0..(n-1) |]
    List.iter (fun i -> forkIO (philosopher i forks.[i] forks.[(i + 1) % n] )) 
                 [0..(n-1)]
    
do main 5


p.. 
We can add to this little program some counters and analyze its performance, in particular how much contention there is in this scenario. Running the simulation for a long period of time we can gather some interesting statistics. These are some numbers I collected.

With two philosophers:

bc.. 
philosopher [0] - percents: 40.16 eating, 41.42 thinking, 18.42 obtaining forks
philosopher [1] - percents: 39.57 eating, 41.96 thinking, 18.47 obtaining forks

p.. 
With five philosophers:

bc.. 
philosopher [0] - percents: 34.54 eating, 35.70 thinking, 29.76 obtaining forks
philosopher [1] - percents: 34.25 eating, 35.86 thinking, 29.89 obtaining forks
philosopher [2] - percents: 34.37 eating, 35.81 thinking, 29.82 obtaining forks
philosopher [3] - percents: 34.46 eating, 35.75 thinking, 29.79 obtaining forks
philosopher [4] - percents: 34.32 eating, 35.58 thinking, 30.10 obtaining forks

p.. 
Notice there is a lot of contention and it seems to go up with the number of philosophers. 
