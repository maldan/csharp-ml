using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class RigidBody
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

  public RigidBody(float mass, float momentOfInertia, Vector3 initialPosition, Vector3 localCenterOfMass)
  {
    Mass = mass;
    MomentOfInertia = momentOfInertia;
    Velocity = Vector3.Zero;
    AngularVelocity = Vector3.Zero;
    Acceleration = Vector3.Zero;
    Torque = Vector3.Zero;
    Position = initialPosition;
    LocalCenterOfMass = localCenterOfMass; // Локальная точка
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
    // Обновляем линейные параметры
    Velocity += Acceleration * deltaTime;
    Position += Velocity * deltaTime;

    // Обновляем угловые параметры
    AngularVelocity += Torque / MomentOfInertia * deltaTime;

    // Обновляем поворот
    UpdateRotation(deltaTime);

    // Очистка сил после применения
    Acceleration = Vector3.Zero;
    Torque = Vector3.Zero;
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