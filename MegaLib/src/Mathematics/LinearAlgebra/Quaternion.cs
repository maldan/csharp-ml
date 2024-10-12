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
      /*var t = 2.0 * (W * Y - Z * X);
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

      return v;*/

      Vector3 euler;
      var q = this;

      // Roll (X)
      var sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
      var cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
      euler.X = MathF.Atan2(sinr_cosp, cosr_cosp);

      // Pitch (Y)
      var sinp = 2 * (q.W * q.Y - q.Z * q.X);
      if (MathF.Abs(sinp) >= 1)
        euler.Y = MathF.CopySign(MathF.PI / 2, sinp); // Используем 90 градусов, если вне диапазона
      else
        euler.Y = MathF.Asin(sinp);

      // Yaw (Z)
      var siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
      var cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
      euler.Z = MathF.Atan2(siny_cosp, cosy_cosp);

      return euler;
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
    /*var roll = v.X;
    var pitch = v.Y;
    var yaw = v.Z;

    if (unit == "deg")
    {
      roll = roll.DegToRad();
      pitch = pitch.DegToRad();
      yaw = yaw.DegToRad();
    }

    //  Roll first, about axis the object is facing, then
    //  pitch upward, then yaw to face into the new heading
    float sr, cr, sp, cp, sy, cy;

    var halfRoll = roll * 0.5f;
    sr = MathF.Sin(halfRoll);
    cr = MathF.Cos(halfRoll);

    var halfPitch = pitch * 0.5f;
    sp = MathF.Sin(halfPitch);
    cp = MathF.Cos(halfPitch);

    var halfYaw = yaw * 0.5f;
    sy = MathF.Sin(halfYaw);
    cy = MathF.Cos(halfYaw);

    Quaternion result;

    result.X = cy * sp * cr + sy * cp * sr;
    result.Y = sy * cp * cr - cy * sp * sr;
    result.Z = cy * cp * sr - sy * sp * cr;
    result.W = cy * cp * cr + sy * sp * sr;

    return result;*/

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

  /*public static Quaternion LookRotation(Vector3 forward, Vector3 up)
  {
    // Нормализуем входные векторы
    forward = Vector3.Normalize(forward);
    up = Vector3.Normalize(up);

    // Проверяем если направление и "вверх" коллинеарны, чтобы избежать ошибки
    if (Vector3.Dot(forward, up) > 0.9999f || Vector3.Dot(forward, up) < -0.9999f)
    {
      up = Vector3.UnitX; // Задаем новый вектор "вверх", если они коллинеарны
    }

    // Определяем вектор "право" (осевое произведение "вверх" и "вперед")
    var right = Vector3.Cross(up, forward);
    right = Vector3.Normalize(right);

    // Пересчитываем новый "вверх" с учетом нормализации направления
    var recalculatedUp = Vector3.Cross(forward, right);

    // Создаем матрицу вращения на основе базисных векторов
    float m00 = right.X, m01 = right.Y, m02 = right.Z;
    float m10 = recalculatedUp.X, m11 = recalculatedUp.Y, m12 = recalculatedUp.Z;
    float m20 = forward.X, m21 = forward.Y, m22 = forward.Z;

    var w = (float)Math.Sqrt(1.0f + m00 + m11 + m22) * 0.5f;
    var w4 = 4.0f * w;
    var x = (m21 - m12) / w4;
    var y = (m02 - m20) / w4;
    var z = (m10 - m01) / w4;

    return new Quaternion(x, y, z, w);
  }*/


  /*public static Quaternion FromDirection(Vector3 direction)
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
  }*/

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

  public static Quaternion operator *(Quaternion quaternion, Matrix4x4 matrix)
  {
    // Извлекаем матрицу вращения из Matrix4x4
    float m00 = matrix.M00, m01 = matrix.M01, m02 = matrix.M02;
    float m10 = matrix.M10, m11 = matrix.M11, m12 = matrix.M12;
    float m20 = matrix.M20, m21 = matrix.M21, m22 = matrix.M22;

    // Вычисляем новый кватернион, умножая на матрицу
    var result = new Quaternion(
      quaternion.W * m00 + quaternion.X * m01 + quaternion.Y * m02,
      quaternion.W * m10 + quaternion.X * m11 + quaternion.Y * m12,
      quaternion.W * m20 + quaternion.X * m21 + quaternion.Y * m22,
      quaternion.W * (1.0f - m00 - m11 - m22) // предположительно, вычисление W компоненты
    );

    return result;
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

  public static Vector3 operator *(Quaternion q, Vector3 v)
  {
    // Преобразуем вектор в кватернион (с нулевой скалярной частью)
    var vQuat = new Quaternion(v.X, v.Y, v.Z, 0);

    // Вычисляем обратный кватернион
    var qInverse = q.Inverted; // Предполагаем, что у тебя есть метод для нахождения обратного кватерниона

    // Выполняем умножение q * v * q^-1
    var resultQuat = q * vQuat * qInverse;

    // Возвращаем результирующий вектор
    return new Vector3(resultQuat.X, resultQuat.Y, resultQuat.Z);
  }

  public static Quaternion Normalize(Quaternion q)
  {
    return q.Normalized;
  }

  // Метод для конвертации кватерниона в ось и угол
  public void ToAxisAngle(out Vector3 axis, out float angle)
  {
    var q = this;

    if (q.W > 1.0f)
      q = Normalize(q);

    angle = 2.0f * (float)Math.Acos(q.W);
    var sinHalfAngle = (float)Math.Sqrt(1.0f - q.W * q.W);

    if (sinHalfAngle < 0.001f) // Защита от деления на очень маленькое значение
    {
      axis = new Vector3(1, 0, 0); // Любая ось (в данном случае X)
    }
    else
    {
      axis = new Vector3(q.X / sinHalfAngle, q.Y / sinHalfAngle, q.Z / sinHalfAngle);
    }
  }

  // Метод создания кватерниона из оси и угла вращения
  public static Quaternion FromAxisAngle(Vector3 axis, float angle)
  {
    // Нормализация оси вращения
    axis = axis.Normalized;

    // Половина угла и синус для вычисления
    var halfAngle = angle * 0.5f;
    var sin = (float)Math.Sin(halfAngle);

    return new Quaternion
    {
      X = axis.X * sin,
      Y = axis.Y * sin,
      Z = axis.Z * sin,
      W = (float)Math.Cos(halfAngle)
    };
  }

  // Функция FromToRotation создает кватернион, который поворачивает один вектор (например, вектор "от")
  // в другой вектор (вектор "к").
  public static Quaternion FromToRotation(Vector3 from, Vector3 to)
  {
    var v0 = from.Normalized;
    var v1 = to.Normalized;

    var cosTheta = Vector3.Dot(v0, v1);
    Vector3 rotationAxis;

    if (cosTheta < -1.0f + 0.001f)
    {
      // 180 градусов поворот, находим ось
      rotationAxis = Vector3.Cross(Vector3.Right, v0);
      if (rotationAxis.LengthSquared < 0.01f)
        rotationAxis = Vector3.Cross(Vector3.Up, v0);

      rotationAxis = rotationAxis.Normalized;
      return new Quaternion(rotationAxis.X, rotationAxis.Y, rotationAxis.Z, 0.0f);
    }
    else
    {
      rotationAxis = Vector3.Cross(v0, v1);
      var s = MathF.Sqrt((1 + cosTheta) * 2);
      var invS = 1 / s;

      return new Quaternion(rotationAxis.X * invS, rotationAxis.Y * invS, rotationAxis.Z * invS, s * 0.5f);
    }
  }

  #endregion

  public override string ToString()
  {
    return $"Quaternion({X}, {Y}, {Z}, {W})";
  }
}