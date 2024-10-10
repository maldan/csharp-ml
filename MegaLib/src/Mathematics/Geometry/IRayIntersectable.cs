using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public interface IRayIntersectable
{
  void RayIntersection(Ray ray, out Vector3 point, out bool isHit);
  bool RayIntersection(Ray ray, out Vector3 point);
}