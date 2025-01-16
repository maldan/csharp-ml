using System;
using System.Collections.Generic;
using MegaLib.Geometry;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics.Collider;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;

namespace MegaLib.Editor;

public enum ManipulatorSpace
{
  Local,
  World
}

public class RotationManipulator
{
  public Transform Transform = new();

  // Радиусы для коллайдеров вращения
  public float Radius = 0.5f;
  public float Thickness = 0.05f; // Толщина кольца для манипуляции

  private bool _isXHover;
  private bool _isYHover;
  private bool _isZHover;

  private bool _isXGrab;
  private bool _isYGrab;
  private bool _isZGrab;

  private Ray _currentRay;
  public Quaternion StartRotation = Quaternion.Identity;

  private Vector3 _startPointX; // Точка начального угла по X
  private Vector3 _startPointY; // Точка начального угла по Y
  private Vector3 _startPointZ; // Точка начального угла по Z

  private Camera_Base _camera; // Камера

  public Action<Quaternion> OnChange;
  public Action OnStart;

  public bool IsUsing { get; private set; }
  public bool IsHover { get; private set; }

  public ManipulatorSpace Space = ManipulatorSpace.Local;

  public RotationManipulator(Camera_Base camera)
  {
    _camera = camera;
  }


  // Метод для проверки коллизии с круговыми коллайдерами
  public void CheckCollision(Ray ray)
  {
    //_isXHover = false;
    //_isYHover = false;
    //_isZHover = false;

    // Проверка для оси X
    if (!_isXGrab && Mouse.IsKeyUp(MouseKey.Left)) _isXHover = CheckRingCollision(ray, Vector3.UnitX);

    // Проверка для оси Y
    if (!_isYGrab && Mouse.IsKeyUp(MouseKey.Left)) _isYHover = CheckRingCollision(ray, Vector3.UnitY);

    // Проверка для оси Z
    if (!_isZGrab && Mouse.IsKeyUp(MouseKey.Left)) _isZHover = CheckRingCollision(ray, Vector3.UnitZ);

    _currentRay = ray;
  }

  /*// Метод для проверки коллизии с кольцом на плоскости
  private bool CheckRingCollision(Ray ray, Vector3 axis)
  {
    // Плоскость вращения перпендикулярна оси
    var rotationPlane = new Plane(axis, Transform.Position);

    // Находим пересечение луча с плоскостью вращения
    if (rotationPlane.RayIntersects(ray, out var intersectionPoint, out var isHit) && isHit)
    {
      // Вычисляем расстояние от точки пересечения до центра
      var distanceToCenter = (intersectionPoint - Transform.Position).Length;

      // Проверяем, находится ли точка на кольце
      return Math.Abs(distanceToCenter - Radius) < Thickness; // Точка находится на кольце
    }

    return false;
  }*/

  // Метод для проверки коллизии с кольцом на плоскости
  private bool CheckRingCollision(Ray ray, Vector3 axis)
  {
    // Применяем поворот к оси кольца
    axis = Transform.Rotation * axis;

    // Плоскость вращения перпендикулярна оси
    var rotationPlane = new Plane(axis, Transform.Position);

    // Находим пересечение луча с плоскостью вращения
    if (rotationPlane.RayIntersection(ray, out var intersectionPoint))
    {
      // Вычисляем расстояние от точки пересечения до центра
      var distanceToCenter = (intersectionPoint - Transform.Position).Length;

      // Проверяем, находится ли точка на кольце
      return Math.Abs(distanceToCenter - Radius) < Thickness; // Точка находится на кольце
    }

    return false;
  }

  // Основная логика для обработки вращения
  public void Update()
  {
    if (Mouse.IsKeyUp(MouseKey.Left))
    {
      _isXGrab = false; // Отпускаем вращение по оси X
      _isYGrab = false; // Отпускаем вращение по оси Y
      _isZGrab = false; // Отпускаем вращение по оси Z
    }

    if (Mouse.IsKeyDown(MouseKey.Left) && !(_isXGrab || _isYGrab || _isZGrab))
    {
      if (_isXHover)
      {
        _isXGrab = true;
        _startPointX = GetRingIntersectionPoint(Vector3.UnitX);
      }
      else if (_isYHover)
      {
        _isYGrab = true;
        _startPointY = GetRingIntersectionPoint(Vector3.UnitY);
      }
      else if (_isZHover)
      {
        _isZGrab = true;
        _startPointZ = GetRingIntersectionPoint(Vector3.UnitZ);
      }

      OnStart?.Invoke();
    }

    // Логика вращения по осям X, Y и Z
    if (_isXGrab)
    {
      RotateAroundAxis(Vector3.UnitX, _startPointX); // Логика вращения вокруг оси X
    }
    else if (_isYGrab)
    {
      RotateAroundAxis(Vector3.UnitY, _startPointY); // Логика вращения вокруг оси Y
    }
    else if (_isZGrab)
    {
      RotateAroundAxis(Vector3.UnitZ, _startPointZ); // Логика вращения вокруг оси Z
    }

    IsUsing = _isXGrab || _isYGrab || _isZGrab;
    IsHover = _isXHover || _isYHover || _isZHover;
  }

  /*private void RotateAroundAxis(Vector3 axis, Vector3 startPoint)
  {
    var currentPoint = GetRingIntersectionPoint(axis);

    // startPoint -= Transform.Position;
    // currentPoint -= Transform.Position;

    Console.WriteLine($"{startPoint} - {currentPoint}");

    // Вычисляем угол между начальной точкой и текущей точкой
    var angle = Vector3.AngleBetween(startPoint, currentPoint);

    // Векторное произведение для определения направления вращения
    var cross = Vector3.Cross(startPoint, currentPoint);

    // Если векторное произведение направлено в ту же сторону, что и ось, то угол положительный,
    // иначе — отрицательный
    var direction = Vector3.Dot(cross, axis) < 0 ? -1 : 1;

    // Корректируем угол с учетом направления
    angle *= direction;

    // Создаём кватернион для поворота вокруг оси на вычисленный угол в мировых координатах
    var rotation = Quaternion.FromAxisAngle(axis, angle);

    if (Space == ManipulatorSpace.Local)
    {
      // Преобразуем ось в локальные координаты объекта
      var localAxis = Vector3.Transform(axis, StartRotation);
      rotation = Quaternion.FromAxisAngle(localAxis, angle);

      OnChange?.Invoke(rotation * StartRotation);
    }

    if (Space == ManipulatorSpace.World)
    {
      OnChange?.Invoke(rotation * StartRotation);
    }
  }*/

  private void RotateAroundAxis(Vector3 axis, Vector3 startPoint)
  {
    // Применяем текущий поворот к оси для приведения её в мировые координаты
    var worldAxis = Transform.Rotation * axis;

    // Получаем текущую точку пересечения кольца и оси
    var currentPoint = GetRingIntersectionPoint(worldAxis);

    // Вычисляем угол между начальной точкой и текущей точкой
    var angle = Vector3.AngleBetween(startPoint, currentPoint);

    // Векторное произведение для определения направления вращения
    var cross = Vector3.Cross(startPoint, currentPoint);

    // Если векторное произведение направлено в ту же сторону, что и ось, то угол положительный,
    // иначе — отрицательный
    var direction = Vector3.Dot(cross, worldAxis) < 0 ? -1 : 1;

    // Корректируем угол с учётом направления
    angle *= direction;

    // Создаём кватернион для поворота вокруг оси на вычисленный угол в мировых координатах
    var worldRotation = Quaternion.FromAxisAngle(worldAxis, angle);

    if (Space == ManipulatorSpace.Local)
    {
      // Преобразуем ось вращения в локальные координаты объекта
      var localAxis = Transform.Rotation.Inverted * axis; // Применяем обратную ротацию объекта

      // Создаём кватернион для локального поворота
      var localRotation = Quaternion.FromAxisAngle(localAxis, angle);

      // Применяем локальный поворот
      OnChange?.Invoke(StartRotation * localRotation);

      // Отладка углов
      Console.WriteLine($"{(StartRotation * localRotation).Euler.ToDegrees}");
    }

    if (Space == ManipulatorSpace.World)
    {
      // В мировом пространстве применяем мировой поворот
      OnChange?.Invoke(worldRotation * StartRotation);
    }
  }

  /*// Метод для получения точки пересечения луча с кольцом
  private Vector3 GetRingIntersectionPoint(Vector3 axis)
  {
    var rotationPlane = new Plane(axis, Transform.Position);

    // Находим пересечение текущего луча с плоскостью вращения
    if (rotationPlane.RayIntersects(_currentRay, out var intersectionPoint, out var isHit) && isHit)
    {
      // Нормализуем вектор до длины радиуса, чтобы линия всегда попадала на кольцо
      var direction = (intersectionPoint - Transform.Position).Normalized * Radius;
      return direction;
    }

    return Vector3.Zero; // Если пересечения нет, возвращаем центр
  }*/

  // Метод для получения точки пересечения луча с кольцом
  private Vector3 GetRingIntersectionPoint(Vector3 axis)
  {
    // Применяем поворот к оси кольца с учётом текущей ротации объекта
    axis = Transform.Rotation * axis;

    // Создаем плоскость на основе трансформированной оси и позиции объекта
    var rotationPlane = new Plane(axis, Transform.Position);

    // Находим пересечение текущего луча с плоскостью вращения
    if (rotationPlane.RayIntersection(_currentRay, out var intersectionPoint))
    {
      // Вычисляем вектор от центра кольца до точки пересечения
      var direction = (intersectionPoint - Transform.Position).Normalized;

      // Применяем обратную ротацию к направлению, чтобы вернуть его в локальное пространство кольца
      direction = Transform.Rotation.Inverted * direction;

      // Нормализуем вектор до длины радиуса
      direction *= Radius;

      // Применяем обратно ротацию, чтобы получить итоговую позицию в мировом пространстве
      return Transform.Rotation * direction;
    }

    return Vector3.Zero; // Если пересечения нет, возвращаем центр
  }

  // Метод для рисования манипулятора
  public void Draw(Layer_Line line)
  {
    var xColor = new RGBA32F(1, 0, 0, 1) * 0.8f;
    var yColor = new RGBA32F(0, 1, 0, 1) * 0.8f;
    var zColor = new RGBA32F(0, 0, 1, 1) * 0.8f;

    if (_isXHover || _isXGrab) xColor *= 1.25f;
    else if (_isYHover || _isYGrab) yColor *= 1.25f;
    else if (_isZHover || _isZGrab) zColor *= 1.25f;

    if (_isXGrab)
    {
      line.DrawRing(Transform.Position, Vector3.UnitX, Transform.Rotation, Radius, xColor, 32, 2f);
    }
    else if (_isYGrab)
    {
      line.DrawRing(Transform.Position, Vector3.UnitY, Transform.Rotation, Radius, yColor, 32, 2f);
    }
    else if (_isZGrab)
    {
      line.DrawRing(Transform.Position, Vector3.UnitZ, Transform.Rotation, Radius, zColor, 32, 2f);
    }
    else
    {
      line.DrawRing(Transform.Position, Vector3.UnitX, Transform.Rotation, Radius, xColor, 32, 2f);
      line.DrawRing(Transform.Position, Vector3.UnitY, Transform.Rotation, Radius, yColor, 32, 2f);
      line.DrawRing(Transform.Position, Vector3.UnitZ, Transform.Rotation, Radius, zColor, 32, 2f);
    }

    // Линии, показывающие начальный и текущий углы для оси X
    if (_isXGrab)
    {
      var currentPointX = GetRingIntersectionPoint(Vector3.UnitX);
      line.DrawLine(Transform.Position, Transform.Position + _startPointX.Normalized * Radius,
        new RGBA32F(1, 0, 0, 1)); // Линия начального угла
      line.DrawLine(Transform.Position, Transform.Position + currentPointX,
        new RGBA32F(1, 0.5f, 0.5f, 1)); // Линия текущего угла
    }

    // Линии, показывающие начальный и текущий углы для оси Y
    if (_isYGrab)
    {
      var currentPointY = GetRingIntersectionPoint(Vector3.UnitY);
      line.DrawLine(Transform.Position, Transform.Position + _startPointY.Normalized * Radius,
        new RGBA32F(0, 1, 0, 1)); // Линия начального угла
      line.DrawLine(Transform.Position, Transform.Position + currentPointY,
        new RGBA32F(0.5f, 1, 0.5f, 1)); // Линия текущего угла
    }

    // Линии, показывающие начальный и текущий углы для оси Z
    if (_isZGrab)
    {
      var currentPointZ = GetRingIntersectionPoint(Vector3.UnitZ);
      line.DrawLine(Transform.Position, Transform.Position + _startPointZ.Normalized * Radius,
        new RGBA32F(0, 0, 1, 1)); // Линия начального угла
      line.DrawLine(Transform.Position, Transform.Position + currentPointZ,
        new RGBA32F(0.5f, 0.5f, 1, 1)); // Линия текущего угла
    }
  }
}