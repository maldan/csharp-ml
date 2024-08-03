using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using NUnit.Framework;

namespace MegaTest.Mathematics.Geometry;

public class PolygonTests
{
  [Test]
  public void TestMain()
  {
    var p = new Polygon([
      new Vector3(0, 0, 0),
      new Vector3(1, 0, 0),
      new Vector3(1, 1, 0),
      new Vector3(0, 1, 0)
    ]);
    var ray = new Ray(new Vector3(0.5f, 0.5f, -5), new Vector3(0.5f, 0.5f, 5));
    var isHit = false;
    p.RayIntersection(ray, out _, out isHit);
    Console.WriteLine(isHit);
  }

  [Test]
  public void TestTriangle()
  {
    var p = new Triangle
    {
      A = new Vector3(0, 0, 0.0001f),
      B = new Vector3(1, 0, 0.0001f),
      C = new Vector3(1, 1, 0.0001f)
    };

    var ray = new Ray(new Vector3(0.2f, 0.2f, -5), new Vector3(0.2f, 0.2f, 5));
    Console.WriteLine(ray);
    p.RayIntersection(ray, out _, out var isHit);
    Console.WriteLine(isHit);
  }
}