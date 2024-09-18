using System.Runtime.InteropServices;

namespace MegaLib.Mathematics.LinearAlgebra;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IVector2
{
  public int X;
  public int Y;

  public IVector2(int x, int y)
  {
    X = x;
    Y = y;
  }

  public static IVector2 operator *(IVector2 a, IVector2 b)
  {
    return new IVector2
    {
      X = a.X * b.X,
      Y = a.Y * b.Y
    };
  }

  public static IVector2 operator *(IVector2 a, int s)
  {
    return new IVector2
    {
      X = a.X * s,
      Y = a.Y * s
    };
  }

  public static IVector2 operator /(IVector2 a, IVector2 b)
  {
    return new IVector2
    {
      X = a.X / b.X,
      Y = a.Y / b.Y
    };
  }

  public static IVector2 operator /(IVector2 a, int b)
  {
    return new IVector2
    {
      X = a.X / b,
      Y = a.Y / b
    };
  }

  public static IVector2 operator +(IVector2 a, IVector2 b)
  {
    return new IVector2
    {
      X = a.X + b.X,
      Y = a.Y + b.Y
    };
  }

  public static IVector2 operator -(IVector2 a, IVector2 b)
  {
    return new IVector2
    {
      X = a.X - b.X,
      Y = a.Y - b.Y
    };
  }

  public static IVector2 operator ++(IVector2 a)
  {
    return new IVector2
    {
      X = a.X + 1,
      Y = a.Y + 1
    };
  }

  public static IVector2 operator --(IVector2 a)
  {
    return new IVector2
    {
      X = a.X - 1,
      Y = a.Y - 1
    };
  }

  public static bool operator ==(IVector2 a, IVector2 b)
  {
    return a.X == b.X && a.Y == b.Y;
  }

  public static bool operator !=(IVector2 a, IVector2 b)
  {
    return !(a == b);
  }

  public static IVector2 operator -(IVector2 a)
  {
    return new IVector2
    {
      X = -a.X,
      Y = -a.Y
    };
  }

  public override string ToString()
  {
    return $"IVector2({X}, {Y})";
  }
}