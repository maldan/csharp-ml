using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public interface IPointIntersectable
{
  void PointIntersection(Vector3 point, out bool isHit);
  bool PointIntersection(Vector3 point);
}