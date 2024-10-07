using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Button : EasyUI_Element
{
  public EasyUI_Button()
  {
    var baseColor = "#545454";
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlignment = TextAlignment.Center;
    Style.SetBackgroundColor(baseColor);
    Style.SetTextColor("#c0c0c0");
    Style.SetBorderRadius(8f);
    Style.BorderWidth = new Vector4(2, 2, 2, 2);
    Style.SetBorderColor(new Vector4(0.1f, 0.1f, 0.1f, 1f));

    var isOver = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.SetBackgroundColor("#646464");
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.SetBackgroundColor(baseColor);
    };
    Events.OnMouseDown += () => { Style.SetBackgroundColor("#ae5c00"); };
    Events.OnMouseUp += () => { Style.SetBackgroundColor(baseColor); };
    Events.OnRender += (delta) =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.Pointer;
    };
  }
}