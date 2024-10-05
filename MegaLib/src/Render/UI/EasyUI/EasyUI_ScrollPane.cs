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

    Style.BorderWidth = 1;
    Style.BorderColor = new Vector4(0, 0, 0, 0.25f);

    Children.Add(_scroll);

    var isOver = false;
    Events.OnMouseOver += () =>
    {
      EasyUI_GlobalState.ScrollElementStack.Push(this);
      isOver = true;
    };
    Events.OnMouseOut += () =>
    {
      if (EasyUI_GlobalState.ScrollElementStack.Count > 0) EasyUI_GlobalState.ScrollElementStack.Pop();
      isOver = false;
    };

    Events.OnRender += (delta) =>
    {
      _scroll.Style.X = Width() - 16;
      _scroll.Style.Y = 0;
      _scroll.Style.Width = 16;
      _scroll.Style.Height = Height();
      StencilRectangle = BoundingBox();

      for (var i = 0; i < Children.Count; i++)
      {
        if (Children[i] == _scroll) continue;
        if (Children[i] == null) continue;
        var innerElement = Children[i];

        // Если внутренний элемент больше основного элемента
        if (innerElement.Height() > Height())
        {
          var pp = innerElement.Height() - Height();
          innerElement.Style.Y = (float)_scroll.Value * -pp;
        }
      }

      if (Mouse.WheelDirection != 0 && isOver && EasyUI_GlobalState.ScrollElementStack.Count > 0 &&
          EasyUI_GlobalState.ScrollElementStack.Peek() == this)
      {
        _scroll.Scroll(Mouse.WheelDirection * delta * -220);
      }
    };
  }

  public float ContentWidth()
  {
    return Width() - 16;
  }
}