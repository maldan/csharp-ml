using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Physics;

public class DistanceConstraint
{
  public VerletPoint PointA { get; private set; } // Первая точка
  public VerletPoint PointB { get; private set; } // Вторая точка
  public float Length { get; private set; } // Длина связи (расстояние между точками)

  // Конструктор, принимает две точки и длину. Если длина 0, то вычисляется исходная длина.
  public DistanceConstraint(VerletPoint pointA, VerletPoint pointB, float length = 0)
  {
    PointA = pointA;
    PointB = pointB;

    // Если длина задана как 0, вычисляем расстояние между точками
    Length = length == 0 ? (PointA.Position - PointB.Position).Length : length;
  }

  // Метод для исправления позиций точек, чтобы сохранять длину связи
  public void Tick()
  {
    // Вектор между двумя точками
    var delta = PointB.Position - PointA.Position;

    // Текущее расстояние между точками
    var currentDistance = delta.Length;

    // Насколько расстояние отличается от требуемой длины
    var difference = (currentDistance - Length) / currentDistance;

    // Проверка на статичность
    if (PointA.IsStatic && PointB.IsStatic) return; // Если обе точки статичны, ничего не делаем

    // Масса точек
    var totalMass = PointA.Mass + PointB.Mass;

    // Сдвигаем точки с учетом их статичности
    if (!PointA.IsStatic)
    {
      var ratioA = PointB.Mass / totalMass;
      PointA.Position += delta * ratioA * difference;
    }

    if (!PointB.IsStatic)
    {
      var ratioB = PointA.Mass / totalMass;
      PointB.Position -= delta * ratioB * difference;
    }
  }
}