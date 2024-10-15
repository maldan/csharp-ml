using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Animation;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Skin;

public class Skeleton : IAnimatable
{
  public Vector3 Position;
  public Quaternion Rotation = Quaternion.Identity;
  public Vector3 Scale = Vector3.One;

  public readonly List<Bone> BoneList = [];

  // Cache
  private readonly Dictionary<string, Bone> _boneMap = new();

  public Bone Root => BoneList[0];

  public Skeleton Clone()
  {
    var s = new Skeleton
    {
      Position = Position,
      Rotation = Rotation,
      Scale = Scale
    };

    // Сначала просто тупое клонирование всего
    foreach (var b in BoneList) s.BoneList.Add(b.Clone());

    // Теперь надо правильно обновить ссылки детей на родителей
    foreach (var b in s.BoneList) b.MapChildren(s.BoneList);

    s.Update();
    return s;
  }

  public Bone GetBone(string name)
  {
    if (_boneMap.ContainsKey(name)) return _boneMap[name];
    var bone = BoneList.FirstOrDefault(bone => bone.Name == name);
    if (bone == null) return null;
    _boneMap.Add(name, bone);
    return bone;
  }

  public void SetBoneRotation(string name, Quaternion rotation)
  {
    var bone = GetBone(name);
    if (bone == null) return;
    bone.Rotation = rotation;
  }

  public void SetBonePosition(string name, Vector3 position)
  {
    var bone = GetBone(name);
    if (bone == null) return;
    bone.Position = position;
  }

  public void SetBoneScale(string name, Vector3 scale)
  {
    var bone = GetBone(name);
    if (bone == null) return;
    bone.Scale = scale;
  }

  public void Update()
  {
    var mx = Matrix4x4.Identity;
    mx = mx.Translate(new Vector3(Position.X, Position.Y, -Position.Z));
    mx = mx.Rotate(Rotation);
    mx = mx.Scale(Scale);

    Root.Update(mx);
  }

  public void Animate(Animation.Animation animation)
  {
    animation?.CurrentFrame.ForEach(frame =>
    {
      if (frame.Type == AnimationKeyType.Translate) SetBonePosition(frame.Key, frame.Value.DropW());
      if (frame.Type == AnimationKeyType.Scale) SetBoneScale(frame.Key, frame.Value.DropW());
      if (frame.Type == AnimationKeyType.Rotate) SetBoneRotation(frame.Key, frame.Value.ToQuaternion());
    });
  }

  public void FromGLTF(GLTF_Skin gltfSkin)
  {
    var boneMap = new Dictionary<int, Bone>();

    // Fill plain bones
    for (var i = 0; i < gltfSkin.BoneList.Count; i++)
    {
      var bone = new Bone();
      bone.FromGLTFBone(gltfSkin.BoneList[i]);
      boneMap[gltfSkin.BoneList[i].JointId] = bone;
      BoneList.Add(bone);
    }

    // Fill hierarchy
    for (var i = 0; i < gltfSkin.BoneList.Count; i++)
    {
      var gBone = gltfSkin.BoneList[i];
      for (var j = 0; j < gBone.ChildrenBone.Count; j++)
      {
        var child = gBone.ChildrenBone[j];
        boneMap[gBone.JointId].Children.Add(BoneList.Find(x => x.Name == child.Name));
      }
    }

    Update();

    /*for (var i = 0; i < BoneList.Count; i++)
    {
      BoneList[i].InverseBindMatrix = BoneList[i].Matrix.Inverted;
    }*/

    if (gltfSkin.GLTF.IsZInverted)
    {
      for (var i = 0; i < BoneList.Count; i++)
      {
        BoneList[i].Position.Z *= -1;
        BoneList[i].Rotation.X *= -1;
        BoneList[i].Rotation.Y *= -1;


        BoneList[i].InverseBindMatrix.M02 *= -1;
        BoneList[i].InverseBindMatrix.M12 *= -1;
        BoneList[i].InverseBindMatrix.M22 *= -1;
        BoneList[i].InverseBindMatrix.M32 *= -1;

        BoneList[i].InverseBindMatrix.M20 *= -1;
        BoneList[i].InverseBindMatrix.M21 *= -1;
        BoneList[i].InverseBindMatrix.M22 *= -1;
        BoneList[i].InverseBindMatrix.M23 *= -1;
      }
    }

    for (var i = 0; i < BoneList.Count; i++)
    {
      if (BoneList[i].ParentBone != null)
      {
        BoneList[i].ParentBone.Length =
          Vector3.Distance(BoneList[i].Matrix.Position, BoneList[i].ParentBone.Matrix.Position);
      }
    }
  }
}