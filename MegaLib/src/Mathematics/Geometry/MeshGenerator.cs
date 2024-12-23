using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public static class MeshBoolean
{
  private static List<Triangle> Triangulate(List<Vector3> vertices)
  {
    var triangles = new List<Triangle>();

    // Simplified triangulation assuming convex polygons
    for (var i = 1; i < vertices.Count - 1; i++)
    {
      triangles.Add(new Triangle
      {
        A = vertices[0],
        B = vertices[i],
        C = vertices[i + 1]
      });
    }

    return triangles;
  }

  public static List<Triangle> SplitTriangle(Triangle triangle, List<Vector3> intersectionPoints)
  {
    // Combine the original triangle vertices with intersection points
    var vertices = new List<Vector3> { triangle.A, triangle.B, triangle.C };
    vertices.AddRange(intersectionPoints);

    // Remove duplicate points with some tolerance for floating-point precision
    vertices = vertices.Distinct().ToList();

    // Triangulate the resulting polygon
    return Triangulate(vertices);
  }

  private static bool IsPointOnEdge(Vector3 start, Vector3 end, Vector3 point)
  {
    // Calculate the distance from the point to the edge
    var edge = end - start;
    var toPoint = point - start;
    var cross = Vector3.Cross(edge, toPoint);

    // Check if the point is collinear and lies within the edge segment
    return cross.LengthSquared < float.Epsilon && Vector3.Dot(toPoint, edge) >= 0 &&
           Vector3.Dot(toPoint, edge) <= edge.LengthSquared;
  }

  private static List<Vector3> DeduplicateAndSortPoints(Triangle triangle, List<Vector3> points)
  {
    // Remove duplicate points with a small epsilon tolerance
    points = points.Distinct().ToList();

    // Sort points along the edges of the triangle
    var sortedPoints = new List<Vector3>();

    // Sort points along edge A-B
    sortedPoints.AddRange(points
      .Where(p => IsPointOnEdge(triangle.A, triangle.B, p))
      .OrderBy(p => (p - triangle.A).LengthSquared));

    // Sort points along edge B-C
    sortedPoints.AddRange(points
      .Where(p => IsPointOnEdge(triangle.B, triangle.C, p))
      .OrderBy(p => (p - triangle.B).LengthSquared));

    // Sort points along edge C-A
    sortedPoints.AddRange(points
      .Where(p => IsPointOnEdge(triangle.C, triangle.A, p))
      .OrderBy(p => (p - triangle.C).LengthSquared));

    return sortedPoints.Distinct().ToList(); // Ensure no duplicate points
  }

  public static Mesh SplitMesh(Mesh mesh, Mesh otherMesh)
  {
    var splitMesh = new Mesh(); // Resulting mesh with split triangles

    // Loop through each triangle in the input mesh
    for (var i = 0; i < mesh.TriangleCount; i++)
    {
      var triangle = mesh.GetTriangleById(i); // Get the current triangle

      // Collect all intersection points for this triangle
      var intersectionPoints = new List<Vector3>();
      for (var j = 0; j < otherMesh.TriangleCount; j++)
      {
        var otherTriangle = otherMesh.GetTriangleById(j);

        // Check for intersection with the other triangle
        if (Triangle.TriangleIntersection(triangle, otherTriangle, out var points))
        {
          intersectionPoints.AddRange(points); // Add the intersection points
        }
      }

      // If there are intersection points, split the triangle
      if (intersectionPoints.Count > 0)
      {
        // Remove duplicates and sort the points along the triangle edges
        intersectionPoints = DeduplicateAndSortPoints(triangle, intersectionPoints);

        // Split the triangle into smaller triangles
        var newTriangles = SplitTriangle(triangle, intersectionPoints);

        // Add all the new triangles to the split mesh
        foreach (var newTriangle in newTriangles)
        {
          splitMesh.AddTriangle(newTriangle);
        }
      }
      else
      {
        // No intersections, keep the original triangle
        splitMesh.AddTriangle(triangle);
      }
    }

    return splitMesh; // Return the new split mesh
  }

  public static Mesh Difference(Mesh meshA, Mesh meshB)
  {
    var resultMesh = new Mesh();

    // Step 1: Split Mesh A and Mesh B
    var splitMeshA = SplitMesh(meshA, meshB);
    var splitMeshB = SplitMesh(meshB, meshA);

    // Step 2: Add triangles from Mesh A that are outside Mesh B
    for (var i = 0; i < splitMeshA.TriangleCount; i++)
    {
      var triangle = splitMeshA.GetTriangleById(i);

      // Keep triangles that are outside Mesh B
      if (!meshB.IsTriangleInsideMesh(triangle))
      {
        if (!triangle.IsDegenerate) resultMesh.AddTriangle(triangle);
      }
    }

    // Step 3: Add boundary triangles from Mesh B
    for (var i = 0; i < splitMeshB.TriangleCount; i++)
    {
      var triangle = splitMeshB.GetTriangleById(i);

      // Add boundary triangles that are inside Mesh A
      if (meshA.IsTriangleInsideMesh(triangle))
      {
        if (!triangle.IsDegenerate) resultMesh.AddTriangle(triangle);
      }
    }

    return resultMesh;
  }
}

public static class MeshGenerator
{
  public static Mesh Cube(float size)
  {
    var m = new Mesh();
    var vertices = new List<Vector3>();
    var normals = new List<Vector3>();
    var uv = new List<Vector2>();
    var indices = new List<uint>();

    // Front
    vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 0.0f, -1.0f));

    // Back
    vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 0.0f, 1.0f));

    // Top
    vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 1.0f, 0.0f));

    // Bottom
    vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
    vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, -1.0f, 0.0f));

    // Left
    vertices.Add(new Vector3(-1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, -1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));

    // Right
    vertices.Add(new Vector3(1.0f, -1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, -1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(1.0f, 0.0f, 0.0f));

    // UV
    for (var i = 0; i < 6; i++)
    {
      uv.Add(new Vector2(0.0f, 0.0f));
      uv.Add(new Vector2(1.0f, 0.0f));
      uv.Add(new Vector2(1.0f, 1.0f));
      uv.Add(new Vector2(0.0f, 1.0f));
    }

    // Indices
    for (var i = 0; i < 6; i++)
    {
      var next = (uint)(i * 4);
      indices.Add(next);
      indices.Add(1 + next);
      indices.Add(2 + next);
      indices.Add(next);
      indices.Add(2 + next);
      indices.Add(3 + next);
    }

    for (var i = 0; i < vertices.Count; i++) vertices[i] *= size / 2f;

    m.VertexList = vertices;
    m.NormalList = normals;
    m.UV0List = uv;
    m.IndexList = indices;

    return m;
  }

  public static Mesh Grid(int xSegments, int zSegments, float size)
  {
    var m = new Mesh();
    var vertices = new List<Vector3>();
    var normals = new List<Vector3>();
    var uv = new List<Vector2>();
    var indices = new List<uint>();

    // Шаг по осям X и Z (чтобы разбить на сегменты)
    var xStep = size / xSegments;
    var zStep = size / zSegments;

    // Генерация вершин и UV
    for (var z = 0; z <= zSegments; z++)
    {
      for (var x = 0; x <= xSegments; x++)
      {
        // Вершина на сетке
        vertices.Add(new Vector3(x * xStep - size / 2, 0.0f, z * zStep - size / 2));

        // Нормаль для плоскости всегда вверх (0, 1, 0)
        normals.Add(new Vector3(0.0f, 1.0f, 0.0f));

        // UV координаты
        uv.Add(new Vector2((float)x / xSegments, (float)z / zSegments));
      }
    }

    // Генерация индексов для треугольников
    for (var z = 0; z < zSegments; z++)
    {
      for (var x = 0; x < xSegments; x++)
      {
        // Индексы для двух треугольников, составляющих квадрат
        var topLeft = z * (xSegments + 1) + x;
        var topRight = topLeft + 1;
        var bottomLeft = topLeft + xSegments + 1;
        var bottomRight = bottomLeft + 1;

        // Первый треугольник
        indices.Add((uint)topLeft);
        indices.Add((uint)bottomLeft);
        indices.Add((uint)bottomRight);

        // Второй треугольник
        indices.Add((uint)topLeft);
        indices.Add((uint)bottomRight);
        indices.Add((uint)topRight);
      }
    }

    m.VertexList = vertices;
    m.NormalList = normals;
    m.UV0List = uv;
    m.IndexList = indices;

    return m;
  }

  public static Mesh UVSphere(int latitudeSegments, int longitudeSegments, float radius)
  {
    var m = new Mesh();
    var vertices = new List<Vector3>();
    var normals = new List<Vector3>();
    var uv = new List<Vector2>();
    var indices = new List<uint>();

    // Верхний полюс (одна вершина)
    vertices.Add(new Vector3(0, radius, 0)); // Вершина на верхнем полюсе
    normals.Add(Vector3.Up); // Нормаль вверх
    uv.Add(new Vector2(0.5f, 0.0f)); // UV для верхнего полюса (фиксированный центр текстуры)

    // Генерация вершин, нормалей и UV координат для сегментов
    for (var lat = 1; lat < latitudeSegments; lat++) // Исключаем полюса
    {
      var theta = (float)lat / latitudeSegments * MathF.PI; // от 0 до PI (вдоль широты)
      var sinTheta = MathF.Sin(theta);
      var cosTheta = MathF.Cos(theta);

      for (var lon = 0; lon <= longitudeSegments; lon++)
      {
        var phi = (float)lon / longitudeSegments * 2.0f * MathF.PI; // от 0 до 2PI (вдоль долготы)
        var sinPhi = MathF.Sin(phi);
        var cosPhi = MathF.Cos(phi);

        // Вершина сферы
        var x = cosPhi * sinTheta;
        var y = cosTheta;
        var z = sinPhi * sinTheta;
        var vertex = new Vector3(x, y, z) * radius;
        vertices.Add(vertex);

        // Нормаль направлена от центра сферы к вершине
        normals.Add(new Vector3(x, y, z).Normalized);

        // UV координаты (u: вдоль долготы, v: вдоль широты)
        var u = (float)lon / longitudeSegments;
        var v = (float)lat / latitudeSegments;
        uv.Add(new Vector2(u, v));
      }
    }

    // Нижний полюс (одна вершина)
    vertices.Add(new Vector3(0, -radius, 0)); // Вершина на нижнем полюсе
    normals.Add(-Vector3.Up); // Нормаль вниз
    uv.Add(new Vector2(0.5f, 1.0f)); // UV для нижнего полюса (фиксированный центр текстуры)

    // Генерация индексов для верхнего полюса
    for (var lon = 0; lon < longitudeSegments; lon++)
    {
      indices.Add(0u); // Верхний полюс
      indices.Add((uint)(lon + 1));
      indices.Add((uint)(lon + 2));
    }

    // Генерация индексов для тела сферы
    for (var lat = 0; lat < latitudeSegments - 2; lat++) // Исключаем полюса
    {
      for (var lon = 0; lon < longitudeSegments; lon++)
      {
        var first = lat * (longitudeSegments + 1) + lon + 1;
        var second = first + longitudeSegments + 1;

        // Треугольники для квадрата на теле сферы
        indices.Add((uint)first);
        indices.Add((uint)second);
        indices.Add((uint)first + 1);

        indices.Add((uint)second);
        indices.Add((uint)second + 1);
        indices.Add((uint)first + 1);
      }
    }

    // Генерация индексов для нижнего полюса
    var bottomIndex = vertices.Count - 1;
    for (var lon = 0; lon < longitudeSegments; lon++)
    {
      indices.Add((uint)bottomIndex);
      indices.Add((uint)(bottomIndex - (longitudeSegments + 1) + lon));
      indices.Add((uint)(bottomIndex - (longitudeSegments + 1) + lon + 1));
    }

    m.VertexList = vertices;
    m.NormalList = normals;
    m.UV0List = uv;
    m.IndexList = indices;

    return m;
  }
}