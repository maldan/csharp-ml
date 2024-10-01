using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public class Mesh
{
  public List<uint> IndexList = [];
  public List<Vector3> NormalList = [];
  public List<Vector3> VertexList = [];
}