using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Sphere : IRayIntersectable, IPointIntersectable
{
  public Vector3 Position;
  public float Radius;

  public Sphere(float radius)
  {
    Radius = radius;
  }

  public void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    isHit = false;
    point = Vector3.Zero;

    // Направление от начала луча к центру сферы
    var L = Position - ray.Position;

    // Проекция вектора L на направление луча
    var tca = Vector3.Dot(L, ray.Direction);

    // Если проекция отрицательная, то луч направлен в сторону от сферы
    if (tca < 0)
      return;

    // Вычисление расстояния от центра сферы до ближайшей точки на луче
    var d2 = Vector3.Dot(L, L) - tca * tca;

    // Если это расстояние больше радиуса сферы, пересечения нет
    if (d2 > Radius * Radius)
      return;

    // Вычисляем расстояние от точки пересечения на луче до сферы
    var thc = MathF.Sqrt(Radius * Radius - d2);

    // Две возможные точки пересечения
    var t0 = tca - thc;
    var t1 = tca + thc;

    // Проверяем, попадают ли эти точки на отрезок луча
    if (t0 < 0 && t1 < 0)
      return;

    // Выбираем ближайшую точку пересечения
    var t = t0 < 0 ? t1 : t0;

    // Вычисляем точку пересечения
    point = ray.Position + ray.Direction * t;
    isHit = true;
  }

  public bool RayIntersection(Ray ray, out Vector3 point)
  {
    RayIntersection(ray, out point, out var isHit);
    return isHit;
  }

  public void PointIntersection(Vector3 point, out bool isHit)
  {
    var distanceSquared = Vector3.DistanceSquared(Position, point);
    isHit = distanceSquared < Radius * Radius;
  }

  public bool PointIntersection(Vector3 point)
  {
    PointIntersection(point, out var isHit);
    return isHit;
  }
}