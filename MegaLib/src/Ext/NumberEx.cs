using System;

namespace MegaLib.Ext;

public static class FloatExtensions
{
  public static float DegToRad(this float number)
  {
    return (float)(number * (Math.PI / 180.0));
  }

  public static float RadToDeg(this float number)
  {
    return (float)(number * (180.0 / Math.PI));
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
}