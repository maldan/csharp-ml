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
using MegaLib.Render.Material;
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

  /*public Texture_2D<RGBA8> AlbedoTexture;
  public Texture_2D<RGB8> NormalTexture;
  public Texture_2D<byte> RoughnessTexture;
  public Texture_2D<byte> MetallicTexture;
  public RGBA32F Tint = new(1, 1, 1, 1);*/
  public Material_PBR Material = new();

  protected AABB _boundingBox;
  public AABB BoundingBox => _boundingBox * Transform.Matrix;
  public AABB LocalBoundingBox => _boundingBox;

  public RO_Mesh()
  {
    Transform = new Transform();
  }

  public RO_Mesh Clone()
  {
    return new RO_Mesh()
    {
      VertexList = VertexList,
      UV0List = UV0List,
      NormalList = NormalList,
      IndexList = IndexList,
      TangentList = TangentList,
      BiTangentList = BiTangentList,
      BoneWeightList = BoneWeightList,
      BoneIndexList = BoneIndexList,

      Material = Material
    };
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
    Material?.InitDefaultTextures();
  }

  public RO_Mesh FromMesh(Mathematics.Geometry.Mesh mesh2)
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
    Material = new Material_PBR();
    Material.InitDefaultTextures();
    Material.FromGLTF(gltfMeshPrimitive);

    // var mat = gltfMeshPrimitive.Material;
    /*// Базовую текстуру
    if (mat is { HasBaseColorTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.BaseColorTexture.Image.URI;
      // Console.WriteLine(texturePath);
      if (TextureManager.Has(texturePath))
      {
        AlbedoTexture = TextureManager.Get<RGBA8>(texturePath);
      }
      else
      {
        var texture = mat.BaseColorTexture.Image.ToTexture2D<RGBA8>();
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
        new RGBA8((byte)(mat.BaseColor.R * 255),
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
        NormalTexture = TextureManager.Get<RGB8>(texturePath);
      }
      else
      {
        var texture = mat.NormalTexture.Image.ToTexture2D<RGB8>();
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
    */

    return this;
  }
}