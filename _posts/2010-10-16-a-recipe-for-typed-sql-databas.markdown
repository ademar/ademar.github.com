--- 
mt_id: 25
layout: post
title: A recipe for typed SQL database access.
date: 2010-10-16 10:29:38 -04:00
tags:
- database
- fsharp
- monad
- sql
- workflow
---
 This is a rehash of ideas I've <a href="http://www.deanchalk.me.uk/post/Lightweight-e28098OR-Mappinge28099-in-F-Interactive.aspx">seen</a> <a href="http://cs.hubfs.net/forums/thread/11156.aspx">around</a>. Here we introduce a workflow encoding typed SQL queries computations. Consider the following code snippet.

```fsharp
open Suave.Data
open System.Data.SQLite

type Person  = { FirstName: string; LastName: string; Age: int }

let cn = new SQLiteConnection(@"Data Source = database")
cn.Open()

let queries = 
    sql cn { 
        //binding to records
        for each in "SELECT first_name, last_name, age FROM family" do
                printfn "%s, %s, %d" each.FirstName each.LastName each.Age

        //binding to tuples       
        for (a,b,c) in "SELECT first_name, last_name, age FROM family" do
                printfn "%s, %s, %d" a b c

        //let! binds to option types, fails the evaluation by returning None 
        //if the query does not have any results
        let! age = "SELECT age FROM family WHERE age = 34"
        let! person = "SELECT first_name, last_name, age FROM family WHERE age = 33"

        return person
     }

match queries with 
|Some(first_name,last_name,age) -> printfn "%s, %s, %d" first_name last_name age
|None -> printfn "query failed"
 
```
 
 There are several advantages of using such approach. The workflow provides type safety since F# will infer the types for the binding values and hides all of the boilerplate code associated with querying SQL databases. Throught the let! bindings we provide success/failure semantics with its obvious benefits. 

The <a href="http://github.com/ademar/suave/blob/master/Suave/Data.fs">code for the workflow is here</a> 
