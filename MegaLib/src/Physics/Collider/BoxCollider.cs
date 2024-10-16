using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics.Collider;

public class BoxCollider : BaseCollider
{
  public Vector3 Size;

  public override void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    isHit = false;
    point = Vector3.Zero;

    // Получаем инвертированную матрицу трансформации бокса
    var inverseTransform = Transform.Matrix.Inverted;

    // Преобразуем луч в локальные координаты бокса
    var localRayOrigin = Vector3.Transform(ray.Position, inverseTransform);
    var localRayDirection = Vector3.TransformNormal(ray.Direction, inverseTransform).Normalized;

    // Получаем половину размеров бокса (учитывая масштабирование по каждой оси)
    var halfSize = Size / 2.0f;

    // Мин и макс координаты бокса в локальных координатах
    var boxMin = -halfSize;
    var boxMax = halfSize;

    // Переменные для расчета пересечения
    var tmin = (boxMin.X - localRayOrigin.X) / localRayDirection.X;
    var tmax = (boxMax.X - localRayOrigin.X) / localRayDirection.X;
    if (tmin > tmax) (tmin, tmax) = (tmax, tmin);

    var tymin = (boxMin.Y - localRayOrigin.Y) / localRayDirection.Y;
    var tymax = (boxMax.Y - localRayOrigin.Y) / localRayDirection.Y;
    if (tymin > tymax) (tymin, tymax) = (tymax, tymin);

    // Если диапазоны не пересекаются, пересечения нет
    if (tmin > tymax || tymin > tmax)
      return;

    // Определяем диапазон пересечения по Y
    if (tymin > tmin)
      tmin = tymin;
    if (tymax < tmax)
      tmax = tymax;

    var tzmin = (boxMin.Z - localRayOrigin.Z) / localRayDirection.Z;
    var tzmax = (boxMax.Z - localRayOrigin.Z) / localRayDirection.Z;
    if (tzmin > tzmax) (tzmin, tzmax) = (tzmax, tzmin);

    // Если диапазоны не пересекаются, пересечения нет
    if (tmin > tzmax || tzmin > tmax)
      return;

    // Определяем диапазон пересечения по Z
    if (tzmin > tmin)
      tmin = tzmin;
    if (tzmax < tmax)
      tmax = tzmax;

    // Проверяем, попадает ли пересечение на ограниченный отрезок луча
    var localRayEnd = Vector3.Transform(ray.End, inverseTransform);
    var transformedRayLength = (localRayEnd - localRayOrigin).Length;

    if (tmin < 0 || tmin > transformedRayLength)
      return;

    // Вычисляем точку пересечения
    point = localRayOrigin + localRayDirection * tmin;

    // Преобразуем точку пересечения обратно в мировые координаты
    point = Vector3.Transform(point, Transform.Matrix);
    isHit = true;
  }

  public override void PointIntersection(Vector3 point, out bool isHit)
  {
    // Получаем инвертированную матрицу трансформации
    var inverseTransform = Transform.Matrix.Inverted;

    // Преобразуем точку в локальные координаты бокса
    var localPoint = Vector3.Transform(point, inverseTransform);

    // Получаем половину размеров бокса
    var halfSize = Size / 2.0f;

    // Мин и макс координаты бокса в локальных координатах
    var boxMin = -halfSize;
    var boxMax = halfSize;

    // Проверяем, находится ли локальная точка внутри границ бокса
    isHit = localPoint.X >= boxMin.X && localPoint.X <= boxMax.X &&
            localPoint.Y >= boxMin.Y && localPoint.Y <= boxMax.Y &&
            localPoint.Z >= boxMin.Z && localPoint.Z <= boxMax.Z;
  }

  // Метод для разрешения коллизии между верле-точкой и боксом
  public void ResolveCollision(VerletPoint point)
  {
    // Получаем инвертированную матрицу трансформации бокса
    var inverseTransform = Transform.Matrix.Inverted;

    // Преобразуем позицию точки в локальные координаты бокса
    var localPoint = Vector3.Transform(point.Position, inverseTransform);

    // Получаем половину размеров бокса (бокс симметричен относительно центра)
    var halfSize = Size / 2.0f;

    // Определяем, находится ли точка внутри бокса
    var isInsideX = localPoint.X > -halfSize.X && localPoint.X < halfSize.X;
    var isInsideY = localPoint.Y > -halfSize.Y && localPoint.Y < halfSize.Y;
    var isInsideZ = localPoint.Z > -halfSize.Z && localPoint.Z < halfSize.Z;

    // Если точка внутри бокса
    if (isInsideX && isInsideY && isInsideZ)
    {
      // Определяем, насколько точка "внутри" бокса по каждой оси
      var dx = halfSize.X - Math.Abs(localPoint.X);
      var dy = halfSize.Y - Math.Abs(localPoint.Y);
      var dz = halfSize.Z - Math.Abs(localPoint.Z);

      // Найдем ближайшую поверхность (выбираем по минимальному значению)
      if (dx < dy && dx < dz)
      {
        // Выталкиваем по оси X
        localPoint.X = localPoint.X > 0 ? halfSize.X : -halfSize.X;
      }
      else if (dy < dz)
      {
        // Выталкиваем по оси Y
        localPoint.Y = localPoint.Y > 0 ? halfSize.Y : -halfSize.Y;
      }
      else
      {
        // Выталкиваем по оси Z
        localPoint.Z = localPoint.Z > 0 ? halfSize.Z : -halfSize.Z;
      }

      // Преобразуем точку обратно в мировые координаты
      point.Position = Vector3.Transform(localPoint, Transform.Matrix);

      // Чтобы избежать "прыжков", синхронизируем с предыдущей позицией
      point.PreviousPosition = point.Position;
    }
  }
}