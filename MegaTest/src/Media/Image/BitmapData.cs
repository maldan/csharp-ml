global using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using ImageMagick;
using MegaLib.Render.Color;
using NUnit.Framework;
using BitmapData = MegaLib.Media.Image.BitmapData;

namespace MegaTest.Media.Image;

public class BitmapDataTest
{
  [Test]
  public void TestBasic()
  {
    /*var bd = new BitmapData(8, 8, 3);
    bd.SetPixel(1, 1, new RGB<byte>(255, 0, 0));
    bd.SetPixel(2, 1, new RGB<byte>(255, 0, 0));

    var bytes = ImageBMP.Encode(bd);
    File.WriteAllBytes("D:/csharp_lib/MegaLib/backup/xx.bmp", bytes);*/


    /*var d = File.ReadAllBytes("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");
    var tt = new Stopwatch();
    tt.Start();
    var bb = ImagePNG.Decode<RGBA<byte>>(d);
    tt.Stop();
    Console.WriteLine(tt.ElapsedMilliseconds);

    var bytes = ImageBMP.Encode(bb);
    File.WriteAllBytes("D:/csharp_lib/MegaLib/backup/xx.bmp", bytes);*/


    var tt = new Stopwatch();
    tt.Start();
    var b = BitmapData.FromFile<RGB<byte>>("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");
    tt.Stop();
    BitmapData.ToFile(b, "D:/csharp_lib/MegaLib/backup/b1.png");
    Console.WriteLine($"From RGB to RGB (1024x1024) {tt.ElapsedMilliseconds}");

    tt.Start();
    var b2 = BitmapData.FromFile<RGBA8>("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");
    tt.Stop();
    BitmapData.ToFile(b2, "D:/csharp_lib/MegaLib/backup/b2.png");
    Console.WriteLine($"From RGB to RGBA (1024x1024) {tt.ElapsedMilliseconds}");

    tt.Start();
    var b3 = BitmapData.FromFile<byte>("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");
    tt.Stop();
    BitmapData.ToFile(b3, "D:/csharp_lib/MegaLib/backup/b3.png");
    Console.WriteLine($"From RGB to L (1024x1024) {tt.ElapsedMilliseconds}");

    // BitmapData.ToFile(b, "D:/csharp_lib/MegaLib/backup/xx.png", ImageFormat.Png);
  }

  [Test]
  public void TestBasic2()
  {
    /*var bd = new BitmapData(8, 8, 3);
    bd.SetPixel(1, 1, new RGB<byte>(255, 0, 0));
    bd.SetPixel(2, 1, new RGB<byte>(255, 0, 0));

    var bytes = ImageBMP.Encode(bd);
    File.WriteAllBytes("D:/csharp_lib/MegaLib/backup/xx.bmp", bytes);*/

    /*var d = File.ReadAllBytes("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");
    var tt = new Stopwatch();
    tt.Start();
    var bb = ImagePNG.Decode<RGBA<byte>>(d);
    tt.Stop();
    Console.WriteLine(tt.ElapsedMilliseconds);

    var bytes = ImageBMP.Encode(bb);
    File.WriteAllBytes("D:/csharp_lib/MegaLib/backup/xx.bmp", bytes);*/

    /*var tt = new Stopwatch();
    tt.Start();
    var b = BitmapData.FromFile<byte>("D:/csharp/VR Waifu/asset/model/Tsunade/MainBody_Roughness.png");
    tt.Stop();
    BitmapData.ToFile(b, "D:/csharp_lib/MegaLib/backup/c1.png", ImageFormat.Png);
    Console.WriteLine($"From RGB to RGB (1024x1024) {tt.ElapsedMilliseconds}");*/

    // Открываем изображение
    using var image = new MagickImage("D:/csharp/VR Waifu/asset/model/Material_BaseColor.png");

    var tt = new Stopwatch();
    tt.Start();
    // Получаем сырые байты в формате RGB
    var rgbBytes = image.GetPixels().ToByteArray(PixelMapping.RGB);
    tt.Stop();
    Console.WriteLine("RGB byte array length: " + rgbBytes.Length);
    Console.WriteLine($"From RGB to RGB (1024x1024) {tt.ElapsedMilliseconds}");

    // Получаем сырые байты в формате RGBA
    tt.Start();
    var rgbaBytes = image.GetPixels().ToByteArray(PixelMapping.RGBA);
    tt.Stop();
    Console.WriteLine("RGBA byte array length: " + rgbaBytes.Length);
    Console.WriteLine($"From RGB to RGB (1024x1024) {tt.ElapsedMilliseconds}");
  }
}