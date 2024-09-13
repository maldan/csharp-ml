using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class BaseCollider
{
  // public Vector3 Center;
  public Transform Transform = new();

  public virtual void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    throw new Exception("unimplemented");
  }
}