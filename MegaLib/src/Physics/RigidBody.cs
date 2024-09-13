using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class RigidBody_Old
{
  public float Mass { get; set; }
  public Vector3 Position { get; set; } // Положение объекта в мировых координатах
  public Vector3 Velocity { get; set; }
  public Vector3 Acceleration { get; set; }

  public Vector3 AngularVelocity { get; set; }
  public Vector3 Torque { get; set; }

  // Локальный центр массы относительно положения объекта
  public Vector3 LocalCenterOfMass { get; set; }

  // Момент инерции
  public float MomentOfInertia { get; set; }

  // Коэффициенты трения
  public float DynamicFriction { get; set; } = 0.5f;
  public float StaticFriction { get; set; } = 0.6f;

  public Quaternion Rotation { get; set; } // Поворот объекта

  public float Drag { get; set; } = 0.4f; // Коэффициент линейного сопротивления
  public float AngularDrag { get; set; } = 0.2f; // Коэффициент углового сопротивления

  public RigidBody_Old(float mass, float momentOfInertia, Vector3 initialPosition, Vector3 localCenterOfMass)
  {
    Mass = mass;
    MomentOfInertia = momentOfInertia;
    Velocity = Vector3.Zero;
    AngularVelocity = Vector3.Zero;
    Acceleration = Vector3.Zero;
    Torque = Vector3.Zero;
    Position = initialPosition;
    LocalCenterOfMass = localCenterOfMass; // Локальная точка
    Rotation = Quaternion.Identity;
  }

  // Мировой центр массы
  public Vector3 WorldCenterOfMass => Position + LocalCenterOfMass;

  // Применение силы на тело в точке относительно мирового центра массы
  public void ApplyForce(Vector3 force, Vector3 point)
  {
    // Линейная сила
    Acceleration += force / Mass;

    // Вращательная сила (момент силы)
    var r = point - WorldCenterOfMass; // Вектор от мирового центра массы до точки приложения силы
    Torque += Vector3.Cross(r, force);
  }

  // Обновление состояния тела
  public void Update(float deltaTime)
  {
    // Применяем линейное сопротивление воздуха
    ApplyDrag(deltaTime);

    // Обновляем линейные параметры
    Velocity += Acceleration * deltaTime;
    Position += Velocity * deltaTime;

    // Применяем угловое сопротивление воздуха
    ApplyAngularDrag(deltaTime);

    // Обновляем угловые параметры
    AngularVelocity += Torque / MomentOfInertia * deltaTime;

    // Обновляем поворот
    UpdateRotation(deltaTime);

    // Очистка сил после применения
    Acceleration = Vector3.Zero;
    Torque = Vector3.Zero;
  }

  // Применение линейного сопротивления воздуха
  private void ApplyDrag(float deltaTime)
  {
    // Сила сопротивления воздуха пропорциональна скорости объекта
    Velocity *= 1 - Drag * deltaTime;
  }

  // Применение углового сопротивления воздуха
  private void ApplyAngularDrag(float deltaTime)
  {
    // Сопротивление влияет на угловую скорость
    AngularVelocity *= 1 - AngularDrag * deltaTime;
  }

  private void UpdateRotation(float deltaTime)
  {
    // Преобразуем угловую скорость в кватернион приращения
    if (AngularVelocity.LengthSquared > 0.0f)
    {
      var axis = AngularVelocity.Normalized;
      var angle = AngularVelocity.Length * deltaTime;
      var deltaRotation = Quaternion.CreateFromAxisAngle(axis, angle);

      // Обновляем текущий поворот
      Rotation = (Rotation * deltaRotation).Normalized;
    }
  }
}

public class RigidBody
{
  // Свойства массы
  public float Mass { get; private set; }
  public float InverseMass { get; private set; }

  // Позиция и ориентация
  public Vector3 Position { get; set; } // Позиция опорной точки тела (обычно геометрический центр)
  public Quaternion Rotation { get; set; }

  // Центр масс
  public Vector3 LocalCenterOfMass { get; set; } // В локальной системе координат
  public Vector3 WorldCenterOfMass => Position + Vector3.Transform(LocalCenterOfMass, Rotation);

  // Скорости
  public Vector3 Velocity { get; set; }
  public Vector3 AngularVelocity { get; set; }

  // Ускорения
  private Vector3 acceleration;
  private Vector3 angularAcceleration;

  // Тензоры инерции
  public Matrix4x4 InertiaTensor { get; private set; }
  public Matrix4x4 InverseInertiaTensor { get; private set; }

  // Накопители сил и моментов
  private Vector3 forceAccumulator;
  private Vector3 torqueAccumulator;

  // Прочие параметры
  public bool IsKinematic { get; set; }
  public bool UseGravity { get; set; }
  public float Drag { get; set; } = 0.1f;
  public float AngularDrag { get; set; } = 0.1f;

  public float Restitution { get; set; } = 0.5f; // Значение по умолчанию

  public List<BaseCollider> Colliders = [];

  // Конструктор
  public RigidBody(float mass)
  {
    SetMass(mass);
    Position = Vector3.Zero;
    Rotation = Quaternion.Identity;
    Velocity = Vector3.Zero;
    AngularVelocity = Vector3.Zero;
    forceAccumulator = Vector3.Zero;
    torqueAccumulator = Vector3.Zero;

    // Инициализируйте тензор инерции в соответствии с формой тела
    // Для простоты возьмем единичную матрицу
    InertiaTensor = Matrix4x4.Identity;
    InverseInertiaTensor = Matrix4x4.Identity;

    // По умолчанию центр масс совпадает с геометрическим центром
    LocalCenterOfMass = Vector3.Zero;
  }

  public void InitializeInertiaTensor(float mass, float radius)
  {
    var inertiaValue = 2.0f / 5.0f * mass * radius * radius;
    InertiaTensor = Matrix4x4.Identity * inertiaValue;
    InverseInertiaTensor = InertiaTensor.Inverted;
  }

  // Установка массы
  public void SetMass(float mass)
  {
    Mass = mass;
    InverseMass = mass > 0 ? 1.0f / mass : 0.0f;

    InitializeInertiaTensor(mass, 0.5f);
  }

  // Применение силы
  public void ApplyForce(Vector3 force)
  {
    forceAccumulator += force;
  }

  // Применение силы в точке
  public void ApplyForceAtPoint(Vector3 force, Vector3 point)
  {
    // Применяем силу к телу
    ApplyForce(force);

    // Вычисляем вектор от центра масс до точки приложения силы
    var r = point - WorldCenterOfMass;

    // Вычисляем крутящий момент: τ = r × F
    var torque = Vector3.Cross(r, force);
    ApplyTorque(torque);
  }

  // Применение крутящего момента
  public void ApplyTorque(Vector3 torque)
  {
    torqueAccumulator += torque;
  }

  // Добавление импульса
  public void AddImpulse(Vector3 impulse)
  {
    Velocity += impulse * InverseMass;
  }

  // Добавление углового импульса
  public void AddAngularImpulse(Vector3 angularImpulse)
  {
    AngularVelocity += Vector3.Transform(angularImpulse, InverseInertiaTensor);
  }

  // Интеграция (обновление состояния)
  public void Update(float deltaTime)
  {
    if (IsKinematic || InverseMass == 0f)
      return;

    // Применение гравитации
    if (UseGravity)
      ApplyForce(new Vector3(0, -9.81f * Mass, 0));

    // Вычисление линейного ускорения
    acceleration = forceAccumulator * InverseMass;
    acceleration -= Velocity * Drag; // Учёт сопротивления среды

    // Обновление линейной скорости и позиции
    Velocity += acceleration * deltaTime;
    Position += Velocity * deltaTime;

    // Вычисление углового ускорения
    angularAcceleration = Vector3.Transform(torqueAccumulator, InverseInertiaTensor);
    angularAcceleration -= AngularVelocity * AngularDrag; // Учёт углового сопротивления

    // Обновление угловой скорости
    AngularVelocity += angularAcceleration * deltaTime;

    // Обновление ориентации
    var dt = AngularVelocity * deltaTime;
    var deltaRotation = new Quaternion(dt.X, dt.Y, dt.Z, 0f);
    Rotation = (Rotation + deltaRotation * Rotation * 0.5f).Normalized;

    // Очистка накопителей
    ClearAccumulators();
  }

  // Очистка накопителей
  public void ClearAccumulators()
  {
    forceAccumulator = Vector3.Zero;
    torqueAccumulator = Vector3.Zero;
  }
}