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

public class ScaleManipulator
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
  private Vector3 _xCollisionPoint; // Точка пересечения при захвате для оси X
  private Vector3 _yCollisionPoint; // Точка пересечения при захвате для оси Y
  private Vector3 _zCollisionPoint; // Точка пересечения при захвате для оси Z

  private bool _isXGrab;
  private bool _isYGrab; // Для отслеживания захвата по оси Y
  private bool _isZGrab; // Для отслеживания захвата по оси Z

  private Camera_Base _camera; // Камера

  private List<Transform> _objects = new(); // Список объектов, к которым применяется манипуляция

  private Vector3 _startScale; // Сохранение начального масштаба

  public ScaleManipulator(Camera_Base camera)
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
    if (!_isYGrab && Mouse.IsKeyUp(MouseKey.Left)) YCollider.RayIntersection(ray, out _yCollisionPoint, out _isYHover);
    if (!_isZGrab && Mouse.IsKeyUp(MouseKey.Left)) ZCollider.RayIntersection(ray, out _zCollisionPoint, out _isZHover);
    _currentRay = ray;
  }

  // Основная логика для обработки захвата и перемещения
  public void Update()
  {
    if (Mouse.IsKeyUp(MouseKey.Left))
    {
      _isXGrab = false;
      _isYGrab = false;
      _isZGrab = false;
    }

    if (Mouse.IsKeyDown(MouseKey.Left) && !(_isXGrab || _isYGrab || _isZGrab))
    {
      if (_isXHover)
      {
        _isXGrab = true;
        _startPosition = Transform.Position;
        _startScale = _objects[0].Scale; // Сохраняем начальный масштаб
      }
      else if (_isYHover)
      {
        _isYGrab = true;
        _startPosition = Transform.Position;
        _startScale = _objects[0].Scale; // Сохраняем начальный масштаб
      }
      else if (_isZHover)
      {
        _isZGrab = true;
        _startPosition = Transform.Position;
        _startScale = _objects[0].Scale; // Сохраняем начальный масштаб
      }
    }

    if (_isXGrab)
    {
      ApplyScale(Vector3.UnitX, _xCollisionPoint);
    }
    else if (_isYGrab)
    {
      ApplyScale(Vector3.UnitY, _yCollisionPoint);
    }
    else if (_isZGrab)
    {
      ApplyScale(Vector3.UnitZ, _zCollisionPoint);
    }
  }

  // Метод для применения масштаба по оси
  // Метод для применения масштаба по оси
  private void ApplyScale(Vector3 axis, Vector3 collisionPoint)
  {
    Plane plane;

    // Используем разные плоскости для каждой оси
    if (axis == Vector3.UnitX)
    {
      var cameraDirection = (_camera.Position - _startPosition).Normalized;
      var dotUp = Math.Abs(Vector3.Dot(cameraDirection, Vector3.UnitY));
      plane = dotUp > 0.5f ? new Plane(Vector3.UnitY, collisionPoint) : new Plane(Vector3.UnitZ, collisionPoint);
    }
    else if (axis == Vector3.UnitY)
    {
      var cameraDirection = (_camera.Position - _startPosition).Normalized;
      var dotUp = Math.Abs(Vector3.Dot(cameraDirection, Vector3.UnitZ));
      plane = dotUp > 0.5f ? new Plane(Vector3.UnitZ, collisionPoint) : new Plane(Vector3.UnitX, collisionPoint);
    }
    else if (axis == Vector3.UnitZ)
    {
      var cameraDirection = (_camera.Position - _startPosition).Normalized;
      var dotUp = Math.Abs(Vector3.Dot(cameraDirection, Vector3.UnitX));
      plane = dotUp > 0.5f ? new Plane(Vector3.UnitX, collisionPoint) : new Plane(Vector3.UnitY, collisionPoint);
    }
    else
    {
      throw new InvalidOperationException("Invalid axis for scaling.");
    }

    // Находим пересечение текущего луча с выбранной плоскостью
    if (plane.RayIntersection(_currentRay, out var intersectionPoint))
    {
      // Вычисляем, насколько изменился масштаб относительно начальной точки
      var scaleDelta = (intersectionPoint - _startPosition).Length / (collisionPoint - _startPosition).Length;

      foreach (var obj in _objects)
      {
        // Применяем изменение масштаба по выбранной оси
        if (axis == Vector3.UnitX)
          obj.Scale = new Vector3(_startScale.X * scaleDelta, obj.Scale.Y, obj.Scale.Z);
        else if (axis == Vector3.UnitY)
          obj.Scale = new Vector3(obj.Scale.X, _startScale.Y * scaleDelta, obj.Scale.Z);
        else if (axis == Vector3.UnitZ)
          obj.Scale = new Vector3(obj.Scale.X, obj.Scale.Y, _startScale.Z * scaleDelta);
      }
    }
  }

  public void Draw(Layer_Line line)
  {
    line.DrawBox(XCollider, _isXHover ? new RGBA32F(1, 0, 0, 1) : new RGBA32F(1, 1, 1, 1));
    line.DrawBox(YCollider, _isYHover ? new RGBA32F(0, 1, 0, 1) : new RGBA32F(1, 1, 1, 1));
    line.DrawBox(ZCollider, _isZHover ? new RGBA32F(0, 0, 1, 1) : new RGBA32F(1, 1, 1, 1));
    line.DrawTranslateManipulator(Transform.Position);
  }
}