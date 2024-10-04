using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Layout : EasyUI_Element
{
  public Direction Direction = Direction.Vertical;
  public float Gap;

  public EasyUI_Layout()
  {
    Style.BackgroundColor = new Vector4(0, 0, 0, 0.01f);

    Events.OnBeforeRender += (delta) =>
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
      // Console.WriteLine(BoundingBox());
    };
  }
}