using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Triangle : IRayIntersectable
{
  public Vector3 A;
  public Vector3 B;
  public Vector3 C;

  public Triangle(Vector3 a, Vector3 b, Vector3 c)
  {
    A = a;
    B = b;
    C = c;
  }

  public void RayIntersection(Ray ray, out Vector3 point, out bool isHit)
  {
    var edge1 = B - A;
    var edge2 = C - A;
    var h = Vector3.Cross(ray.Direction, edge2);
    var a = Vector3.Dot(edge1, h);

    if (a is > -float.Epsilon and < float.Epsilon)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    var f = 1.0 / a;
    var s = ray.Start - A;
    var u = f * Vector3.Dot(s, h);

    if (u is < 0.0 or > 1.0)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    var q = Vector3.Cross(s, edge1);
    var v = f * Vector3.Dot(ray.Direction, q);

    if (v < 0.0 || u + v > 1.0)
    {
      point = Vector3.Zero;
      isHit = false;
      return;
    }

    // Теперь вычисляем t, чтобы получить точку пересечения
    var t = (float)(f * Vector3.Dot(edge2, q));
    if (t > float.Epsilon && t <= ray.Length)
    {
      point = ray.Start + ray.Direction * t;
      isHit = true;
      return;
    }

    point = Vector3.Zero;
    isHit = false;
  }

  public bool RayIntersection(Ray ray, out Vector3 point)
  {
    RayIntersection(ray, out point, out var isHit);
    return isHit;
  }

  public static bool TriangleIntersection(Triangle t1, Triangle t2, out List<Vector3> points)
  {
    var intersectionPoints = new HashSet<Vector3>(); // Ensure unique points

    // Test all edges of t1 against t2
    TestTriangleEdges(t1, t2, intersectionPoints);

    // Test all edges of t2 against t1
    TestTriangleEdges(t2, t1, intersectionPoints);

    points = intersectionPoints.ToList();
    return points.Count > 0;
  }

  private static void TestTriangleEdges(
    Triangle tri,
    Triangle target,
    HashSet<Vector3> intersectionPoints)
  {
    TestEdgeIntersection(tri.A, tri.B, target, intersectionPoints);
    TestEdgeIntersection(tri.B, tri.C, target, intersectionPoints);
    TestEdgeIntersection(tri.C, tri.A, target, intersectionPoints);
  }

  private static void TestEdgeIntersection(
    Vector3 edgeStart,
    Vector3 edgeEnd,
    Triangle target,
    HashSet<Vector3> intersectionPoints)
  {
    if (RayIntersectsTriangle(edgeStart, edgeEnd - edgeStart, target, out var intersectionPoint))
    {
      // Ensure the intersection point lies on the bounded edge
      if (IsPointOnEdge(edgeStart, edgeEnd, intersectionPoint))
      {
        intersectionPoints.Add(intersectionPoint);
      }
    }
  }

  private static bool RayIntersectsTriangle(Vector3 rayOrigin, Vector3 rayDirection, Triangle tri,
    out Vector3 intersectionPoint)
  {
    intersectionPoint = default;

    const float epsilon = 1e-6f; // Avoid numerical precision issues

    var edge1 = tri.B - tri.A;
    var edge2 = tri.C - tri.A;
    var h = Vector3.Cross(rayDirection, edge2);
    var a = Vector3.Dot(edge1, h);

    if (MathF.Abs(a) < epsilon) return false; // Parallel ray and triangle plane

    var f = 1.0f / a;
    var s = rayOrigin - tri.A;
    var u = f * Vector3.Dot(s, h);
    if (u < 0.0f || u > 1.0f) return false;

    var q = Vector3.Cross(s, edge1);
    var v = f * Vector3.Dot(rayDirection, q);
    if (v < 0.0f || u + v > 1.0f) return false;

    var t = f * Vector3.Dot(edge2, q);
    if (t > epsilon) // Intersection point is along the ray
    {
      intersectionPoint = rayOrigin + rayDirection * t;
      return true;
    }

    return false; // No intersection
  }

// Helper: Check if a point lies on the edge
  private static bool IsPointOnEdge(Vector3 edgeStart, Vector3 edgeEnd, Vector3 point, float epsilon = 1e-6f)
  {
    // Vector from start to end of the edge
    var edgeVector = edgeEnd - edgeStart;

    // Vector from start to the point
    var pointVector = point - edgeStart;

    // Check if the point lies along the edge within numerical precision
    var edgeLengthSquared = edgeVector.LengthSquared;
    var projectionLengthSquared = Vector3.Dot(pointVector, edgeVector) / edgeLengthSquared;

    return projectionLengthSquared >= 0.0f && projectionLengthSquared <= 1.0f &&
           (pointVector - edgeVector * projectionLengthSquared).LengthSquared < epsilon * epsilon;
  }
}