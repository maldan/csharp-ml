using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Skin
{
  public class Bone
  {
    // public int Index;
    public string Name;

    public Vector3 Position;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4x4 InverseBindMatrix = Matrix4x4.Identity;
    public List<Bone> Children = new();
    public Matrix4x4 Matrix = Matrix4x4.Identity;
    public Matrix4x4 ParentMatrix = Matrix4x4.Identity;
    public Bone ParentBone;

    public void Update(Matrix4x4 parent)
    {
      Matrix = Matrix4x4.Identity;
      Matrix = Matrix.Translate(Position);
      Matrix = Matrix.Rotate(Rotation);
      Matrix *= parent;

      ParentMatrix = parent;

      foreach (var bone in Children)
      {
        bone.ParentBone = this;
        bone.Update(Matrix);
      }
    }
  }
}