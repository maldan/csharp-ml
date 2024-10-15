using System.Diagnostics;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using NUnit.Framework;

namespace MegaTest.Render.Color;

public class RGBA
{
  [Test]
  public void TestBasicA()
  {
    var t = Stopwatch.StartNew();
    var x = new Vector4();
    for (var i = 0; i < 1_000_000; i++)
    {
      x = new RGBA<float>().Vector4;
    }

    Console.WriteLine(x);
    Console.WriteLine($"Gay {t.ElapsedTicks}");
  }

  /*[Test]
  public void TestBasicB()
  {
    var t = Stopwatch.StartNew();
    var x = new Vector4();
    for (var i = 0; i < 1_000_000; i++)
    {
      x = new RGBA<float>().Vector42;
    }

    Console.WriteLine(x);
    Console.WriteLine($"42 {t.ElapsedTicks}");
  }*/

  [Test]
  public void TestLerp()
  {
    var a = new RGBA<float>(1, 1, 1, 1);
    var b = new RGBA<float>(0, 1, 0, 1);
    //var a = new RGBA<float>(0, 0, 0, 0);
    //var b = new RGBA<float>(1, 1, 1, 1);
    Console.WriteLine(RGBA<float>.Lerp(a, b, 0));
    Console.WriteLine(RGBA<float>.Lerp(a, b, 0.5f));
    Console.WriteLine(RGBA<float>.Lerp(a, b, 1));
  }

  [Test]
  public void Test2()
  {
    var bb = ((byte)1, (byte)2, (byte)3, (byte)4).ToUIntBE();
    Console.WriteLine(bb);
    var (r, g, b, a) = bb.ToBytesBE();
    Console.WriteLine($"{r} {g} {b} {a}");
  }

  public void TestBasicС()
  {
    var t = Stopwatch.StartNew();
    var x = new Vector4();
    for (var i = 0; i < 1_000_000; i++)
    {
      var c = new RGBA<float>();
      x = new Vector4(c.R, c.G, c.B, c.A);
    }

    Console.WriteLine(x);
    Console.WriteLine($"Native {t.ElapsedTicks}");
  }
}