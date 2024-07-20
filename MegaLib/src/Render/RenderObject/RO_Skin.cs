using System;
using System.Collections.Generic;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Skin;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject;

public class RO_Skin : RO_Base
{
  public Skeleton Skeleton;
  public List<RO_Mesh> MeshList = new();
  public Texture_2D<float> BoneTexture;

  public RO_Skin()
  {
    Skeleton = new Skeleton();
    BoneTexture = new Texture_2D<float>(64, 64);
    BoneTexture.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    BoneTexture.Options.WrapMode = TextureWrapMode.Clamp;
    BoneTexture.Options.Format = TextureFormat.R32F;
    BoneTexture.Options.UseMipMaps = false;
  }

  public void Update()
  {
    Skeleton.Update();
    if (Transform != null)
    {
      Skeleton.Position.X = Transform.Position.X;
      Skeleton.Position.Y = Transform.Position.Y;
      Skeleton.Position.Z = -Transform.Position.Z;

      Skeleton.Rotation = Transform.Rotation;
    }

    // Update bone texture
    // var pixel = new float[64 * 64];
    var id = 0;

    foreach (var bone in Skeleton.BoneList)
    {
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
}

public static class SkinEx
{
  public static void FromGLTF(this RO_Skin skin, GLTF_Skin gltfSkin)
  {
    var skeleton = new Skeleton();
    skeleton.FromGLTFSkin(gltfSkin);
    skin.Skeleton = skeleton;

    foreach (var gMesh in gltfSkin.MeshList)
    foreach (var gPrimitive in gMesh.Primitives)
    {
      var mesh = new RO_Mesh
      {
        Name = gMesh.Name
      };
      mesh.FromGLTF(gPrimitive);
      skin.MeshList.Add(mesh);
    }

    skin.Update();
  }

  public static void FromGLTFSkin(this Skeleton skeleton, GLTF_Skin gltfSkin)
  {
    var boneMap = new Dictionary<int, Bone>();

    // Fill plain bones
    for (var i = 0; i < gltfSkin.BoneList.Count; i++)
    {
      var bone = new Bone();
      // bone.Index = gltfSkin.BoneList[i].NodeId;
      bone.FromGLTFBone(gltfSkin.BoneList[i]);
      boneMap[gltfSkin.BoneList[i].JointId] = bone;
      skeleton.BoneList.Add(bone);
    }

    // Fill hierarchy
    for (var i = 0; i < gltfSkin.BoneList.Count; i++)
    {
      var gBone = gltfSkin.BoneList[i];
      for (var j = 0; j < gBone.ChildrenBone.Count; j++)
      {
        var child = gBone.ChildrenBone[j];
        boneMap[gBone.JointId].Children.Add(skeleton.BoneList.Find(x => x.Name == child.Name));
      }
    }

    // var rootBone = boneMap[gltfSkin.BoneList[0].Id];
  }

  public static void FromGLTFBone(this Bone bone, GLTF_Bone gltfBone)
  {
    bone.Position = gltfBone.Position;
    bone.Rotation = gltfBone.Rotation;
    bone.Scale = gltfBone.Scale;
    bone.Name = gltfBone.Name;
    bone.InverseBindMatrix = gltfBone.InverseBindMatrix;
    // bone.Index = gltfBone.Id;

    /*for (var i = 0; i < gltfBone.Children.Count; i++)
    {
      var childBone = gltfBone.GLTF.SkinList[gltfBone.SkinId].BoneList.Find(x => x.Id == gltfBone.Children[i]);
      var newBone = new Bone();
      newBone.FromGLTFBone(childBone);
      bone.Children.Add(newBone);
    }*/
  }
}