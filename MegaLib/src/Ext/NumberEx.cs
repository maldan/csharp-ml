using System;

namespace MegaLib.Ext;

public static class FloatExtensions
{
  public static float DegToRad(this float number)
  {
    return number * (MathF.PI / 180.0f);
  }

  public static float RadToDeg(this float number)
  {
    return number * (180.0f / MathF.PI);
  }

  public static float Sin(this float number)
  {
    return (float)Math.Sin(number);
  }

  public static float Clamp(this float value, float min, float max)
  {
    return Math.Min(Math.Max(value, min), max);
  }

  public static float Clamp01(this float value)
  {
    return Math.Min(Math.Max(value, 0), 1);
  }

  public static float Remap(this float value, float from1, float to1, float from2, float to2)
  {
    return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
  }

  public static bool Between(this float value, float min, float max)
  {
    return value >= min && value <= max;
  }

  public static (byte R, byte G, byte B, byte A) ToBytesBE(this uint index)
  {
    var byte0 = (byte)(index & 0xFF); // 0-й байт
    var byte1 = (byte)((index >> 8) & 0xFF); // 1-й байт
    var byte2 = (byte)((index >> 16) & 0xFF); // 2-й байт
    var byte3 = (byte)((index >> 24) & 0xFF); // 3-й байт
    return (byte3, byte2, byte1, byte0);
  }

  public static uint ToUIntBE(this (byte R, byte G, byte B, byte A) index)
  {
    return (uint)index.A |
           ((uint)index.B << 8) |
           ((uint)index.G << 16) |
           ((uint)index.R << 24);
  }
}