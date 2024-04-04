using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MegaLib.Render.Color
{
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

    public override string ToString()
    {
      return $"RGBA({R}, {G}, {B}, {A})";
    }
  }
}