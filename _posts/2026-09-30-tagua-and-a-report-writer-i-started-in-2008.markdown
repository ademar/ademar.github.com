---
layout: post
title: "Tagua, and a report writer I started in 2008"
date: 2026-09-30 10:00:00 -04:00
tags:
- tagua
- dotnet
- pdf
- reporting
- rdlc
- accessibility
---

In June 2008 I started [melon-reports](https://github.com/ademar/melon-reports) on
Google Code: a small report generator for .NET, inspired by JasperReports. It had
XML templates with bands, groups and variables, a PDF writer of its own, and a
sample that listed the countries of the world by population, grouped by the
first letter of their name:

```xml
<Field name="Name" type="System.String"/>
<Field name="Population" type="System.Int64"/>

<Variable name="FirstLetter" type="System.String" level="report" formula="none">Name.Substring(0,1).ToUpper()</Variable>

<Group name="FirstLetterGroup" invariant="FirstLetter">
```

The expressions were C#, compiled on the fly with CodeDOM. It worked, I learned a
lot, and it went no further than that.

Reports never stopped being a problem in .NET, though, and this year I went back
to them. The result is [Tagua](https://taguareports.com).

## Why reports, again

Three things have changed since 2008, and none of them for the better if you print
documents from .NET.

Many applications print their invoices, statements and lists with **RDLC reports**,
rendered by Microsoft's **ReportViewer**. ReportViewer stayed on .NET Framework:
there is no Microsoft-supported way to render RDLC in ASP.NET Core, in a Linux
container or with native AOT. The reports are often the last thing tying an
application to .NET Framework.

PDFs increasingly have to meet **standards**. The European Accessibility Act
applies from June 2025, and accessible means *tagged*: a structure tree that says
what each thing on the page is, which is **PDF/UA**. Archives want **PDF/A**, and
e-invoicing mandates want **Factur-X**, a PDF/A-3 with the invoice's XML inside.
Most PDF libraries leave all of that to you.

And we now write a lot of our code with AI models, which do much better with a
declarative format they can read and check than with a designer's binary files or
code that draws text at coordinates.

## What Tagua is

I kept the concepts of melon-reports (bands, groups, variables) and the world
population sample, which is still in the test suite as a golden test, and rewrote
everything else. A template is YAML:

```yaml
title: Countries of the World
data:
  countries:
    fields: { Name: string, Continent: string, Region: string, Population: int64 }
body:
  dataset: countries
  groups:
    - name: continent
      by: =Continent
      header: [ { content: [ { type: text, value: "{Continent}" } ] } ]
      footer: [ { content: [ { type: text, value: "{Sum(Population):N0} people" } ] } ]
  detail:
    - layout: row
      content:
        - { type: text, value: "{Name}", width: 200pt }
        - { type: text, value: "{Population:N0}", align: right }
```

The expressions are no longer C#: they are a small, typed, sandboxed language,
checked when the template is compiled, so a template from an untrusted source
cannot run code, and a mistake is an error with its line and column, not a
surprise at render time. And the template holds no connection string or SQL,
as the 2008 one did: the application passes the rows, from memory or straight
from a data reader.

Underneath is a PDF writer written from scratch, again, but this time with font
subsetting, Unicode text shaping, and tagging. One line in the template makes a
document [PDF/UA](https://taguareports.com/guides/accessible-pdf-ua/),
[PDF/A](https://taguareports.com/guides/pdf-a/) or a
[Factur-X invoice](https://taguareports.com/guides/factur-x-zugferd/), and every
build checks the samples with veraPDF, the reference validator. It runs on Windows,
Linux and macOS, in containers, and compiles with native AOT; it uses no
`System.Drawing`. The same template also exports to Excel with real numbers.
Here is [the world population sample](https://taguareports.com/examples/world-population/),
seventeen years on.

## 4,759 RDLC reports

Tagua converts RDLC reports: `tagua import` reads an `.rdlc` file and writes a
template, and lists anything that needs a person. To find out what real reports
contain, rather than guess, I collected 4,759 RDLC reports from public .NET
projects on GitHub and ran every one through it. A few things surprised me:

- Most reports are ordinary: tables, groups with totals, a page header and a logo.
  Matrices are in only 5% of them.
- The features people fear are rare: custom code calls in 3.5% of reports,
  `ReportItems!` in 1.4%, `Lookup` in 0.1%.
- 44% name a font a PDF does not have built in, and in 31% the body is wider than
  the printable page, which is where the familiar blank pages in RDLC PDFs come
  from.

71% of them convert with nothing to review, and 96% with five items or fewer. I
wrote up the numbers in
[What's inside 4,759 RDLC reports](https://taguareports.com/articles/whats-inside-rdlc-reports/),
and the [migration guide](https://taguareports.com/guides/migrate-from-rdlc/) walks
through a conversion.

## Trying it

Tagua is commercial, but free for companies with fewer than 100 people and under
USD 1 million in revenue, and for noncommercial use
([pricing](https://taguareports.com/pricing/)). The packages are on NuGet:

```bash
dotnet tool install --global Tagua.Cli
tagua import Reports --out converted
```

That converts a folder of RDLC reports and writes a summary of what, if anything,
needs a look. If you have reports stuck on ReportViewer, I would like to know what
it says, and what breaks: [taguareports.com/contact](https://taguareports.com/contact/).
