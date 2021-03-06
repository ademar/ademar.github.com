--- 
mt_id: 9
layout: post
title: Melon-reports is up at Google Code.
date: 2008-06-11 11:17:00 -04:00
tags:
- csharp
- pdf
- printing
- reporting
---
It is a library providing basic reporting and PDF printing. The following test methods illustrate its usage.

Generating a PDF document.

```csharp
[Test]
public void Works()
{
	var pdf = new PdfDocument();

	var font  = pdf.CreateFont(PdfFontTypes.TYPE1, "Helvetica");
	var image = pdf.CreateImage(@"C:\Temp\dotnet.gif");

	var page = pdf.CreatePage( 612, 792);

	page.DrawText("a GIF image :",50,550,font,24, 
		new RgbColor());

	page.DrawImage(image, 20, 100);
	page.DrawImage(image, 250, 570, 61, 35);
			
	page.DrawRectangle(20, 40, 400, 50, new RgbColor("#330000"), 
		new RgbColor(Color.Red));

	pdf.MakeOutline(pdf.OutlineRoot, "root", page);
	pdf.Print(new FileStream(@"C:\Temp\test.pdf", FileMode.Create, 
		FileAccess.Write));
			
}

```

Loading and generating a report.
<br/>

```csharp

private Report report;
private Document document;

[Test]
public void Load()
{
	var reader = new ReportReader();
	report = reader.Load("WorldPopulation.xml");
}

[Test]
public void Generate()
{
	IDbConnection cn = 
		new MySqlConnection("Server=localhost;Database=world;User ID=user;Password=password;");

	var generator = new Generator(report) {Connection = cn};
	generator.FillReport();

	document = generator.Doc;

}

[Test]
public void Print()
{
	var printer = new PrintManager();
	var f = new FileStream("report.pdf", FileMode.Create, FileAccess.Write);
	var driver = new PDFDriver(f);

	printer.Print(document, driver);

	f.Close();
}

```

<br/>

The code is hosted at : <a href="http://code.google.com/p/melon-reports/">http://code.google.com/p/melon-reports/</a> 
