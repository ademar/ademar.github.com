--- 
mt_id: 3
layout: post
title: Calling IIsWebServer.Status via ADSI and C#
date: 2005-11-09 01:26:25 -05:00
tags:
- adsi
- csharp
- iis
---
The <a href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/iissdk/html/1103907a-56ab-4b8d-84b6-5a73bd8532ef.asp">IIS ADSI Provider reference</a> (incorrectly) states that IIsWebServer.Status is a method call, but the following code snippet fails with HRESULT 0x80020003.

```csharp
DirectoryEntry ent = 
                 new DirectoryEntry("IIS://localhost/W3SVC/1");
// next call throws 
// System.Runtime.InteropServices.COMException (0x80020003): Member not found.
int status  = (int) ent.Invoke("Status",null);

```
 
It happens that IIsWebServer.Status is actually a property and the proper calling mechanism would be :

```csharp

DirectoryEntry ent = 
                 new DirectoryEntry("IIS://localhost/W3SVC/1");
Object ads = ent.NativeObject; Type type = ads.GetType();
int status  = (int)type.InvokeMember("Status", 
                                       BindingFlags.GetProperty,
                                       null, ads, null); 

``` 

