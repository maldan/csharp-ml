using System;
using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics;
using MegaLib.Physics.Collider;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Skin;

namespace MegaLib.Render.Layer;

public class Layer_Line : Layer_Base
{
  public Camera_Orthographic Camera;
  public override int Count => _lineList.Count;
  private List<RO_Line> _lineList = [];

  public bool IsSmooth = true;
  public bool DisableDepthTest;

  private int _timer;

  public void Add(RO_Line obj)
  {
    _lineList.Add(obj);
  }

  public void ForEach(Action<RO_Line> fn)
  {
    for (var i = 0; i < _lineList.Count; i++) fn(_lineList[i]);
  }

  public override void Clear()
  {
    _lineList.Clear();
  }

  /*public void Draw(VerletLine line)
  {
    for (var i = 0; i < line.Points.Count - 1; i++)
    {
      Add(new RO_Line(line.Points[i].Position, line.Points[i + 1].Position, line.Points[i].Color,
        line.Points[i + 1].Color));
    }
  }

  public void Draw(Ray ray, RGBA32F color)
  {
    Add(new RO_Line(ray.Start, ray.End, color));
  }*/

  public void DrawArc(
    Matrix4x4 transform,
    Vector3 axis,
    float startAngle,
    float endAngle,
    float radius,
    int segments,
    RGBA32F color)
  {
    // Выбираем начальный вектор для каждой оси
    Vector3 startVector;
    var dir = 1;
    if (axis == Vector3.UnitX) // Если ось вращения X
    {
      startVector = new Vector3(0, 0, radius); // Начальная точка на оси Z (смотрит вперед)
      dir = -1;
    }
    else if (axis == Vector3.UnitY) // Если ось вращения Y
    {
      startVector = new Vector3(radius, 0, 0); // Начальная точка на оси X (смотрит вправо)
      dir = -1;
    }
    else if (axis == Vector3.UnitZ) // Если ось вращения Z
    {
      startVector = new Vector3(radius, 0, 0); // Начальная точка на оси X (смотрит вправо)
    }
    else
    {
      throw new ArgumentException("Ось должна быть X, Y или Z");
    }

    // Шаг угла между сегментами
    var angleStep = (endAngle - startAngle) / segments;

    // Переменная для предыдущей точки (инициализируем как пустую)
    var prevPoint = Vector3.Zero;

    // Проходим по сегментам и вычисляем точки дуги
    for (var i = 0; i <= segments; i++)
    {
      // Текущий угол
      var angle = startAngle + i * angleStep * dir;

      // Кватернион для поворота вокруг заданной оси
      var rotation = Quaternion.FromAxisAngle(axis, angle);

      // Вращаем начальный вектор на текущий угол
      var currentPoint = rotation * startVector;

      // Применяем трансформацию для перевода в мировые координаты
      currentPoint = Vector3.Transform(currentPoint, transform);

      // Рисуем линию между предыдущей и текущей точками (начинаем со второй итерации)
      if (i > 0)
      {
        DrawLine(prevPoint, currentPoint, color);
      }

      // Обновляем предыдущую точку
      prevPoint = currentPoint;
    }
  }

  public void DrawCollider(BaseCollider collider, RGBA32F color)
  {
    switch (collider)
    {
      case BoxCollider boxCollider:
        DrawBox(boxCollider, color);
        break;
      case SphereCollider sphereCollider:
        DrawSphere(sphereCollider, color);
        break;
    }
  }

  public void DrawSphere(SphereCollider sphere, RGBA32F color)
  {
    DrawSphere(sphere.Transform.Matrix, sphere.Radius, color);
  }

  public void DrawBox(BoxCollider box, RGBA32F color)
  {
    DrawBox(box.Transform.Matrix, box.Size, color);

    /*// Получаем размеры бокса
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
    Add(new RO_Line(vertices[3], vertices[7], color));*/
  }

  public void DrawSphere(
    Vector3 position,
    float radius,
    RGBA32F color,
    float latitudeSegments = 16 / 2f,
    float longitudeSegments = 16 / 2f
  )
  {
    //var longitudeSegments = 16 / 2;
    //var latitudeSegments = 16 / 2;
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

  public void DrawSimpleSphere(
    Sphere sphere,
    RGBA32F color,
    float latitudeSegments = 16 / 2f,
    float longitudeSegments = 16 / 2f
  )
  {
    //var longitudeSegments = 16 / 2;
    //var latitudeSegments = 16 / 2;
    var radius = sphere.Radius;

    // матрица трансформации сферы (позиция, вращение, масштаб)

    // Пройтись по долготе (угол вокруг Y оси)
    for (var i = 0; i <= longitudeSegments; i++)
    {
      var lon = (float)(i * MathF.PI * 2 / longitudeSegments);
      var cosLon = (float)MathF.Cos(lon);
      var sinLon = (float)MathF.Sin(lon);

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

        p1 += sphere.Position;
        p2 += sphere.Position;
        //p1 *= transform.Matrix;
        //p2 *= transform.Matrix;

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

        p1 += sphere.Position;
        p2 += sphere.Position;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }
  }

  public void DrawBox(Box box, RGBA32F color)
  {
    DrawBox(box.Matrix, box.Size, color);
  }

  public void DrawCapsule(CapsuleCollider collider, RGBA32F color)
  {
    DrawCapsule(collider.Transform.Matrix, collider.Radius, collider.Height, color);
  }

  public void DrawCapsule(Matrix4x4 matrix, float radius, float height, RGBA32F color)
  {
    // Высота цилиндра (без учета сфер)
    var cylinderHeight = height - 2 * radius;
    if (cylinderHeight < 0) cylinderHeight = 0; // Если высота меньше, чем диаметр сфер

    // Нижняя полусфера
    var lowerMatrix = Matrix4x4.Identity.Translate(0, 0, cylinderHeight / 2 + radius) * matrix;
    DrawArc(lowerMatrix, new Vector3(1, 0, 0), MathF.PI / 2, 3 * MathF.PI / 2, radius, 16, color);
    DrawArc(lowerMatrix, new Vector3(0, 1, 0), 0, MathF.PI, radius, 16, color);
    DrawArc(lowerMatrix, new Vector3(0, 0, 1), 0, MathF.PI * 2, radius, 16, color);

    // Верхняя полусфера
    var upperMatrix = Matrix4x4.Identity.Translate(0, 0, -(cylinderHeight / 2 + radius)) * matrix;
    DrawArc(upperMatrix, new Vector3(1, 0, 0), -MathF.PI / 2, MathF.PI / 2, radius, 16, color);
    DrawArc(upperMatrix, new Vector3(0, 1, 0), 0, -MathF.PI, radius, 16, color);
    DrawArc(upperMatrix, new Vector3(0, 0, 1), 0, -MathF.PI * 2, radius, 16, color);

    // Соединяем концы дуг линиями
    var lowerStartX = Vector3.Transform(new Vector3(radius, 0, cylinderHeight / 2 + radius), matrix);
    var lowerEndX = Vector3.Transform(new Vector3(-radius, 0, cylinderHeight / 2 + radius), matrix);
    var lowerStartY = Vector3.Transform(new Vector3(0, radius, cylinderHeight / 2 + radius), matrix);
    var lowerEndY = Vector3.Transform(new Vector3(0, -radius, cylinderHeight / 2 + radius), matrix);

    var upperStartX = Vector3.Transform(new Vector3(radius, 0, -(cylinderHeight / 2 + radius)), matrix);
    var upperEndX = Vector3.Transform(new Vector3(-radius, 0, -(cylinderHeight / 2 + radius)), matrix);
    var upperStartY = Vector3.Transform(new Vector3(0, radius, -(cylinderHeight / 2 + radius)), matrix);
    var upperEndY = Vector3.Transform(new Vector3(0, -radius, -(cylinderHeight / 2 + radius)), matrix);

// Рисуем линии между верхними и нижними точками
    DrawLine(lowerStartX, upperStartX, color); // Линия по оси X (справа)
    DrawLine(lowerEndX, upperEndX, color); // Линия по оси X (слева)
    DrawLine(lowerStartY, upperStartY, color); // Линия по оси Y (сверху)
    DrawLine(lowerEndY, upperEndY, color); // Линия по оси Y (снизу)
  }

  public void DrawBox(Matrix4x4 matrix, Vector3 size, RGBA32F color)
  {
    // Половина размеров коробки (это будут смещения для вершин)
    var halfSize = size * 0.5f;

    // Вершины коробки в локальной системе координат
    var vertices = new Vector3[8]
    {
      new(-halfSize.X, -halfSize.Y, -halfSize.Z), // Нижняя передняя левая
      new(halfSize.X, -halfSize.Y, -halfSize.Z), // Нижняя передняя правая
      new(halfSize.X, halfSize.Y, -halfSize.Z), // Верхняя передняя правая
      new(-halfSize.X, halfSize.Y, -halfSize.Z), // Верхняя передняя левая

      new(-halfSize.X, -halfSize.Y, halfSize.Z), // Нижняя задняя левая
      new(halfSize.X, -halfSize.Y, halfSize.Z), // Нижняя задняя правая
      new(halfSize.X, halfSize.Y, halfSize.Z), // Верхняя задняя правая
      new(-halfSize.X, halfSize.Y, halfSize.Z) // Верхняя задняя левая
    };

    // Преобразуем вершины в мировые координаты через матрицу трансформации
    for (var i = 0; i < vertices.Length; i++)
    {
      vertices[i] = Vector3.Transform(vertices[i], matrix);
    }

    // Рисуем линии между соответствующими вершинами (12 ребер)

    // Передняя грань
    DrawLine(vertices[0], vertices[1], color); // Нижняя
    DrawLine(vertices[1], vertices[2], color); // Правая
    DrawLine(vertices[2], vertices[3], color); // Верхняя
    DrawLine(vertices[3], vertices[0], color); // Левая

    // Задняя грань
    DrawLine(vertices[4], vertices[5], color); // Нижняя
    DrawLine(vertices[5], vertices[6], color); // Правая
    DrawLine(vertices[6], vertices[7], color); // Верхняя
    DrawLine(vertices[7], vertices[4], color); // Левая

    // Соединяем переднюю и заднюю грань
    DrawLine(vertices[0], vertices[4], color); // Нижняя левая
    DrawLine(vertices[1], vertices[5], color); // Нижняя правая
    DrawLine(vertices[2], vertices[6], color); // Верхняя правая
    DrawLine(vertices[3], vertices[7], color); // Верхняя левая
  }

  public void DrawSphere(Matrix4x4 matrix, float radius, RGBA32F color)
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

        p1 *= matrix;
        p2 *= matrix;

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

        p1 *= matrix;
        p2 *= matrix;

        // Рисуем линию между этими двумя точками
        Add(new RO_Line(p1, p2, color));
      }
    }
  }

  public void DrawRigidBody(RigidBody body, RGBA32F color)
  {
    var tr = new Transform();
    tr.Position = body.Position;
    tr.Rotation = body.Rotation;
    DrawSphere(tr.Matrix, 0.1f, color);

    foreach (var collider in body.Colliders)
    {
      if (collider is SphereCollider sphereCollider)
      {
        var tr2 = new Transform(tr.Matrix * collider.Transform.Matrix);
        DrawSphere(tr2.Matrix, sphereCollider.Radius, new RGBA32F(0, 1, 0, 1));
      }

      if (collider is PlaneCollider planeCollider)
      {
        var tr2 = new Transform(tr.Matrix * collider.Transform.Matrix);
        DrawPlane(tr2, 4, new RGBA32F(0, 1, 0, 1));

        var p = planeCollider.Transform.Position;
        var n = planeCollider.Transform.Position + planeCollider.Normal;

        Add(new RO_Line(p * tr2.Matrix, n * tr2.Matrix, new RGBA32F(0, 1, 0, 1)));
      }
    }
  }

  public void DrawCone(Vector3 apex, Vector3 normal, float height, float radius, int segments, RGBA32F color)
  {
    // Вычисление основания конуса
    var baseCenter = apex - normal.Normalized * height; // Центр основания
    var rotation = Quaternion.FromToRotation(Vector3.Up, normal.Normalized); // Создаем кватернион для поворота

    var basePoints = new List<Vector3>();
    var angleStep = 360.0f / segments;

    for (var i = 0; i < segments; i++)
    {
      var angle = (i * angleStep).DegToRad();
      var point = new Vector3(MathF.Cos(angle) * radius, 0, MathF.Sin(angle) * radius);
      point = rotation * point; // Применяем поворот к точке
      point += baseCenter; // Смещаем к центру основания
      basePoints.Add(point);
    }

    // Рисуем основание
    for (var i = 0; i < segments; i++)
    {
      var p1 = basePoints[i];
      var p2 = basePoints[(i + 1) % segments];
      DrawLine(p1, p2, color, 1.0f); // Соединяем точки основания
    }

    // Рисуем боковые стороны конуса
    foreach (var point in basePoints)
    {
      DrawLine(apex, point, color, 1.0f); // Соединяем верхнюю точку с основанием
    }
  }

  public void DrawPlane(Plane plane, float size, RGBA32F color)
  {
    var normal = plane.Normal.Normalized;

    // Определяем векторы, перпендикулярные нормали
    Vector3 right;
    if (Math.Abs(normal.Y) < 0.9999f) // Проверяем, не направлен ли вектор вверх/вниз
    {
      right = Vector3.Cross(normal, Vector3.Up).Normalized; // Если нет, используем "вверх"
    }
    else
    {
      right = Vector3.Cross(normal, Vector3.Right).Normalized; // Иначе используем "вправо"
    }

    var up = Vector3.Cross(normal, right).Normalized; // Вычисляем второй вектор

    var halfSize = size / 2;

    // Расчет вершин квадрата
    var p1 = plane.D * normal + right * halfSize + up * halfSize;
    var p2 = plane.D * normal + right * halfSize - up * halfSize;
    var p3 = plane.D * normal - right * halfSize - up * halfSize;
    var p4 = plane.D * normal - right * halfSize + up * halfSize;

    // Рисуем квадрат
    DrawLine(p1, p2, color, 1.0f);
    DrawLine(p2, p3, color, 1.0f);
    DrawLine(p3, p4, color, 1.0f);
    DrawLine(p4, p1, color, 1.0f);

    // Рисуем нормаль
    var normalEnd = plane.D * normal + normal;
    DrawLine(plane.D * normal, normalEnd, color, 1.0f);

    // Рисуем конус на конце нормали
    DrawCone(normalEnd, normal, 0.1f, 0.05f, 12, color);
  }

  public void DrawPlane(Transform t, float size, RGBA32F color)
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

  /*public void Draw(SphereCollider sphere, RGBA32F color)
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
  */

  public void DrawRectangle(Rectangle r, RGBA32F color)
  {
    DrawRectangle(new Vector3(r.FromX, r.FromY, 0), new Vector3(r.ToX, r.ToX, 0), color);
  }

  public void DrawRectangle(Vector3 lt, Vector3 rb, RGBA32F color)
  {
    Add(new RO_Line(lt.X, rb.Y, lt.Z, lt.X, lt.Y, lt.Z, color, color));
    Add(new RO_Line(lt.X, lt.Y, lt.Z, rb.X, lt.Y, lt.Z, color, color));
    Add(new RO_Line(rb.X, lt.Y, lt.Z, rb.X, rb.Y, lt.Z, color, color));
    Add(new RO_Line(rb.X, rb.Y, lt.Z, lt.X, rb.Y, lt.Z, color, color));
  }

  public void DrawAABB(Vector3 center, Vector3 size, RGBA32F color)
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

  public void DrawAABB(AABB aabb, RGBA32F color)
  {
    DrawAABB(aabb.Center, aabb.Size, color);
  }

  public void DrawLine(Vector3 from, Vector3 to, RGBA32F color)
  {
    Add(new RO_Line(from, to, color));
  }

  public void DrawLine(Vector3 from, Vector3 to, RGBA32F fromColor, RGBA32F toColor)
  {
    Add(new RO_Line(from, to, fromColor, toColor));
  }

  public void DrawLine(Vector3 from, Vector3 to, RGBA32F color, float width)
  {
    var l = new RO_Line(from, to, color)
    {
      Width = width
    };
    Add(l);
  }

  public void DrawFrustum(Frustum frustum, RGBA32F color)
  {
    // Отрисовывал плейны чтобы разобраться правильно ли работает
    /*DrawPlane(frustum.TopPlane, 3f, new RGBA32F(0, 1, 0, 1));
    DrawPlane(frustum.BottomPlane, 3f, new RGBA32F(0, 1, 0, 1));
    DrawPlane(frustum.NearPlane, 3f, new RGBA32F(0, 0, 1, 1));
    DrawPlane(frustum.FarPlane, 3f, new RGBA32F(0, 0, 1, 1));
    DrawPlane(frustum.RightPlane, 3f, new RGBA32F(1, 0, 0, 1));
    DrawPlane(frustum.LeftPlane, 3f, new RGBA32F(1, 0, 0, 1));*/

    var ntl = frustum.NearTopLeft;
    var ftl = frustum.FarTopLeft;
    var ntr = frustum.NearTopRight;
    var ftr = frustum.FarTopRight;
    var nbl = frustum.NearBottomLeft;
    var fbl = frustum.FarBottomLeft;
    var nbr = frustum.NearBottomRight;
    var fbr = frustum.FarBottomRight;

    DrawLine(ntl, ftl, color * 0.5f, color);
    DrawLine(ntr, ftr, color * 0.5f, color);
    DrawLine(nbl, fbl, color * 0.5f, color);
    DrawLine(nbr, fbr, color * 0.5f, color);

    DrawLine(ntl, ntr, color * 0.5f, color * 0.5f);
    DrawLine(nbl, nbr, color * 0.5f, color * 0.5f);
    DrawLine(ntl, nbl, color * 0.5f, color * 0.5f);
    DrawLine(ntr, nbr, color * 0.5f, color * 0.5f);

    DrawLine(ftl, ftr, color, color);
    DrawLine(fbl, fbr, color, color);
    DrawLine(ftl, fbl, color, color);
    DrawLine(ftr, fbr, color, color);
  }

  public void DrawRing(
    Vector3 center, Vector3 axis,
    Quaternion transformRotation,
    float radius,
    RGBA32F color,
    int segments = 64,
    float width = 1.0f)
  {
    // Нормализуем ось вращения
    axis = axis.Normalized;

    // Выбираем вектор, перпендикулярный оси вращения
    Vector3 perpendicularVector;
    if (Math.Abs(Vector3.Dot(axis, Vector3.UnitY)) > 0.99f)
    {
      perpendicularVector = Vector3.Cross(axis, Vector3.UnitX).Normalized;
    }
    else
    {
      perpendicularVector = Vector3.Cross(axis, Vector3.UnitY).Normalized;
    }

    // Множитель для вращения каждой точки
    var angleStep = 2 * (float)Math.PI / segments;

    // Применяем начальную трансформацию к первой точке
    var prevPoint = center + transformRotation * (perpendicularVector * radius);

    // Проходим по каждой точке окружности
    for (var i = 1; i <= segments; i++)
    {
      // Вычисляем угол для текущей точки
      var angle = i * angleStep;

      // Вычисляем вращение вокруг оси
      var rotation = Quaternion.FromAxisAngle(axis, angle);

      // Вращаем начальный вектор на текущий угол с учетом трансформации
      var currentPoint = center + transformRotation * (rotation * (perpendicularVector * radius));

      // Рисуем линию между предыдущей и текущей точками
      DrawLine(prevPoint, currentPoint, color, width);

      // Обновляем предыдущую точку
      prevPoint = currentPoint;
    }
  }

  public void DrawBone(Bone bone, RGBA32F color, bool drawColliders = false)
  {
    DrawBone(bone.Matrix, bone.Length, color);

    if (drawColliders)
    {
      foreach (var collider in bone.Colliders)
      {
        if (collider is BoxCollider boxCollider)
        {
          var mx = collider.Transform.Matrix * bone.Matrix;
          DrawBox(mx, boxCollider.Size, color);
        }

        if (collider is CapsuleCollider capsuleCollider)
        {
          var mx = collider.Transform.Matrix * bone.Matrix;
          DrawCapsule(mx, capsuleCollider.Radius, capsuleCollider.Height, color);
        }

        if (collider is SphereCollider sphereCollider)
        {
          var mx = collider.Transform.Matrix * bone.Matrix;
          DrawSphere(mx, sphereCollider.Radius, color);
        }
      }
    }
  }

  public void DrawBone(Matrix4x4 transformMatrix, float length, RGBA32F color)
  {
    /*// Рассчитываем длину и базовый размер октаэдра
    var direction = Vector3.Forward * length; // Локальное направление кости теперь вперед по оси Z
    var baseSize = length * 0.1f; // Размер основания октаэдра

    // Локальные координаты точек
    var start = Vector3.Zero; // Начало кости (локально)
    var end = direction; // Конец кости (локально)
    var midPoint = (start + end) / 4; // Средняя точка для построения основания

    // Рассчитываем локальные оси для октаэдра
    var up = Vector3.Up * baseSize; // Вверх по оси Y (для определения верхней и нижней границы)
    var right = Vector3.Right * baseSize; // Вправо по оси X

    // Вершины октаэдра в локальных координатах
    var topBase = midPoint + up;
    var bottomBase = midPoint - up;
    var leftBase = midPoint - right;
    var rightBase = midPoint + right;

    // Преобразуем все точки в мировые координаты
    start = Vector3.Transform(start, transformMatrix);
    end = Vector3.Transform(end, transformMatrix);
    topBase = Vector3.Transform(topBase, transformMatrix);
    bottomBase = Vector3.Transform(bottomBase, transformMatrix);
    leftBase = Vector3.Transform(leftBase, transformMatrix);
    rightBase = Vector3.Transform(rightBase, transformMatrix);

    // Рисуем линии от начала кости к вершинам основания
    DrawLine(start, topBase, color); // Линия от начала к topBase
    DrawLine(start, bottomBase, color); // Линия от начала к bottomBase
    DrawLine(start, leftBase, color); // Линия от начала к leftBase
    DrawLine(start, rightBase, color); // Линия от начала к rightBase

    // Рисуем линии от конца кости к вершинам основания
    DrawLine(end, topBase, color); // Линия от конца к topBase
    DrawLine(end, bottomBase, color); // Линия от конца к bottomBase
    DrawLine(end, leftBase, color); // Линия от конца к leftBase
    DrawLine(end, rightBase, color); // Линия от конца к rightBase

    // Соединяем вершины основания между собой
    DrawLine(topBase, rightBase, color);
    DrawLine(rightBase, bottomBase, color);
    DrawLine(bottomBase, leftBase, color);
    DrawLine(leftBase, topBase, color);*/

    // Рассчитываем длину и базовый размер октаэдра
    var direction = Vector3.Up * length; // Локальное направление кости теперь вверх по оси Y
    var baseSize = length * 0.1f; // Размер основания октаэдра

    // Локальные координаты точек
    var start = Vector3.Zero; // Начало кости (локально)
    var end = direction; // Конец кости (локально)
    var midPoint = (start + end) / 4; // Средняя точка для построения основания

    // Рассчитываем локальные оси для октаэдра
    var forward = Vector3.Forward * baseSize;
    var right = Vector3.Right * baseSize;

    // Вершины октаэдра в локальных координатах
    var topBase = midPoint + forward;
    var bottomBase = midPoint - forward;
    var leftBase = midPoint - right;
    var rightBase = midPoint + right;

    // Преобразуем все точки в мировые координаты
    start = Vector3.Transform(start, transformMatrix);
    end = Vector3.Transform(end, transformMatrix);
    topBase = Vector3.Transform(topBase, transformMatrix);
    bottomBase = Vector3.Transform(bottomBase, transformMatrix);
    leftBase = Vector3.Transform(leftBase, transformMatrix);
    rightBase = Vector3.Transform(rightBase, transformMatrix);

    // Рисуем линии от начала кости к вершинам основания
    DrawLine(start, topBase, color); // Линия от начала к topBase
    DrawLine(start, bottomBase, color); // Линия от начала к bottomBase
    DrawLine(start, leftBase, color); // Линия от начала к leftBase
    DrawLine(start, rightBase, color); // Линия от начала к rightBase

    // Рисуем линии от конца кости к вершинам основания
    DrawLine(end, topBase, color); // Линия от конца к topBase
    DrawLine(end, bottomBase, color); // Линия от конца к bottomBase
    DrawLine(end, leftBase, color); // Линия от конца к leftBase
    DrawLine(end, rightBase, color); // Линия от конца к rightBase

    // Соединяем вершины основания между собой
    DrawLine(topBase, rightBase, color);
    DrawLine(rightBase, bottomBase, color);
    DrawLine(bottomBase, leftBase, color);
    DrawLine(leftBase, topBase, color);
  }

  public void DrawGrid(int gridSize, float cellSize, RGBA32F mainColor, int subdivisions, RGBA32F subColor)
  {
    // Грид рисуется в плоскости XZ (ось Y вертикальная)
    for (float i = -gridSize; i < gridSize; i += cellSize)
    {
      // Линии параллельные оси X (горизонтальные)
      var p1 = new Vector3(i, 0, -gridSize);
      var p2 = new Vector3(i, 0, gridSize);
      Add(new RO_Line(p1, p2, mainColor));

      // Линии параллельные оси Z (вертикальные)
      var p3 = new Vector3(-gridSize, 0, i);
      var p4 = new Vector3(gridSize, 0, i);
      Add(new RO_Line(p3, p4, mainColor));

      // Если есть деления внутри клеток
      if (subdivisions > 0)
      {
        var subCellSize = cellSize / (subdivisions + 1); // Размер каждой подячейки

        for (var j = 1; j <= subdivisions; j++)
        {
          // Рассчитываем смещение для делений
          var subOffset = j * subCellSize;

          // Проверяем, что деления не выходят за границу сетки
          if (i + subOffset < gridSize)
          {
            // Горизонтальные деления внутри клетки (параллельные X)
            var subP1 = new Vector3(i + subOffset, 0, -gridSize);
            var subP2 = new Vector3(i + subOffset, 0, gridSize);
            Add(new RO_Line(subP1, subP2, subColor));
          }

          if (i + subOffset < gridSize)
          {
            // Вертикальные деления внутри клетки (параллельные Z)
            var subP3 = new Vector3(-gridSize, 0, i + subOffset);
            var subP4 = new Vector3(gridSize, 0, i + subOffset);
            Add(new RO_Line(subP3, subP4, subColor));
          }
        }
      }
    }

    // Добавляем правую и верхнюю границы для основных линий (так как цикл идёт до < gridSize)
    var rightBorderP1 = new Vector3(gridSize, 0, -gridSize);
    var rightBorderP2 = new Vector3(gridSize, 0, gridSize);
    Add(new RO_Line(rightBorderP1, rightBorderP2, mainColor));

    var topBorderP1 = new Vector3(-gridSize, 0, gridSize);
    var topBorderP2 = new Vector3(gridSize, 0, gridSize);
    Add(new RO_Line(topBorderP1, topBorderP2, mainColor));
  }

  public void DrawAxis(Vector3 center)
  {
    DrawLine(center + Vector3.Down * 4, center + Vector3.Up * 4, new RGBA32F(0, 1, 0, 1));
    DrawLine(center + Vector3.Left * 4, center + Vector3.Right * 4, new RGBA32F(1, 0, 0, 1));
    DrawLine(center + Vector3.Backward * 4, center + Vector3.Forward * 4, new RGBA32F(0.2f, 0.5f, 1, 1));
  }

  public void DrawTranslateManipulator(Vector3 center)
  {
    Add(new RO_Line(center, center + Vector3.Up * 0.5f, new RGBA32F(0, 1, 0, 1), 3.0f));
    Add(new RO_Line(center, center + Vector3.Right * 0.5f, new RGBA32F(1, 0, 0, 1), 3.0f));
    Add(new RO_Line(center, center + Vector3.Forward * 0.5f, new RGBA32F(0.2f, 0.5f, 1, 1), 3.0f));
  }
}