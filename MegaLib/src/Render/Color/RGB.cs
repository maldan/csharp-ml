using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MegaLib.Render.Color;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct RGB<T>
{
  public readonly T R;
  public readonly T G;
  public readonly T B;

  public RGB(T r, T g, T b)
  {
    R = r;
    G = g;
    B = b;
  }

  public override string ToString()
  {
    return $"RGB({R}, {G}, {B})";
  }
}

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

  public override string ToString()
  {
    return $"RGBA({R}, {G}, {B}, {A})";
  }
}