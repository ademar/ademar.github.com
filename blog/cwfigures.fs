#light
open List;;

let board n = [ for i in 0 .. n-1 do
                for j in 0 .. n-1 -> (i,j)];;

let print n positions =
   for x in 0 .. n-1 do
    for y in 0 .. n-1 do
       printf "%s" (if mem (x,y) positions then "Q" else ".")
    printf "\n"
   printf "\n";;
   
let inline row      (x,y) = x;
let inline col      (x,y) = y;
let inline pdiag    (x,y) = x + y;;
let inline ndiag n  (x,y) = n+(x-y)-1;;

let inline same_row   (x1, _) (x2, _) = (x1 = x2)
let inline same_col   ( _,y1) ( _,y2) = (y1 = y2)
let inline same_pdiag (x1,y1) (x2,y2) = (x1+y1 = x2+y2)
let inline same_ndiag (x1,y1) (x2,y2) = (x1-y1 = x2-y2)

type BoardState =
    val public n        : int
    val public w        : int
    val public Columns  : int[]
    val public Rows     : int[]
    val public Diag1    : int[]
    val public Diag2    : int[]

    new (N,W) = {
                 n = N; w = W
                 Columns = Array.create N W
                 Rows    = Array.create N W
                 Diag1   = Array.create (2*N-1) W
                 Diag2   = Array.create (2*N-1) W
             }

    member inline bs.c  p = bs.Columns.[col p]
    member inline bs.r  p = bs.Rows.[row p]
    member inline bs.pd p = bs.Diag1.[pdiag p]
    member inline bs.nd p = bs.Diag2.[ndiag bs.n p]

    member inline bs.alter f p =
        bs.Rows.[row p]         <- f(bs.Rows.[row p])
        bs.Columns.[col p]      <- f(bs.Columns.[col p])
        bs.Diag1.[pdiag p]      <- f(bs.Diag1.[pdiag p])
        bs.Diag2.[ndiag bs.n p] <- f(bs.Diag2.[ndiag bs.n p])  
    
    member inline bs.full p =
           (bs.c  p = 0)
        || (bs.r  p = 0)
        || (bs.pd p = 0)
        || (bs.nd p = 0);;

let mutable state = BoardState (1, 1)

let inline not_full p = not (state.full p)    

let inline take     p = state.alter (fun x -> x-1) p; 
let inline restore  p = state.alter (fun x -> x+1) p;

let inline board_without_corners n w =
    filter ( fun p ->  (pdiag p   >=     w-1) 
                    && (pdiag p   <= 2*n-w-1) 
                    && (ndiag n p <= 2*n-w-1) 
                    && (ndiag n p >=     w-1))
           (board n);;
   
let inline filter_and_count f S =
    let rec loop len P = function
        |[]       -> (len,P)
        |p::tail  -> if f p then loop (len+1) (p::P) tail
                     else loop len P tail
    loop 0 [] S

let inline collect q availables = 
    let rec loop av (r,rows) (c,columns) (d1,diag1) (d2,diag2) rem= 
        match av with
        |[] -> ((r,rows), (c,columns), (d1,diag1), (d2,diag2), rem)
        |p::tail -> if same_row q p then 
                        loop tail (r+1,p::rows) (c,columns) (d1,diag1) (d2,diag2) rem
                    else if same_col q p then 
                        loop tail (r,rows) (c+1,p::columns) (d1,diag1) (d2,diag2) rem
                    else if same_pdiag q p then 
                        loop tail (r,rows) (c,columns) (d1+1,p::diag1) (d2,diag2) rem
                    else if same_ndiag q p then 
                        loop tail (r,rows) (c,columns) (d1,diag1) (d2+1,p::diag2) rem
                    else 
                        loop tail (r,rows) (c,columns) (d1,diag1) (d2,diag2) (p::rem)
    loop availables (0,[]) (0,[]) (0,[]) (0,[]) [];;
    
let inline prune_before_continue availables F =
    F (filter_and_count not_full availables);;
   
let inline combinator availables w F (bufflen,buff) =
    let rec loop SS R k =
        match SS with
        |_ when k = 0 -> F (bufflen + w, buff @ R)
        |[] -> () 
        |p::tail -> if not_full p then
                        take p;
                        loop tail (p::R) (k-1);
                        restore p;
                    loop tail R k
    loop availables [] w ;; 

let find len F availables buffer =
    let rec search sol len (avlen,avail) (bufflen,buff) =
        match buff with
        |_ when avlen + bufflen < len -> ()
        |[]      -> F sol
        |p::tail -> let (r,rows), (c,columns), (d1,diag1), (d2,diag2), remaining = collect p avail
                    if     r  < state.r  p 
                        || c  < state.c  p 
                        || d1 < state.pd p 
                        || d2 < state.nd p then ()
                    else
                    (search (p::sol) (len-1)
                        |> (prune_before_continue remaining 
                            >> combinator diag2   (state.nd p)
                            >> combinator diag1   (state.pd p)
                            >> combinator columns (state.c  p)
                            >> combinator rows    (state.r  p)
                            )) (bufflen - 1, tail)
    search [] len availables buffer;;      
    
let mutable count = 0;;

let timer = new System.Diagnostics.Stopwatch()
                
let leo n k w =
    state <- BoardState (n,w)
    let F S =
        print  n S
        count <- count + 1;
        printf "time %dms\n\n" timer.ElapsedMilliseconds
    let rec loop L =
        match L with
        |[] -> ()
        |p::tail -> take p ; 
                    find (k*w) F (length tail,tail) (1,[p]); 
                    restore p
                    loop tail
    loop (board_without_corners n w) 

timer.Start();;

leo 11 10 3;;
printf  "%d solutions\n" count;;
printf  "took %dms\n" timer.ElapsedMilliseconds;;