using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Texture;
using NUnit.Framework;

namespace MegaTest
{
  public class Tests
  {
    [SetUp]
    public void Setup()
    {
    }

    /*[Test]
    public void Test1()
    {
      var v1 = new Vector3(1.0f, 0.0f, 0.0f);
      var v2 = new Vector3(1.0f, 0.0f, 0.0f);
      var v3 = v1 + v2;
      Assert.AreEqual(v3.X, 2.0f);
    }*/
    
    [Test]
    public void Test2()
    {
      var v1 = Quaternion.FromEuler(45.0f, 0.0f, 0.0f, "deg");
      var v2 = Quaternion.FromEuler(45.0f, 0.0f, 0.0f, "deg");
      var v3 = v1 * v2;
      Assert.AreEqual((int)v3.Euler.ToDegrees.X, 90);
    }
  }
}