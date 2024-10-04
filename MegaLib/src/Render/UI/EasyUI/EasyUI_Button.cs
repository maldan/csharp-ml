using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Button : EasyUI_Element
{
  public EasyUI_Button()
  {
    var baseColor = "#545454";
    Style.BackgroundColor = baseColor;
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlign = TextAlignment.Center;
    Style.TextColor = "#c0c0c0";

    var isOver = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.BackgroundColor = "#646464";
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.BackgroundColor = baseColor;
    };
    Events.OnMouseDown += () => { Style.BackgroundColor = "#ae5c00"; };
    Events.OnMouseUp += () => { Style.BackgroundColor = baseColor; };
    Events.OnRender += (delta) =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.Pointer;
    };
  }
}