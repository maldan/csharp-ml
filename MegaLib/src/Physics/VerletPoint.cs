using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Physics;

public class VerletPoint
{
  public Vector3 Position { get; set; } // Текущая позиция
  public Vector3 PreviousPosition { get; set; } // Предыдущая позиция
  public Vector3 Acceleration { get; set; } // Сила (ускорение, действующая на точку)
  public float Mass { get; set; } // Масса точки
  public float Damping { get; set; } = 0.99f; // Коэффициент демпфирования для затухания
  public bool IsStatic;
  public RGBA32F Color;

  public VerletPoint(Vector3 position, float mass)
  {
    Position = position;
    PreviousPosition = position;
    Acceleration = Vector3.Zero;
    Mass = mass;
  }

  // Добавить силу к точке (ускорение = сила / масса)
  public void ApplyForce(Vector3 force)
  {
    Acceleration += force / Mass;
  }

  // Основной метод для обновления позиции точки по методу Верле
  public void Tick(float deltaTime)
  {
    if (IsStatic) return;

    // Вычисление изменения позиции по методу Верле
    var velocity = (Position - PreviousPosition) * Damping;
    var nextPosition = Position + velocity + Acceleration * deltaTime * deltaTime;

    // Обновляем предыдущую позицию на текущую
    PreviousPosition = Position;

    // Обновляем текущую позицию
    Position = nextPosition;

    // Обнуляем ускорение после применения
    Acceleration = Vector3.Zero;
  }
}

public class VerletLine
{
  public List<VerletPoint> Points { get; private set; } // Список всех точек линии
  public List<DistanceConstraint> Constraints { get; private set; } // Список всех ограничений по расстоянию

  public VerletPoint Start => Points[0];

  // Конструктор, который создаёт линию между двумя точками с заданным количеством сегментов
  public VerletLine(Vector3 from, Vector3 to, int segmentCount, float mass = 1.0f, bool fixedStart = false,
    bool fixedEnd = false)
  {
    Points = new List<VerletPoint>();
    Constraints = new List<DistanceConstraint>();

    // Вычисляем длину одного сегмента
    var segmentVector = (to - from) / segmentCount;

    // Создаём верле-точки
    for (var i = 0; i <= segmentCount; i++)
    {
      var position = from + segmentVector * i;
      var isStatic = (i == 0 && fixedStart) || (i == segmentCount && fixedEnd);
      var vp = new VerletPoint(position, mass)
      {
        IsStatic = isStatic
      };
      Points.Add(vp);
    }

    // Добавляем констрейнты между соседними точками
    for (var i = 0; i < Points.Count - 1; i++)
    {
      Constraints.Add(new DistanceConstraint(Points[i], Points[i + 1]));
    }
  }

  // Добавить силу к точке (ускорение = сила / масса)
  public void ApplyForce(Vector3 force)
  {
    foreach (var point in Points) point.ApplyForce(force);
  }

  // Метод для обновления всех точек (физическое обновление)
  public void Tick(float deltaTime)
  {
    foreach (var point in Points)
    {
      point.Tick(deltaTime); // Обновляем каждую точку
    }

    foreach (var constraint in Constraints)
    {
      constraint.Tick(); // Применяем все констрейнты
    }
  }
}