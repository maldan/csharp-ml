using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics.Collider;
using MegaLib.Render.Animation;
using MegaLib.Render.Color;
using MegaLib.Render.Skin;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject;

internal struct GasLight
{
  public int BoneIndex;
  public float Weight;
}

public class RO_Skin : RO_Base, IAnimatable
{
  public Skeleton Skeleton;
  public List<RO_Mesh> MeshList = [];
  public Texture_2D<float> BoneTexture;
  public RGBA32F Tint = new(1, 1, 1, 1);

  public RO_Skin()
  {
    Transform = new Transform();
    Skeleton = new Skeleton();
    BoneTexture = new Texture_2D<float>(64, 64);
    BoneTexture.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    BoneTexture.Options.WrapMode = TextureWrapMode.Clamp;
    BoneTexture.Options.Format = TextureFormat.R32F;
    BoneTexture.Options.UseMipMaps = false;
  }

  public RO_Skin Instantiate()
  {
    var s = new RO_Skin();

    s.Skeleton = Skeleton.Clone();
    // s.Transform = Transform.Clone();
    s.MeshList = MeshList;

    return s;
  }

  /*public RO_Skin Clone()
  {
    var s = new RO_Skin();
    s.Skeleton = Skeleton.Clone();

    if (Transform != null) s.Transform = Transform.Clone();
    foreach (var mesh in MeshList)
    {
      mesh.Transform = mesh.Transform.Clone();
      s.MeshList.Add(mesh);
    }

    return s;
  }*/

  public void Update()
  {
    Skeleton.Update();
    if (Transform != null)
    {
      Skeleton.Position.X = Transform.Position.X;
      Skeleton.Position.Y = Transform.Position.Y;
      Skeleton.Position.Z = -Transform.Position.Z;

      Skeleton.Rotation = Transform.Rotation;
      Skeleton.Scale = Transform.Scale;
    }

    // Update bone texture
    // var pixel = new float[64 * 64];
    var id = 0;

    foreach (var bone in Skeleton.BoneList)
    {
      // Надо бы будет вынести на GPU умножение
      var mx = bone.InverseBindMatrix * bone.Matrix;
      var raw = mx.Raw;

      foreach (var v in raw)
      {
        BoneTexture.RAW[id] = v;
        id += 1;
      }
    }

    //BoneTexture.SetPixels(pixel);
    //BoneTexture.IsChanged = true;
  }

  public void CalculateWeights()
  {
    // Очищаем старое
    foreach (var mesh in MeshList)
    {
      for (var j = 0; j < mesh.BoneIndexList.Count; j++) mesh.BoneIndexList[j] = 0;
      for (var j = 0; j < mesh.BoneWeightList.Count; j++) mesh.BoneWeightList[j] = new Vector4();
    }

    /*Update();

    for (var j = 0; j < Skeleton.BoneList.Count; j++)
    {
      Skeleton.BoneList[j].CalculateInverseMatrix();
    }*/

    foreach (var mesh in MeshList)
    {
      var meshData = new List<List<GasLight>>(mesh.VertexList.Count);
      for (var k = 0; k < mesh.VertexList.Count; k++) meshData.Add([]);

      for (var j = 0; j < Skeleton.BoneList.Count; j++)
      {
        var bone = Skeleton.BoneList[j];

        foreach (var collider in bone.Colliders)
        {
          var c2 = collider.Clone();
          c2.Transform *= bone.Matrix;

          for (var k = 0; k < mesh.VertexList.Count; k++)
          {
            var isInside = c2.PointIntersection(mesh.VertexList[k]);
            if (!isInside) continue;

            // var (r, g, b, a) = mesh.BoneIndexList[k].ToBytesBE();
            // var w = mesh.BoneWeightList[k];

            var distance = Vector3.Distance(c2.Transform.Position, mesh.VertexList[k]);
            var weight = 1.0f / Math.Max(distance, 0.0001f); // Чем ближе вершина, тем больше вес

            meshData[k].Add(new GasLight
            {
              BoneIndex = j,
              Weight = weight
            });

            /*if (r == 0)
            {
              r = (byte)j;
              w.X = weight;
            }
            else if (g == 0)
            {
              g = (byte)j;
              w.Y = weight;
            }
            else if (b == 0)
            {
              b = (byte)j;
              w.Z = weight;
            }
            else if (a == 0)
            {
              a = (byte)j;
              w.W = weight;
            }

            mesh.BoneIndexList[k] = (r, g, b, a).ToUIntBE();
            mesh.BoneWeightList[k] = w;*/
          }
        }
      }

      // Применяем веса
      for (var k = 0; k < mesh.VertexList.Count; k++)
      {
        // Сортируем по влиянию
        meshData[k].Sort((a, b) => a.Weight.CompareTo(b.Weight));

        // Проставляем индексы
        mesh.BoneIndexList[k] = (
          (byte)(meshData[k].Count > 0 ? meshData[k][0].BoneIndex : 0),
          (byte)(meshData[k].Count > 1 ? meshData[k][1].BoneIndex : 0),
          (byte)(meshData[k].Count > 2 ? meshData[k][2].BoneIndex : 0),
          (byte)(meshData[k].Count > 3 ? meshData[k][3].BoneIndex : 0)
        ).ToUIntBE();

        // Проставляем веса
        mesh.BoneWeightList[k] = Vector4.NormalizeWeights(new Vector4(
          meshData[k].Count > 0 ? meshData[k][0].Weight : 0,
          meshData[k].Count > 1 ? meshData[k][1].Weight : 0,
          meshData[k].Count > 2 ? meshData[k][2].Weight : 0,
          meshData[k].Count > 3 ? meshData[k][3].Weight : 0
        ));
      }
    }

    // Нормализация весов
    /*foreach (var mesh in MeshList)
    {
      for (var k = 0; k < mesh.VertexList.Count; k++)
      {
        mesh.BoneWeightList[k] = mesh.BoneWeightList[k].Normalized;
      }
    }*/
  }

  public void Animate(Animation.Animation animation)
  {
    Skeleton.Animate(animation);
  }

  public void FromGLTF(GLTF_Skin gltfSkin)
  {
    var skeleton = new Skeleton();
    skeleton.FromGLTF(gltfSkin);
    Skeleton = skeleton;

    foreach (var gMesh in gltfSkin.MeshList)
    foreach (var gPrimitive in gMesh.Primitives)
    {
      var mesh = new RO_Mesh
      {
        Name = gMesh.Name
      };
      mesh.FromGLTF(gPrimitive);
      MeshList.Add(mesh);
    }

    Update();
  }
}