--- 
mt_id: 23
layout: post
title: A little Nmap clone
date: 2010-02-05 20:26:17 -05:00
tags:
- fsharp
- nmap
- parallel
- tcp
---
This is an anemic clone of the famous hacker tool <a href="http://nmap.org/">nmap</a>. It takes advantage of F# asynchronous workflows to parallelize the network and port scanning. It is notable that without any low level TCP trickery this little program achieves performance comparable to the original nmap.

bc.. 
open System
open System.Net
open System.Net.Sockets
open System.Net.NetworkInformation

let parallelize input f = input |> Seq.map (f)  |> Async.Parallel

let is_port_open (ip:IPAddress) (port:int)=
    try
       let ipe = new IPEndPoint(ip, port)
       use tempSocket = 
            new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
       tempSocket.Connect(ipe);
       tempSocket.Connected
    with
       |_ -> false

let inline is_port_open_async (ip:IPAddress) (port:int) = 
    async { return (port, is_port_open ip port) }

let scan_ports ports ip = parallelize ports (is_port_open_async ip)

let inline q (x:uint32[]) = 
    (x.[0] <<< 24) 
    ||| (x.[1] <<< 16) 
    ||| (x.[2] <<< 8) 
    ||| x.[3]

let getip z =
    ((z &&& uint32(0xFF000000))>>>24),
    ((z &&& uint32(0x00FF0000))>>>16),
    ((z &&& uint32(0x0000FF00))>>> 8),
    ( z &&& uint32(0x000000FF))

let _generate n m =
    seq {
          let j = ref n
          while (!j<m) do
             let (a,b,c,d) = getip !j;
             yield (new IPAddress([| byte(a);byte(b);byte(c);byte(d) |]));
             j := !j + uint32(1)
        }

let generate (ip:IPAddress) bits =
    let mask = ~~~(UInt32.MaxValue >>> bits)
    let ipBytes = ip.GetAddressBytes();
    let maskBytes = Array.rev(BitConverter.GetBytes(mask));
    let firstIPBytes = Array.map2 (fun x y -> uint32(x &&& y)) ipBytes maskBytes
    let lastIPBytes = Array.map2 (fun x y -> uint32(x ||| ~~~y)) ipBytes maskBytes
    let n = q (firstIPBytes)
    let m = q (lastIPBytes)
    _generate n m

let pingSender = new Ping()

let is_host_up (t:int) (ip:IPAddress)  =
    let reply = pingSender.Send(ip,t)
    reply.Status = IPStatus.Success

let inline is_host_up_async (t:int) (ip:IPAddress)  
    =  async { return (ip, is_host_up t ip) }

let scan_hosts ip bits = 
    parallelize (generate ip bits) (is_host_up_async 1)

let parallel_port_scanner ports host =
    async {
        let! ports = scan_ports ports host
        for (port,_open) in ports do
        if _open then printf "port\t%d\tis open\n" port
    }

let parallel_host_scanner ip bits f =
    async  {
        let! hosts = scan_hosts ip bits
        for (host,up) in hosts do
            if up then do! f host
 }

let mutable ip = IPAddress.Parse("192.168.13.146");
let mutable bits = 24;

let args = System.Environment.GetCommandLineArgs()

let all_ports     = {1 .. 65535}
let default_ports = {1 .. 1024}

let mutable port_scanning_enabled = false
let mutable range_scanning_enabled = false
let mutable ports  = default_ports

let set_port_params param_index =
    port_scanning_enabled <- true
    if Array.length  args > param_index then
        if args.[param_index].Equals("all") then ports <- all_ports
        else failwith "bad input"

let set_params _ =
    if Array.length  args < 2 then failwith "bad input"
    else
        ip <- IPAddress.Parse(args.[1])
        if Array.length args > 2 then
            if args.[2].Equals("-p") then set_port_params 3
            else
                bits <- Int32.Parse(args.[2])
                range_scanning_enabled <- true
                if Array.length args > 3 && args.[3].Equals("-p") 
                then set_port_params 4

try
    set_params()
with
    |_  ->  printf "usage: netdiscover ipaddress [bits] [-p [all]]\n"
            exit(1);

let timer = new System.Diagnostics.Stopwatch()
timer.Start();

Async.RunSynchronously <|
match range_scanning_enabled, port_scanning_enabled with
|true,true  ->  parallel_host_scanner ip bits 
                  (fun x -> async { printf "%A is up\n" x; 
                                    do! parallel_port_scanner ports x })
|true,false ->  parallel_host_scanner ip bits 
                  (fun x -> async { printf "%A is up\n" x } )
|_          ->  parallel_port_scanner ports ip

printf  "took %dms\n" timer.ElapsedMilliseconds;

p.. 
For example to scan the network 10.0.1.1/24 including a port scan you would go like this:

bc.. 
sh-3.2# mono discover.exe 10.0.1.1 24 -p
10.0.1.1 is up
port	53	is open
port	554	is open
10.0.1.200 is up
port	53	is open
10.0.1.201 is up
10.0.1.205 is up
port	88	is open
port	548	is open
took 37742ms 
