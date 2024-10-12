using System;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Color;

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

  // Add
  public static RGBA<T> operator +(RGBA<T> a, RGBA<float> b)
  {
    return new RGBA<T>(
      (T)Convert.ChangeType(Convert.ToSingle(a.R) + b.R, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.G) + b.G, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.B) + b.B, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.A) + b.A, typeof(T))
    );
  }

  public static RGBA<T> operator -(RGBA<T> a, RGBA<float> b)
  {
    return new RGBA<T>(
      (T)Convert.ChangeType(Convert.ToSingle(a.R) - b.R, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.G) - b.G, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.B) - b.B, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.A) - b.A, typeof(T))
    );
  }

  public static RGBA<T> operator /(RGBA<T> a, float b)
  {
    return new RGBA<T>(
      (T)Convert.ChangeType(Convert.ToSingle(a.R) / b, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.G) / b, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.B) / b, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.A) / b, typeof(T))
    );
  }

  public static RGBA<T> operator *(RGBA<T> a, float b)
  {
    return new RGBA<T>(
      (T)Convert.ChangeType(Convert.ToSingle(a.R) * b, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.G) * b, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.B) * b, typeof(T)),
      (T)Convert.ChangeType(Convert.ToSingle(a.A) * b, typeof(T))
    );
  }

  // Lerp function
  public static RGBA<T> Lerp(RGBA<T> a, RGBA<T> b, float t)
  {
    return new RGBA<T>(
      (T)Convert.ChangeType((1 - t) * Convert.ToSingle(a.R) + t * Convert.ToSingle(b.R), typeof(T)),
      (T)Convert.ChangeType((1 - t) * Convert.ToSingle(a.G) + t * Convert.ToSingle(b.G), typeof(T)),
      (T)Convert.ChangeType((1 - t) * Convert.ToSingle(a.B) + t * Convert.ToSingle(b.B), typeof(T)),
      (T)Convert.ChangeType((1 - t) * Convert.ToSingle(a.A) + t * Convert.ToSingle(b.A), typeof(T))
    );
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