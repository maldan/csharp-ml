using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct AABB
{
  public Vector3 Min;
  public Vector3 Max;

  public AABB(Vector3 p1, Vector3 p2)
  {
    Min = new Vector3(
      Math.Min(p1.X, p2.X),
      Math.Min(p1.Y, p2.Y),
      Math.Min(p1.Z, p2.Z)
    );
    Max = new Vector3(
      Math.Max(p1.X, p2.X),
      Math.Max(p1.Y, p2.Y),
      Math.Max(p1.Z, p2.Z)
    );
  }

  public Vector3 Center => (Min + Max) / 2;
  public Vector3 Size => Max - Min;

  public static AABB FromCenterAndSize(Vector3 center, Vector3 size)
  {
    return new AABB(
      new Vector3(center.X - size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2),
      new Vector3(center.X + size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2)
    );
  }

  // Проверка пересечения двух AABB
  public bool Intersects(AABB other)
  {
    return Min.X <= other.Max.X && Max.X >= other.Min.X &&
           Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
           Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
  }

  // Проверка, содержится ли точка внутри AABB
  public bool Contains(Vector3 point)
  {
    return point.X >= Min.X && point.X <= Max.X &&
           point.Y >= Min.Y && point.Y <= Max.Y &&
           point.Z >= Min.Z && point.Z <= Max.Z;
  }

  // Add
  public static AABB operator +(AABB a, Vector3 b)
  {
    return new AABB
    {
      Min = a.Min + b,
      Max = a.Max + b
    };
  }

  public override string ToString()
  {
    return $"AABB(Min: {Min}, Max: {Max})";
  }
}