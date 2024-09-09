using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public static class GeometryIntersection
{
  public static void RayIntersection(Ray ray, Triangle triangle, out Vector3 point, out bool isHit)
  {
    var edge1 = triangle.B - triangle.A;
    var edge2 = triangle.C - triangle.A;
    var h = Vector3.Cross(ray.Direction, edge2);
    var a = Vector3.Dot(edge1, h);

    if (a is > -float.Epsilon and < float.Epsilon)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    var f = 1.0 / a;
    var s = ray.Start - triangle.A;
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

  public static void RayIntersection(Ray ray, Sphere sphere, out Vector3 point, out bool isHit)
  {
    point = new Vector3();
    isHit = false;
  }

  public static void RayIntersection(Ray ray, Box box, out Vector3 point, out bool isHit)
  {
    point = new Vector3();
    isHit = false;
  }
}