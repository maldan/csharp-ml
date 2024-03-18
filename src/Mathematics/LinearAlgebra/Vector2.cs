using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MegaLib.Mathematics.LinearAlgebra
{
  public struct Vector2
  {
    [JsonInclude][JsonPropertyName("x")] public float X;
    [JsonInclude][JsonPropertyName("y")] public float Y;

    public Vector2(float x, float y)
    {
      X = x;
      Y = y;
    }

    public static float Distance(Vector2 from, Vector2 to)
    {
      var a = from.X - to.X;
      var b = from.Y - to.Y;

      return (float)Math.Sqrt(a * a + b * b);
    }
  }
}