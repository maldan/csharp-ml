using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class PhysicsWorld
{
  public List<RigidBody> RigidBodies { get; private set; } = [];
  public List<CollisionData> Collisions { get; private set; } = [];

  public void Add(RigidBody body)
  {
    RigidBodies.Add(body);
  }

  public void Update(float deltaTime)
  {
    // 1. Сбор всех коллайдеров и их тел
    List<(BaseCollider Collider, RigidBody Body)> colliders = [];
    foreach (var body in RigidBodies)
    {
      foreach (var collider in body.Colliders)
      {
        colliders.Add((collider, body));
      }
    }

    // Проверяем каждую пару коллайдеров на наличие коллизий
    Collisions = [];
    for (var i = 0; i < colliders.Count; i++)
    {
      for (var j = i + 1; j < colliders.Count; j++)
      {
        var colliderA = colliders[i].Collider;
        var bodyA = colliders[i].Body;
        var colliderB = colliders[j].Collider;
        var bodyB = colliders[j].Body;

        // Здесь предполагается, что у вас есть метод для обнаружения коллизий между двумя коллайдерами
        if (CollisionDetection.Detect(colliderA, bodyA, colliderB, bodyB, out var collisionData))
        {
          Collisions.Add(collisionData);
        }
      }
    }

    ResolveCollisions(Collisions);

    foreach (var body in RigidBodies)
    {
      body.Update(deltaTime);
    }
  }

  private void ResolveCollisions(List<CollisionData> collisions)
  {
    foreach (var collision in collisions)
    {
      var bodyA = collision.BodyA;
      var bodyB = collision.BodyB;

      // 1. Позиционная коррекция (раздвигаем тела)
      PositionalCorrection(bodyA, bodyB, collision);

      // 2. Применение импульса (изменение скорости)
      ApplyImpulse(bodyA, bodyB, collision);
    }
  }

  private void PositionalCorrection(RigidBody bodyA, RigidBody bodyB, CollisionData collision)
  {
    const float correctionPercent = 0.8f; // Процент коррекции
    const float slop = 0.01f; // Допустимое проникновение

    var totalInverseMass = (bodyA.IsKinematic ? 0 : bodyA.InverseMass) + (bodyB.IsKinematic ? 0 : bodyB.InverseMass);
    if (totalInverseMass == 0) return; // Оба тела неподвижны

    var correction = collision.CollisionNormal *
                     (Math.Max(collision.PenetrationDepth - slop, 0.0f) / totalInverseMass) * correctionPercent;

    if (!bodyA.IsKinematic) bodyA.Position -= correction * bodyA.InverseMass;
    if (!bodyB.IsKinematic) bodyB.Position += correction * bodyB.InverseMass;
  }

  private void ApplyImpulse(RigidBody bodyA, RigidBody bodyB, CollisionData collision)
  {
    // Линейный импульс
    var relativeVelocity = bodyB.Velocity - bodyA.Velocity;
    var velocityAlongNormal = Vector3.Dot(relativeVelocity, collision.CollisionNormal);

    if (velocityAlongNormal > 0)
      return; // Объекты уже разделяются, не применяем импульс

    // Коэффициент упругости
    var restitution = Math.Min(bodyA.Restitution, bodyB.Restitution);

    // Рычаги (векторы от центра масс до точки контакта)
    var ra = collision.ContactPoint - bodyA.Position; // Вектор от центра масс до точки касания
    var rb = collision.ContactPoint - bodyB.Position;

    // Инверсная масса
    var totalInverseMass = (bodyA.IsKinematic ? 0 : bodyA.InverseMass) + (bodyB.IsKinematic ? 0 : bodyB.InverseMass);
    if (totalInverseMass == 0) return; // Оба тела кинематические, не применяем импульс

    // Линейная часть импульса
    var impulseScalar = -(1 + restitution) * velocityAlongNormal;
    impulseScalar /= totalInverseMass;

    var impulse = impulseScalar * collision.CollisionNormal;

    // Применяем линейный импульс
    if (!bodyA.IsKinematic) bodyA.Velocity -= impulse * bodyA.InverseMass;
    if (!bodyB.IsKinematic) bodyB.Velocity += impulse * bodyB.InverseMass;

    // 1. Рассчитываем момент импульса для каждого тела
    if (!bodyA.IsKinematic && ra.Length > 0)
    {
      var angularImpulseA = Vector3.Cross(ra, impulse);
      bodyA.AngularVelocity += Vector3.Transform(angularImpulseA, bodyA.InverseInertiaTensor);
    }

    if (!bodyB.IsKinematic && rb.Length > 0)
    {
      var angularImpulseB = Vector3.Cross(rb, impulse);
      bodyB.AngularVelocity += Vector3.Transform(angularImpulseB, bodyB.InverseInertiaTensor);
    }
  }
}