using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MegaLib.Mathematics.LinearAlgebra
{
  public struct Vector2
  {
    [JsonInclude] [JsonPropertyName("x")] public float X;
    [JsonInclude] [JsonPropertyName("y")] public float Y;

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

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
      return new Vector2
      {
        X = a.X - b.X,
        Y = a.Y - b.Y,
      };
    }
  }
}