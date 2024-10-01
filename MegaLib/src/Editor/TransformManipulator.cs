using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics.Collider;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;

namespace MegaLib.Editor;

public class TransformManipulator
{
  public Transform Transform = new();

  public BoxCollider XCollider = new() { Size = new Vector3(1, 0.1f, 0.1f) };
  public BoxCollider YCollider = new() { Size = new Vector3(0.1f, 1f, 0.1f) };
  public BoxCollider ZCollider = new() { Size = new Vector3(0.1f, 0.1f, 1f) };

  private bool _isXHover;
  private bool _isYHover;
  private bool _isZHover;

  private Ray _currentRay;
  private Vector3 _startPosition;
  private Vector3 _grabOffset;
  private Vector3 _xCollisionPoint; // Точка пересечения при захвате для оси X
  private Vector3 _yCollisionPoint; // Точка пересечения при захвате для оси Y
  private Vector3 _zCollisionPoint; // Точка пересечения при захвате для оси Z

  private bool _isXGrab;
  private bool _isYGrab; // Для отслеживания захвата по оси Y
  private bool _isZGrab; // Для отслеживания захвата по оси Z

  private Camera_Base _camera; // Камера

  public TransformManipulator(Camera_Base camera)
  {
    XCollider.Transform = Transform;
    YCollider.Transform = Transform;
    ZCollider.Transform = Transform;
    _camera = camera; // Используем камеру для расчёта
  }

  // Метод для проверки коллизии
  public void CheckCollision(Ray ray)
  {
    if (!_isXGrab) XCollider.RayIntersection(ray, out _xCollisionPoint, out _isXHover);
    if (!_isYGrab) YCollider.RayIntersection(ray, out _yCollisionPoint, out _isYHover); // Проверка коллизии по оси Y
    if (!_isZGrab) ZCollider.RayIntersection(ray, out _zCollisionPoint, out _isZHover); // Проверка коллизии по оси Z
    _currentRay = ray;
  }

  // Основная логика для обработки захвата и перемещения
  public void Update()
  {
    if (Mouse.IsKeyUp(MouseKey.Left))
    {
      _isXGrab = false; // Отпускаем манипулятор по оси X
      _isYGrab = false; // Отпускаем манипулятор по оси Y
      _isZGrab = false; // Отпускаем манипулятор по оси Z
    }

    // Логика для оси X
    if (Mouse.IsKeyDown(MouseKey.Left) && !(_isXGrab || _isYGrab || _isZGrab))
    {
      if (_isXHover)
      {
        _isXGrab = true;
        _startPosition = Transform.Position;
        _grabOffset = Transform.Position - _xCollisionPoint;
      }
      else if (_isYHover)
      {
        _isYGrab = true;
        _startPosition = Transform.Position;
        _grabOffset = Transform.Position - _yCollisionPoint;
      }
      else if (_isZHover)
      {
        _isZGrab = true;
        _startPosition = Transform.Position;
        _grabOffset = Transform.Position - _zCollisionPoint;
      }
    }

    // Логика для оси X
    if (_isXGrab)
    {
      var cameraDirection = (_camera.Position - _startPosition).Normalized;
      var dotUp = Math.Abs(Vector3.Dot(cameraDirection, Vector3.UnitY));

      Plane plane;
      if (dotUp > 0.5f)
      {
        plane = new Plane(Vector3.UnitY, _xCollisionPoint);
      }
      else
      {
        plane = new Plane(Vector3.UnitZ, _xCollisionPoint);
      }

      if (plane.RayIntersects(_currentRay, out var intersectionPoint, out var isHit) && isHit)
      {
        Transform.Position = new Vector3(intersectionPoint.X, _startPosition.Y, _startPosition.Z) +
                             new Vector3(_grabOffset.X, 0, 0);
      }
    }
    else if (_isYGrab)
    {
      var cameraDirection = (_camera.Position - _startPosition).Normalized;
      var dotUp = Math.Abs(Vector3.Dot(cameraDirection, Vector3.UnitZ));

      Plane plane;
      if (dotUp > 0.5f)
      {
        plane = new Plane(Vector3.UnitZ, _yCollisionPoint);
      }
      else
      {
        plane = new Plane(Vector3.UnitX, _yCollisionPoint);
      }

      if (plane.RayIntersects(_currentRay, out var intersectionPoint, out var isHit) && isHit)
      {
        Transform.Position = new Vector3(_startPosition.X, intersectionPoint.Y, _startPosition.Z) +
                             new Vector3(0, _grabOffset.Y, 0);
      }
    }
    else if (_isZGrab) // Логика для оси Z
    {
      // Определяем направление камеры относительно манипулятора
      var cameraDirection = (_camera.Position - _startPosition).Normalized;

      // Проверяем, насколько камера смотрит спереди или сбоку
      var dotUp = Math.Abs(Vector3.Dot(cameraDirection, Vector3.UnitX));

      Plane plane;
      if (dotUp > 0.5f)
      {
        // Если камера смотрит сбоку, используем плоскость YZ (нормаль по оси X)
        plane = new Plane(Vector3.UnitX, _zCollisionPoint);
      }
      else
      {
        // Если камера спереди или сзади, используем плоскость XY (нормаль по оси Z)
        plane = new Plane(Vector3.UnitY, _zCollisionPoint);
      }

      // Вычисляем пересечение текущего луча с выбранной плоскостью
      if (plane.RayIntersects(_currentRay, out var intersectionPoint, out var isHit) && isHit)
      {
        // Обновляем позицию манипулятора по оси Z, используя точку пересечения
        Transform.Position = new Vector3(_startPosition.X, _startPosition.Y, intersectionPoint.Z) +
                             new Vector3(0, 0, _grabOffset.Z);
      }
    }
  }

  public void Draw(Layer_Line line)
  {
    line.DrawBoxCollider(XCollider, _isXHover ? new RGBA<float>(1, 0, 0, 1) : new RGBA<float>(1, 1, 1, 1));
    line.DrawBoxCollider(YCollider, _isYHover ? new RGBA<float>(0, 1, 0, 1) : new RGBA<float>(1, 1, 1, 1));
    line.DrawBoxCollider(ZCollider, _isZHover ? new RGBA<float>(0, 0, 1, 1) : new RGBA<float>(1, 1, 1, 1));
    line.DrawTranslateManipulator(Transform.Position);
  }
}