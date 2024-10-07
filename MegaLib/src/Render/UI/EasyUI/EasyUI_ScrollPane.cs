using System;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ScrollPane : EasyUI_Element
{
  private EasyUI_Slider _scroll;
  private EasyUI_Element _bar;

  public EasyUI_ScrollPane()
  {
    _scroll = new EasyUI_Slider();
    _scroll.ShowText = false;
    _scroll.Direction = Direction.Vertical;

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

    Events.OnRender += (delta) =>
    {
      _scroll.Style.X = Style.Width - 16;
      _scroll.Style.Y = 0;
      _scroll.Style.Width = 16;
      _scroll.Style.Height = Style.Height;
      StencilRectangle = Style.BoundingBox;

      for (var i = 0; i < Children.Count; i++)
      {
        if (Children[i] == _scroll) continue;
        if (Children[i] == null) continue;
        var innerElement = Children[i];

        // Если внутренний элемент больше основного элемента
        if (innerElement.Style.Height > Style.Height)
        {
          var pp = innerElement.Style.Height - Style.Height;
          innerElement.Style.Y = (float)_scroll.Value * -pp;
        }
      }

      if (Mouse.WheelDirection != 0 && isOver && LayerEasyUi.ScrollElementStack.Count > 0 &&
          LayerEasyUi.ScrollElementStack.Peek() == this)
      {
        _scroll.Scroll(Mouse.WheelDirection * delta * -220);
      }
    };
  }

  public float ContentWidth()
  {
    return Style.Width - 16;
  }
}