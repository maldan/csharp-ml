using System;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Color;

/*
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct RGBA<T> where T : struct
{
  public readonly T R;
  public readonly T G;
  public readonly T B;
  public readonly T A;

  public RGBA(T r, T g, T b, T a)
  {
    R = r;
    G = g;
    B = b;
    A = a;
  }

  // public Vector4 Vector42 => new(Convert.ToSingle(R), Convert.ToSingle(G), Convert.ToSingle(B), Convert.ToSingle(A));

  public Vector4 Vector4
  {
    get
    {
      if (this is RGBA<float> r) return new Vector4(r.R, r.G, r.B, r.A);
      return new Vector4(Convert.ToSingle(R), Convert.ToSingle(G), Convert.ToSingle(B), Convert.ToSingle(A));
    }
  }

  public static RGBA<T> White => new(
    (T)Convert.ChangeType(Convert.ToSingle(1), typeof(T)),
    (T)Convert.ChangeType(Convert.ToSingle(1), typeof(T)),
    (T)Convert.ChangeType(Convert.ToSingle(1), typeof(T)),
    (T)Convert.ChangeType(Convert.ToSingle(1), typeof(T))
  );

  // Lerp function
  public static RGBA<float> Lerp(RGBA<float> a, RGBA<float> b, float t)
  {
    return new RGBA<float>(
      a.R + (b.R - a.R) * t,
      a.G + (b.G - a.G) * t,
      a.B + (b.B - a.B) * t,
      a.A + (b.A - a.A) * t
    );
  }

  public RGBA<float> Mul(float b)
  {
    if (this is RGBA<float> a)
    {
      return new RGBA<float>(
        a.R * b,
        a.G * b,
        a.B * b,
        a.A * b
      );
    }

    return new RGBA<float>();
  }

  public static RGBA<float> FromHex(string color)
  {
    if (color.Length == 9)
    {
      // Извлекаем цвет
      var r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
      var g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
      var b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
      var a = int.Parse(color.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

      // Преобразовываем в значения от 0 до 1
      return new RGBA<float>(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    return new RGBA<float>();
  }

  public override string ToString()
  {
    return $"RGBA({R}, {G}, {B}, {A})";
  }
}
*/

public struct RGBA8
{
  public byte R;
  public byte G;
  public byte B;
  public byte A;

  public RGBA8(byte r, byte g, byte b, byte a)
  {
    R = r;
    G = g;
    B = b;
    A = a;
  }
}

public struct RGBA32F : IEquatable<RGBA32F>
{
  public float R;
  public float G;
  public float B;
  public float A;

  public RGBA32F(float r, float g, float b, float a)
  {
    R = r;
    G = g;
    B = b;
    A = a;
  }

  public static RGBA32F operator +(RGBA32F a, RGBA32F b)
  {
    return new RGBA32F()
    {
      R = a.R + b.R,
      G = a.G + b.G,
      B = a.B + b.B,
      A = a.A + b.A
    };
  }

  public static RGBA32F operator *(RGBA32F a, float b)
  {
    return new RGBA32F()
    {
      R = a.R * b,
      G = a.G * b,
      B = a.B * b,
      A = a.A * b
    };
  }

  // Lerp function
  public static RGBA32F Lerp(RGBA32F a, RGBA32F b, float t)
  {
    return new RGBA32F(
      a.R + (b.R - a.R) * t,
      a.G + (b.G - a.G) * t,
      a.B + (b.B - a.B) * t,
      a.A + (b.A - a.A) * t
    );
  }

  public static RGBA32F FromHex(string color)
  {
    if (color.Length == 9)
    {
      // Извлекаем цвет
      var r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
      var g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
      var b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
      var a = int.Parse(color.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

      // Преобразовываем в значения от 0 до 1
      return new RGBA32F(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    return new RGBA32F();
  }

  public static explicit operator Vector4(RGBA32F rgba)
  {
    return new Vector4(rgba.R, rgba.G, rgba.B, rgba.A);
  }

  public override string ToString()
  {
    return $"RGBA32F({R}, {G}, {B}, {A})";
  }

  public bool Equals(RGBA32F other)
  {
    return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
  }

  public override bool Equals(object obj)
  {
    return obj is RGBA32F other && Equals(other);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(R, G, B, A);
  }
}