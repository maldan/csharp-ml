using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Box : IRayIntersectable
{
  public Matrix4x4 Matrix;
  public Vector3 Size;

  public void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    isHit = false;
    point = Vector3.Zero;

    // Получаем инвертированную матрицу трансформации бокса
    var inverseTransform = Matrix.Inverted;

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
    point = Vector3.Transform(point, Matrix);
    isHit = true;
  }

  public bool RayIntersection(Ray ray, out Vector3 point)
  {
    RayIntersection(ray, out point, out var isHit);
    return isHit;
  }
}