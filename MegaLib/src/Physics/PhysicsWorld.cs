using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public static class CollisionDetection
{
  public static bool Detect(
    BaseCollider colliderA,
    RigidBody bodyA,
    BaseCollider colliderB,
    RigidBody bodyB,
    out CollisionData collisionData)
  {
    if (colliderA is SphereCollider ca && colliderB is SphereCollider cb)
    {
      return Detect(ca, bodyA, cb, bodyB, out collisionData);
    }

    collisionData = null;
    return false;
  }

  public static bool Detect(
    SphereCollider colliderA,
    RigidBody bodyA,
    SphereCollider colliderB,
    RigidBody bodyB,
    out CollisionData collisionData)
  {
    collisionData = null;

    var positionA = bodyA.Position + colliderA.Transform.Position;
    var positionB = bodyB.Position + colliderB.Transform.Position;

    var delta = positionB - positionA;
    var distanceSquared = delta.LengthSquared;
    var radiusSum = colliderA.Radius + colliderB.Radius;

    if (distanceSquared <= radiusSum * radiusSum)
    {
      var distance = MathF.Sqrt(distanceSquared);
      var collisionNormal = distance > 0 ? delta / distance : new Vector3(1, 0, 0);
      var penetrationDepth = radiusSum - distance;
      var contactPoint = positionA + collisionNormal * (colliderA.Radius - penetrationDepth * 0.5f);

      collisionData = new CollisionData
      {
        ColliderA = colliderA,
        BodyA = bodyA,
        ColliderB = colliderB,
        BodyB = bodyB,
        ContactPoint = contactPoint,
        CollisionNormal = collisionNormal,
        PenetrationDepth = penetrationDepth
      };

      return true;
    }

    return false;
  }
}

public class CollisionData
{
  public SphereCollider ColliderA;
  public SphereCollider ColliderB;
  public RigidBody BodyA;
  public RigidBody BodyB;
  public Vector3 ContactPoint;
  public Vector3 CollisionNormal;
  public float PenetrationDepth;
}

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

    var correction = collision.CollisionNormal * (Math.Max(collision.PenetrationDepth - slop, 0.0f) /
                                                  (bodyA.InverseMass + bodyB.InverseMass)) * correctionPercent;

    bodyA.Position -= correction * bodyA.InverseMass;
    bodyB.Position += correction * bodyB.InverseMass;
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
    var ra = collision.ContactPoint - bodyA.Position;
    var rb = collision.ContactPoint - bodyB.Position;

    // Линейная часть импульса
    var impulseScalar = -(1 + restitution) * velocityAlongNormal;
    impulseScalar /= bodyA.InverseMass + bodyB.InverseMass;

    var impulse = impulseScalar * collision.CollisionNormal;

    // Применяем линейный импульс
    bodyA.Velocity -= impulse * bodyA.InverseMass;
    bodyB.Velocity += impulse * bodyB.InverseMass;

    // 1. Рассчитываем момент импульса для каждого тела
    var angularImpulseA = Vector3.Cross(ra, impulse);
    var angularImpulseB = Vector3.Cross(rb, impulse);

    // 2. Обновляем угловую скорость на основе момента импульса и инерционного тензора
    bodyA.AngularVelocity += Vector3.Transform(angularImpulseA, bodyA.InverseInertiaTensor);
    bodyB.AngularVelocity += Vector3.Transform(angularImpulseB, bodyB.InverseInertiaTensor);
  }
}