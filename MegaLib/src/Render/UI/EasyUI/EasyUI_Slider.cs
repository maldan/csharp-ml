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

    Style.BackgroundColor = "#232323";
    Style.TextAlign = TextAlignment.Center;

    if (Direction == Direction.Horizontal)
    {
      Style.Width = 128;
      Style.Height = 24;

      _bar = new EasyUI_Element
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
      _bar.Events.OnMouseOver += () => { _bar.Style.BackgroundColor = "#db7400"; };
      _bar.Events.OnMouseOut += () => { _bar.Style.BackgroundColor = "#ae5c00"; };

      Children.Add(_bar);

      _bar.Events.OnMouseDown += () => { isDrag = true; };
      _bar.Events.OnMouseUp += () => { isDrag = false; };

      ValueRender();
      Events.OnRender += delta =>
      {
        if (!isOver) return;
        if (isDrag || Mouse.WheelDirection != 0f)
        {
          if (isDrag) _bar.Style.X = _bar.Position().X + Mouse.ClientDelta.X;
          else _bar.Style.X = _bar.Position().X + Mouse.WheelDirection * delta * 220;

          if (_bar.Position().X < 4) _bar.Style.X = 4;
          if (_bar.Position().X > Size().X - 8 - 4) _bar.Style.X = Size().X - 8 - 4;

          var percentage = (_bar.Position().X - 4) / (Size().X - 8 - 4 - 4);
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
          Height = 8,
          X = 4f,
          Y = 4,
          BackgroundColor = "#ae5c00"
        }
      };
      _bar.Events.OnMouseOver += () => { _bar.Style.BackgroundColor = "#db7400"; };
      _bar.Events.OnMouseOut += () => { _bar.Style.BackgroundColor = "#ae5c00"; };
      Children.Add(_bar);

      _bar.Events.OnMouseDown += () => { isDrag = true; };
      _bar.Events.OnMouseUp += () => { isDrag = false; };
      _bar.Style.Width = Width() - 8;

      ValueRender();
      Events.OnRender += delta =>
      {
        if (!isOver) return;
        if (isDrag || Mouse.WheelDirection != 0f)
        {
          _bar.Style.X = 4;
          _bar.Style.Width = Width() - 8;

          if (isDrag) _bar.Style.Y = _bar.Position().Y + Mouse.ClientDelta.Y;
          else _bar.Style.Y = _bar.Position().Y + Mouse.WheelDirection * delta * -220f;

          if (_bar.Position().Y < 4) _bar.Style.Y = 4;
          if (_bar.Position().Y > Size().Y - 8 - 4) _bar.Style.Y = Size().Y - 8 - 4;

          var percentage = (_bar.Position().Y - 4) / (Size().Y - 8 - 4 - 4);
          Value = percentage.Remap(0, 1, Min, Max);
          Events.OnChange?.Invoke(Value);

          ValueRender();
        }
      };
    }
  }

  public bool OnTopBorder()
  {
    return _bar.Position().Y <= 4;
  }

  public void Scroll(float v)
  {
    _bar.Style.Y = _bar.Position().Y + v;
    if (_bar.Position().Y < 4) _bar.Style.Y = 4;
    if (_bar.Position().Y > Size().Y - 8 - 4) _bar.Style.Y = Size().Y - 8 - 4;

    var percentage = (_bar.Position().Y - 4) / (Size().Y - 8 - 4 - 4);
    Value = percentage.Remap(0, 1, Min, Max);
    Events.OnChange?.Invoke(Value);

    ValueRender();
  }

  private void ValueRender()
  {
    Text = ShowText ? $"{Value:F}".Replace(",", ".") : "";
  }
}