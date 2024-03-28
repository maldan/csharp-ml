using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Skin
{
  public class Skeleton
  {
    public Vector3 Position;
    public Quaternion Rotation = Quaternion.Identity;

    //public readonly Dictionary<string, Bone> BoneMap = new();
    public readonly List<Bone> BoneList = new();

    public Bone Root => BoneList[0];

    //private Bone _root;

    /*public Bone Root
    {
      get => _root;
      set
      {
        AddBone(value);
        _root = value;
      }
    }*/

    /*private void AddBone(Bone bone)
    {
      BoneMap.Add(bone.Name, bone);
      BoneList.Add(bone);
      foreach (var b in bone.Children) AddBone(b);
    }*/

    public void Update()
    {
      var mx = Matrix4x4.Identity;
      mx = mx.Translate(Position);
      mx = mx.Rotate(Rotation);

      Root.Update(mx);
    }
  }
}