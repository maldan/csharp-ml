using System;

namespace MegaLib.Ext;

public static class ArrayExtensions
{
  private static readonly Random _random = new();

  public static T RandomChoice<T>(this T[] array)
  {
    if (array == null || array.Length == 0)
      return default;
    var index = _random.Next(array.Length);
    return array[index];
  }
}