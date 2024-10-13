using System;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Camera;

public class Camera_Base
{
  protected Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
  protected Matrix4x4 _viewMatrix = Matrix4x4.Identity;

  public Matrix4x4 ProjectionMatrix => _projectionMatrix;

  public Matrix4x4 ViewMatrix
  {
    get => _viewMatrix;
    set => _viewMatrix = value;
  }

  private Vector3 _position;
  private Quaternion _rotation = Quaternion.Identity;

  public Vector3 Forward =>
    Vector3.Transform(Vector3.Forward, Rotation).Normalized;

  public Vector3 Up =>
    Vector3.Transform(Vector3.Up, Rotation).Normalized;

  public Vector3 Right =>
    Vector3.Transform(Vector3.Right, Rotation).Normalized;

  public Vector3 Position
  {
    get => _position;
    set
    {
      _position = value;
      CalculateView();
    }
  }

  public Quaternion Rotation
  {
    get => _rotation;
    set
    {
      _rotation = value;
      CalculateView();
    }
  }

  private float _near = 0;
  private float _far = 32.0f;

  public float Near
  {
    get => _near;
    set
    {
      _near = value;
      CalculateProjection();
    }
  }

  public float Far
  {
    get => _far;
    set
    {
      _far = value;
      CalculateProjection();
    }
  }

  public virtual void CalculateProjection()
  {
  }

  public void CalculateView()
  {
    // Матрица вида начинается с единичной матрицы
    _viewMatrix = Matrix4x4.Identity;

    // Инвертируем поворот по оси Y (yaw)
    // var invertedRotation = Quaternion.FromEuler(new Vector3(-_rotation.X, -_rotation.Y, _rotation.Z), "rad");

    // Применяем поворот камеры
    _viewMatrix = _viewMatrix.Rotate(_rotation.Inverted); // Убеждаемся, что это матрица поворота

    // Применяем трансляцию камеры, но с отрицательными значениями для обратного смещения
    var p = -_position; // Обратное смещение, так как это камера
    _viewMatrix = _viewMatrix.Translate(p); // Камера перемещает мир в противоположную сторону
  }

  public void OffsetPosition(float x = 0f, float y = 0f, float z = 0f)
  {
    OffsetPosition(new Vector3(x, y, z));
  }

  public void OffsetPosition(Vector3 dir)
  {
    var hh = Rotation;
    var head = Matrix4x4.Identity.Rotate(hh);

    var dirNew = dir.AddW(1.0f) * head;
    // this.position.add_(dirNew.toVector3());
    Position += dirNew.DropW();

    // this._positionOffset = this._positionOffset.add(dirNew.toVector3());
  }

  public void OrbitalCameraMovement(float delta, ref float radius, ref Vector3 focusPoint)
  {
    // Получаем изменения мыши
    var deltaX = Mouse.ClientDelta.X * 48f; // Изменение мыши по X
    var deltaY = Mouse.ClientDelta.Y * 48f; // Изменение мыши по Y

    if (Keyboard.IsKeyDown(KeyboardKey.Shift) && Mouse.IsKeyDown(MouseKey.Center))
    {
      // Реализуем перемещение фокусной точки, если зажат Shift
      var shiftSpeed = 0.005f * radius; // Скорость перемещения зависит от расстояния (чтобы перемещение было плавным)

      // Вычисляем вектор вправо и вверх в локальной системе координат камеры
      var right = Vector3.Normalize(Vector3.Transform(Vector3.UnitX, Rotation)); // Локальный вектор вправо
      var up = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Rotation)); // Локальный вектор вверх

      // Обновляем фокусную точку на основе движения мыши
      focusPoint += right * (-deltaX * shiftSpeed * delta) + up * (deltaY * shiftSpeed * delta);

      // Рассчитываем новую позицию камеры
      Position = focusPoint +
                 Vector3.Transform(new Vector3(0, 0, -radius), Rotation); // Позиция камеры с учётом вращения
      return; // Прерываем дальнейшее обновление, так как уже переместили сцену
    }

    if (!Mouse.IsKeyDown(MouseKey.Center))
    {
      deltaX = 0;
      deltaY = 0;
    }

    switch (Mouse.WheelDirection)
    {
      case > 0:
        radius *= 1.0f - 0.05f * delta * 128f; // Уменьшаем радиус с учётом delta
        break;
      case < 0:
        radius *= 1.0f + 0.05f * delta * 128f; // Увеличиваем радиус с учётом delta
        break;
    }

    // Обновляем углы вращения на основе движения мыши
    var azimuth = deltaX * delta * 0.01f; // Вращение по горизонтали (вокруг оси Y)
    var zenith = deltaY * delta * 0.01f; // Вращение по вертикали (вокруг локальной оси X)

    // Ограничиваем зенитный угол (чтобы избежать переворота камеры)
    zenith = Math.Clamp(zenith, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);

    // Создаем вращение для оси Y (азимут) вокруг глобальной оси Y
    var rotationY = Quaternion.FromAxisAngle(Vector3.UnitY, azimuth);

    // Создаем вращение для оси X (зенит) вокруг локальной оси X
    var rightVec = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, focusPoint - Position));
    var rotationX = Quaternion.FromAxisAngle(rightVec, zenith);

    // Обновляем общее вращение камеры (накладываем вращения на существующее)
    Rotation = Quaternion.Normalize(rotationY * rotationX * Rotation);

    // Рассчитываем новую позицию камеры
    var offset = new Vector3(0, 0, -radius); // Камера "смотрит" назад
    Position = focusPoint + Vector3.Transform(offset, Rotation); // Позиция камеры с учётом вращения
  }

  public void BasicMovement(float delta)
  {
    if (Keyboard.IsKeyDown(KeyboardKey.A)) OffsetPosition(-delta);
    if (Keyboard.IsKeyDown(KeyboardKey.D)) OffsetPosition(delta);

    if (Keyboard.IsKeyDown(KeyboardKey.W)) OffsetPosition(z: delta);
    if (Keyboard.IsKeyDown(KeyboardKey.S)) OffsetPosition(z: -delta);

    if (Keyboard.IsKeyDown(KeyboardKey.ArrowUp)) OffsetPosition(y: delta);
    if (Keyboard.IsKeyDown(KeyboardKey.ArrowDown)) OffsetPosition(y: -delta);

    if (Keyboard.IsKeyDown(KeyboardKey.Q)) Rotation = Rotation.RotateEuler(0f, -delta, 0f, "rad");
    if (Keyboard.IsKeyDown(KeyboardKey.E)) Rotation = Rotation.RotateEuler(0f, delta, 0f, "rad");

    if (Keyboard.IsKeyDown(KeyboardKey.ArrowLeft)) Rotation = Rotation.RotateEuler(-delta, 0f, 0f, "rad");
    if (Keyboard.IsKeyDown(KeyboardKey.ArrowRight)) Rotation = Rotation.RotateEuler(delta, 0f, 0f, "rad");
  }
}