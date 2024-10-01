using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Plane
{
  public Vector3 Normal; // Нормаль плоскости
  public float D; // Смещение от начала координат

  // Конструктор, принимающий нормаль плоскости и точку на плоскости
  public Plane(Vector3 normal, Vector3 point)
  {
    Normal = normal;
    D = Vector3.Dot(normal, point); // Смещение вычисляется через скалярное произведение
  }

  // Конструктор, принимающий три точки на плоскости (для создания плоскости через три точки)
  public Plane(Vector3 p1, Vector3 p2, Vector3 p3)
  {
    Normal = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1)); // Нормаль через векторное произведение
    D = Vector3.Dot(Normal, p1); // Смещение через любую из точек
  }

  // Функция для пересечения луча с плоскостью
  public bool RayIntersects(Ray ray, out Vector3 point, out bool isHit)
  {
    point = Vector3.Zero;
    isHit = false;

    // Вычисляем знаменатель для проверки параллельности
    var denominator = Vector3.Dot(Normal, ray.Direction);

    // Если знаменатель мал, луч параллелен плоскости
    if (Math.Abs(denominator) > 0.0001f)
    {
      // Вычисляем параметр t для определения точки пересечения
      var t = (D - Vector3.Dot(Normal, ray.Start)) / denominator;

      // Если t >= 0, значит пересечение происходит "впереди" луча
      if (t >= 0)
      {
        point = ray.Start + t * ray.Direction; // Точка пересечения
        isHit = true;
        return true;
      }
    }

    // Если пересечения нет, возвращаем false
    return false;
  }
}