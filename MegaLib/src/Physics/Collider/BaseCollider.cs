using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics.Collider;

public abstract class BaseCollider : IRayIntersectable, IPointIntersectable
{
  public Transform Transform = new();

  public abstract BaseCollider Clone();

  public abstract void RayIntersection(Ray ray, out Vector3 point, out bool isHit);

  public abstract void PointIntersection(Vector3 point, out bool isHit);

  public bool RayIntersection(Ray ray, out Vector3 point)
  {
    RayIntersection(ray, out point, out var isHit);
    return isHit;
  }

  public bool PointIntersection(Vector3 point)
  {
    PointIntersection(point, out var isHit);
    return isHit;
  }
}