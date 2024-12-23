using System;
using System.Runtime.InteropServices;

namespace MegaLib.Mathematics.LinearAlgebra;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IVector3
{
  public int X;
  public int Y;
  public int Z;

  // Конструктор для инициализации вектора
  public IVector3(int x, int y, int z)
  {
    X = x;
    Y = y;
    Z = z;
  }

  public static IVector3 operator *(IVector3 a, IVector3 b)
  {
    return new IVector3
    {
      X = a.X * b.X,
      Y = a.Y * b.Y,
      Z = a.Z * b.Z
    };
  }

  public static IVector3 operator *(IVector3 a, int s)
  {
    return new IVector3
    {
      X = a.X * s,
      Y = a.Y * s,
      Z = a.Z * s
    };
  }

  public static IVector3 operator /(IVector3 a, IVector3 b)
  {
    return new IVector3
    {
      X = a.X / b.X,
      Y = a.Y / b.Y,
      Z = a.Z / b.Z
    };
  }

  public static IVector3 operator /(IVector3 a, int b)
  {
    return new IVector3
    {
      X = a.X / b,
      Y = a.Y / b,
      Z = a.Z / b
    };
  }

  public static IVector3 operator +(IVector3 a, IVector3 b)
  {
    return new IVector3
    {
      X = a.X + b.X,
      Y = a.Y + b.Y,
      Z = a.Z + b.Z
    };
  }

  public static IVector3 operator -(IVector3 a, IVector3 b)
  {
    return new IVector3
    {
      X = a.X - b.X,
      Y = a.Y - b.Y,
      Z = a.Z - b.Z
    };
  }

  public static IVector3 operator ++(IVector3 a)
  {
    return new IVector3
    {
      X = a.X + 1,
      Y = a.Y + 1,
      Z = a.Z + 1
    };
  }

  public static IVector3 operator --(IVector3 a)
  {
    return new IVector3
    {
      X = a.X - 1,
      Y = a.Y - 1,
      Z = a.Z - 1
    };
  }

  public static IVector3 operator +(IVector3 a) // Унарный оператор +
  {
    return new IVector3
    {
      X = a.X,
      Y = a.Y,
      Z = a.Z
    };
  }

  public static bool operator ==(IVector3 a, IVector3 b)
  {
    return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
  }

  public static bool operator !=(IVector3 a, IVector3 b)
  {
    return !(a == b);
  }

  public static IVector3 operator -(IVector3 a)
  {
    return new IVector3
    {
      X = -a.X,
      Y = -a.Y,
      Z = -a.Z
    };
  }

  public bool Equals(IVector3 other)
  {
    return X == other.X && Y == other.Y && Z == other.Z;
  }

  public override bool Equals(object obj)
  {
    return obj is IVector3 other && Equals(other);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(X, Y, Z);
  }

  public override string ToString()
  {
    return $"IVector3({X}, {Y}, {Z})";
  }

  public static explicit operator IVector3(Vector3 v)
  {
    return new IVector3((int)v.X, (int)v.Y, (int)v.Z);
  }
}