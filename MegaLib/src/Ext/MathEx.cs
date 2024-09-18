using System;

namespace MegaLib.Ext;

public static class MathEx
{
  public static float Lerp(float start, float end, float t)
  {
    return (1 - t) * start + t * end;
  }

  // Обобщенная функция для линейной интерполяции между двумя числовыми типами
  public static T Lerp<T>(T start, T end, float t) where T : struct
  {
    // Преобразуем в float для интерполяции
    var startFloat = Convert.ToSingle(start);
    var endFloat = Convert.ToSingle(end);
    var resultFloat = (1 - t) * startFloat + t * endFloat;

    // Преобразуем обратно в тип T
    return (T)Convert.ChangeType(resultFloat, typeof(T));
  }
}