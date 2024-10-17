using System;
using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics.LinearAlgebra;

public class Matrix4x4Tests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public void Test2()
  {
    var mx = Matrix4x4.Identity;
    mx = mx.Rotate(Quaternion.FromEuler(-10, 0, 0, "deg"));
    Console.WriteLine(mx.Rotation.Euler.ToDegrees);
    Console.WriteLine(mx);

    var v = Vector3.Up;

    Console.WriteLine(v * mx);
    Console.WriteLine();
    var mmx = new Matrix4x4(
      1.41, 0.00, -0.00, 0.00,
      0.00, 1.41, 0.00, 0.00,
      0.00, 0.00, 1.41, 0.00,
      -0.00, -0.00, 0.00, 4.00
    );
    Console.WriteLine(mmx.Position);
    Console.WriteLine(mmx.Rotation.Euler.ToDegrees);
    Console.WriteLine(mmx.Scaling);

    //mx = mx.Translate(1, 2, 3);
    //Console.WriteLine(mx.Position);
    /*Console.WriteLine(mx.Scaling);
    Console.WriteLine(mx);
    Console.WriteLine();
    var mx2 = mx;

    mx = Matrix4x4.Identity;
    mx = mx.Translate(mx2.Position);
    mx = mx.Rotate(mx2.Rotation);
    mx = mx.Scale(mx2.Scaling);
    Console.WriteLine(mx.Position);
    Console.WriteLine(mx.Rotation.Euler.ToDegrees);
    Console.WriteLine(mx.Scaling);
    Console.WriteLine(mx);
    Console.WriteLine();*/
  }
}