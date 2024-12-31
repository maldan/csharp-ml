using MegaLib.Mathematics;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Voxel;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics;

public class NoiseTest
{
  [Test]
  public void TestBasic()
  {
    var smp = new SimpleNoise(123);

    for (int i = 0; i < 10; i++)
    {
      for (int j = 0; j < 10; j++)
      {
        for (int k = 0; k < 10; k++)
        {
          Console.WriteLine(smp.Noise(i, j, k));
        }
      }
    }
  }
}