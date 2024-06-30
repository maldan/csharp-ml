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

  public static Rectangle FromLeftTopWidthHeight()
  {
    return new Rectangle();
  }
}