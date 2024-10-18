using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics.Collider;

public class CapsuleCollider : BaseCollider
{
  public float Radius;
  public float Height;

  public override BaseCollider Clone()
  {
    return new CapsuleCollider { Radius = Radius, Height = Height };
  }

  public override void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    throw new NotImplementedException();
  }

  public override void PointIntersection(Vector3 point, out bool isHit)
  {
    // Шаг 1: Преобразуем точку в локальные координаты капсулы
    var inverseTransform = Transform.Matrix.Inverted;
    var localPoint = Vector3.Transform(point, inverseTransform);

    // Шаг 2: Определим начало и конец капсулы в локальном пространстве
    var capsuleStart = new Vector3(0, 0, -Height / 2); // Начальная точка на оси Z
    var capsuleEnd = new Vector3(0, 0, Height / 2); // Конечная точка на оси Z

    // Шаг 3: Проверим, находится ли точка внутри расширенной капсулы
    // 1. Рассчитаем расстояние от точки до оси цилиндра
    var capsuleAxis = capsuleEnd - capsuleStart;
    var t = Vector3.Dot(localPoint - capsuleStart, capsuleAxis) / capsuleAxis.LengthSquared;
    t = Math.Clamp(t, 0, 1); // Ограничиваем t, чтобы оно находилось в пределах [0, 1]

    // Определяем ближайшую точку на оси капсулы
    var closestPointOnAxis = capsuleStart + t * capsuleAxis;

    // 2. Рассчитываем расстояние от точки до ближайшей точки на оси
    var distanceToAxis = (localPoint - closestPointOnAxis).Length;

    // Шаг 4: Проверяем попадание точки в капсулу
    isHit = distanceToAxis <= Radius;
  }
}