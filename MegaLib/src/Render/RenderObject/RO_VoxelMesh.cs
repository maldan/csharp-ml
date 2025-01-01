using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;

namespace MegaLib.Render.RenderObject;

public class RO_VoxelMesh : RO_Base
{
  public string Name;

  public ListGPU<Vector3> VertexList = [];
  public ListGPU<uint> ColorList = [];
  public ListGPU<int> VoxelInfoList = [];
  public ListGPU<int> ShadowInfoList = [];
  
  public RO_VoxelMesh()
  {
    
  }

  public RO_VoxelMesh Clone()
  {
    return new RO_VoxelMesh
    {
      ColorList = ColorList,
      VertexList = VertexList,
      VoxelInfoList = VoxelInfoList,
      ShadowInfoList = ShadowInfoList,
    };
  }
}