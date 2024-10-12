using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Button : EasyUI_Element
{
  public string BaseColor = "#545454";
  public string HoverColor = "#646464";

  public EasyUI_Button()
  {
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlignment = TextAlignment.Center;
    Style.SetBackgroundColor(BaseColor);
    Style.SetTextColor("#c0c0c0");
    Style.SetBorderRadius(8f);
    Style.BorderWidth = new Vector4(2, 2, 2, 2);
    Style.SetBorderColor(new Vector4(0.1f, 0.1f, 0.1f, 1f));

    /*Style.BorderColor = new[]
    {
      new Vector4(1f, 0.1f, 0.1f, 1f),
      new Vector4(0.1f, 1f, 0.1f, 1f),
      new Vector4(0.1f, 0.1f, 1f, 1f),
      new Vector4(0.5f, 0.4f, 0.5f, 1f)
    };*/

    var isOver = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.SetBackgroundColor("#646464");
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.SetBackgroundColor(HoverColor);
    };
    Events.OnMouseDown += () => { Style.SetBackgroundColor("#ae5c00"); };
    Events.OnMouseUp += () => { Style.SetBackgroundColor(HoverColor); };
    Events.OnRender += (delta) =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.Pointer;
    };
  }
}