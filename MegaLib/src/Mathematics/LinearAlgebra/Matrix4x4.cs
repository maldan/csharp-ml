using System;

namespace MegaLib.Mathematics.LinearAlgebra
{
  public struct Matrix4x4
  {
    public float M00, M01, M02, M03;
    public float M10, M11, M12, M13;
    public float M20, M21, M22, M23;
    public float M30, M31, M32, M33;

    public float[] Raw => new[]
    {
      M00, M01, M02, M03,
      M10, M11, M12, M13,
      M20, M21, M22, M23,
      M30, M31, M32, M33
    };

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

        return new Quaternion(qx, qy, qz, qw);
      }
    }

    public Matrix4x4 Inverted
    {
      get
      {
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

        // Calculate the determinant
        var det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;
        if (det == 0) return new Matrix4x4();

        det = 1.0f / det;

        var m = new Matrix4x4();
        M00 = (M11 * b11 - M12 * b10 + M13 * b09) * det;
        M01 = (M02 * b10 - M01 * b11 - M03 * b09) * det;
        M02 = (M31 * b05 - M32 * b04 + M33 * b03) * det;
        M03 = (M22 * b04 - M21 * b05 - M23 * b03) * det;

        M10 = (M12 * b08 - M10 * b11 - M13 * b07) * det;
        M11 = (M00 * b11 - M02 * b08 + M03 * b07) * det;
        M12 = (M32 * b02 - M30 * b05 - M33 * b01) * det;
        M13 = (M20 * b05 - M22 * b02 + M23 * b01) * det;

        M20 = (M10 * b10 - M11 * b08 + M13 * b06) * det;
        M21 = (M01 * b08 - M00 * b10 - M03 * b06) * det;
        M22 = (M30 * b04 - M31 * b02 + M33 * b00) * det;
        M23 = (M21 * b02 - M20 * b04 - M23 * b00) * det;

        M30 = (M11 * b07 - M10 * b09 - M12 * b06) * det;
        M31 = (M00 * b09 - M01 * b07 + M02 * b06) * det;
        M32 = (M31 * b01 - M30 * b03 - M32 * b00) * det;
        M33 = (M20 * b03 - M21 * b01 + M22 * b00) * det;

        return m;
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

    // Scale
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

      /*var m = Matrix4x4.Identity;

      var a00 = a.Raw[0];
      var a01 = a.Raw[1];
      var a02 = a.Raw[2];
      var a03 = a.Raw[3];
      var a10 = a.Raw[4];
      var a11 = a.Raw[5];
      var a12 = a.Raw[6];
      var a13 = a.Raw[7];
      var a20 = a.Raw[8];
      var a21 = a.Raw[9];
      var a22 = a.Raw[10];
      var a23 = a.Raw[11];
      var a30 = a.Raw[12];
      var a31 = a.Raw[13];
      var a32 = a.Raw[14];
      var a33 = a.Raw[15];

      // Cache only the current line of the second matrix
      var b0 = b.Raw[0];
      var b1 = b.Raw[1];
      var b2 = b.Raw[2];
      var b3 = b.Raw[3];
      m.Raw[0] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
      m.Raw[1] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
      m.Raw[2] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
      m.Raw[3] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

      b0 = b.Raw[4];
      b1 = b.Raw[5];
      b2 = b.Raw[6];
      b3 = b.Raw[7];
      m.Raw[4] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
      m.Raw[5] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
      m.Raw[6] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
      m.Raw[7] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

      b0 = b.Raw[8];
      b1 = b.Raw[9];
      b2 = b.Raw[10];
      b3 = b.Raw[11];
      m.Raw[8] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
      m.Raw[9] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
      m.Raw[10] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
      m.Raw[11] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

      b0 = b.Raw[12];
      b1 = b.Raw[13];
      b2 = b.Raw[14];
      b3 = b.Raw[15];
      m.Raw[12] = b0 * a00 + b1 * a10 + b2 * a20 + b3 * a30;
      m.Raw[13] = b0 * a01 + b1 * a11 + b2 * a21 + b3 * a31;
      m.Raw[14] = b0 * a02 + b1 * a12 + b2 * a22 + b3 * a32;
      m.Raw[15] = b0 * a03 + b1 * a13 + b2 * a23 + b3 * a33;

      return m;*/
    }

    public override string ToString()
    {
      return $@"Matrix4x4(
        {M00}, {M01}, {M02}, {M03},
        {M10}, {M11}, {M12}, {M13},
        {M20}, {M21}, {M22}, {M23},
        {M30}, {M31}, {M32}, {M33},
      )";
    }
  }
}