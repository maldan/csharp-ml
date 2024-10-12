using System;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ScrollPane : EasyUI_Element
{
  private EasyUI_Slider _scroll;
  private EasyUI_Element _bar;
  private bool _isOverSized;

  public EasyUI_ScrollPane()
  {
    _scroll = new EasyUI_Slider();
    _scroll.ShowText = false;
    _scroll.Direction = Direction.Vertical;
    _scroll.BarSize = 16f;

    //Style.BorderWidth = 1;
    Style.SetBorderWidth(2f);
    Style.SetBorderColor(new Vector4(0, 0, 0, 0.25f));
    // Style.BackgroundColor = new Vector4(1, 0, 0, 0.25f);

    Children.Add(_scroll);

    var isOver = false;
    Events.OnMouseOver += () =>
    {
      LayerEasyUi.ScrollElementStack.Push(this);
      isOver = true;
    };
    Events.OnMouseOut += () =>
    {
      if (LayerEasyUi.ScrollElementStack.Count > 0) LayerEasyUi.ScrollElementStack.Pop();
      isOver = false;
    };

    Events.OnBeforeRender += (delta) =>
    {
      _scroll.Style.X = Style.Width - 24;
      _scroll.Style.Y = 0;
      _scroll.Style.Width = 24;
      _scroll.Style.Height = Style.Height;
      StencilRectangle = Style.BoundingBox;

      var overSize = 1f;

      // Немного сбивает с толку, но внутри должен быть обычно 1 элемент только
      for (var i = 0; i < Children.Count; i++)
      {
        if (Children[i] == _scroll) continue;
        if (Children[i] == null) continue;
        var innerElement = Children[i];

        // Если внутренний элемент больше основного элемента
        if (innerElement.Style.Height > Style.Height)
        {
          // Смещаем элемент вверх 
          var pp = innerElement.Style.Height - Style.Height;
          innerElement.Style.Y = (float)_scroll.Value * -pp;
        }

        overSize = innerElement.Style.Height / Style.Height;
      }

      _isOverSized = overSize > 1f;
      _scroll.IsVisible = _isOverSized;

      _scroll.BarSize = Style.Height - 16 - overSize * 16f;
      if (_scroll.BarSize <= 16) _scroll.BarSize = 16f;

      if (Mouse.WheelDirection != 0 && isOver && LayerEasyUi.ScrollElementStack.Count > 0 &&
          LayerEasyUi.ScrollElementStack.Peek() == this)
      {
        _scroll.Scroll(Mouse.WheelDirection * delta * -_scroll.BarSize * 10f);
      }
    };
  }

  public float ContentWidth()
  {
    if (_isOverSized) return Style.Width - 24;
    return Style.Width;
  }
}