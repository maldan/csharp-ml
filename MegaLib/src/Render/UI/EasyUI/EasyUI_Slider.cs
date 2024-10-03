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
    Style.BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
    Style.TextAlign = TextAlignment.Center;

    var bar = new EasyUI_Element();
    bar.Style.Width = 8;
    bar.Style.Height = 24;
    bar.Style.X = 0f;
    bar.Style.Y = 0;
    bar.Style.BackgroundColor = new Vector4(0.75f, 0.5f, 0.5f, 1.0f);
    bar.Events.OnMouseOver += () => { bar.Style.BackgroundColor = new Vector4(0.9f, 0.5f, 0.5f, 1.0f); };
    bar.Events.OnMouseOut += () => { bar.Style.BackgroundColor = new Vector4(0.75f, 0.5f, 0.5f, 1.0f); };
    Children.Add(bar);

    var isDrag = false;
    bar.Events.OnMouseDown += () => { isDrag = true; };
    bar.Events.OnMouseUp += () => { isDrag = false; };

    Text = $"{Value:F}";
    Events.OnRender += d =>
    {
      if (!isDrag) return;
      bar.Style.X = bar.Position().X + Mouse.ClientDelta.X;
      if (bar.Position().X < 0) bar.Style.X = 0;
      if (bar.Position().X > Size().X - 8) bar.Style.X = Size().X - 8;

      var percentage = bar.Position().X / (Size().X - 8);
      Value = percentage.Remap(0, 1, Min, Max);
      Events.OnChange(Value);
      Text = $"{Value:F}";
    };
  }
}