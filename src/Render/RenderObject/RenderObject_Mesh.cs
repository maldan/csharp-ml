using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject
{
  public class RenderObject_Mesh : RenderObject_Base
  {
    public string Name = "";
    public List<Vector3> NormalList = new();

    private List<Vector3> _vertexList = new();

    public List<Vector3> VertexList
    {
      get => _vertexList;
      set
      {
        _vertexList = value;

        var v = new List<float>();
        for (var i = 0; i < value.Count; i++)
        {
          v.Add(value[i].X);
          v.Add(value[i].Y);
          v.Add(value[i].Z);
        }

        GpuVertexList = v.ToArray();
      }
    }

    private List<Vector2> _uvList = new();

    public List<Vector2> UVList
    {
      get => _uvList;
      set
      {
        _uvList = value;

        var v = new List<float>();
        for (var i = 0; i < value.Count; i++)
        {
          v.Add(value[i].X);
          v.Add(value[i].Y);
        }

        GpuUVList = v.ToArray();
      }
    }

    private List<uint> _indexList = new();

    public List<uint> IndexList
    {
      get => _indexList;
      set
      {
        _indexList = value;
        GpuIndexList = Enumerable.ToArray(value);
      }
    }

    public float[] GpuVertexList { get; private set; }
    public float[] GpuUVList { get; private set; }
    public uint[] GpuIndexList { get; private set; }

    public Texture_Base Texture;
  }
}