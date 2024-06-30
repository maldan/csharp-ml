using System;

namespace MegaLib.Ext;

public static class MathEx
{
  public static float Lerp(float start, float end, float t)
  {
    return (1 - t) * start + t * end;
  }
}