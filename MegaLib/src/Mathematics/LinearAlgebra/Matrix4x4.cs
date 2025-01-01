using System;

namespace MegaLib.Mathematics.LinearAlgebra;

public struct Matrix4x4
{
  public float M00, M01, M02, M03;
  public float M10, M11, M12, M13;
  public float M20, M21, M22, M23;
  public float M30, M31, M32, M33;

  public float[] Raw =>
  [
    M00, M01, M02, M03,
    M10, M11, M12, M13,
    M20, M21, M22, M23,
    M30, M31, M32, M33
  ];

  public Vector3 Position => new(M30, M31, M32);

  public Vector3 Scaling =>
    new(
      (float)Math.Sqrt(M00 * M00 + M01 * M01 + M02 * M02),
      (float)Math.Sqrt(M10 * M10 + M11 * M11 + M12 * M12),
      (float)Math.Sqrt(M20 * M20 + M21 * M21 + M22 * M22)
    );

  public Quaternion Rotation
  {
    get
    {
      // Получаем масштаб по каждой оси
      var scaleX = (float)Math.Sqrt(M00 * M00 + M01 * M01 + M02 * M02);
      var scaleY = (float)Math.Sqrt(M10 * M10 + M11 * M11 + M12 * M12);
      var scaleZ = (float)Math.Sqrt(M20 * M20 + M21 * M21 + M22 * M22);

      // Нормализуем столбцы матрицы трансформации
      var invScaleX = 1.0f / scaleX;
      var invScaleY = 1.0f / scaleY;
      var invScaleZ = 1.0f / scaleZ;

      var r00 = M00 * invScaleX;
      var r10 = M10 * invScaleY;
      var r20 = M20 * invScaleZ;
      var r01 = M01 * invScaleX;
      var r11 = M11 * invScaleY;
      var r21 = M21 * invScaleZ;
      var r02 = M02 * invScaleX;
      var r12 = M12 * invScaleY;
      var r22 = M22 * invScaleZ;

      var trace = r00 + r11 + r22;
      float qw, qx, qy, qz;

      if (trace > 0)
      {
        var s = 0.5f / (float)Math.Sqrt(trace + 1.0f);
        qw = 0.25f / s;
        qx = (r21 - r12) * s;
        qy = (r02 - r20) * s;
        qz = (r10 - r01) * s;
      }
      else if (r00 > r11 && r00 > r22)
      {
        var s = 2.0f * (float)Math.Sqrt(1.0f + r00 - r11 - r22);
        qw = (r21 - r12) / s;
        qx = 0.25f * s;
        qy = (r01 + r10) / s;
        qz = (r02 + r20) / s;
      }
      else if (r11 > r22)
      {
        var s = 2.0f * (float)Math.Sqrt(1.0f + r11 - r00 - r22);
        qw = (r02 - r20) / s;
        qx = (r01 + r10) / s;
        qy = 0.25f * s;
        qz = (r12 + r21) / s;
      }
      else
      {
        var s = 2.0f * (float)Math.Sqrt(1.0f + r22 - r00 - r11);
        qw = (r10 - r01) / s;
        qx = (r02 + r20) / s;
        qy = (r12 + r21) / s;
        qz = 0.25f * s;
      }

      return new Quaternion(qx, qy, qz, qw).Inverted;
    }
  }

  public Matrix4x4 Inverted
  {
    get
    {
      // Вычисляем миноры матрицы
      var b00 = M00 * M11 - M01 * M10;
      var b01 = M00 * M12 - M02 * M10;
      var b02 = M00 * M13 - M03 * M10;
      var b03 = M01 * M12 - M02 * M11;
      var b04 = M01 * M13 - M03 * M11;
      var b05 = M02 * M13 - M03 * M12;
      var b06 = M20 * M31 - M21 * M30;
      var b07 = M20 * M32 - M22 * M30;
      var b08 = M20 * M33 - M23 * M30;
      var b09 = M21 * M32 - M22 * M31;
      var b10 = M21 * M33 - M23 * M31;
      var b11 = M22 * M33 - M23 * M32;

      // Вычисляем детерминант
      var det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;
      if (det == 0) return Identity; // Возвращаем единичную матрицу в случае вырожденности

      det = 1.0f / det;

      // Возвращаем обратную матрицу
      return new Matrix4x4
      {
        M00 = (M11 * b11 - M12 * b10 + M13 * b09) * det,
        M01 = (M02 * b10 - M01 * b11 - M03 * b09) * det,
        M02 = (M31 * b05 - M32 * b04 + M33 * b03) * det,
        M03 = (M22 * b04 - M21 * b05 - M23 * b03) * det,
        M10 = (M12 * b08 - M10 * b11 - M13 * b07) * det,
        M11 = (M00 * b11 - M02 * b08 + M03 * b07) * det,
        M12 = (M32 * b02 - M30 * b05 - M33 * b01) * det,
        M13 = (M20 * b05 - M22 * b02 + M23 * b01) * det,
        M20 = (M10 * b10 - M11 * b08 + M13 * b06) * det,
        M21 = (M01 * b08 - M00 * b10 - M03 * b06) * det,
        M22 = (M30 * b04 - M31 * b02 + M33 * b00) * det,
        M23 = (M21 * b02 - M20 * b04 - M23 * b00) * det,
        M30 = (M11 * b07 - M10 * b09 - M12 * b06) * det,
        M31 = (M00 * b09 - M01 * b07 + M02 * b06) * det,
        M32 = (M31 * b01 - M30 * b03 - M32 * b00) * det,
        M33 = (M20 * b03 - M21 * b01 + M22 * b00) * det
      };
    }
  }

  public static Matrix4x4 Identity =>
    new()
    {
      M00 = 1,
      M01 = 0,
      M02 = 0,
      M03 = 0,

      M10 = 0,
      M11 = 1,
      M12 = 0,
      M13 = 0,

      M20 = 0,
      M21 = 0,
      M22 = 1,
      M23 = 0,

      M30 = 0,
      M31 = 0,
      M32 = 0,
      M33 = 1
    };

  public Matrix4x4(
    float m00, float m01, float m02, float m03,
    float m10, float m11, float m12, float m13,
    float m20, float m21, float m22, float m23,
    float m30, float m31, float m32, float m33)
  {
    M00 = m00;
    M01 = m01;
    M02 = m02;
    M03 = m03;
    M10 = m10;
    M11 = m11;
    M12 = m12;
    M13 = m13;
    M20 = m20;
    M21 = m21;
    M22 = m22;
    M23 = m23;
    M30 = m30;
    M31 = m31;
    M32 = m32;
    M33 = m33;
  }

  public Matrix4x4(
    double m00, double m01, double m02, double m03,
    double m10, double m11, double m12, double m13,
    double m20, double m21, double m22, double m23,
    double m30, double m31, double m32, double m33)
  {
    M00 = (float)m00;
    M01 = (float)m01;
    M02 = (float)m02;
    M03 = (float)m03;
    M10 = (float)m10;
    M11 = (float)m11;
    M12 = (float)m12;
    M13 = (float)m13;
    M20 = (float)m20;
    M21 = (float)m21;
    M22 = (float)m22;
    M23 = (float)m23;
    M30 = (float)m30;
    M31 = (float)m31;
    M32 = (float)m32;
    M33 = (float)m33;
  }

  public Matrix4x4(float[] array)
  {
    if (array is not { Length: 16 })
      throw new ArgumentException("Input array must contain exactly 16 elements", nameof(array));

    M00 = array[0];
    M01 = array[1];
    M02 = array[2];
    M03 = array[3];
    M10 = array[4];
    M11 = array[5];
    M12 = array[6];
    M13 = array[7];
    M20 = array[8];
    M21 = array[9];
    M22 = array[10];
    M23 = array[11];
    M30 = array[12];
    M31 = array[13];
    M32 = array[14];
    M33 = array[15];
  }

  public Matrix4x4 Translate(float x, float y, float z)
  {
    return Translate(new Vector3(x, y, z));
  }

  public Matrix4x4 Translate(Vector3 v)
  {
    return new Matrix4x4
    {
      M00 = M00, M01 = M01, M02 = M02, M03 = M03,
      M10 = M10, M11 = M11, M12 = M12, M13 = M13,
      M20 = M20, M21 = M21, M22 = M22, M23 = M23,

      M30 = M00 * v.X + M10 * v.Y + M20 * v.Z + M30,
      M31 = M01 * v.X + M11 * v.Y + M21 * v.Z + M31,
      M32 = M02 * v.X + M12 * v.Y + M22 * v.Z + M32,
      M33 = M03 * v.X + M13 * v.Y + M23 * v.Z + M33
    };
  }

  public Matrix4x4 Rotate(Quaternion angles)
  {
    return angles.Matrix4x4 * this;
  }

  public Matrix4x4 Scale(Vector3 scale)
  {
    return new Matrix4x4
    {
      M00 = M00 * scale.X, M01 = M01 * scale.X, M02 = M02 * scale.X, M03 = M03,
      M10 = M10 * scale.Y, M11 = M11 * scale.Y, M12 = M12 * scale.Y, M13 = M13,
      M20 = M20 * scale.Z, M21 = M21 * scale.Z, M22 = M22 * scale.Z, M23 = M23,
      M30 = M30, M31 = M31, M32 = M32, M33 = M33
    };
  }
  
  public Matrix4x4 Scale(float x, float y, float z)
  {
    return Scale(new Vector3(x, y, z));
  }

  // Статическая функция для нахождения обратной матрицы
  public static Matrix4x4 Inverse(Matrix4x4 matrix)
  {
    return matrix.Inverted;
  }

  // Scale
  public static Vector4 operator *(Matrix4x4 matrix, Vector4 vector)
  {
    var x = matrix.M00 * vector.X + matrix.M01 * vector.Y + matrix.M02 * vector.Z + matrix.M03 * vector.W;
    var y = matrix.M10 * vector.X + matrix.M11 * vector.Y + matrix.M12 * vector.Z + matrix.M13 * vector.W;
    var z = matrix.M20 * vector.X + matrix.M21 * vector.Y + matrix.M22 * vector.Z + matrix.M23 * vector.W;
    var w = matrix.M30 * vector.X + matrix.M31 * vector.Y + matrix.M32 * vector.Z + matrix.M33 * vector.W;

    return new Vector4(x, y, z, w);
  }

  public static Matrix4x4 operator +(Matrix4x4 a, Matrix4x4 b)
  {
    return new Matrix4x4
    {
      M00 = a.M00 + b.M00,
      M01 = a.M01 + b.M01,
      M02 = a.M02 + b.M02,
      M03 = a.M03 + b.M03,

      M10 = a.M10 + b.M10,
      M11 = a.M11 + b.M11,
      M12 = a.M12 + b.M12,
      M13 = a.M13 + b.M13,

      M20 = a.M20 + b.M20,
      M21 = a.M21 + b.M21,
      M22 = a.M22 + b.M22,
      M23 = a.M23 + b.M23,

      M30 = a.M30 + b.M30,
      M31 = a.M31 + b.M31,
      M32 = a.M32 + b.M32,
      M33 = a.M33 + b.M33
    };
  }

  public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
  {
    return new Matrix4x4
    {
      M00 = a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20 + a.M03 * b.M30,
      M01 = a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21 + a.M03 * b.M31,
      M02 = a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22 + a.M03 * b.M32,
      M03 = a.M00 * b.M03 + a.M01 * b.M13 + a.M02 * b.M23 + a.M03 * b.M33,

      M10 = a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20 + a.M13 * b.M30,
      M11 = a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
      M12 = a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
      M13 = a.M10 * b.M03 + a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,

      M20 = a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20 + a.M23 * b.M30,
      M21 = a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
      M22 = a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
      M23 = a.M20 * b.M03 + a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,

      M30 = a.M30 * b.M00 + a.M31 * b.M10 + a.M32 * b.M20 + a.M33 * b.M30,
      M31 = a.M30 * b.M01 + a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
      M32 = a.M30 * b.M02 + a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
      M33 = a.M30 * b.M03 + a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
    };
  }

  public static Matrix4x4 operator *(Matrix4x4 matrix, float scalar)
  {
    return new Matrix4x4
    {
      M00 = matrix.M00 * scalar, M01 = matrix.M01 * scalar, M02 = matrix.M02 * scalar, M03 = matrix.M03,
      M10 = matrix.M10 * scalar, M11 = matrix.M11 * scalar, M12 = matrix.M12 * scalar, M13 = matrix.M13,
      M20 = matrix.M20 * scalar, M21 = matrix.M21 * scalar, M22 = matrix.M22 * scalar, M23 = matrix.M23,
      M30 = matrix.M30 * scalar, M31 = matrix.M31 * scalar, M32 = matrix.M32 * scalar, M33 = matrix.M33
    };
  }

  public static Matrix4x4 operator *(float scalar, Matrix4x4 matrix)
  {
    return new Matrix4x4
    {
      M00 = matrix.M00 * scalar, M01 = matrix.M01 * scalar, M02 = matrix.M02 * scalar, M03 = matrix.M03,
      M10 = matrix.M10 * scalar, M11 = matrix.M11 * scalar, M12 = matrix.M12 * scalar, M13 = matrix.M13,
      M20 = matrix.M20 * scalar, M21 = matrix.M21 * scalar, M22 = matrix.M22 * scalar, M23 = matrix.M23,
      M30 = matrix.M30 * scalar, M31 = matrix.M31 * scalar, M32 = matrix.M32 * scalar, M33 = matrix.M33
    };
  }

  public override string ToString()
  {
    return $@"Matrix4x4(
        {M00:F}, {M01:F}, {M02:F}, {M03:F},
        {M10:F}, {M11:F}, {M12:F}, {M13:F},
        {M20:F}, {M21:F}, {M22:F}, {M23:F},
        {M30:F}, {M31:F}, {M32:F}, {M33:F},
      )";
  }
}