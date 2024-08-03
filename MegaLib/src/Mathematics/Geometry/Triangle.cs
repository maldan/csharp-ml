using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Triangle
{
  public Vector3 A;
  public Vector3 B;
  public Vector3 C;

  public Triangle(Vector3 a, Vector3 b, Vector3 c)
  {
    A = a;
    B = b;
    C = c;
  }

  public void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    var edge1 = B - A;
    var edge2 = C - A;
    var h = Vector3.Cross(ray.Direction, edge2);
    var a = Vector3.Dot(edge1, h);

    if (a is > -float.Epsilon and < float.Epsilon)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    var f = 1.0 / a;
    var s = ray.Start - A;
    var u = f * Vector3.Dot(s, h);

    if (u is < 0.0 or > 1.0)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    var q = Vector3.Cross(s, edge1);
    var v = f * Vector3.Dot(ray.Direction, q);

    if (v < 0.0 || u + v > 1.0)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    // Теперь вычисляем t, чтобы получить точку пересечения
    var t = (float)(f * Vector3.Dot(edge2, q));
    if (t > float.Epsilon && t <= ray.Length)
    {
      point = ray.Start + ray.Direction * t;
      isHit = true;
      return;
    }

    point = Vector3.Zero;
    isHit = false;
  }
}