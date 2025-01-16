using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Geometry;

public struct Plane : IRayIntersectable
{
  public Vector3 Normal; // Нормаль плоскости
  public float D; // Смещение от начала координат

  // Конструктор, принимающий нормаль плоскости и точку на плоскости
  public Plane(Vector3 normal, Vector3 point)
  {
    Normal = normal.Normalized;
    D = Vector3.Dot(normal, point); // Смещение вычисляется через скалярное произведение
  }

  public Plane(Vector3 normal, float d)
  {
    Normal = normal.Normalized;
    D = d;
  }

  // Конструктор, принимающий три точки на плоскости (для создания плоскости через три точки)
  public Plane(Vector3 p1, Vector3 p2, Vector3 p3)
  {
    Normal = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1)); // Нормаль через векторное произведение
    D = Vector3.Dot(Normal, p1); // Смещение через любую из точек
  }

  // Функция для пересечения луча с плоскостью
  /*public bool RayIntersects(Ray ray, out Vector3 point, out bool isHit)
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
  }*/

  public static Vector3 GetIntersectionPoint(Plane p1, Plane p2, Plane p3)
  {
    // Вычисляем векторное произведение двух нормалей
    var cross = Vector3.Cross(p2.Normal, p3.Normal);
    // Вычисляем знаменатель
    var denominator = Vector3.Dot(p1.Normal, cross);

    if (MathF.Abs(denominator) > 1e-6f) // Проверяем, не параллельны ли плоскости
    {
      // Вычисляем точку пересечения
      var point = (p1.D * cross +
                   p2.D * Vector3.Cross(p3.Normal, p1.Normal) +
                   p3.D * Vector3.Cross(p1.Normal, p2.Normal)) / denominator;
      return point;
    }

    return Vector3.Zero; // Если плоскости не пересекаются, возвращаем (0, 0, 0)
  }

  public void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
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
        return;
      }
    }

    // Если пересечения нет, возвращаем false
    isHit = false;
  }

  public bool RayIntersection(Ray ray, out Vector3 point)
  {
    RayIntersection(ray, out point, out var isHit);
    return isHit;
  }
}