using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Chart : EasyUI_Element
{
  private List<float> _values = [];
  public int MaxValues = 100;

  public EasyUI_Chart()
  {
    Style.SetBackgroundColor("#2b2b2b");

    // Style.IsBackgroundChanged = true;
  }

  public void AddValue(float v)
  {
    _values.Add(v);
    // Хак что типа поменялись значения
    Style.IsBackgroundChanged = true;
    if (_values.Count > MaxValues) _values.RemoveAt(0);
  }

  public void AddValues(IEnumerable<float> list)
  {
    foreach (var v in list) AddValue(v);
  }

  public override void OnChanged()
  {
    ClearLines();

    if (_values.Count == 0)
      return;

    var maxValue = _values.Max();
    var minValue = _values.Min();
    var range = maxValue - minValue;
    var wItem = Style.Width / (_values.Count - 1);

    // Если все значения одинаковы, масштабируйте по умолчанию
    if (range == 0) range = 1; // предотвращаем деление на ноль

    for (var i = 0; i < _values.Count - 1; i++)
    {
      var h = (_values[i] - minValue) / range; // нормализация
      var h2 = (_values[i + 1] - minValue) / range; // нормализация

      var line = new RO_Line
      {
        From = new Vector3(i * wItem, Style.Height - h * Style.Height, 0),
        To = new Vector3((i + 1) * wItem, Style.Height - h2 * Style.Height, 0)
      };
      Add(line);
    }
  }
}