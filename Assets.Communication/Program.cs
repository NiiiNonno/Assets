// See https://aka.ms/new-console-template for more information
using Nonno.Assets.Scrolls;

Console.WriteLine("Hello, World!");

using var ds = new DirectoryScroll(new DirectoryInfo(@"C:\Users\niiin\Documents\垢穢\新しいフォルダー"));
var p = ds.Point;
ds.Insert(single: 0.5829f);
ds.Insert(@double: 0.1122334455);
ds.Insert(int32: 114514);
ds.Point = p;

//ds.Remove(out float f);
//ds.Remove(out double d);
//ds.Remove(out int i);
//Console.WriteLine(f);
//Console.WriteLine(d);
//Console.WriteLine(i);