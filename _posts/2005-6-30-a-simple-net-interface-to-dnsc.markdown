--- 
mt_id: 2
layout: post
title: A simple .NET interface to dnscmd.exe
date: 2005-06-30 23:49:52 -04:00
tags:
- csharp
- dns
---
Example of usage: 

```csharp 

DnsCmd dnsCmd = new DnsCmd("my.dns.server");

dnsCmd.CreatePrimaryZone("zone.ademar.name");
dnsCmd.CreateRecord("zone.ademar.name",
    new SOARecord("@","ns1.isqsolutions.com",
DateTime.Today.ToString("yyyyMMdd") + "00",
    "admin.ademar.name",3600,600,1209600,172800));
dnsCmd.CreateRecord("zone.ademar.name",
    new ARecord("comfort","127.0.0.1"));

MXRecord mx = new MXRecord("@",10,"ma.isqsolutions.com.");
dnsCmd.CreateRecord("zone.ademar.name",mx);

``` 

Here is the link to the class implementation : <a href="http://ademar.name/blog/DnsCmd.cs.txt">DnsCmd.cs</a> 
