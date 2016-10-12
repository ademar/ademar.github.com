--- 
mt_id: 8
layout: post
title: Quick ASP.NET visitor counter
date: 2008-06-11 10:22:30 -04:00
tags:
- asp.net
- code
- mutex
- web
---

```chsarp
private static Mutex mut = new Mutex();

private MemoryStream GetImage(string s,int width,int height,ImageFormat f) {
          
	Bitmap b = new Bitmap(width,height);
	Graphics g = Graphics.FromImage(b);
	g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0,0,width,height));
	g.DrawString(s,new Font("Arial",16),new SolidBrush(Color.Black),new RectangleF(0,0,width,height));
	MemoryStream mstream = new MemoryStream();
	b.Save(mstream,f);
		
	return mstream ;
}
      
private string GetCount(){

	int count = 0 ; 
	FileStream fs ;
	string FILE_NAME = Server.MapPath("~") + @"\counter.txt";
		
	mut.WaitOne();
	
	if (File.Exists(FILE_NAME)){
		
		fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader(fs);
		
		count = r.ReadInt32();r.Close();fs.Close();
	}
	else
	{
		fs = new FileStream(FILE_NAME, FileMode.CreateNew);
		fs.Close() ;
	}
		
	count++ ;
        
	fs = new FileStream(FILE_NAME, FileMode.Open);
	BinaryWriter w = new BinaryWriter(fs);
		
	w.Write(count);w.Close();fs.Close();
	mut.ReleaseMutex();

	return count.ToString() ;	
} 

```


