using System;
using System.Collections.Generic;

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

  public static T[] Align<T>(this T[] array, int step) where T : struct
  {
    // Вычисляем новый размер массива, кратный step
    var newSize = (array.Length + step - 1) / step * step;

    // Создаем новый массив нужного размера и заполняем его нулями
    var newArray = new T[newSize];

    // Копируем элементы из исходного массива в новый массив
    Array.Copy(array, newArray, array.Length);

    return newArray;
  }

  public static List<T> Pop<T>(this List<T> list, int n)
  {
    var count = list.Count;
    if (n > count) n = count;

    // Получаем последние N элементов
    var lastNElements = list.GetRange(count - n, n);

    // Удаляем последние N элементов
    list.RemoveRange(count - n, n);

    return lastNElements;
  }
}