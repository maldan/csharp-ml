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
  public virtual void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    throw new System.NotImplementedException();
  }

  public virtual bool RayIntersection(Ray ray, out Vector3 point)
  {
    throw new System.NotImplementedException();
  }

  public virtual void PointIntersection(Vector3 point, out bool isHit)
  {
    throw new System.NotImplementedException();
  }

  public virtual bool PointIntersection(Vector3 point)
  {
    throw new System.NotImplementedException();
  }
}