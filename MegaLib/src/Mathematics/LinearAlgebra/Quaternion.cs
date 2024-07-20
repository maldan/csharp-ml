using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;
using MegaLib.Ext;

namespace MegaLib.Mathematics.LinearAlgebra;

public struct Quaternion
{
  public float X;
  public float Y;
  public float Z;
  public float W;

  public Quaternion(float x, float y, float z, float w)
  {
    X = x;
    Y = y;
    Z = z;
    W = w;
  }

  #region Properties

  public static Quaternion Identity => new() { X = 0, Y = 0, Z = 0, W = 1 };
  public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

  public Matrix4x4 Matrix4x4
  {
    get
    {
      var x2 = X + X;
      var y2 = Y + Y;
      var z2 = Z + Z;

      var xx = X * x2;
      var xy = X * y2;
      var xz = X * z2;
      var yy = Y * y2;
      var yz = Y * z2;
      var zz = Z * z2;
      var wx = W * x2;
      var wy = W * y2;
      var wz = W * z2;

      return new Matrix4x4
      {
        M00 = 1.0f - (yy + zz),
        M01 = xy + wz,
        M02 = xz - wy,
        M03 = 0.0f,

        M10 = xy - wz,
        M11 = 1.0f - (xx + zz),
        M12 = yz + wx,
        M13 = 0.0f,

        M20 = xz + wy,
        M21 = yz - wx,
        M22 = 1.0f - (xx + yy),
        M23 = 0.0f,

        M30 = 0.0f,
        M31 = 0.0f,
        M32 = 0.0f,
        M33 = 1.0f
      };
    }
  }

  public Quaternion Normalized
  {
    get
    {
      var length = Length;
      return new Quaternion
      {
        X = X / length,
        Y = Y / length,
        Z = Z / length,
        W = W / length
      };
    }
  }

  public Vector3 Euler
  {
    get
    {
      var t = 2.0 * (W * Y - Z * X);
      var v = new Vector3(0, 0, 0);

      // Set X
      var a = 2.0 * (W * X + Y * Z);
      v.X = (float)Math.Atan2(a, 1.0 - 2.0 * (X * X + Y * Y));

      // Set Y
      if (t >= 1.0)
      {
        v.Y = (float)(Math.PI / 2.0);
      }
      else
      {
        if (t <= -1.0)
          v.Y = (float)(-Math.PI / 2.0);
        else
          v.Y = (float)Math.Asin(t);
      }

      // Set Z
      a = 2.0 * (W * Z + X * Y);
      v.Z = (float)Math.Atan2(a, 1.0 - 2.0 * (Y * Y + Z * Z));

      return v;
    }
  }

  public Quaternion Inverted => new() { X = -X, Y = -Y, Z = -Z, W = W };
  public Quaternion Conjugated => new() { X = -X, Y = -Y, Z = -Z, W = W };

  #endregion

  public Vector4 ToVector4()
  {
    return new Vector4(X, Y, Z, W);
  }

  public Quaternion RotateEuler(Vector3 v, string unit)
  {
    return this * FromEuler(v, unit);
  }

  public Quaternion RotateEuler(float x, float y, float z, string unit)
  {
    return this * FromEuler(x, y, z, unit);
  }

  public readonly override bool Equals(object obj)
  {
    return obj is Quaternion other && Equals(other);
  }

  public readonly bool Equals(Quaternion other)
  {
    return X.Equals(other.X)
           && Y.Equals(other.Y)
           && Z.Equals(other.Z)
           && W.Equals(other.W);
  }

  public readonly override int GetHashCode()
  {
    return HashCode.Combine(X, Y, Z, W);
  }

  #region Static

  public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
  {
    return quaternion1.X * quaternion2.X
           + quaternion1.Y * quaternion2.Y
           + quaternion1.Z * quaternion2.Z
           + quaternion1.W * quaternion2.W;
  }

  public static Quaternion FromEuler(float x, float y, float z, string unit)
  {
    return FromEuler(new Vector3(x, y, z), unit);
  }

  public static Quaternion FromEuler(Vector3 v, string unit)
  {
    var x = v.X;
    var y = v.Y;
    var z = v.Z;

    if (unit == "deg")
    {
      x = x.DegToRad();
      y = y.DegToRad();
      z = z.DegToRad();
    }

    var qx =
      Math.Sin(x / 2) * Math.Cos(y / 2) * Math.Cos(z / 2) -
      Math.Cos(x / 2) * Math.Sin(y / 2) * Math.Sin(z / 2);
    var qy =
      Math.Cos(x / 2) * Math.Sin(y / 2) * Math.Cos(z / 2) +
      Math.Sin(x / 2) * Math.Cos(y / 2) * Math.Sin(z / 2);
    var qz =
      Math.Cos(x / 2) * Math.Cos(y / 2) * Math.Sin(z / 2) -
      Math.Sin(x / 2) * Math.Sin(y / 2) * Math.Cos(z / 2);
    var qw =
      Math.Cos(x / 2) * Math.Cos(y / 2) * Math.Cos(z / 2) +
      Math.Sin(x / 2) * Math.Sin(y / 2) * Math.Sin(z / 2);

    return new Quaternion
    {
      X = (float)qx, Y = (float)qy, Z = (float)qz, W = (float)qw
    };
  }

  public static Quaternion Lerp(Quaternion a, Quaternion b, float t)
  {
    if (t < 0) t = 0;
    if (t > 1) t = 1;
    var result = new Quaternion(0, 0, 0, 0);
    var tInv = (float)1.0 - t;

    // Linear interpolation for the quaternion components
    result.W = a.W * tInv + b.W * t;
    result.X = a.X * tInv + b.X * t;
    result.Y = a.Y * tInv + b.Y * t;
    result.Z = a.Z * tInv + b.Z * t;

    // Normalize the resulting quaternion
    var norm = (float)Math.Sqrt(
      result.W * result.W + result.X * result.X + result.Y * result.Y + result.Z * result.Z
    );
    result.W /= norm;
    result.X /= norm;
    result.Y /= norm;
    result.Z /= norm;

    return result;
  }

  public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
  {
    var t = amount;

    var cosOmega = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y +
                   quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;

    var flip = false;

    if (cosOmega < 0.0f)
    {
      flip = true;
      cosOmega = -cosOmega;
    }

    float s1, s2;

    if (cosOmega > 1.0f - 1e-6f)
    {
      // Too close, do straight linear interpolation.
      s1 = 1.0f - t;
      s2 = flip ? -t : t;
    }
    else
    {
      var omega = MathF.Acos(cosOmega);
      var invSinOmega = 1 / MathF.Sin(omega);

      s1 = MathF.Sin((1.0f - t) * omega) * invSinOmega;
      s2 = flip
        ? -MathF.Sin(t * omega) * invSinOmega
        : MathF.Sin(t * omega) * invSinOmega;
    }

    Quaternion ans;

    ans.X = s1 * quaternion1.X + s2 * quaternion2.X;
    ans.Y = s1 * quaternion1.Y + s2 * quaternion2.Y;
    ans.Z = s1 * quaternion1.Z + s2 * quaternion2.Z;
    ans.W = s1 * quaternion1.W + s2 * quaternion2.W;

    return ans;
  }

  public static Quaternion FromDirection(Vector3 direction)
  {
    // Нормализуем направление
    direction = direction.Normalized;

    // Вычисляем угол поворота вокруг оси Y
    var yaw = (float)Math.Atan2(direction.X, direction.Z);

    // Вычисляем угол поворота вокруг оси X
    var pitch = (float)Math.Asin(-direction.Y);

    // Создаем кватернион из углов Эйлера (в радианах)
    var rotation = FromEuler(yaw, pitch, 0, "rad");

    return rotation;
  }

  #endregion

  #region Operators

  public static Quaternion operator +(Quaternion value1, Quaternion value2)
  {
    return new Quaternion(
      value1.X + value2.X,
      value1.Y + value2.Y,
      value1.Z + value2.Z,
      value1.W + value2.W
    );
  }

  public static Quaternion operator -(Quaternion value1, Quaternion value2)
  {
    return new Quaternion(
      value1.X - value2.X,
      value1.Y - value2.Y,
      value1.Z - value2.Z,
      value1.W - value2.W
    );
  }

  public static Quaternion operator *(Quaternion a, Quaternion b)
  {
    var x1 = a.X;
    var y1 = a.Y;
    var z1 = a.Z;
    var w1 = a.W;

    var x2 = b.X;
    var y2 = b.Y;
    var z2 = b.Z;
    var w2 = b.W;

    return new Quaternion
    {
      X = w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2,
      Y = w1 * y2 + y1 * w2 + z1 * x2 - x1 * z2,
      Z = w1 * z2 + z1 * w2 + x1 * y2 - y1 * x2,
      W = w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2
    };
  }

  public static Quaternion operator *(Quaternion value1, float value2)
  {
    return new Quaternion(
      value1.X * value2,
      value1.Y * value2,
      value1.Z * value2,
      value1.W * value2
    );
  }

  public static Quaternion operator /(Quaternion value1, Quaternion value2)
  {
    Quaternion ans;

    var q1x = value1.X;
    var q1y = value1.Y;
    var q1z = value1.Z;
    var q1w = value1.W;

    //-------------------------------------
    // Inverse part.
    var ls = value2.X * value2.X + value2.Y * value2.Y +
             value2.Z * value2.Z + value2.W * value2.W;
    var invNorm = 1.0f / ls;

    var q2x = -value2.X * invNorm;
    var q2y = -value2.Y * invNorm;
    var q2z = -value2.Z * invNorm;
    var q2w = value2.W * invNorm;

    //-------------------------------------
    // Multiply part.

    // cross(av, bv)
    var cx = q1y * q2z - q1z * q2y;
    var cy = q1z * q2x - q1x * q2z;
    var cz = q1x * q2y - q1y * q2x;

    var dot = q1x * q2x + q1y * q2y + q1z * q2z;

    ans.X = q1x * q2w + q2x * q1w + cx;
    ans.Y = q1y * q2w + q2y * q1w + cy;
    ans.Z = q1z * q2w + q2z * q1w + cz;
    ans.W = q1w * q2w - dot;

    return ans;
  }

  public static bool operator ==(Quaternion value1, Quaternion value2)
  {
    return value1.X == value2.X
           && value1.Y == value2.Y
           && value1.Z == value2.Z
           && value1.W == value2.W;
  }

  public static bool operator !=(Quaternion value1, Quaternion value2)
  {
    return !(value1 == value2);
  }

  public static Quaternion operator -(Quaternion value)
  {
    return new Quaternion(0, 0, 0, 0) - value;
  }

  #endregion
}