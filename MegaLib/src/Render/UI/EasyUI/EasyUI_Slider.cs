using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Slider : EasyUI_Element
{
  public float Min = 0;
  public float Max = 1;

  public EasyUI_Slider()
  {
    Style.Width = 128;
    Style.Height = 24;
    Style.BackgroundColor = "#232323";
    Style.TextAlign = TextAlignment.Center;

    var bar = new EasyUI_Element();
    bar.Style.Width = 8;
    bar.Style.Height = 16;
    bar.Style.X = 4f;
    bar.Style.Y = 4;
    bar.Style.BackgroundColor = "#ae5c00";
    bar.Events.OnMouseOver += () => { bar.Style.BackgroundColor = "#db7400"; };
    bar.Events.OnMouseOut += () => { bar.Style.BackgroundColor = "#ae5c00"; };
    Children.Add(bar);

    var isDrag = false;
    bar.Events.OnMouseDown += () => { isDrag = true; };
    bar.Events.OnMouseUp += () => { isDrag = false; };

    Text = $"{Value:F}";
    Events.OnRender += d =>
    {
      if (!isDrag) return;
      bar.Style.X = bar.Position().X + Mouse.ClientDelta.X;
      if (bar.Position().X < 4) bar.Style.X = 4;
      if (bar.Position().X > Size().X - 8 - 4) bar.Style.X = Size().X - 8 - 4;

      var percentage = (bar.Position().X - 4) / (Size().X - 8 - 4 - 4);
      Value = percentage.Remap(0, 1, Min, Max);
      Events.OnChange?.Invoke(Value);
      Text = $"{Value:F}".Replace(",", ".");
    };
  }
}