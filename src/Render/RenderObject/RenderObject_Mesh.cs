using System;
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
    public Texture_Base RoughnessTexture;
    public Texture_Base MetallicTexture;

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

    public void CalculateNormals()
    {
      List<Vector3> normals = new List<Vector3>();

      for (int i = 0; i < VertexList.Count; i++)
      {
        normals.Add(Vector3.Zero);
      }

      for (int i = 0; i < IndexList.Count; i += 3)
      {
        uint i0 = IndexList[i];
        uint i1 = IndexList[i + 1];
        uint i2 = IndexList[i + 2];

        Vector3 v1 = VertexList[(int)i1] - VertexList[(int)i0];
        Vector3 v2 = VertexList[(int)i2] - VertexList[(int)i0];
        Vector3 normal = Vector3.Cross(v1, v2);

        normals[(int)i0] += normal;
        normals[(int)i1] += normal;
        normals[(int)i2] += normal;
      }

      for (int i = 0; i < normals.Count; i++)
      {
        normals[i] = normals[i].Normalized;
      }

      NormalList = normals;
    }

    public static RenderObject_Mesh GenerateUVSphere(float radius, int longitudeSegments, int latitudeSegments)
    {
      List<Vector3> vertices = new List<Vector3>();
      List<Vector2> uvs = new List<Vector2>();
      List<uint> indices = new List<uint>();

      for (int lat = 0; lat <= latitudeSegments; lat++)
      {
        float theta = lat * MathF.PI / latitudeSegments;
        float sinTheta = MathF.Sin(theta);
        float cosTheta = MathF.Cos(theta);

        for (int lon = 0; lon <= longitudeSegments; lon++)
        {
          float phi = lon * 2 * MathF.PI / longitudeSegments;
          float sinPhi = MathF.Sin(phi);
          float cosPhi = MathF.Cos(phi);

          float x = cosPhi * sinTheta;
          float y = cosTheta;
          float z = sinPhi * sinTheta;

          vertices.Add(new Vector3(x, y, z) * radius);
          uvs.Add(new Vector2((float)lon / longitudeSegments, (float)lat / latitudeSegments));
        }
      }

      for (int lat = 0; lat < latitudeSegments; lat++)
      {
        for (int lon = 0; lon < longitudeSegments; lon++)
        {
          int current = lat * (longitudeSegments + 1) + lon;
          int next = current + longitudeSegments + 1;

          indices.Add((uint)current);
          indices.Add((uint)next);
          indices.Add((uint)current + 1);

          indices.Add((uint)next);
          indices.Add((uint)next + 1);
          indices.Add((uint)current + 1);
        }
      }

      var mesh = new RenderObject_Mesh();
      mesh.UVList = uvs;
      mesh.VertexList = vertices;
      mesh.IndexList = indices;

      return mesh;
    }

    public static RenderObject_Mesh GenerateCube(float size)
    {
      var m = new RenderObject_Mesh();
      var vertices = new List<Vector3>();
      var normals = new List<Vector3>();
      var uv = new List<Vector2>();
      var indices = new List<uint>();

      // Front
      vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
      vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
      vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
      vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
      for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 0.0f, 1.0f));

      // Back
      vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
      vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
      vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
      vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
      for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 0.0f, -1.0f));

      // Top
      vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
      vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
      vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
      vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
      for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 1.0f, 0.0f));

      // Bottom
      vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
      vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
      vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
      vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
      for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, -1.0f, 0.0f));

      // Left
      vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
      vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
      vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
      vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
      for (var i = 0; i < 4; i++) normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));

      // Right
      vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
      vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
      vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
      vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
      for (var i = 0; i < 4; i++) normals.Add(new Vector3(1.0f, 0.0f, 0.0f));


      // UV
      for (var i = 0; i < 6; i++)
      {
        uv.Add(new Vector2(0.0f, 0.0f));
        uv.Add(new Vector2(1.0f, 0.0f));
        uv.Add(new Vector2(1.0f, 1.0f));
        uv.Add(new Vector2(0.0f, 1.0f));
      }

      // Indices
      for (var i = 0; i < 6; i++)
      {
        var next = (uint)(i * 4);
        indices.Add(next);
        indices.Add(1 + next);
        indices.Add(2 + next);
        indices.Add(next);
        indices.Add(2 + next);
        indices.Add(3 + next);
      }

      for (var i = 0; i < vertices.Count; i++) vertices[i] *= size;

      m.VertexList = vertices;
      m.NormalList = normals;
      m.UVList = uv;
      m.IndexList = indices;

      m.CalculateTangent();

      return m;
    }
  }
}