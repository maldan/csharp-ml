using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Layout : EasyUI_Element
{
  public Direction Direction = Direction.Vertical;
  public bool IsAdjustChildrenSize;
  public float Gap;

  public EasyUI_Layout()
  {
    Style.BackgroundColor = new Vector4(0, 0, 0, 0.01f);

    Events.OnBeforeRender += (delta) =>
    {
      if (Direction == Direction.Vertical)
      {
        var w = Width();
        var totalH = 0f;

        for (var i = 0; i < Children.Count; i++)
        {
          Children[i].Style.Width = w - Gap * 2;
          Children[i].Style.Y = Gap + totalH;
          Children[i].Style.X = Gap;
          totalH += Children[i].Height() + Gap;
        }

        Style.Height = totalH;
      }

      if (Direction == Direction.Horizontal)
      {
        if (IsAdjustChildrenSize)
        {
          var h = Height();
          var w = Width();
          var availableWidth = w - Gap * (Children.Count + 1);
          var itemSize = availableWidth / Children.Count;
          var totalW = 0f;

          for (var i = 0; i < Children.Count; i++)
          {
            Children[i].Style.Height = h - Gap * 2;
            Children[i].Style.Y = Gap;
            Children[i].Style.X = Gap + totalW;
            Children[i].Style.Width = itemSize;
            totalW += itemSize + Gap;
          }
        }
        else
        {
          var h = Height();
          var totalW = 0f;

          for (var i = 0; i < Children.Count; i++)
          {
            Children[i].Style.Height = h - Gap * 2;
            Children[i].Style.Y = Gap;
            Children[i].Style.X = Gap + totalW;
            totalW += Children[i].Width() + Gap;
          }

          Style.Width = totalW;
        }
      }
    };
  }
}