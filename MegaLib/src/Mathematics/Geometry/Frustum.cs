using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Camera;

namespace MegaLib.Mathematics.Geometry;

public struct Frustum
{
  public Plane TopPlane;
  public Plane BottomPlane;
  public Plane LeftPlane;
  public Plane RightPlane;
  public Plane NearPlane;
  public Plane FarPlane;

  // Свойства для получения угловых точек near и far плоскостей
  public Vector3 NearTopLeft => Plane.GetIntersectionPoint(NearPlane, LeftPlane, TopPlane);
  public Vector3 NearTopRight => Plane.GetIntersectionPoint(NearPlane, RightPlane, TopPlane);
  public Vector3 NearBottomLeft => Plane.GetIntersectionPoint(NearPlane, LeftPlane, BottomPlane);
  public Vector3 NearBottomRight => Plane.GetIntersectionPoint(NearPlane, RightPlane, BottomPlane);

  public Vector3 FarTopLeft => Plane.GetIntersectionPoint(FarPlane, LeftPlane, TopPlane);
  public Vector3 FarTopRight => Plane.GetIntersectionPoint(FarPlane, RightPlane, TopPlane);
  public Vector3 FarBottomLeft => Plane.GetIntersectionPoint(FarPlane, LeftPlane, BottomPlane);
  public Vector3 FarBottomRight => Plane.GetIntersectionPoint(FarPlane, RightPlane, BottomPlane);

  public Frustum(Plane topPlane, Plane bottomPlane, Plane leftPlane, Plane rightPlane, Plane nearPlane, Plane farPlane)
  {
    TopPlane = topPlane;
    BottomPlane = bottomPlane;
    LeftPlane = leftPlane;
    RightPlane = rightPlane;
    NearPlane = nearPlane;
    FarPlane = farPlane;
  }

  // Метод для построения Frustum из 4 лучей (по углам выделения)
  public static Frustum FromRays(
    Ray topLeftRay,
    Ray topRightRay,
    Ray bottomLeftRay,
    Ray bottomRightRay,
    float nearDistance,
    float farDistance)
  {
    // Вычисляем точки на near и far плоскостях для каждого луча
    var nearTopLeft = topLeftRay.GetPointAtDistance(nearDistance);
    var nearTopRight = topRightRay.GetPointAtDistance(nearDistance);
    var nearBottomLeft = bottomLeftRay.GetPointAtDistance(nearDistance);
    var nearBottomRight = bottomRightRay.GetPointAtDistance(nearDistance);

    var farTopLeft = topLeftRay.GetPointAtDistance(farDistance);
    var farTopRight = topRightRay.GetPointAtDistance(farDistance);
    var farBottomLeft = bottomLeftRay.GetPointAtDistance(farDistance);
    var farBottomRight = bottomRightRay.GetPointAtDistance(farDistance);

    // Построение плоскостей Frustum
    var topPlane = new Plane(nearTopLeft, farTopLeft, farTopRight); // Верхняя плоскость
    var bottomPlane = new Plane(nearBottomRight, farBottomRight, farBottomLeft); // Нижняя плоскость
    var leftPlane = new Plane(nearBottomLeft, farBottomLeft, farTopLeft); // Левая плоскость
    var rightPlane = new Plane(nearTopRight, farTopRight, farBottomRight); // Правая плоскость
    var nearPlane = new Plane(nearTopLeft, nearTopRight, nearBottomRight); // Near-плоскость
    var farPlane = new Plane(farTopRight, farTopLeft, farBottomLeft); // Far-плоскость

    return new Frustum(topPlane, bottomPlane, leftPlane, rightPlane, nearPlane, farPlane);
  }

  // Метод для построения Frustum из двух лучей (по диагональным углам выделения)
  public static Frustum FromRays(Ray topLeftRay, Ray bottomRightRay, float nearDistance, float farDistance)
  {
    // Вычисляем точки на near и far плоскостях для каждого луча
    var nearTopLeft = topLeftRay.GetPointAtDistance(nearDistance);
    var nearBottomRight = bottomRightRay.GetPointAtDistance(nearDistance);
    var farTopLeft = topLeftRay.GetPointAtDistance(farDistance);
    var farBottomRight = bottomRightRay.GetPointAtDistance(farDistance);

    // Теперь вычислим недостающие углы на near и far плоскостях
    var nearTopRight = new Vector3(nearBottomRight.X, nearTopLeft.Y, nearTopLeft.Z); // Точка на near-плоскости
    var nearBottomLeft = new Vector3(nearTopLeft.X, nearBottomRight.Y, nearBottomRight.Z); // Точка на near-плоскости

    var farTopRight = new Vector3(farBottomRight.X, farTopLeft.Y, farTopLeft.Z); // Точка на far-плоскости
    var farBottomLeft = new Vector3(farTopLeft.X, farBottomRight.Y, farBottomRight.Z); // Точка на far-плоскости

    // Построение плоскостей Frustum
    var topPlane = new Plane(nearTopLeft, farTopLeft, farTopRight); // Верхняя плоскость
    var bottomPlane = new Plane(nearBottomRight, farBottomRight, farBottomLeft); // Нижняя плоскость
    var leftPlane = new Plane(nearBottomLeft, farBottomLeft, farTopLeft); // Левая плоскость
    var rightPlane = new Plane(nearTopRight, farTopRight, farBottomRight); // Правая плоскость
    var nearPlane = new Plane(nearTopLeft, nearTopRight, nearBottomRight); // Near-плоскость
    var farPlane = new Plane(farTopRight, farTopLeft, farBottomLeft); // Far-плоскость

    return new Frustum(topPlane, bottomPlane, leftPlane, rightPlane, nearPlane, farPlane);
  }

  public static Frustum FromCamera(Camera_Perspective camera)
  {
    // Положение камеры
    var cameraPosition = camera.Position;

    // Направление камеры 
    var forward = camera.Forward.Normalized;
    var up = camera.Up.Normalized;
    var right = camera.Right.Normalized;

    // Параметры камеры
    var nearClip = camera.Far; // Меняем местами near и far для Reversed-Z
    var farClip = camera.Near; // Меняем местами near и far для Reversed-Z
    var fov = camera.FOV;
    var aspectRatio = camera.AspectRatio;

    // Половина угла обзора по вертикали и горизонтали
    var tanFovY = (float)Math.Tan(fov * 0.5f);
    var tanFovX = tanFovY * aspectRatio;

    // Определяем размеры near и far плоскостей
    var nearHeight = 2.0f * nearClip * tanFovY;
    var nearWidth = nearHeight * aspectRatio;

    var farHeight = 2.0f * farClip * tanFovY;
    var farWidth = farHeight * aspectRatio;

    // Определяем центры near и far плоскостей
    var nearCenter = cameraPosition + forward * nearClip;
    var farCenter = cameraPosition + forward * farClip;

    // Вычисляем угловые точки near-плоскости
    var nearTopLeft = nearCenter + up * (nearHeight / 2.0f) - right * (nearWidth / 2.0f);
    var nearTopRight = nearCenter + up * (nearHeight / 2.0f) + right * (nearWidth / 2.0f);
    var nearBottomLeft = nearCenter - up * (nearHeight / 2.0f) - right * (nearWidth / 2.0f);
    var nearBottomRight = nearCenter - up * (nearHeight / 2.0f) + right * (nearWidth / 2.0f);

    // Вычисляем угловые точки far-плоскости
    var farTopLeft = farCenter + up * (farHeight / 2.0f) - right * (farWidth / 2.0f);
    var farTopRight = farCenter + up * (farHeight / 2.0f) + right * (farWidth / 2.0f);
    var farBottomLeft = farCenter - up * (farHeight / 2.0f) - right * (farWidth / 2.0f);
    var farBottomRight = farCenter - up * (farHeight / 2.0f) + right * (farWidth / 2.0f);

    // Построение плоскостей Frustum
    var topPlane = new Plane(nearTopLeft, farTopLeft, farTopRight); // Верхняя плоскость
    var bottomPlane = new Plane(nearBottomRight, farBottomRight, farBottomLeft); // Нижняя плоскость
    var leftPlane = new Plane(nearBottomLeft, farBottomLeft, farTopLeft); // Левая плоскость
    var rightPlane = new Plane(nearTopRight, farTopRight, farBottomRight); // Правая плоскость
    var nearPlane = new Plane(nearTopLeft, nearTopRight, nearBottomRight); // Near-плоскость
    var farPlane = new Plane(farTopRight, farTopLeft, farBottomLeft); // Far-плоскость

    return new Frustum(topPlane, bottomPlane, leftPlane, rightPlane, nearPlane, farPlane);
  }
}