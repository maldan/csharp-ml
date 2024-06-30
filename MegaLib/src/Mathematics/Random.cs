using System;

namespace MegaLib.Mathematics;

public class Random
{
  private readonly System.Random _random;

  public Random(int seed)
  {
    _random = new System.Random(seed);
  }

  public Random()
  {
    _random = new System.Random();
  }

  public float Range(float value1, float value2)
  {
    var minValue = Math.Min(value1, value2);
    var maxValue = Math.Max(value1, value2);

    var range = maxValue - minValue;
    var sample = _random.NextDouble(); // [0,1)
    var scaled = sample * range + minValue;
    return (float)scaled;
  }
}