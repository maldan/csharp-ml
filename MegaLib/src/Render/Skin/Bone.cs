using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Skin;

public class Bone
{
  // public int Index;
  public string Name;

  public Vector3 Position;
  public Quaternion Rotation = Quaternion.Identity;
  public Vector3 Scale = Vector3.One;
  public float Length;

  public Matrix4x4 InverseBindMatrix = Matrix4x4.Identity;
  public List<Bone> Children = [];

  // Auto
  public Matrix4x4 Matrix = Matrix4x4.Identity;
  public Matrix4x4 ParentMatrix = Matrix4x4.Identity;
  public Bone ParentBone;

  public bool IsConstrained;

  public Quaternion WorldRotation
  {
    get
    {
      // Получаем полное мировое вращение всех родителей
      var totalParentRotation = GetTotalParentRotation();

      // Мировое вращение = полное вращение родителя * локальное вращение
      return totalParentRotation * Rotation;
    }
    set
    {
      // Получаем полное мировое вращение всех родителей
      var totalParentRotation = GetTotalParentRotation();

      // Инвертируем вращение родителя
      var totalParentRotationInverse = totalParentRotation.Inverted;

      // Преобразуем мировое вращение в локальное
      Rotation = totalParentRotationInverse * value;
    }
  }

  public Vector3 WorldPosition
  {
    get => Matrix.Position;
    set
    {
      var mx = ParentMatrix.Inverted;
      Position = mx.Translate(value).Position;
      Update(ParentMatrix);
    }
  }

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

  private void Constraint()
  {
    /*if (!IsConstrained) return;

    var euler = Rotation.Euler.ToDegrees;
    if (euler.X < -90) euler.X = -90;
    if (euler.X > 0) euler.X = 0;
    Rotation = Quaternion.FromEuler(euler, "deg");*/
  }

  public void Update(Matrix4x4 parent)
  {
    Constraint();

    Matrix = Matrix4x4.Identity;
    Matrix = Matrix.Translate(Position);
    Matrix = Matrix.Rotate(Rotation);
    Matrix = Matrix.Scale(Scale);
    Matrix *= parent;

    ParentMatrix = parent;

    foreach (var bone in Children)
    {
      bone.ParentBone = this;
      bone.Update(Matrix);
    }
  }

  private Quaternion GetTotalParentRotation()
  {
    var totalRotation = Quaternion.Identity;

    // Перемещаемся вверх по иерархии родителей и комбинируем их вращения
    var currentParent = ParentBone; // Начинаем с непосредственного родителя
    while (currentParent != null)
    {
      totalRotation = currentParent.Rotation * totalRotation;
      currentParent = currentParent.ParentBone; // Переходим к следующему родителю
    }

    return totalRotation;
  }

  public void FromGLTFBone(GLTF_Bone gltfBone)
  {
    Position = gltfBone.Position;
    Rotation = gltfBone.Rotation;
    Scale = gltfBone.Scale;
    Name = gltfBone.Name;
    InverseBindMatrix = gltfBone.InverseBindMatrix;
  }
}