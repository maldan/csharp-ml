using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics.Collider;

public class BaseCollider : IRayIntersectable, IPointIntersectable
{
  // public Vector3 Center;
  public Transform Transform = new();

  /*public virtual void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    throw new Exception("unimplemented");
  }*/

  public virtual BaseCollider Clone()
  {
    return new BaseCollider { Transform = Transform.Clone() };
  }

  public virtual void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    throw new System.NotImplementedException();
  }

  public virtual bool RayIntersection(Ray ray, out Vector3 point)
  {
    RayIntersection(ray, out point, out var isHit);
    return isHit;
  }

  public virtual void PointIntersection(Vector3 point, out bool isHit)
  {
    throw new System.NotImplementedException();
  }

  public virtual bool PointIntersection(Vector3 point)
  {
    PointIntersection(point, out var isHit);
    return isHit;
  }
}