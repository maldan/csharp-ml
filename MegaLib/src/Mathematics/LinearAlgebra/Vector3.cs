using System;
using System.Runtime.InteropServices;

namespace MegaLib.Mathematics.LinearAlgebra;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector3
{
  public float X;
  public float Y;
  public float Z;

  public Vector3(float x, float y, float z)
  {
    X = x;
    Y = y;
    Z = z;
  }

  #region Properties

  public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
  public Vector3 Inverted => new(-X, -Y, -Z);
  public Vector3 ToDegrees => new(X.RadToDeg(), Y.RadToDeg(), Z.RadToDeg());
  public Vector3 ToRadians => new(X.DegToRad(), Y.DegToRad(), Z.DegToRad());

  public Vector3 Normalized
  {
    get
    {
      var l = Length;
      return l == 0 ? new Vector3() : new Vector3(X / l, Y / l, Z / l);
    }
  }

  #endregion

  #region Static

  public static float Dot(Vector3 v1, Vector3 v2)
  {
    return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
  }

  public static Vector3 Cross(Vector3 v1, Vector3 v2)
  {
    var v = new Vector3
    {
      X = v1.Y * v2.Z - v1.Z * v2.Y,
      Y = v1.Z * v2.X - v1.X * v2.Z,
      Z = v1.X * v2.Y - v1.Y * v2.X
    };
    return v;
  }

  public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
  {
    return new Vector3
    {
      X = MathEx.Lerp(from.X, to.X, t),
      Y = MathEx.Lerp(from.Y, to.Y, t),
      Z = MathEx.Lerp(from.Z, to.Z, t)
    };
  }

  public static float Distance(Vector3 from, Vector3 to)
  {
    var a = from.X - to.X;
    var b = from.Y - to.Y;
    var c = from.Z - to.Z;

    return (float)Math.Sqrt(a * a + b * b + c * c);
  }

  public static Vector3 Direction(Vector3 from, Vector3 to)
  {
    var dir = to - from;
    return dir.Normalized;
  }

  #endregion

  #region Operators

  // Add
  public static Vector3 operator +(Vector3 a, Vector3 b)
  {
    return new Vector3
    {
      X = a.X + b.X,
      Y = a.Y + b.Y,
      Z = a.Z + b.Z
    };
  }

  // Sub
  public static Vector3 operator -(Vector3 a, Vector3 b)
  {
    return new Vector3
    {
      X = a.X - b.X,
      Y = a.Y - b.Y,
      Z = a.Z - b.Z
    };
  }

  // Scale
  public static Vector3 operator *(Vector3 a, float s)
  {
    return new Vector3
    {
      X = a.X * s,
      Y = a.Y * s,
      Z = a.Z * s
    };
  }

  public static Vector3 operator *(Vector3 a, Vector3 b)
  {
    return new Vector3
    {
      X = a.X * b.X,
      Y = a.Y * b.Y,
      Z = a.Z * b.Z
    };
  }

  public static Vector3 operator *(Vector3 vector, Matrix4x4 matrix)
  {
    Vector3 result;
    result.X = matrix.M00 * vector.X + matrix.M10 * vector.Y + matrix.M20 * vector.Z + matrix.M30;
    result.Y = matrix.M01 * vector.X + matrix.M11 * vector.Y + matrix.M21 * vector.Z + matrix.M31;
    result.Z = matrix.M02 * vector.X + matrix.M12 * vector.Y + matrix.M22 * vector.Z + matrix.M32;
    return result;
  }

  #endregion

  #region Set

  // Already set
  public static Vector3 One => new(1, 1, 1);
  public static Vector3 Zero => new(0, 0, 0);
  public static Vector3 Left => new(-1, 0, 0);
  public static Vector3 Right => new(1, 0, 0);
  public static Vector3 Up => new(0, 1, 0);
  public static Vector3 Down => new(0, -1, 0);
  public static Vector3 Forward => new(0, 0, 1);
  public static Vector3 Backward => new(0, 0, -1);

  #endregion

  public Vector3 Clone()
  {
    return new Vector3 { X = X, Y = Y, Z = Z };
  }

  /*public Vector4 ToVector4(float w)
  {
    return new Vector4(X, Y, Z, w);
  }*/

  public Vector4 AddW(float w)
  {
    return new Vector4(X, Y, Z, w);
  }

  public override string ToString()
  {
    return $"Vector3({X}, {Y}, {Z})";
  }
}