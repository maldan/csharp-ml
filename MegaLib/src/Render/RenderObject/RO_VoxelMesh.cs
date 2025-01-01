using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;

namespace MegaLib.Render.RenderObject;

public class RO_VoxelMesh : RO_Base
{
  public string Name;

  public ListGPU<Vector3> VertexList = [];
  
  public RO_VoxelMesh()
  {
    
  }

  public RO_VoxelMesh Clone()
  {
    return new RO_VoxelMesh
    {
      VertexList = VertexList,
    };
  }
}