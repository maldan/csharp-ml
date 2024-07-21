using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Skin;

public class Bone
{
  // public int Index;
  public string Name;

  public Vector3 Position;
  public Quaternion Rotation = Quaternion.Identity;
  public Vector3 Scale = Vector3.One;

  public Matrix4x4 InverseBindMatrix = Matrix4x4.Identity;
  public List<Bone> Children = [];

  // Auto
  public Matrix4x4 Matrix = Matrix4x4.Identity;
  public Matrix4x4 ParentMatrix = Matrix4x4.Identity;
  public Bone ParentBone;

  public Bone Clone()
  {
    var b = new Bone
    {
      Name = Name,
      Position = Position,
      Rotation = Rotation,
      Scale = Scale,
      InverseBindMatrix = InverseBindMatrix,
      Children = Children.Select(x => x.Clone()).ToList()
    };

    return b;
  }

  // Эта функция нужна при клонировании. Когда я делаю клон то я клонирую всю иерархию.
  // Но дети должны иметь ссылки на кости в самом верхнем массиве, а не просто копии
  // Эта функция заменяет по имени всех детей на массив в котором лежат кости все
  public void MapChildren(List<Bone> bones)
  {
    for (var i = 0; i < Children.Count; i++)
    {
      Children[i] = bones.Find(x => x.Name == Children[i].Name);
      Children[i].MapChildren(bones);
    }
  }

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