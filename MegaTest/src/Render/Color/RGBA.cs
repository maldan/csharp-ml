using System.Diagnostics;
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