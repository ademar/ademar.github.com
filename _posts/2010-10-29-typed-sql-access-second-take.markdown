--- 
mt_id: 26
layout: post
title: Typed SQL access, second take
date: 2010-10-29 10:28:22 -04:00
tags:
- database
- fsharp
- sql
- typed
- workflow
---
 It gets better. Using this <a href="http://bugsquash.blogspot.com/2010/07/abusing-printfformat-in-f.html">nifty trick</a> from Mauricio Scheffer we can enhance our previous <a href="http://ademar.name/blog/2010/10/a-recipe-for-typed-sql-databas.html">SQL computations</a> with typed parameters and protect our code against SQL injections.

bc.. 
open Suave.Data
open System.Data.SQLite

type Person  = { FirstName: string; LastName: string; Age: int }

let cn = new SQLiteConnection("Data Source = database")
cn.Open()

let tx = sql cn

let queries = 
    [ 
        ("Albert", "Einstein", 32);
        ("Leonard", "Euler", 33);
        ("Benoit", "Mandelbrot", 34)
    ]
    |> List.iter (fun (a,b,c)  
                    -> tx.Query @"insert into family (first_name,last_name,age) 
                                                    values (%s,%s,%d)" a b c 
                    |> executeNonQuery)
    
    tx { 
        // binding to records
        for each in tx.Query "SELECT * FROM family WHERE age>%d" 30 do
                printfn "%s, %s, %d" each.FirstName each.LastName each.Age

        // binding to tuples       
        for (a,b,c) in tx.Query "SELECT * FROM family WHERE last_name like %s" "%brot" do
                printfn "%s, %s, %d" a b c
        
        // let! binds to option types, fails the evaluation by returning None 
        //if the query does not have any results
        let!  age = tx.Query "SELECT age FROM family WHERE first_name = %s" "Leonard"
        let query = tx.Query "SELECT * FROM family WHERE age = %d and first_name = %s"
        let! person =   query 33 "Leonard" 

        return person
     }

match queries with 
|Some(first_name,last_name,age) -> printfn "%s, %s, %d" first_name last_name age
|None -> printfn "query failed."

tx.Query "delete from family" |> executeNonQuery

System.Console.ReadLine() |> ignore 
