using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject
{
  public class RenderObject_Mesh : RenderObject_Base
  {
    public string Name = "";

    private List<Vector3> _normalList = new();

    public List<Vector3> NormalList
    {
      get => _normalList;
      set
      {
        _normalList = value;

        var v = new List<float>();
        for (var i = 0; i < value.Count; i++)
        {
          v.Add(value[i].X);
          v.Add(value[i].Y);
          v.Add(value[i].Z);
        }

        GpuNormalList = v.ToArray();
      }
    }

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
    public float[] GpuNormalList { get; private set; }
    public float[] GpuTangentList { get; private set; }
    public float[] GpuBiTangentList { get; private set; }
    public float[] GpuUVList { get; private set; }
    public uint[] GpuIndexList { get; private set; }

    public Texture_Base Texture;
    public Texture_Base NormalTexture;

    public RenderObject_Mesh()
    {
      Transform = new Transform();
    }

    public void CalculateTangent()
    {
      var tangentList = new Vector3[VertexList.Count];
      var biTangentList = new Vector3[VertexList.Count];

      for (var i = 0; i < IndexList.Count; i += 3)
      {
        var i0 = IndexList[i];
        var i1 = IndexList[i + 1];
        var i2 = IndexList[i + 2];

        var v0 = VertexList[(int)i0];
        var v1 = VertexList[(int)i1];
        var v2 = VertexList[(int)i2];

        var uv0 = UVList[(int)i0];
        var uv1 = UVList[(int)i1];
        var uv2 = UVList[(int)i2];

        var deltaPos1 = v1 - v0;
        var deltaPos2 = v2 - v0;

        var deltaUV1 = uv1 - uv0;
        var deltaUV2 = uv2 - uv0;

        var r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);

        var a = deltaPos1 * deltaUV2.Y;
        var b = deltaPos2 * deltaUV1.X;
        var a1 = deltaPos2 * deltaUV1.Y;
        var b1 = deltaPos1 * deltaUV2.X;

        var tangent = (a1 - a) * r;
        var biTangent = (b1 - b) * r;

        tangentList[i0] = tangent;
        tangentList[i1] = tangent;
        tangentList[i2] = tangent;

        biTangentList[i0] = biTangent;
        biTangentList[i1] = biTangent;
        biTangentList[i2] = biTangent;
      }

      // Fill tangent
      var v = new List<float>();
      for (var i = 0; i < tangentList.Length; i++)
      {
        v.Add(tangentList[i].X);
        v.Add(tangentList[i].Y);
        v.Add(tangentList[i].Z);
      }

      GpuTangentList = v.ToArray();

      // Fill biTangent
      v = new List<float>();
      for (var i = 0; i < biTangentList.Length; i++)
      {
        v.Add(biTangentList[i].X);
        v.Add(biTangentList[i].Y);
        v.Add(biTangentList[i].Z);
      }

      GpuBiTangentList = v.ToArray();
    }
  }
}