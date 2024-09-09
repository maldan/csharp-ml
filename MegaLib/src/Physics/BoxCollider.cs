using System.Diagnostics;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class BoxCollider : BaseCollider
{
  public Vector3 Size;

  public override void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    isHit = false;
    point = Vector3.Zero;

    // Получаем половину размеров бокса (центр бокса находится в его середине)
    var halfSize = Size / 2.0f;

    // Преобразуем луч в локальные координаты бокса
    // Вывод матрицы до инверсии
    //Console.WriteLine(Transform.Matrix);
    var inverseTransform = Transform.Matrix.Inverted;
    //Console.WriteLine(inverseTransform);

    var localRayOrigin = ray.Position * inverseTransform;
    var localRayDirection = Vector3.TransformNormal(ray.Direction, inverseTransform).Normalized;


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
    {
      Debug.WriteLine("Пересечения по осям X и Y нет.");
      return;
    }

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
    {
      Debug.WriteLine("Пересечения по оси Z нет.");
      return;
    }

    // Определяем диапазон пересечения по Z
    if (tzmin > tmin)
      tmin = tzmin;
    if (tzmax < tmax)
      tmax = tzmax;

    // Проверяем, попадает ли пересечение на ограниченный отрезок луча
    if (tmin < 0 || tmin > ray.Length)
    {
      Debug.WriteLine("Пересечение находится за пределами длины луча.");
      return;
    }

    // Вычисляем точку пересечения
    point = ray.Position + ray.Direction * tmin;
    isHit = true;
    Debug.WriteLine($"Пересечение произошло в точке {point}");
  }
}