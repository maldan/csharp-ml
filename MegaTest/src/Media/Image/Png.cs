global using System;
using System.Diagnostics;
using System.IO;
using MegaLib.Media.Image;
using MegaLib.Render.Color;
using NUnit.Framework;

namespace MegaTest.Media.Image;

public class PngTest
{
  [Test]
  public void TestBasic()
  {
    /*var bd = new BitmapData(8, 8, 3);
    bd.SetPixel(1, 1, new RGB<byte>(255, 0, 0));
    bd.SetPixel(2, 1, new RGB<byte>(255, 0, 0));

    var bytes = ImageBMP.Encode(bd);
    File.WriteAllBytes("D:/csharp_lib/MegaLib/backup/xx.bmp", bytes);*/

    var tt = new Stopwatch();
    tt.Start();
    var d = File.ReadAllBytes("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");
    var bb = ImagePNG.Decode(d);
    tt.Stop();
    Console.WriteLine(tt.ElapsedMilliseconds);

    var bytes = ImageBMP.Encode(bb);
    File.WriteAllBytes("D:/csharp_lib/MegaLib/backup/xx.bmp", bytes);
  }
}