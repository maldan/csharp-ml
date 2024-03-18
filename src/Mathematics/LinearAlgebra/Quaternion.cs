using System;

namespace MegaLib.Mathematics.LinearAlgebra
{
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

    #endregion

    public Quaternion RotateEuler(Vector3 v, string unit)
    {
      return this * FromEuler(v, unit);
    }

    public Quaternion RotateEuler(float x, float y, float z, string unit)
    {
      return this * FromEuler(x, y, z, unit);
    }

    #region Static

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

    #endregion

    #region Operators

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

    #endregion
  }
}