using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace MegaLib.Mathematics.LinearAlgebra;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector2
{
  [JsonInclude] [JsonPropertyName("x")] public float X;
  [JsonInclude] [JsonPropertyName("y")] public float Y;

  public Vector2(float x, float y)
  {
    X = x;
    Y = y;
  }

  public float R
  {
    get => X;
    set => X = value;
  }

  public float G
  {
    get => Y;
    set => Y = value;
  }

  public static float Distance(Vector2 from, Vector2 to)
  {
    var a = from.X - to.X;
    var b = from.Y - to.Y;

    return (float)Math.Sqrt(a * a + b * b);
  }

  public static Vector2 operator *(Vector2 a, Vector2 b)
  {
    return new Vector2
    {
      X = a.X * b.X,
      Y = a.Y * b.Y
    };
  }

  public static Vector2 operator *(Vector2 a, float s)
  {
    return new Vector2
    {
      X = a.X * s,
      Y = a.Y * s
    };
  }

  public static Vector2 operator /(Vector2 a, Vector2 b)
  {
    return new Vector2
    {
      X = a.X / b.X,
      Y = a.Y / b.Y
    };
  }

  public static Vector2 operator /(Vector2 a, float b)
  {
    return new Vector2
    {
      X = a.X / b,
      Y = a.Y / b
    };
  }

  public static Vector2 operator +(Vector2 a, Vector2 b)
  {
    return new Vector2
    {
      X = a.X + b.X,
      Y = a.Y + b.Y
    };
  }

  public static Vector2 operator +(Vector2 a, float b)
  {
    return new Vector2
    {
      X = a.X + b,
      Y = a.Y + b
    };
  }

  public static Vector2 operator -(Vector2 a, Vector2 b)
  {
    return new Vector2
    {
      X = a.X - b.X,
      Y = a.Y - b.Y
    };
  }

  public static Vector2 operator -(Vector2 a, float b)
  {
    return new Vector2
    {
      X = a.X - b,
      Y = a.Y - b
    };
  }

  public static Vector2 operator ++(Vector2 a)
  {
    return new Vector2
    {
      X = a.X + 1,
      Y = a.Y + 1
    };
  }

  public static Vector2 operator --(Vector2 a)
  {
    return new Vector2
    {
      X = a.X - 1,
      Y = a.Y - 1
    };
  }

  public static bool operator ==(Vector2 a, Vector2 b)
  {
    return a.X == b.X && a.Y == b.Y;
  }

  public static bool operator !=(Vector2 a, Vector2 b)
  {
    return !(a == b);
  }

  public static Vector2 operator -(Vector2 a)
  {
    return new Vector2
    {
      X = -a.X,
      Y = -a.Y
    };
  }

  public override string ToString()
  {
    return $"Vector2({X}, {Y})";
  }

  #region Set

  // Already set
  public static Vector2 One => new(1, 1);
  public static Vector2 Zero => new(0, 0);

  #endregion
}