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

  private EasyUI_Element _bar;

  public float BarSize = 8;

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
    var isDrag = false;
    Events.OnMouseOver += () =>
    {
      LayerEasyUi.ScrollElementStack.Push(this);
      isOver = true;
    };
    Events.OnMouseOut += () =>
    {
      if (LayerEasyUi.ScrollElementStack.Count > 0) LayerEasyUi.ScrollElementStack.Pop();
      isOver = false;
    };

    Style.SetBackgroundColor("#232323");
    Style.TextAlignment = TextAlignment.Center;
    Style.SetBorderRadius(2f);

    if (Direction == Direction.Horizontal)
    {
      Style.Width = 128;
      Style.Height = 24;

      _bar = new EasyUI_Element
      {
        Style =
        {
          Width = BarSize,
          Height = 16,
          X = 4f,
          Y = 4
          //BackgroundColor = "#ae5c00"
        }
      };
      _bar.Style.SetBackgroundColor("#ae5c00");
      _bar.Style.SetBorderRadius(2f);
      _bar.Events.OnMouseOver += () => { _bar.Style.SetBackgroundColor("#db7400"); };
      _bar.Events.OnMouseOut += () => { _bar.Style.SetBackgroundColor("#ae5c00"); };

      Children.Add(_bar);

      _bar.Events.OnMouseDown += () => { isDrag = true; };
      _bar.Events.OnMouseUp += () => { isDrag = false; };

      ValueRender();
      Events.OnRender += delta =>
      {
        if (!isOver) return;
        if (isDrag || Mouse.WheelDirection != 0f)
        {
          if (isDrag) _bar.Style.X = _bar.Style.Position.X + Mouse.ClientDelta.X;
          else _bar.Style.X = _bar.Style.Position.X + Mouse.WheelDirection * delta * 220;

          if (_bar.Style.Position.X < 4) _bar.Style.X = 4;
          if (_bar.Style.Position.X > Style.Width - BarSize - 4) _bar.Style.X = Style.Width - BarSize - 4;

          var percentage = (_bar.Style.Position.X - 4) / (Style.Width - BarSize - 4 - 4);
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

      _bar = new EasyUI_Element
      {
        Style =
        {
          Width = 16,
          Height = BarSize,
          X = 4f,
          Y = 4
        }
      };
      _bar.Style.SetBackgroundColor("#ae5c00");
      _bar.Events.OnMouseOver += () => { _bar.Style.SetBackgroundColor("#db7400"); };
      _bar.Events.OnMouseOut += () => { _bar.Style.SetBackgroundColor("#ae5c00"); };
      Children.Add(_bar);

      _bar.Events.OnMouseDown += () => { isDrag = true; };
      _bar.Events.OnMouseUp += () => { isDrag = false; };
      _bar.Style.Width = Style.Width - 8;
      _bar.Style.SetBorderRadius(2);

      ValueRender();
      Events.OnBeforeRender += delta =>
      {
        _bar.Style.X = 4;
        _bar.Style.Width = Style.Width - 8;
        _bar.Style.Height = BarSize;

        if (!isOver) return;
        if (isDrag || Mouse.WheelDirection != 0f)
        {
          if (isDrag) _bar.Style.Y = _bar.Style.Position.Y + Mouse.ClientDelta.Y;
          else _bar.Style.Y = _bar.Style.Position.Y + Mouse.WheelDirection * delta * -220f;

          if (_bar.Style.Position.Y < 4) _bar.Style.Y = 4;
          if (_bar.Style.Position.Y > Style.Height - BarSize - 4) _bar.Style.Y = Style.Height - BarSize - 4;

          var percentage = (_bar.Style.Position.Y - 4) / (Style.Height - BarSize - 4 - 4);
          Value = percentage.Remap(0, 1, Min, Max);
          Events.OnChange?.Invoke(Value);

          ValueRender();
        }
      };
    }
  }

  public bool OnTopBorder()
  {
    return _bar.Style.Position.Y <= 4;
  }

  public void Scroll(float v)
  {
    if (v == 0) return;

    _bar.Style.Y = _bar.Style.Position.Y + v;
    if (_bar.Style.Position.Y < 4) _bar.Style.Y = 4;
    if (_bar.Style.Position.Y > Style.Height - BarSize - 4) _bar.Style.Y = Style.Height - BarSize - 4;

    var percentage = (_bar.Style.Position.Y - 4) / (Style.Height - BarSize - 4 - 4);
    Value = percentage.Remap(0, 1, Min, Max);
    Events.OnChange?.Invoke(Value);

    ValueRender();
  }

  private void ValueRender()
  {
    Text = ShowText ? $"{Value:F}".Replace(",", ".") : "";
  }
}