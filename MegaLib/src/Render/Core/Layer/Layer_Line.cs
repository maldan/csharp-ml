using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Core.Layer;

public class Layer_Line : Layer_Base
{
  public Camera_Orthographic Camera;
  public float LineWidth = 1.0f;
  public bool IsSmooth = true;
  public bool IsYInverted = false;

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

        // Преобразуем точки в мировые координаты с помощью матрицы трансформации
        //p1 = Vector3.Transform(p1, transform);
        //p2 = Vector3.Transform(p2, transform);

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

        // Преобразуем точки в мировые координаты с помощью матрицы трансформации
        //p1 = Vector3.Transform(p1, transform);
        //p2 = Vector3.Transform(p2, transform);

        p1 *= transform.Matrix;
        p2 *= transform.Matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }

    /*var numSegments = 8;
    var position = Vector3.Zero;
    var radius = sphere.Radius;

    // Рисуем окружности вдоль каждой из трех координатных плоскостей
    for (var i = 0; i < numSegments; i++)
    {
      var angle = i / (float)numSegments * 2.0f * (float)Math.PI;

      // Рисуем окружность в плоскости XY
      var x0 = position.X + radius * (float)Math.Cos(angle);
      var y0 = position.Y + radius * (float)Math.Sin(angle);
      var z0 = position.Z;
      var x1 = position.X + radius * (float)Math.Cos(angle + 2.0 * Math.PI / numSegments);
      var y1 = position.Y + radius * (float)Math.Sin(angle + 2.0 * Math.PI / numSegments);
      var z1 = position.Z;
      var p1 = (new Vector4(x0, y0, z0, 1.0f) * sphere.Transform.Matrix).DropW();
      var p2 = (new Vector4(x1, y1, z1, 1.0f) * sphere.Transform.Matrix).DropW();
      Add(new RO_Line(p1, p2, color));

      // Рисуем окружность в плоскости XZ
      x0 = position.X + radius * (float)Math.Cos(angle);
      y0 = position.Y;
      z0 = position.Z + radius * (float)Math.Sin(angle);
      x1 = position.X + radius * (float)Math.Cos(angle + 2.0 * Math.PI / numSegments);
      y1 = position.Y;
      z1 = position.Z + radius * (float)Math.Sin(angle + 2.0 * Math.PI / numSegments);
      p1 = (new Vector4(x0, y0, z0, 1.0f) * sphere.Transform.Matrix).DropW();
      p2 = (new Vector4(x1, y1, z1, 1.0f) * sphere.Transform.Matrix).DropW();
      Add(new RO_Line(p1, p2, color));

      // Рисуем окружность в плоскости YZ
      x0 = position.X;
      y0 = position.Y + radius * (float)Math.Cos(angle);
      z0 = position.Z + radius * (float)Math.Sin(angle);
      x1 = position.X;
      y1 = position.Y + radius * (float)Math.Cos(angle + 2.0 * Math.PI / numSegments);
      z1 = position.Z + radius * (float)Math.Sin(angle + 2.0 * Math.PI / numSegments);
      p1 = (new Vector4(x0, y0, z0, 1.0f) * sphere.Transform.Matrix).DropW();
      p2 = (new Vector4(x1, y1, z1, 1.0f) * sphere.Transform.Matrix).DropW();
      Add(new RO_Line(p1, p2, color));
    }*/
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