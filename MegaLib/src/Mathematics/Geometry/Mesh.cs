using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public class Mesh : IPointIntersectable
{
  public List<Vector3> VertexList = [];
  public List<Vector2> UV0List = [];
  public List<Vector3> NormalList = [];
  public List<uint> IndexList = [];

  public int TriangleCount => IndexList.Count / 3;

  public void AddTriangle(Triangle triangle)
  {
    // Add the vertices of the triangle to the vertex list
    VertexList.Add(triangle.A);
    VertexList.Add(triangle.B);
    VertexList.Add(triangle.C);

    // Add the indices for this triangle to the index list
    var baseIndex = (uint)(VertexList.Count - 3); // Index of the first vertex of this triangle
    IndexList.Add(baseIndex); // First vertex
    IndexList.Add(baseIndex + 1); // Second vertex
    IndexList.Add(baseIndex + 2); // Third vertex
  }

  // Метод для автоматического расчета нормалей
  public void CalculateNormals()
  {
    // Инициализируем список нормалей (нужно очистить, если есть предыдущие данные)
    NormalList = new List<Vector3>(new Vector3[VertexList.Count]);

    // Проходим по всем треугольникам
    for (var i = 0; i < IndexList.Count; i += 3)
    {
      // Получаем индексы трёх вершин треугольника
      var index0 = (int)IndexList[i];
      var index1 = (int)IndexList[i + 1];
      var index2 = (int)IndexList[i + 2];

      // Вершины треугольника
      var v0 = VertexList[index0];
      var v1 = VertexList[index1];
      var v2 = VertexList[index2];

      // Вычисляем два вектора, задающих стороны треугольника
      var edge1 = v1 - v0;
      var edge2 = v2 - v0;

      // Вычисляем нормаль треугольника через векторное произведение
      var triangleNormal = Vector3.Cross(edge1, edge2).Normalized;

      // Добавляем нормаль к каждой вершине треугольника
      NormalList[index0] += triangleNormal;
      NormalList[index1] += triangleNormal;
      NormalList[index2] += triangleNormal;
    }

    // Нормализуем все нормали
    for (var i = 0; i < NormalList.Count; i++)
    {
      NormalList[i] = -NormalList[i].Normalized;
    }
  }

  public Triangle GetTriangleById(int triangleId)
  {
    // Each triangle is represented by 3 consecutive indices in the IndexList.
    var indexOffset = triangleId * 3;

    // Ensure the ID is within bounds
    if (indexOffset + 2 >= IndexList.Count)
      throw new ArgumentOutOfRangeException(nameof(triangleId), "Triangle ID is out of bounds.");

    // Get the indices of the triangle's vertices
    var indexA = IndexList[indexOffset];
    var indexB = IndexList[indexOffset + 1];
    var indexC = IndexList[indexOffset + 2];

    // Retrieve the corresponding vertices from the VertexList
    var vertexA = VertexList[(int)indexA];
    var vertexB = VertexList[(int)indexB];
    var vertexC = VertexList[(int)indexC];

    // Create and return the triangle
    return new Triangle { A = vertexA, B = vertexB, C = vertexC };
  }

  public Mesh Rotate(Quaternion q)
  {
    for (var i = 0; i < VertexList.Count; i++) VertexList[i] *= q;
    return this;
  }

  public Mesh Translate(Vector3 offset)
  {
    for (var i = 0; i < VertexList.Count; i++) VertexList[i] += offset;
    return this;
  }

  public static Mesh operator *(Mesh mesh, Matrix4x4 matrix)
  {
    for (var i = 0; i < mesh.VertexList.Count; i++)
    {
      mesh.VertexList[i] *= matrix;
    }

    return mesh;
  }

  public bool IsTriangleInsideMesh(Triangle triangle)
  {
    // Test the centroid of the triangle
    var centroid = (triangle.A + triangle.B + triangle.C) / 3;
    return PointIntersection(centroid);
  }

  public void PointIntersection(Vector3 point, out bool isHit)
  {
    var intersections = 0;
    // Use a diagonal ray direction to avoid alignment issues
    var rayDirection = Vector3.Forward;

    for (var i = 0; i < TriangleCount; i++)
    {
      var triangle = GetTriangleById(i);
      var ray = Ray.FromPointDirection(point, rayDirection);
      ray.Length = 100000;
      if (triangle.RayIntersection(ray, out _))
      {
        intersections++;
      }
    }

    // If the number of intersections is odd, the point is inside the mesh
    isHit = intersections % 2 == 1;
  }

  public bool PointIntersection(Vector3 point)
  {
    PointIntersection(point, out var isHit);
    return isHit;
  }
}