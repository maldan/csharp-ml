using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Slider : EasyUI_Element
{
  public float Min = 0;
  public float Max = 1;
  private Direction _direction = Direction.Horizontal;
  public bool ShowText = true;

  public Direction Direction
  {
    get => _direction;
    set
    {
      _direction = value;
      BuildElement();
    }
  }

  public EasyUI_Slider()
  {
    BuildElement();
  }

  private void BuildElement()
  {
    Value = 0f;
    Children.Clear();
    Events.OnMouseOver = null;
    Events.OnMouseOut = null;
    Events.OnRender = null;

    var isOver = false;
    Events.OnMouseOver += () => { isOver = true; };
    Events.OnMouseOut += () => { isOver = false; };

    Style.BackgroundColor = "#232323";
    Style.TextAlign = TextAlignment.Center;

    if (Direction == Direction.Horizontal)
    {
      Style.Width = 128;
      Style.Height = 24;

      var bar = new EasyUI_Element
      {
        Style =
        {
          Width = 8,
          Height = 16,
          X = 4f,
          Y = 4,
          BackgroundColor = "#ae5c00"
        }
      };
      bar.Events.OnMouseOver += () => { bar.Style.BackgroundColor = "#db7400"; };
      bar.Events.OnMouseOut += () => { bar.Style.BackgroundColor = "#ae5c00"; };

      Children.Add(bar);

      var isDrag = false;
      bar.Events.OnMouseDown += () => { isDrag = true; };
      bar.Events.OnMouseUp += () => { isDrag = false; };

      ValueRender();
      Events.OnRender += delta =>
      {
        if (!isOver) return;
        if (isDrag || Mouse.WheelDirection != 0f)
        {
          if (isDrag) bar.Style.X = bar.Position().X + Mouse.ClientDelta.X;
          else bar.Style.X = bar.Position().X + Mouse.WheelDirection * delta * 220;

          if (bar.Position().X < 4) bar.Style.X = 4;
          if (bar.Position().X > Size().X - 8 - 4) bar.Style.X = Size().X - 8 - 4;

          var percentage = (bar.Position().X - 4) / (Size().X - 8 - 4 - 4);
          Value = percentage.Remap(0, 1, Min, Max);
          Events.OnChange?.Invoke(Value);

          ValueRender();
        }
      };
    }
    else
    {
      Style.Width = 24;
      Style.Height = 128;

      var bar = new EasyUI_Element
      {
        Style =
        {
          Width = 16,
          Height = 8,
          X = 4f,
          Y = 4,
          BackgroundColor = "#ae5c00"
        }
      };
      bar.Events.OnMouseOver += () => { bar.Style.BackgroundColor = "#db7400"; };
      bar.Events.OnMouseOut += () => { bar.Style.BackgroundColor = "#ae5c00"; };
      Children.Add(bar);

      var isDrag = false;
      bar.Events.OnMouseDown += () => { isDrag = true; };
      bar.Events.OnMouseUp += () => { isDrag = false; };

      ValueRender();
      Events.OnRender += delta =>
      {
        if (!isOver) return;
        if (isDrag || Mouse.WheelDirection != 0f)
        {
          bar.Style.X = 4;
          bar.Style.Width = Width() - 8;

          if (isDrag) bar.Style.Y = bar.Position().Y + Mouse.ClientDelta.Y;
          else bar.Style.Y = bar.Position().Y + Mouse.WheelDirection * delta * -220f;

          if (bar.Position().Y < 4) bar.Style.Y = 4;
          if (bar.Position().Y > Size().Y - 8 - 4) bar.Style.Y = Size().Y - 8 - 4;

          var percentage = (bar.Position().Y - 4) / (Size().Y - 8 - 4 - 4);
          Value = percentage.Remap(0, 1, Min, Max);
          Events.OnChange?.Invoke(Value);

          ValueRender();
        }
      };
    }
  }

  private void ValueRender()
  {
    Text = ShowText ? $"{Value:F}".Replace(",", ".") : "";
  }
}