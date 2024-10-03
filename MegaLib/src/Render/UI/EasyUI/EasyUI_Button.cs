using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Button : EasyUI_Element
{
  public EasyUI_Button()
  {
    var baseColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
    Style.BackgroundColor = baseColor;
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlign = TextAlignment.Center;

    var isOver = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1);
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.BackgroundColor = baseColor;
    };
    Events.OnMouseDown += () => { Style.BackgroundColor = new Vector4(0.5f, 0.2f, 0.2f, 1); };
    Events.OnMouseUp += () => { Style.BackgroundColor = baseColor; };
    Events.OnRender += (delta) =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.Pointer;
    };
  }
}