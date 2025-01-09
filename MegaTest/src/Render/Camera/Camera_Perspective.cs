using MegaLib.Mathematics;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Camera;
using NUnit.Framework;

namespace MegaTest.Render.Camera;

public class Camera_PerspectiveTest
{
  [Test]
  public void TestBasic()
  {
    var pp = new Camera_Perspective();
    pp.AspectRatio = 800f / 600f;
    pp.Position = new Vector3(0, 0, 0);
    pp.Near = 0.1f;
    pp.Far = 64f;
    pp.CalculateProjection();
    pp.CalculateView();

    Console.WriteLine(pp.ProjectionMatrix);

    for (var i = 0f; i <= 32; i++)
    {
      var v1 = new Vector4(1, 0, i, 1.0f);
      var pm = pp.ProjectionMatrix * pp.ViewMatrix;
      var gul = v1 * pm;
      Console.WriteLine($"{gul} - {gul / gul.W}");

      var z = gul.Z / gul.W;
      var near = 0.1f;
      var far = 64f;

      var zBufferParams = new Vector4(
        1.0f - far / near, // x
        far / near, // y
        (1.0f - far / near) / far, // z
        far / near / far // w
      );
      var depth = 1.0f / (zBufferParams.Z * z + zBufferParams.W);
      Console.WriteLine(depth / (far - near));
    }
  }

  [Test]
  public void TestBasicXX()
  {
    var pp = new Camera_Perspective();
    pp.AspectRatio = 800f / 600f;
    pp.Position = new Vector3(0, 0, 0);
    pp.Near = 0.1f;
    pp.Far = 64f;
    pp.FOV = 75f;
    pp.CalculateProjection();
    pp.CalculateView();

    Console.WriteLine(pp.ProjectionMatrix);

    for (var i = -1; i <= 1; i++)
    {
      for (var j = -1; j <= 1; j++)
      {
        for (var k = -1; k <= 1; k++)
        {
          var v1 = new Vector4(i, i, k, 1.0f);
          var pm = pp.ProjectionMatrix * pp.ViewMatrix;
          var gul = v1 * pm;
          Console.WriteLine($"{v1} -> {gul}");
        }
      }
    }
  }

  [Test]
  public void TestX()
  {
    var pp = new Camera_Perspective();
    pp.AspectRatio = 800f / 600f;
    pp.Position = new Vector3(0, 0, 0);
    pp.Near = 0.1f;
    pp.Far = 64f;
    pp.XrFOV = new Vector4(-0.9075712f, 0.83775806f, 0.7330383f, -0.87266463f);
    //pp.CalculateProjection();
    pp.CalculateView();

    Console.WriteLine(pp.ProjectionMatrix);

    for (var i = -1; i <= 1; i++)
    {
      for (var j = -1; j <= 1; j++)
      {
        for (var k = -1; k <= 1; k++)
        {
          var v1 = new Vector4(i, i, k, 1.0f);
          var pm = pp.ProjectionMatrix * pp.ViewMatrix;
          var gul = v1 * pm;
          Console.WriteLine($"{v1} -> {gul}");
        }
      }
    }
  }

  [Test]
  public void TestBasicXX3()
  {
    var pp1 = new Camera_Perspective();
    pp1.AspectRatio = 800f / 600f;
    pp1.Position = new Vector3(0, 0, 0);
    pp1.Near = 0.1f;
    pp1.Far = 64f;
    pp1.FOV = 75f;
    pp1.CalculateProjection();
    pp1.CalculateView();

    var pp2 = new Camera_Perspective();
    pp2.AspectRatio = 800f / 600f;
    pp2.Position = new Vector3(0, 0, 0);
    pp2.Near = 0.1f;
    pp2.Far = 64f;
    pp2.XrFOV = new Vector4(-0.9075712f, 0.83775806f, 0.7330383f, -0.87266463f);
    pp2.CalculateView();

    Console.WriteLine(pp1.ProjectionMatrix);
    Console.WriteLine(pp2.ProjectionMatrix);

    for (var i = -1; i <= 1; i++)
    {
      for (var j = -1; j <= 1; j++)
      {
        for (var k = -1; k <= 1; k++)
        {
          var v1 = new Vector4(i, i, 5 + k, 1.0f);
          var pm1 = pp1.ProjectionMatrix * pp1.ViewMatrix;
          var pm2 = pp2.ProjectionMatrix * pp2.ViewMatrix;
          Console.WriteLine($"{v1} -> {v1 * pm1} -> {v1 * pm2}");
        }
      }
    }
  }
}