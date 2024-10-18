using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics.Collider;

public class SphereCollider : BaseCollider
{
  public float Radius;

  public override BaseCollider Clone()
  {
    return new SphereCollider { Transform = Transform.Clone(), Radius = Radius };
  }

  public override void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    isHit = false;
    point = Vector3.Zero;

    // Получаем инвертированную матрицу трансформации сферы
    var inverseTransform = Transform.Matrix.Inverted;

    // Преобразуем луч в локальные координаты сферы
    var localRayOrigin = Vector3.Transform(ray.Position, inverseTransform);
    var localRayDirection = Vector3.TransformNormal(ray.Direction, inverseTransform).Normalized;

    // Рассчитываем длину трансформированного луча
    var localRayEnd = Vector3.Transform(ray.End, inverseTransform);
    var transformedRayLength = (localRayEnd - localRayOrigin).Length;

    // В локальных координатах сфера имеет центр в (0, 0, 0) и реальный радиус
    var sphereCenter = Vector3.Zero;
    var radius = Radius; // Используем реальный радиус сферы, который задаётся вручную

    // Направление от начала луча к центру сферы
    var L = sphereCenter - localRayOrigin;

    // Проекция вектора L на направление луча
    var tca = Vector3.Dot(L, localRayDirection);

    // Если проекция отрицательная, то луч направлен в сторону от сферы
    if (tca < 0)
      return;

    // Вычисление расстояния от центра сферы до ближайшей точки на луче
    var d2 = Vector3.Dot(L, L) - tca * tca;

    // Если это расстояние больше радиуса сферы, пересечения нет
    if (d2 > radius * radius)
      return;

    // Вычисляем расстояние от точки пересечения на луче до сферы
    var thc = (float)Math.Sqrt(radius * radius - d2);

    // Две возможные точки пересечения
    var t0 = tca - thc;
    var t1 = tca + thc;

    // Проверяем, попадают ли эти точки на ограниченный отрезок **трансформированного** луча
    if (t0 > transformedRayLength || t1 < 0)
      return;

    // Выбираем ближайшую точку пересечения
    var t = t0 < 0 ? t1 : t0;

    // Преобразуем точку пересечения обратно в мировые координаты
    var localPoint = localRayOrigin + localRayDirection * t;
    point = Vector3.Transform(localPoint, Transform.Matrix);
    isHit = true;
  }

  public override void PointIntersection(Vector3 point, out bool isHit)
  {
    isHit = false;

    // Получаем инвертированную матрицу трансформации сферы
    var inverseTransform = Transform.Matrix.Inverted;

    // Преобразуем точку в локальные координаты сферы
    var localPoint = Vector3.Transform(point, inverseTransform);

    // В локальных координатах сфера имеет центр в (0, 0, 0) и реальный радиус
    var sphereCenter = Vector3.Zero;
    var radius = Radius; // Используем реальный радиус сферы, который задаётся вручную

    // Рассчитываем расстояние от центра сферы до точки
    var distance = (localPoint - sphereCenter).Length;

    // Проверяем, находится ли точка внутри сферы
    if (distance <= radius) isHit = true;
  }

  public void ResolveCollision(VerletPoint point)
  {
    // Получаем инвертированную матрицу трансформации сферы
    var inverseTransform = Transform.Matrix.Inverted;

    // Преобразуем позицию верле-точки в локальные координаты сферы
    var localPoint = Vector3.Transform(point.Position, inverseTransform);

    // В локальных координатах сфера имеет центр в (0, 0, 0) и реальный радиус
    var sphereCenter = Vector3.Zero;
    var radius = Radius; // Радиус сферы

    // Рассчитываем вектор от центра сферы до точки
    var direction = localPoint - sphereCenter;

    // Рассчитываем расстояние от центра сферы до точки
    var distance = direction.Length;

    // Если точка находится внутри сферы
    if (distance < radius)
    {
      // Нормализуем вектор направления
      var normal = direction.Normalized;

      // Сдвигаем точку на поверхность сферы
      localPoint = sphereCenter + normal * radius;

      // Преобразуем точку обратно в мировые координаты
      point.Position = Vector3.Transform(localPoint, Transform.Matrix);

      // Дополнительно обновляем PreviousPosition, чтобы избежать "прыжков" точки
      point.PreviousPosition = point.Position;
    }
  }
}