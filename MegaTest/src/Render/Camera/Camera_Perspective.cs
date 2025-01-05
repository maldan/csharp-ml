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
}