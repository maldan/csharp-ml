using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Color;

public struct RGB8
{
  public byte R;
  public byte G;
  public byte B;

  public RGB8(byte r, byte g, byte b)
  {
    R = r;
    G = g;
    B = b;
  }

  public static RGB8 FromHex(string color)
  {
    if (color.Length == 7)
    {
      // Извлекаем цвет
      var r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
      var g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
      var b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
      return new RGB8((byte)r, (byte)g, (byte)b);
    }

    return new RGB8();
  }


  public override string ToString()
  {
    return $"RGB8({R}, {G}, {B})";
  }
}

public struct RGB32F
{
  public float R;
  public float G;
  public float B;

  public RGB32F(float r, float g, float b)
  {
    R = r;
    G = g;
    B = b;
  }

  public static RGB32F FromHex(string color)
  {
    if (color.Length == 7)
    {
      // Извлекаем цвет
      var r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
      var g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
      var b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
      return new RGB32F(r / 255f, g / 255f, b / 255f);
    }

    return new RGB32F();
  }

  public static explicit operator Vector3(RGB32F rgba)
  {
    return new Vector3(rgba.R, rgba.G, rgba.B);
  }

  public override string ToString()
  {
    return $"RGB32F({R}, {G}, {B})";
  }
}
/*[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct RGB<T>
{
  public readonly T R;
  public readonly T G;
  public readonly T B;

  public Vector3 Vector3 => new(Convert.ToSingle(R), Convert.ToSingle(G), Convert.ToSingle(B));

  public RGB(T r, T g, T b)
  {
    R = r;
    G = g;
    B = b;
  }

  public static RGB<float> FromHex(string color)
  {
    if (color.Length == 7)
    {
      // Извлекаем цвет
      var r = int.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
      var g = int.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
      var b = int.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

      // Преобразовываем в значения от 0 до 1
      return new RGB<float>(r / 255f, g / 255f, b / 255f);
    }

    return new RGB<float>();
  }

  public override string ToString()
  {
    return $"RGB({R}, {G}, {B})";
  }
}*/