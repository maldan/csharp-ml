using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Rectangle
{
  public float FromX;
  public float FromY;
  public float ToX;
  public float ToY;

  public float Width => ToX - FromX;
  public float Height => ToY - FromY;

  public bool IsEmpty => FromX == 0 && FromY == 0 && ToX == 0 && ToY == 0;

  public Rectangle(float x1, float y1, float x2, float y2)
  {
    FromX = x1;
    FromY = y1;
    ToX = x2;
    ToY = y2;
  }

  // Add
  public static Rectangle operator +(Rectangle a, Vector2 b)
  {
    return new Rectangle
    {
      FromX = a.FromX + b.X,
      FromY = a.FromY + b.Y,
      ToX = a.ToX + b.X,
      ToY = a.ToY + b.Y
    };
  }

  public bool IsCollide(Rectangle other)
  {
    return Math.Min(FromX, ToX) < Math.Max(other.FromX, other.ToX) &&
           Math.Max(FromX, ToX) > Math.Min(other.FromX, other.ToX) &&
           Math.Min(FromY, ToY) < Math.Max(other.FromY, other.ToY) &&
           Math.Max(FromY, ToY) > Math.Min(other.FromY, other.ToY);
  }

  // Проверка, находится ли точка внутри прямоугольника
  public bool IsPointInside(float x, float y)
  {
    return x >= FromX && x <= ToX && y >= FromY && y <= ToY;
  }

  public bool IsPointInside(Vector2 point)
  {
    return IsPointInside(point.X, point.Y);
  }

  // Проверка, находится ли прямоугольник внутри другого прямоугольника или пересекается с ним
  public bool IsInsideOrIntersects(Rectangle other)
  {
    // Проверка, находятся ли углы одного прямоугольника внутри другого
    return IsPointInside(other.FromX, other.FromY) ||
           IsPointInside(other.FromX, other.ToY) ||
           IsPointInside(other.ToX, other.FromY) ||
           IsPointInside(other.ToX, other.ToY) ||
           other.IsPointInside(FromX, FromY) ||
           other.IsPointInside(FromX, ToY) ||
           other.IsPointInside(ToX, FromY) ||
           other.IsPointInside(ToX, ToY);
  }

  public Rectangle ToUV(float maxWidth, float maxHeight)
  {
    var uMin = FromX / maxWidth;
    var vMin = FromY / maxHeight;
    var uMax = ToX / maxWidth;
    var vMax = ToY / maxHeight;

    return new Rectangle(uMin, vMin, uMax, vMax);
  }

  public static bool operator ==(Rectangle r1, Rectangle r2)
  {
    return Math.Abs(r1.FromX - r2.FromX) < 0.0001f &&
           Math.Abs(r1.FromY - r2.FromY) < 0.0001f &&
           Math.Abs(r1.ToX - r2.ToX) < 0.0001f &&
           Math.Abs(r1.ToY - r2.ToY) < 0.0001f;
  }

  public static bool operator !=(Rectangle r1, Rectangle r2)
  {
    return !(r1 == r2);
  }

  public override string ToString()
  {
    return $"Rectangle(FromX: {FromX}, FromY: {FromY}, ToX: {ToX}, ToY: {ToY})";
  }

  public static Rectangle Zero => new(0, 0, 0, 0);

  public static Rectangle FromLeftTopWidthHeight(float x, float y, float width, float height)
  {
    return new Rectangle(x, y, x + width, y + height);
  }
}