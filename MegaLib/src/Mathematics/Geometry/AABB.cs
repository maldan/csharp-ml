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

  public static AABB operator *(AABB a, Matrix4x4 matrix)
  {
    return Transform(a, matrix);
  }

  public static AABB Transform(AABB aabb, Matrix4x4 matrix)
  {
    var corners = new Vector3[8];
    corners[0] = new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Min.Z);
    corners[1] = new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Min.Z);
    corners[2] = new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Min.Z);
    corners[3] = new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Min.Z);
    corners[4] = new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Max.Z);
    corners[5] = new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Max.Z);
    corners[6] = new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Max.Z);
    corners[7] = new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Max.Z);

    for (var i = 0; i < corners.Length; i++)
    {
      corners[i] = Vector3.Transform(corners[i], matrix);
    }

    var newMin = corners[0];
    var newMax = corners[0];

    for (var i = 1; i < corners.Length; i++)
    {
      newMin = Vector3.Min(newMin, corners[i]);
      newMax = Vector3.Max(newMax, corners[i]);
    }

    return new AABB(newMin, newMax);
  }

  public bool RayIntersection(Ray ray, out Vector3 intersectionPoint)
  {
    intersectionPoint = Vector3.Zero;

    var tMin = (Min.X - ray.Position.X) / ray.Direction.X;
    var tMax = (Max.X - ray.Position.X) / ray.Direction.X;

    if (tMin > tMax)
    {
      (tMin, tMax) = (tMax, tMin); // Swap if needed
    }

    var tyMin = (Min.Y - ray.Position.Y) / ray.Direction.Y;
    var tyMax = (Max.Y - ray.Position.Y) / ray.Direction.Y;

    if (tyMin > tyMax)
    {
      (tyMin, tyMax) = (tyMax, tyMin); // Swap if needed
    }

    // If no overlap in X and Y, no intersection
    if (tMin > tyMax || tyMin > tMax)
    {
      return false;
    }

    // Find the largest tMin and smallest tMax to narrow down the intersection
    if (tyMin > tMin)
    {
      tMin = tyMin;
    }

    if (tyMax < tMax)
    {
      tMax = tyMax;
    }

    var tzMin = (Min.Z - ray.Position.Z) / ray.Direction.Z;
    var tzMax = (Max.Z - ray.Position.Z) / ray.Direction.Z;

    if (tzMin > tzMax)
    {
      (tzMin, tzMax) = (tzMax, tzMin); // Swap if needed
    }

    // If no overlap in X, Y, and Z, no intersection
    if (tMin > tzMax || tzMin > tMax)
    {
      return false;
    }

    if (tzMin > tMin)
    {
      tMin = tzMin;
    }

    if (tzMax < tMax)
    {
      tMax = tzMax;
    }

    // Ensure the intersection occurs along the ray and is within bounds of its length
    if (tMin < 0 && tMax < 0)
    {
      return false; // Ray points away from the box
    }

    var intersectionTime = tMin > 0 ? tMin : tMax;
    intersectionPoint = ray.Position + ray.Direction * intersectionTime;

    return true;
  }


  public override string ToString()
  {
    return $"AABB(Min: {Min}, Max: {Max})";
  }
}