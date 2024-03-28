using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry
{
  public class Mesh
  {
    public List<uint> IndexList = new();
    public List<Vector3> NormalList = new();
    public List<Vector3> VertexList = new();
  }
}