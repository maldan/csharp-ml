using System;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Color;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct RGBA<T>
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

  public Vector4 Vector4 => new(Convert.ToSingle(R), Convert.ToSingle(G), Convert.ToSingle(B), Convert.ToSingle(A));

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


  public override string ToString()
  {
    return $"RGBA({R}, {G}, {B}, {A})";
  }
}