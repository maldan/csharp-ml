using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics;
using MegaLib.Physics.Collider;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Layer;

public class Layer_Line : Layer_Base
{
  public Camera_Orthographic Camera;
  public float LineWidth = 1.0f;
  public bool IsSmooth = true;

  public void Draw(VerletLine line)
  {
    for (var i = 0; i < line.Points.Count - 1; i++)
    {
      Add(new RO_Line(line.Points[i].Position, line.Points[i + 1].Position, line.Points[i].Color,
        line.Points[i + 1].Color));
    }
  }

  public void Draw(Ray ray, RGBA<float> color)
  {
    Add(new RO_Line(ray.Start, ray.End, color));
  }

  public void Draw(BoxCollider box, RGBA<float> color)
  {
    // Получаем размеры бокса
    var halfSize = box.Size / 2.0f;
    var transform = box.Transform;

    // Вершины бокса в локальных координатах (8 вершин куба)
    var vertices = new Vector3[]
    {
      new(-halfSize.X, -halfSize.Y, -halfSize.Z), // V0
      new(halfSize.X, -halfSize.Y, -halfSize.Z), // V1
      new(halfSize.X, halfSize.Y, -halfSize.Z), // V2
      new(-halfSize.X, halfSize.Y, -halfSize.Z), // V3
      new(-halfSize.X, -halfSize.Y, halfSize.Z), // V4
      new(halfSize.X, -halfSize.Y, halfSize.Z), // V5
      new(halfSize.X, halfSize.Y, halfSize.Z), // V6
      new(-halfSize.X, halfSize.Y, halfSize.Z) // V7
    };

    // Преобразование вершин в мировые координаты с учетом трансформации
    for (var i = 0; i < vertices.Length; i++)
    {
      vertices[i] *= transform.Matrix;
    }

    // Линии между вершинами бокса (12 линий для 8 вершин)
    Add(new RO_Line(vertices[0], vertices[1], color)); // Нижняя сторона
    Add(new RO_Line(vertices[1], vertices[2], color));
    Add(new RO_Line(vertices[2], vertices[3], color));
    Add(new RO_Line(vertices[3], vertices[0], color));

    Add(new RO_Line(vertices[4], vertices[5], color)); // Верхняя сторона
    Add(new RO_Line(vertices[5], vertices[6], color));
    Add(new RO_Line(vertices[6], vertices[7], color));
    Add(new RO_Line(vertices[7], vertices[4], color));

    Add(new RO_Line(vertices[0], vertices[4], color)); // Вертикальные линии
    Add(new RO_Line(vertices[1], vertices[5], color));
    Add(new RO_Line(vertices[2], vertices[6], color));
    Add(new RO_Line(vertices[3], vertices[7], color));
  }

  public void DrawSphere(Vector3 position, float radius, RGBA<float> color)
  {
    var longitudeSegments = 16 / 2;
    var latitudeSegments = 16 / 2;
    var transform = new Transform
    {
      Position = position
    };

    // матрица трансформации сферы (позиция, вращение, масштаб)

    // Пройтись по долготе (угол вокруг Y оси)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * Math.PI * 2 / longitudeSegments);
      var cosLon = (float)Math.Cos(lon);
      var sinLon = (float)Math.Sin(lon);

      // Пройтись по широте (угол от полюса к полюсу)
      for (var j = 0; j <= latitudeSegments; j++)
      {
        var lat = (float)(j * Math.PI / latitudeSegments - Math.PI / 2); // от -PI/2 до PI/2
        var cosLat = (float)Math.Cos(lat);
        var sinLat = (float)Math.Sin(lat);

        // Координаты точки на поверхности сферы в локальных координатах
        var p1 = new Vector3(
          radius * cosLat * cosLon, // x
          radius * sinLat, // y
          radius * cosLat * sinLon // z
        );

        // Следующая точка по широте (для соединения линией)
        var nextLat = (float)((j + 1) * Math.PI / latitudeSegments - Math.PI / 2);
        var p2 = new Vector3(
          radius * (float)Math.Cos(nextLat) * cosLon,
          radius * (float)Math.Sin(nextLat),
          radius * (float)Math.Cos(nextLat) * sinLon
        );

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }

    // Повторить то же самое для другой стороны (соединение вдоль долготы)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * Math.PI * 2 / longitudeSegments);
      var nextLon = (float)((i + 1) * Math.PI * 2 / longitudeSegments);

      for (var j = 0; j <= latitudeSegments; j++)
      {
        var lat = (float)(j * Math.PI / latitudeSegments - Math.PI / 2);

        // Текущая и следующая долгота
        var p1 = new Vector3(
          radius * (float)Math.Cos(lat) * (float)Math.Cos(lon),
          radius * (float)Math.Sin(lat),
          radius * (float)Math.Cos(lat) * (float)Math.Sin(lon)
        );

        var p2 = new Vector3(
          radius * (float)Math.Cos(lat) * (float)Math.Cos(nextLon),
          radius * (float)Math.Sin(lat),
          radius * (float)Math.Cos(lat) * (float)Math.Sin(nextLon)
        );

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }
  }

  public void DrawSphere(Transform transform, float radius, RGBA<float> color)
  {
    var longitudeSegments = 16 / 2;
    var latitudeSegments = 16 / 2;

    // Пройтись по долготе (угол вокруг Y оси)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * Math.PI * 2 / longitudeSegments);
      var cosLon = (float)Math.Cos(lon);
      var sinLon = (float)Math.Sin(lon);

      // Пройтись по широте (угол от полюса к полюсу)
      for (var j = 0; j <= latitudeSegments; j++)
      {
        var lat = (float)(j * Math.PI / latitudeSegments - Math.PI / 2); // от -PI/2 до PI/2
        var cosLat = (float)Math.Cos(lat);
        var sinLat = (float)Math.Sin(lat);

        // Координаты точки на поверхности сферы в локальных координатах
        var p1 = new Vector3(
          radius * cosLat * cosLon, // x
          radius * sinLat, // y
          radius * cosLat * sinLon // z
        );

        // Следующая точка по широте (для соединения линией)
        var nextLat = (float)((j + 1) * Math.PI / latitudeSegments - Math.PI / 2);
        var p2 = new Vector3(
          radius * (float)Math.Cos(nextLat) * cosLon,
          radius * (float)Math.Sin(nextLat),
          radius * (float)Math.Cos(nextLat) * sinLon
        );

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }

    // Повторить то же самое для другой стороны (соединение вдоль долготы)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * Math.PI * 2 / longitudeSegments);
      var nextLon = (float)((i + 1) * Math.PI * 2 / longitudeSegments);

      for (var j = 0; j <= latitudeSegments; j++)
      {
        var lat = (float)(j * Math.PI / latitudeSegments - Math.PI / 2);

        // Текущая и следующая долгота
        var p1 = new Vector3(
          radius * (float)Math.Cos(lat) * (float)Math.Cos(lon),
          radius * (float)Math.Sin(lat),
          radius * (float)Math.Cos(lat) * (float)Math.Sin(lon)
        );

        var p2 = new Vector3(
          radius * (float)Math.Cos(lat) * (float)Math.Cos(nextLon),
          radius * (float)Math.Sin(lat),
          radius * (float)Math.Cos(lat) * (float)Math.Sin(nextLon)
        );

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }
  }

  public void DrawRigidBody(RigidBody body, RGBA<float> color)
  {
    var tr = new Transform();
    tr.Position = body.Position;
    tr.Rotation = body.Rotation;
    DrawSphere(tr, 0.1f, color);

    foreach (var collider in body.Colliders)
    {
      if (collider is SphereCollider sphereCollider)
      {
        var tr2 = new Transform(tr.Matrix * collider.Transform.Matrix);
        DrawSphere(tr2, sphereCollider.Radius, new RGBA<float>(0, 1, 0, 1));
      }

      if (collider is PlaneCollider planeCollider)
      {
        var tr2 = new Transform(tr.Matrix * collider.Transform.Matrix);
        DrawPlane(tr2, 4, new RGBA<float>(0, 1, 0, 1));

        var p = planeCollider.Transform.Position;
        var n = planeCollider.Transform.Position + planeCollider.Normal;

        Add(new RO_Line(p * tr2.Matrix, n * tr2.Matrix, new RGBA<float>(0, 1, 0, 1)));
      }
    }
  }

  public void DrawPlane(Transform t, float size, RGBA<float> color)
  {
    // Центр плоскости
    var center = t.Position;

    // Вектор правого направления (ось X) и вперед (ось Z) с учетом вращения
    var right = Vector3.Transform(Vector3.Right * size / 2, t.Rotation);
    var forward = Vector3.Transform(Vector3.Forward * size / 2, t.Rotation);

    // Вершины плоскости
    var p1 = center - right - forward; // Левый нижний
    var p2 = center - right + forward; // Левый верхний
    var p3 = center + right + forward; // Правый верхний
    var p4 = center + right - forward; // Правый нижний

    // Рисуем 4 линии для плоскости
    Add(new RO_Line(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, color, color)); // Левый нижний - левый верхний
    Add(new RO_Line(p2.X, p2.Y, p2.Z, p3.X, p3.Y, p3.Z, color, color)); // Левый верхний - правый верхний
    Add(new RO_Line(p3.X, p3.Y, p3.Z, p4.X, p4.Y, p4.Z, color, color)); // Правый верхний - правый нижний
    Add(new RO_Line(p4.X, p4.Y, p4.Z, p1.X, p1.Y, p1.Z, color, color)); // Правый нижний - левый нижний
  }

  public void Draw(SphereCollider sphere, RGBA<float> color)
  {
    var longitudeSegments = 16 / 2;
    var latitudeSegments = 16 / 2;
    var radius = sphere.Radius;
    var transform = sphere.Transform; // матрица трансформации сферы (позиция, вращение, масштаб)

    // Пройтись по долготе (угол вокруг Y оси)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * Math.PI * 2 / longitudeSegments);
      var cosLon = (float)Math.Cos(lon);
      var sinLon = (float)Math.Sin(lon);

      // Пройтись по широте (угол от полюса к полюсу)
      for (var j = 0; j <= latitudeSegments; j++)
      {
        var lat = (float)(j * Math.PI / latitudeSegments - Math.PI / 2); // от -PI/2 до PI/2
        var cosLat = (float)Math.Cos(lat);
        var sinLat = (float)Math.Sin(lat);

        // Координаты точки на поверхности сферы в локальных координатах
        var p1 = new Vector3(
          radius * cosLat * cosLon, // x
          radius * sinLat, // y
          radius * cosLat * sinLon // z
        );

        // Следующая точка по широте (для соединения линией)
        var nextLat = (float)((j + 1) * Math.PI / latitudeSegments - Math.PI / 2);
        var p2 = new Vector3(
          radius * (float)Math.Cos(nextLat) * cosLon,
          radius * (float)Math.Sin(nextLat),
          radius * (float)Math.Cos(nextLat) * sinLon
        );

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }

    // Повторить то же самое для другой стороны (соединение вдоль долготы)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * Math.PI * 2 / longitudeSegments);
      var nextLon = (float)((i + 1) * Math.PI * 2 / longitudeSegments);

      for (var j = 0; j <= latitudeSegments; j++)
      {
        var lat = (float)(j * Math.PI / latitudeSegments - Math.PI / 2);

        // Текущая и следующая долгота
        var p1 = new Vector3(
          radius * (float)Math.Cos(lat) * (float)Math.Cos(lon),
          radius * (float)Math.Sin(lat),
          radius * (float)Math.Cos(lat) * (float)Math.Sin(lon)
        );

        var p2 = new Vector3(
          radius * (float)Math.Cos(lat) * (float)Math.Cos(nextLon),
          radius * (float)Math.Sin(lat),
          radius * (float)Math.Cos(lat) * (float)Math.Sin(nextLon)
        );

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }
  }

  public void DrawRectangle(Rectangle r, RGBA<float> color)
  {
    DrawRectangle(new Vector3(r.FromX, r.FromY, 0), new Vector3(r.ToX, r.ToX, 0), color);
  }

  public void DrawRectangle(Vector3 lt, Vector3 rb, RGBA<float> color)
  {
    Add(new RO_Line(lt.X, rb.Y, lt.Z, lt.X, lt.Y, lt.Z, color, color));
    Add(new RO_Line(lt.X, lt.Y, lt.Z, rb.X, lt.Y, lt.Z, color, color));
    Add(new RO_Line(rb.X, lt.Y, lt.Z, rb.X, rb.Y, lt.Z, color, color));
    Add(new RO_Line(rb.X, rb.Y, lt.Z, lt.X, rb.Y, lt.Z, color, color));
  }

  public void DrawAABB(Vector3 center, Vector3 size, RGBA<float> color)
  {
    //var xOffset = new float[] { -size.X / 2, size.X / 2 };
    var yOffset = new float[] { -size.Y / 2, size.Y / 2 };

    foreach (var y in yOffset)
    {
      Add(new RO_Line(
        center.X - size.X / 2, center.Y + y, center.Z - size.Z / 2,
        center.X - size.X / 2, center.Y + y, center.Z + size.Z / 2,
        color, color));
      Add(new RO_Line(
        center.X + size.X / 2, center.Y + y, center.Z - size.Z / 2,
        center.X + size.X / 2, center.Y + y, center.Z + size.Z / 2,
        color, color));
      Add(new RO_Line(
        center.X - size.X / 2, center.Y + y, center.Z - size.Z / 2,
        center.X + size.X / 2, center.Y + y, center.Z - size.Z / 2,
        color, color));
      Add(new RO_Line(
        center.X - size.X / 2, center.Y + y, center.Z + size.Z / 2,
        center.X + size.X / 2, center.Y + y, center.Z + size.Z / 2,
        color, color));
    }

    Add(new RO_Line(
      center.X - size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2,
      center.X - size.X / 2, center.Y + size.Y / 2, center.Z - size.Z / 2,
      color, color));
    Add(new RO_Line(
      center.X + size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2,
      center.X + size.X / 2, center.Y + size.Y / 2, center.Z - size.Z / 2,
      color, color));
    Add(new RO_Line(
      center.X - size.X / 2, center.Y - size.Y / 2, center.Z + size.Z / 2,
      center.X - size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2,
      color, color));
    Add(new RO_Line(
      center.X + size.X / 2, center.Y - size.Y / 2, center.Z + size.Z / 2,
      center.X + size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2,
      color, color));
  }

  public void DrawAABB(AABB aabb, RGBA<float> color)
  {
    DrawAABB(aabb.Center, aabb.Size, color);
  }
}