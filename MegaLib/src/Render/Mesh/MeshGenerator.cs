using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Mesh;

public class Mesh
{
  public List<Vector3> VertexList = [];
  public List<Vector2> UV0List = [];
  public List<Vector3> NormalList = [];
  public List<uint> IndexList = [];

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
      NormalList[i] = NormalList[i].Normalized;
    }
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
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 0.0f, 1.0f));

    // Back
    vertices.Add(new Vector3(-1.0f, -1.0f, 1.0f));
    vertices.Add(new Vector3(-1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, 1.0f, 1.0f));
    vertices.Add(new Vector3(1.0f, -1.0f, 1.0f));
    for (var i = 0; i < 4; i++) normals.Add(new Vector3(0.0f, 0.0f, -1.0f));

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
}