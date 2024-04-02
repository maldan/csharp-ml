using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject
{
  public class RO_Mesh : RO_Base
  {
    public string Name;

    public ListGPU<Vector3> VertexList;
    public ListGPU<Vector2> UV0List;
    public ListGPU<Vector3> NormalList;
    public ListGPU<uint> IndexList;
    public ListGPU<Vector3> TangentList;
    public ListGPU<Vector3> BiTangentList;

    public Texture_2D<(byte, byte, byte)> AlbedoTexture;
    public Texture_2D<(byte, byte, byte)> NormalTexture;
    public Texture_2D<(byte, byte, byte)> RoughnessTexture;
    public Texture_2D<(byte, byte, byte)> MetallicTexture;

    public RO_Mesh()
    {
      Transform = new Transform();
    }

    public override dynamic GetDataByName(RO_DataType type, string name)
    {
      switch (type)
      {
        case RO_DataType.Buffer:
          switch (name)
          {
            case "vertex":
              return VertexList;
            case "normal":
              return NormalList;
            case "uv0":
              return UV0List;
            case "tangent":
              return TangentList;
            case "biTangent":
              return BiTangentList;
            case "index":
              return IndexList;
          }

          break;
        case RO_DataType.Texture:
          switch (name)
          {
            case "albedo":
              return AlbedoTexture;
            case "normal":
              return NormalTexture;
            case "roughness":
              return RoughnessTexture;
            case "metallic":
              return MetallicTexture;
          }

          break;
      }

      throw new Exception($"Type {type} with Name {name} - Not found");
    }

    public void CalculateTangent()
    {
      var tangentList = new Vector3[VertexList.Count];
      var biTangentList = new Vector3[VertexList.Count];
      TangentList = new ListGPU<Vector3>(VertexList.Count);
      BiTangentList = new ListGPU<Vector3>(VertexList.Count);

      for (var i = 0; i < IndexList.Count; i += 3)
      {
        var i0 = IndexList[i];
        var i1 = IndexList[i + 1];
        var i2 = IndexList[i + 2];

        var v0 = VertexList[(int)i0];
        var v1 = VertexList[(int)i1];
        var v2 = VertexList[(int)i2];

        var uv0 = UV0List[(int)i0];
        var uv1 = UV0List[(int)i1];
        var uv2 = UV0List[(int)i2];

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

      foreach (var v in tangentList) TangentList.Add(v);
      foreach (var v in biTangentList) BiTangentList.Add(v);
    }

    public static RO_Mesh GenerateUVSphere(float radius, int longitudeSegments, int latitudeSegments)
    {
      var vertices = new ListGPU<Vector3>();
      var uvs = new ListGPU<Vector2>();
      var indices = new ListGPU<uint>();

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

      var mesh = new RO_Mesh();
      mesh.UV0List = uvs;
      mesh.VertexList = vertices;
      mesh.IndexList = indices;

      return mesh;
    }

    public static RO_Mesh GenerateCube(float size)
    {
      var m = new RO_Mesh();
      var vertices = new ListGPU<Vector3>();
      var normals = new ListGPU<Vector3>();
      var uv = new ListGPU<Vector2>();
      var indices = new ListGPU<uint>();

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
      m.UV0List = uv;
      m.IndexList = indices;

      m.CalculateTangent();

      return m;
    }
  }
}