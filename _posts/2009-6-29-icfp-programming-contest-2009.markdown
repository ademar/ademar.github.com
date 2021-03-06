--- 
mt_id: 16
layout: post
title: ICFP Programming Contest 2009
date: 2009-06-29 15:49:58 -04:00
tags:
- contest
- fsharp
- icfp
- programming
---
I gave it a try at this year's contest (<a href="http://icfpcontest.org/">http://icfpcontest.org/</a>) and although it was a failed attempt it was a very interesting problem.

It is given a virtual machine specification and a set of binaries that runs different simulation scenarios involving satellites orbiting earth. For each scenario the contestant needs to provide a set of instructions to control  the satellite thrusters and achieve the designated goals.

The first case involves performing a <a href="http://en.wikipedia.org/wiki/Hohmann_transfer_orbit">Hohmann transfer orbit</a>. That is, transferring a satellite from one orbit to another using the thrusters only two times.

<center><div style="margin: 0pt 0pt 20px 20px;" ><img alt="cwfigure-sample-11-11-3.GIF" src="http://ademar.name/blog/hohmann-transfer-orbit.PNG"  /> <br/> <center>Hohmann transfer orbit</center></div></center>
<br/>

Apparently there are some rounding errors in my implementation since even thought the trajectories appear correct to the eye (see picture above) the satellite doesn't hold within the 1000 meters of the target orbit required to scored any points. 

Here is the link to the code, which contains the virtual machine implementation and the attempt to perform the Hohmann maneuver: <a href="http://ademar.name/blog/satellites.fs.txt">satellites.fs</a>
 
