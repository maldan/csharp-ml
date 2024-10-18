using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics.Collider;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;

namespace MegaLib.Editor;

public class TranslateManipulator
{
  public Transform Transform = new();

  public BoxCollider XCollider = new() { Size = new Vector3(1, 0.05f, 0.05f) };
  public BoxCollider YCollider = new() { Size = new Vector3(0.05f, 1f, 0.05f) };
  public BoxCollider ZCollider = new() { Size = new Vector3(0.05f, 0.05f, 1f) };

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

  private List<Transform> _objects = new(); // Список объектов, к которым применяется манипуляция

  public TranslateManipulator(Camera_Base camera)
  {
    XCollider.Transform = Transform;
    YCollider.Transform = Transform;
    ZCollider.Transform = Transform;
    _camera = camera; // Используем камеру для расчёта
  }

  // Метод для установки объектов
  public void SetElements(List<Transform> objects)
  {
    _objects = objects;
  }

  public void Clear()
  {
    _objects.Clear();
  }

  // Метод для проверки коллизии
  public void CheckCollision(Ray ray)
  {
    if (!_isXGrab && Mouse.IsKeyUp(MouseKey.Left)) XCollider.RayIntersection(ray, out _xCollisionPoint, out _isXHover);
    if (!_isYGrab && Mouse.IsKeyUp(MouseKey.Left))
      YCollider.RayIntersection(ray, out _yCollisionPoint, out _isYHover); // Проверка коллизии по оси Y
    if (!_isZGrab && Mouse.IsKeyUp(MouseKey.Left))
      ZCollider.RayIntersection(ray, out _zCollisionPoint, out _isZHover); // Проверка коллизии по оси Z
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

      var plane = dotUp > 0.5f
        ? new Plane(Vector3.UnitY, _xCollisionPoint)
        : new Plane(Vector3.UnitZ, _xCollisionPoint);

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

      var plane = dotUp > 0.5f
        ? new Plane(Vector3.UnitZ, _yCollisionPoint)
        : new Plane(Vector3.UnitX, _yCollisionPoint);

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

      var plane =
        dotUp > 0.5f
          ? new Plane(Vector3.UnitX, _zCollisionPoint)
          : new Plane(Vector3.UnitY, _zCollisionPoint);

      // Вычисляем пересечение текущего луча с выбранной плоскостью
      if (plane.RayIntersects(_currentRay, out var intersectionPoint, out var isHit) && isHit)
      {
        // Обновляем позицию манипулятора по оси Z, используя точку пересечения
        Transform.Position = new Vector3(_startPosition.X, _startPosition.Y, intersectionPoint.Z) +
                             new Vector3(0, 0, _grabOffset.Z);
      }
    }

    foreach (var obj in _objects)
    {
      // Вращение относительно мировой оси, не переводим её в локальные координаты
      obj.Position = Transform.Position; // Применяем поворот в мировой системе
    }
  }

  public void Draw(Layer_Line line)
  {
    line.DrawBox(XCollider, _isXHover ? new RGBA<float>(1, 0, 0, 1) : new RGBA<float>(1, 1, 1, 1));
    line.DrawBox(YCollider, _isYHover ? new RGBA<float>(0, 1, 0, 1) : new RGBA<float>(1, 1, 1, 1));
    line.DrawBox(ZCollider, _isZHover ? new RGBA<float>(0, 0, 1, 1) : new RGBA<float>(1, 1, 1, 1));
    line.DrawTranslateManipulator(Transform.Position);
  }
}