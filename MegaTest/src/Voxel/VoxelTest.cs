using System.Diagnostics;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Voxel;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Voxel;

public class VoxelTest
{
  [Test]
  public void TestBasic()
  {
    var vx = new SparseVoxelMap8();

    ClassicAssert.AreEqual(vx.GetChunkPosition(0, 0, 0), new IVector3(0, 0, 0));
    ClassicAssert.AreEqual(vx.GetChunkPosition(-1, 0, 0), new IVector3(-1, 0, 0));
    ClassicAssert.AreEqual(vx.GetChunkPosition(-2, 0, 0), new IVector3(-1, 0, 0));
  }

  [Test]
  public void TestBasic2()
  {
    var vx = new SparseVoxelMap8();

    Console.WriteLine(vx.GetChunkPosition(0, 0, 0) * 16);
    Console.WriteLine(vx.GetChunkPosition(-1, 0, 0) * 16);
    Console.WriteLine(vx.GetChunkPosition(-2, 0, 0) * 16);
  }

  [Test]
  public void TestSpeed()
  {
    var vx = new SparseVoxelMap8();
    var areaSize = 64;
    var aa = areaSize * 2;
    var tt = new Stopwatch();
    tt.Start();
    for (var x = -areaSize; x < areaSize; x++)
    {
      for (var y = -areaSize; y < areaSize; y++)
      {
        for (var z = -areaSize; z < areaSize; z++)
        {
          //vx[x, y, z] = 1;
        }
      }
    }

    tt.Stop();
    Console.WriteLine($"Total: {aa * aa * aa}");
    Console.WriteLine(tt.ElapsedTicks);
  }

  [Test]
  public void TestSpeed2()
  {
    var vx = new SparseVoxelMap8();
    var areaSize = 64;
    var aa = areaSize * 2;
    var tt = new Stopwatch();
    tt.Start();
    var a = 0;
    for (var x = -areaSize; x < areaSize; x++)
    {
      for (var y = -areaSize; y < areaSize; y++)
      {
        for (var z = -areaSize; z < areaSize; z++)
        {
          a += vx.HasDataAt(x, y, z) ? 1 : 0;
        }
      }
    }

    tt.Stop();
    Console.WriteLine($"Total: {aa * aa * aa}");
    Console.WriteLine(tt.ElapsedTicks);
    Console.WriteLine(a);
  }
}