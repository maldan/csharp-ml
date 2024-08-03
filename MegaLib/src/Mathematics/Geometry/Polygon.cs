using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Polygon
{
  public List<Vector3> Points;

  public Polygon(List<Vector3> points)
  {
    Points = points;
  }

  // Метод проверки пересечения луча с полигоном
  public void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    point = Vector3.Zero;
    isHit = false;

    if (Points.Count < 3)
      return; // Полигон должен иметь как минимум три вершины

    // Вычисление нормали полигона (предполагается, что полигон не деформирован)
    var edge1 = Points[1] - Points[0];
    var edge2 = Points[2] - Points[0];
    var normal = Vector3.Cross(edge1, edge2);

    if (normal == Vector3.Zero)
      return; // Плохой полигон

    // Уравнение плоскости: n·(p - p0) = 0
    // Луч: p = o + t*d
    // Решаем для t: n·(o + t*d - p0) = 0
    // t = (n·(p0 - o)) / (n·d)

    var denom = Vector3.Dot(normal, ray.Direction);
    if (Math.Abs(denom) < float.Epsilon)
      return; // Луч параллелен плоскости полигона

    var t = Vector3.Dot(normal, Points[0] - ray.Position) / denom;
    if (t < 0 || t > ray.Length)
      return; // Пересечение за пределами длины луча

    var intersection = ray.Position + t * ray.Direction;

    // Проверка, находится ли точка пересечения внутри полигона
    var inside = true;
    var verticesCount = Points.Count;
    for (var i = 0; i < verticesCount; i++)
    {
      var v0 = Points[i];
      var v1 = Points[(i + 1) % verticesCount];
      var edge = v1 - v0;
      var toPoint = intersection - v0;
      var edgeNormal = Vector3.Cross(edge, normal);

      if (Vector3.Dot(edgeNormal, toPoint) < 0)
      {
        inside = false;
        break;
      }
    }

    if (inside)
    {
      point = intersection;
      isHit = true;
    }
  }
}