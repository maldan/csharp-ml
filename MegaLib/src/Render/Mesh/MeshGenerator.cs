using System;
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
      NormalList[i] = -NormalList[i].Normalized;
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