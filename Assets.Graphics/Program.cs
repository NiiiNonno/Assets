//using System.Numerics;
//using Nonno.Assets.Graphics;
//using Nonno.Assets.Graphics.Kage;

//var writer = new GothicWriter();
//await writer.Load(@"C:\Users\niiin\Downloads\dump.tar\dump\dump_newest_only.txt");
////await writer.Load(@"E:\bigdata\wiktionary\forTest\Uni2000single.txt");
//var bmp = new ColorBitmap() { Range = new(1600,800) };
//bmp.Clear();
//var pen = new BitmapLinePen(bmp);

Console.WriteLine("<<<<読込完了>>>>");

//int c = 0;
//int ofs_x = 0;
//int ofs_y = 0;
//for (int j = 0x3400; true; j++)
//{
//    var key = $"u{j:x4}";
//    Console.WriteLine($"{key}");

//    try
//    {
//        writer.Mode = 0;
//        pen.ColorNumber = 0;
//        writer.Draw(with: pen, key, (new(new(ofs_x * 50, ofs_y * 50)), new(50, 50)));
//        var lines = writer._dict[key];
//    }
//    catch (Exception e)
//    {
//        Console.WriteLine(e);
//    }

//    if ((j & 0x1FF) == 0) Console.ReadLine();

//    if (++ofs_x == 32)
//    {
//        ofs_x = 0;
//        if (++ofs_y == 16)
//        {
//            ofs_y = 0;
//            using (var stream = File.OpenWrite($@"E:\picture\描画物\生成\d{c++}.bmp"))
//            {
//                bmp.Save(stream);
//                bmp.Clear();
//            }
//        }
//    }
//}

//while (true)
//{
//    Console.Write(">>> ");
//    if (Console.ReadLine() is not { } code) continue;

//    writer.Draw(with: pen, code, (default, new(200, 200)));

//    using (var stream = File.OpenWrite($@"E:\picture\描画物\生成\{code}.bmp"))
//    {
//        bmp.Save(stream);
//        bmp.Clear();
//    }
//}
