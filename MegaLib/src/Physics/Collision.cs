using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics.Collider;

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
    if (colliderA is SphereCollider sphereCollider1 && colliderB is SphereCollider sphereCollider12)
    {
      return Detect(sphereCollider1, bodyA, sphereCollider12, bodyB, out collisionData);
    }

    if (colliderA is SphereCollider && colliderB is PlaneCollider)
    {
      return Detect((SphereCollider)colliderA, bodyA, (PlaneCollider)colliderB, bodyB, out collisionData);
    }

    if (colliderA is PlaneCollider && colliderB is SphereCollider)
    {
      return Detect((SphereCollider)colliderB, bodyB, (PlaneCollider)colliderA, bodyA, out collisionData);
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

  public static bool Detect(
    SphereCollider colliderA,
    RigidBody bodyA,
    PlaneCollider colliderB,
    RigidBody bodyB,
    out CollisionData collisionData)
  {
    collisionData = null;

    // Позиция центра сферы
    var sphereCenter = bodyA.Position + colliderA.Transform.Position;

    // Нормаль плоскости с учетом поворота
    var planeNormal = Vector3.Transform(colliderB.Normal, bodyB.Rotation);

    // Позиция плоскости
    var planePosition = bodyB.Position + colliderB.Transform.Position;

    // Вектор от позиции плоскости до центра сферы
    var planeToSphere = sphereCenter - planePosition;

    // Расстояние от центра сферы до плоскости (проекция на нормаль плоскости)
    var distanceFromPlane = Vector3.Dot(planeToSphere, planeNormal);

    // Проверка на столкновение: если расстояние от центра сферы до плоскости меньше или равно радиусу сферы
    if (distanceFromPlane <= colliderA.Radius)
    {
      // Глубина проникновения
      var penetrationDepth = colliderA.Radius - distanceFromPlane;

      // Точка касания: всегда на поверхности сферы по направлению нормали плоскости
      var contactPoint = sphereCenter - planeNormal * colliderA.Radius;

      collisionData = new CollisionData
      {
        ColliderA = colliderA,
        BodyA = bodyA,
        ColliderB = colliderB,
        BodyB = bodyB,
        ContactPoint = contactPoint,
        CollisionNormal = -planeNormal,
        PenetrationDepth = penetrationDepth
      };

      return true;
    }

    return false;
  }
}

public class CollisionData
{
  public BaseCollider ColliderA;
  public BaseCollider ColliderB;
  public RigidBody BodyA;
  public RigidBody BodyB;
  public Vector3 ContactPoint;
  public Vector3 CollisionNormal;
  public float PenetrationDepth;
}