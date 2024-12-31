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
}