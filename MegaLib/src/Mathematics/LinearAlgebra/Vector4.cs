using System;
using System.Runtime.InteropServices;

namespace MegaLib.Mathematics.LinearAlgebra;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector4Int
{
  public int X;
  public int Y;
  public int Z;
  public int W;

  public Vector4Int(int x, int y, int z, int w)
  {
    X = x;
    Y = y;
    Z = z;
    W = w;
  }

  public uint UInt32LE => (uint)(X | (Y << 8) | (Z << 16) | (W << 24));
  public uint UInt32BE => (uint)(W | (Z << 8) | (Y << 16) | (X << 24));

  public override string ToString()
  {
    return $"Vector4Int({X}, {Y}, {Z}, {W})";
  }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector4
{
  public float X;
  public float Y;
  public float Z;
  public float W;

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

  public float B
  {
    get => Z;
    set => Z = value;
  }

  public float A
  {
    get => W;
    set => W = value;
  }

  public float LengthSquared => X * X + Y * Y + Z * Z + W * W;
  public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);

  public Vector4(Vector3 v, float w)
  {
    X = v.X;
    Y = v.Y;
    Z = v.Z;
    W = w;
  }

  public Vector4(float x, float y, float z, float w)
  {
    X = x;
    Y = y;
    Z = z;
    W = w;
  }

  public float this[int index]
  {
    get
    {
      return index switch
      {
        0 => X,
        1 => Y,
        2 => Z,
        3 => W,
        _ => 0
      };
    }
    set
    {
      switch (index)
      {
        case 0:
          X = value;
          break;
        case 1:
          Y = value;
          break;
        case 2:
          Z = value;
          break;
        case 3:
          W = value;
          break;
      }
    }
  }

  public Vector3 XYZ => new(X, Y, Z);

  public Vector3 DropW()
  {
    return new Vector3(X, Y, Z);
  }

  public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix)
  {
    return new Vector4(
      vector.X * matrix.M00 + vector.Y * matrix.M10 + vector.Z * matrix.M20 + vector.W * matrix.M30,
      vector.X * matrix.M01 + vector.Y * matrix.M11 + vector.Z * matrix.M21 + vector.W * matrix.M31,
      vector.X * matrix.M02 + vector.Y * matrix.M12 + vector.Z * matrix.M22 + vector.W * matrix.M32,
      vector.X * matrix.M03 + vector.Y * matrix.M13 + vector.Z * matrix.M23 + vector.W * matrix.M33
    );
  }

  public Quaternion ToQuaternion()
  {
    return new Quaternion(X, Y, Z, W);
  }

  public static Vector4 operator *(Vector4 vector, Matrix4x4 matrix)
  {
    Vector4 result;
    result.X = matrix.M00 * vector.X + matrix.M10 * vector.Y + matrix.M20 * vector.Z + matrix.M30 * vector.W;
    result.Y = matrix.M01 * vector.X + matrix.M11 * vector.Y + matrix.M21 * vector.Z + matrix.M31 * vector.W;
    result.Z = matrix.M02 * vector.X + matrix.M12 * vector.Y + matrix.M22 * vector.Z + matrix.M32 * vector.W;
    result.W = matrix.M03 * vector.X + matrix.M13 * vector.Y + matrix.M23 * vector.Z + matrix.M33 * vector.W;
    return result;
  }

  public static Vector4 operator *(float s, Vector4 a)
  {
    return a * s;
  }

  public static Vector4 operator *(Vector4 a, float s)
  {
    return new Vector4
    {
      X = a.X * s,
      Y = a.Y * s,
      Z = a.Z * s,
      W = a.W * s
    };
  }

  public static Vector4 operator /(Vector4 a, float s)
  {
    return new Vector4
    {
      X = a.X / s,
      Y = a.Y / s,
      Z = a.Z / s,
      W = a.W / s
    };
  }

  public static Vector4 operator *(Vector4 a, Vector4 b)
  {
    return new Vector4
    {
      X = a.X * b.X,
      Y = a.Y * b.Y,
      Z = a.Z * b.Z,
      W = a.W * b.W
    };
  }

  public static bool operator ==(Vector4 v1, Vector4 v2)
  {
    return MathF.Abs(v1.X - v2.X) < float.Epsilon &&
           MathF.Abs(v1.Y - v2.Y) < float.Epsilon &&
           MathF.Abs(v1.Z - v2.Z) < float.Epsilon &&
           MathF.Abs(v1.W - v2.W) < float.Epsilon;
  }

  public static bool operator !=(Vector4 v1, Vector4 v2)
  {
    return !(v1 == v2);
  }

  #region Set

  public static Vector4 One => new(1, 1, 1, 1);
  public static Vector4 Zero => new(0, 0, 0, 0);

  #endregion

  public override string ToString()
  {
    return $"Vector4({X}, {Y}, {Z}, {W})";
  }
}