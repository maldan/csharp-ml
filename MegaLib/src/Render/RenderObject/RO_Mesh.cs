using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject;

public class RO_Mesh : RO_Base
{
  public string Name;

  public ListGPU<Vector3> VertexList;
  public ListGPU<Vector2> UV0List;
  public ListGPU<Vector3> NormalList;
  public ListGPU<uint> IndexList;
  public ListGPU<Vector3> TangentList;
  public ListGPU<Vector3> BiTangentList;
  public ListGPU<Vector4> BoneWeightList;
  public ListGPU<uint> BoneIndexList;

  public Texture_2D<RGBA<byte>> AlbedoTexture;
  public Texture_2D<RGB<byte>> NormalTexture;
  public Texture_2D<byte> RoughnessTexture;
  public Texture_2D<byte> MetallicTexture;

  public RGBA<float> Tint = new(1, 1, 1, 1);

  protected AABB _boundingBox;
  public AABB BoundingBox => _boundingBox * Transform.Matrix;
  public AABB LocalBoundingBox => _boundingBox;

  public RO_Mesh()
  {
    Transform = new Transform();
  }

  public override void CalculateBoundingBox()
  {
    // var matrix = Transform.Matrix;

    var min = new Vector3(float.MaxValue);
    var max = new Vector3(float.MinValue);

    foreach (var v in VertexList)
    {
      // var transformedVertex = v * matrix;
      min = Vector3.Min(min, v);
      max = Vector3.Max(max, v);
    }

    _boundingBox = new AABB(min, max);
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

  public void InitDefaultTextures()
  {
    var albedo = new Texture_2D<RGBA<byte>>(2, 2)
    {
      RAW =
      {
        [0] = new RGBA<byte>(255, 255, 255, 255),
        [1] = new RGBA<byte>(128, 128, 128, 255),
        [2] = new RGBA<byte>(128, 128, 128, 255),
        [3] = new RGBA<byte>(255, 255, 255, 255)
      }
    };
    albedo.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    AlbedoTexture = albedo;

    var normal = new Texture_2D<RGB<byte>>(1, 1)
    {
      RAW =
      {
        [0] = new RGB<byte>(128, 128, 255)
      }
    };
    normal.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    NormalTexture = normal;

    var roughness = new Texture_2D<byte>(1, 1)
    {
      RAW =
      {
        [0] = 128
      }
    };
    roughness.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    RoughnessTexture = roughness;

    var metalic = new Texture_2D<byte>(1, 1)
    {
      RAW =
      {
        [0] = 0
      }
    };
    metalic.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    MetallicTexture = metalic;
  }

  /*public static RO_Mesh GenerateUVSphere(float radius, int longitudeSegments, int latitudeSegments)
  {
    var vertices = new ListGPU<Vector3>();
    var uvs = new ListGPU<Vector2>();
    var indices = new ListGPU<uint>();

    for (var lat = 0; lat <= latitudeSegments; lat++)
    {
      var theta = lat * MathF.PI / latitudeSegments;
      var sinTheta = MathF.Sin(theta);
      var cosTheta = MathF.Cos(theta);

      for (var lon = 0; lon <= longitudeSegments; lon++)
      {
        var phi = lon * 2 * MathF.PI / longitudeSegments;
        var sinPhi = MathF.Sin(phi);
        var cosPhi = MathF.Cos(phi);

        var x = cosPhi * sinTheta;
        var y = cosTheta;
        var z = sinPhi * sinTheta;

        vertices.Add(new Vector3(x, y, z) * radius);
        uvs.Add(new Vector2((float)lon / longitudeSegments, (float)lat / latitudeSegments));
      }
    }

    for (var lat = 0; lat < latitudeSegments; lat++)
    for (var lon = 0; lon < longitudeSegments; lon++)
    {
      var current = lat * (longitudeSegments + 1) + lon;
      var next = current + longitudeSegments + 1;

      indices.Add((uint)current);
      indices.Add((uint)next);
      indices.Add((uint)current + 1);

      indices.Add((uint)next);
      indices.Add((uint)next + 1);
      indices.Add((uint)current + 1);
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
    vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
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
  }*/

  public RO_Mesh FromMesh(Mesh.Mesh mesh2)
  {
    VertexList = new ListGPU<Vector3>(mesh2.VertexList);
    UV0List = new ListGPU<Vector2>(mesh2.UV0List);
    NormalList = new ListGPU<Vector3>(mesh2.NormalList);
    IndexList = new ListGPU<uint>(mesh2.IndexList);
    CalculateTangent();
    CalculateBoundingBox();
    return this;
  }

  public RO_Mesh FromGLTF(GLTF_MeshPrimitive gltfMeshPrimitive)
  {
    VertexList = new ListGPU<Vector3>(gltfMeshPrimitive.Vertices);
    UV0List = new ListGPU<Vector2>(gltfMeshPrimitive.UV0);
    IndexList = new ListGPU<uint>(gltfMeshPrimitive.Indices);
    NormalList = new ListGPU<Vector3>(gltfMeshPrimitive.Normals);

    if (gltfMeshPrimitive.Gltf.IsZInverted)
    {
      for (var i = 0; i < VertexList.Count; i++)
      {
        VertexList[i] = VertexList[i] with { Z = VertexList[i].Z * -1 };
      }

      for (var i = 0; i < NormalList.Count; i++)
      {
        NormalList[i] = NormalList[i] with { Z = NormalList[i].Z * -1 };
      }
    }

    if (gltfMeshPrimitive.BoneWeight.Count > 0)
      BoneWeightList = new ListGPU<Vector4>(gltfMeshPrimitive.BoneWeight);

    if (gltfMeshPrimitive.BoneIndex.Count > 0)
    {
      var l = new ListGPU<uint>(gltfMeshPrimitive.BoneIndex.Count);
      gltfMeshPrimitive.BoneIndex.ForEach(x => { l.Add(x.UInt32BE); });
      BoneIndexList = l;
    }

    CalculateTangent();
    CalculateBoundingBox();
    InitDefaultTextures();

    // Назначаем материалы
    var mat = gltfMeshPrimitive.Material;

    // Базовую текстуру
    if (mat is { HasBaseColorTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.BaseColorTexture.Image.URI;
      // Console.WriteLine(texturePath);
      if (TextureManager.Has(texturePath))
      {
        AlbedoTexture = TextureManager.Get<RGBA<byte>>(texturePath);
      }
      else
      {
        var texture = mat.BaseColorTexture.Image.ToTexture2D<RGBA<byte>>();
        if (texture != null)
        {
          AlbedoTexture = texture;
          TextureManager.Add(texturePath, texture);
        }
      }
    }

    if (mat is { HasBaseColorTexture: false })
    {
      AlbedoTexture.RAW.Resize(1, 1);
      AlbedoTexture.RAW.SetPixels([
        new RGBA<byte>((byte)(mat.BaseColor.R * 255),
          (byte)(mat.BaseColor.G * 255), (byte)(mat.BaseColor.B * 255),
          (byte)(mat.BaseColor.A * 255))
      ]);
    }

    // Базовую текстуру
    if (mat is { HasNormalTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.NormalTexture.Image.URI;
      Console.WriteLine(texturePath);
      if (TextureManager.Has(texturePath))
      {
        NormalTexture = TextureManager.Get<RGB<byte>>(texturePath);
      }
      else
      {
        var texture = mat.NormalTexture.Image.ToTexture2D<RGB<byte>>();
        if (texture != null)
        {
          NormalTexture = texture;
          TextureManager.Add(texturePath, texture);
        }
      }
    }

    // Базовую текстуру
    if (mat is { HasRoughnessTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.RoughnessTexture.Image.URI;
      if (TextureManager.Has(texturePath))
      {
        RoughnessTexture = TextureManager.Get<byte>(texturePath);
      }
      else
      {
        var texture = mat.RoughnessTexture.Image.ToTexture2D<byte>();
        if (texture != null)
        {
          RoughnessTexture = texture;
          TextureManager.Add(texturePath, texture);
        }
      }
    }

    return this;
  }
}