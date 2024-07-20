using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Animation;

namespace MegaLib.Render.Skin;

public class Skeleton : IAnimatable
{
  public Vector3 Position;
  public Quaternion Rotation = Quaternion.Identity;

  public readonly List<Bone> BoneList = [];

  // Cache
  private readonly Dictionary<string, Bone> _boneMap = new();

  public Bone Root => BoneList[0];

  public Bone GetBone(string name)
  {
    if (_boneMap.ContainsKey(name)) return _boneMap[name];
    var bone = BoneList.FirstOrDefault(bone => bone.Name == name);
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
    mx = mx.Translate(Position);
    mx = mx.Rotate(Rotation);

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
}