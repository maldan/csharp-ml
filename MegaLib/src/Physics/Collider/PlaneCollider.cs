using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics.Collider;

public class PlaneCollider : BaseCollider
{
  public Vector3 Normal = Vector3.Up;

  public override BaseCollider Clone()
  {
    return new PlaneCollider { Transform = Transform.Clone(), Normal = Normal };
  }

  public override void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    throw new System.NotImplementedException();
  }

  public override void PointIntersection(Vector3 point, out bool isHit)
  {
    throw new System.NotImplementedException();
  }
}