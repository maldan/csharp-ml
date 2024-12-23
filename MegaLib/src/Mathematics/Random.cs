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

  public float RangeFloat(float value1, float value2)
  {
    var minValue = Math.Min(value1, value2);
    var maxValue = Math.Max(value1, value2);

    var range = maxValue - minValue;
    var sample = _random.NextDouble(); // [0,1)
    var scaled = sample * range + minValue;
    return (float)scaled;
  }

  public int RangeInt(int value1, int value2)
  {
    var minValue = Math.Min(value1, value2);
    var maxValue = Math.Max(value1, value2);

    // Добавляем 1 к диапазону, чтобы включить maxValue
    var range = maxValue - minValue + 1;
    var sample = _random.Next(range); // [0, range)
    var scaled = sample + minValue;
    return scaled;
  }
}