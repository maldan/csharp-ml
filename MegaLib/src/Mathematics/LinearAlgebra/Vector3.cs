using System;
using System.Runtime.InteropServices;
using MegaLib.Ext;

namespace MegaLib.Mathematics.LinearAlgebra;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector3
{
  public float X;
  public float Y;
  public float Z;

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


  public Vector3(float v)
  {
    X = v;
    Y = v;
    Z = v;
  }

  public Vector3(float x, float y, float z)
  {
    X = x;
    Y = y;
    Z = z;
  }

  #region Properties

  public float LengthSquared => (float)(X * X + Y * Y + Z * Z);
  public float Length => (float)MathF.Sqrt(X * X + Y * Y + Z * Z);
  public Vector3 Inverted => new(-X, -Y, -Z);
  public Vector3 ToDegrees => new(X.RadToDeg(), Y.RadToDeg(), Z.RadToDeg());
  public Vector3 ToRadians => new(X.DegToRad(), Y.DegToRad(), Z.DegToRad());

  public Vector2 XY => new(X, Y);

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

  public static Vector3 Reflect(Vector3 incident, Vector3 normal)
  {
    var dot = incident.X * normal.X + incident.Y * normal.Y + incident.Z * normal.Z;
    return new Vector3
    {
      X = incident.X - 2 * dot * normal.X,
      Y = incident.Y - 2 * dot * normal.Y,
      Z = incident.Z - 2 * dot * normal.Z
    };
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

  public static Vector3 Normalize(Vector3 v)
  {
    return v.Normalized;
  }

  public static Vector3 Random(Random r, float x, float y, float z)
  {
    return new Vector3(r.RangeFloat(-x, x), r.RangeFloat(-y, y), r.RangeFloat(-z, z));
  }

  public static Vector3 TransformNormal(Vector3 vector, Matrix4x4 matrix)
  {
    Vector3 result;
    result.X = matrix.M00 * vector.X + matrix.M10 * vector.Y + matrix.M20 * vector.Z;
    result.Y = matrix.M01 * vector.X + matrix.M11 * vector.Y + matrix.M21 * vector.Z;
    result.Z = matrix.M02 * vector.X + matrix.M12 * vector.Y + matrix.M22 * vector.Z;
    return result;
  }

  public static Vector3 Transform(Vector3 vector, Matrix4x4 matrix)
  {
    Vector3 result;
    result.X = matrix.M00 * vector.X + matrix.M10 * vector.Y + matrix.M20 * vector.Z + matrix.M30;
    result.Y = matrix.M01 * vector.X + matrix.M11 * vector.Y + matrix.M21 * vector.Z + matrix.M31;
    result.Z = matrix.M02 * vector.X + matrix.M12 * vector.Y + matrix.M22 * vector.Z + matrix.M32;
    return result;
  }

  public static Vector3 Transform(Vector3 vector, Quaternion rotation)
  {
    // Нормализуем кватернион (на всякий случай)
    var q = rotation.Normalized;

    // Извлекаем компоненты кватерниона
    var x = q.X;
    var y = q.Y;
    var z = q.Z;
    var w = q.W;

    // Вычисляем квадрат компонентов
    var xx = x * x;
    var yy = y * y;
    var zz = z * z;
    var xy = x * y;
    var xz = x * z;
    var yz = y * z;
    var wx = w * x;
    var wy = w * y;
    var wz = w * z;

    // Вычисляем результат преобразования
    return new Vector3
    {
      X = (1 - 2 * (yy + zz)) * vector.X + 2 * (xy - wz) * vector.Y + 2 * (xz + wy) * vector.Z,
      Y = 2 * (xy + wz) * vector.X + (1 - 2 * (xx + zz)) * vector.Y + 2 * (yz - wx) * vector.Z,
      Z = 2 * (xz - wy) * vector.X + 2 * (yz + wx) * vector.Y + (1 - 2 * (xx + yy)) * vector.Z
    };
  }

  // Функция возведения вектора в степень
  public static Vector3 Pow(Vector3 baseVec, Vector3 exponentVec)
  {
    return new Vector3(
      (float)Math.Pow(baseVec.X, exponentVec.X),
      (float)Math.Pow(baseVec.Y, exponentVec.Y),
      (float)Math.Pow(baseVec.Z, exponentVec.Z)
    );
  }

  public static Vector3 Min(Vector3 v1, Vector3 v2)
  {
    return new Vector3
    {
      X = MathF.Min(v1.X, v2.X),
      Y = MathF.Min(v1.Y, v2.Y),
      Z = MathF.Min(v1.Z, v2.Z)
    };
  }

  public static Vector3 Max(Vector3 v1, Vector3 v2)
  {
    return new Vector3
    {
      X = MathF.Max(v1.X, v2.X),
      Y = MathF.Max(v1.Y, v2.Y),
      Z = MathF.Max(v1.Z, v2.Z)
    };
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

  public static Vector3 operator +(Vector3 a, Vector2 b)
  {
    return new Vector3
    {
      X = a.X + b.X,
      Y = a.Y + b.Y,
      Z = a.Z
    };
  }

  // Sub
  public static Vector3 operator -(float scalar, Vector3 vec)
  {
    return new Vector3(scalar - vec.X, scalar - vec.Y, scalar - vec.Z);
  }

  public static Vector3 operator -(Vector3 vec, float scalar)
  {
    return new Vector3(vec.X - scalar, vec.Y - scalar, vec.Z - scalar);
  }

  public static Vector3 operator -(Vector3 a, Vector3 b)
  {
    return new Vector3
    {
      X = a.X - b.X,
      Y = a.Y - b.Y,
      Z = a.Z - b.Z
    };
  }

  public static Vector3 operator -(Vector3 a, Vector2 b)
  {
    return new Vector3
    {
      X = a.X - b.X,
      Y = a.Y - b.Y,
      Z = a.Z
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

  public static Vector3 operator *(float s, Vector3 a)
  {
    return new Vector3
    {
      X = a.X * s,
      Y = a.Y * s,
      Z = a.Z * s
    };
  }

  public static Vector3 operator /(Vector3 a, float s)
  {
    return new Vector3
    {
      X = a.X / s,
      Y = a.Y / s,
      Z = a.Z / s
    };
  }

  // Оператор деления вектора на вектор
  public static Vector3 operator /(Vector3 vec1, Vector3 vec2)
  {
    return new Vector3(
      vec1.X / vec2.X,
      vec1.Y / vec2.Y,
      vec1.Z / vec2.Z
    );
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

  public static bool operator ==(Vector3 v1, Vector3 v2)
  {
    return Math.Abs(v1.X - v2.X) < 0.0001f && Math.Abs(v1.Y - v2.Y) < 0.0001f && Math.Abs(v1.Z - v2.Z) < 0.0001f;
  }

  public static bool operator !=(Vector3 v1, Vector3 v2)
  {
    return !(v1 == v2);
  }

  public static Vector3 operator -(Vector3 v)
  {
    return new Vector3(-v.X, -v.Y, -v.Z);
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

  public static Vector3 UnitX => new(1, 0, 0);
  public static Vector3 UnitY => new(0, 1, 0);
  public static Vector3 UnitZ => new(0, 0, 1);

  #endregion

  public Vector3 Clone()
  {
    return new Vector3 { X = X, Y = Y, Z = Z };
  }

  public Vector3 Floor()
  {
    return new Vector3(MathF.Floor(X), MathF.Floor(Y), MathF.Floor(Z));
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
      }
    }
  }

  public static float AngleBetween(Vector3 from, Vector3 to)
  {
    // Нормализуем векторы
    var normalizedFrom = from.Normalized;
    var normalizedTo = to.Normalized;

    // Скалярное произведение
    var dotProduct = Dot(normalizedFrom, normalizedTo);

    // Ограничиваем значение dotProduct в пределах [-1, 1] для корректного вычисления арккосинуса
    dotProduct = Math.Clamp(dotProduct, -1.0f, 1.0f);

    // Возвращаем угол в радианах через арккосинус
    return MathF.Acos(dotProduct);
  }

  /*public Vector4 ToVector4(float w)
  {
    return new Vector4(X, Y, Z, w);
  }*/

  public Vector4 AddW(float w)
  {
    return new Vector4(X, Y, Z, w);
  }

  public Vector2 DropZ()
  {
    return new Vector2(X, Y);
  }

  public override string ToString()
  {
    return $"Vector3({X}, {Y}, {Z})";
  }
}