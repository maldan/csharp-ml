using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class SphereCollider : BaseCollider
{
  public float Radius;

  public override void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    isHit = false;
    point = Vector3.Zero;

    // Получаем позицию центра сферы и радиус
    var sphereCenter = Transform.Position; // Предполагаем, что Transform содержит позицию сферы

    // Направление от начала луча к центру сферы
    var L = sphereCenter - ray.Position;

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
    var thc = (float)Math.Sqrt(Radius * Radius - d2);

    // Две возможные точки пересечения
    var t0 = tca - thc;
    var t1 = tca + thc;

    // Проверяем, попадают ли эти точки на ограниченный отрезок луча
    if (t0 > ray.Length || t1 < 0)
      return;

    // Выбираем ближайшую точку пересечения
    var t = t0 < 0 ? t1 : t0;

    // Находим точку пересечения
    point = ray.Position + ray.Direction * t;
    isHit = true;
  }
}