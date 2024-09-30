using System;
using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ElementEvents
{
  public Action OnClick;
  public Action OnMouseOver;
  public Action OnMouseOut;
  public Action OnMouseDown;
  public Action OnMouseUp;
  public Action<object> OnChange;
  public Action<float> OnRender;
}

public struct EasyUI_BuildIn
{
  public FontData FontData;
  public EasyUI_Element Parent;
  public float Delta;
  public Vector2 ParentPosition;
}

public struct EasyUI_BuildOut
{
}

public class EasyUI_Element
{
  public bool IsVisible = true;
  public EasyUI_ElementStyle Style = new();
  public List<EasyUI_RenderData> RenderData = [];
  public EasyUI_ElementEvents Events = new();
  public List<Triangle> Collision = [];
  public List<EasyUI_Element> Children = [];
  public string Text;
  private bool _isClick;
  private bool _isMouseDown;
  private bool _isMouseOver;
  public object Value; // Пользовательское значение

  public void InitCollision(Rectangle r)
  {
    Collision.Clear();
    Collision.Add(new Triangle
    {
      A = new Vector3(r.FromX, r.FromY, 0),
      B = new Vector3(r.ToX, r.FromY, 0),
      C = new Vector3(r.ToX, r.ToY, 0)
    });
    Collision.Add(new Triangle
    {
      A = new Vector3(r.FromX, r.ToY, 0),
      B = new Vector3(r.FromX, r.FromY, 0),
      C = new Vector3(r.ToX, r.ToY, 0)
    });
  }

  public bool CheckCollision()
  {
    var ray = new Ray(
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, -10),
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, 10)
    );

    for (var i = 0; i < Collision.Count; i++)
    {
      Collision[i].RayIntersection(ray, out _, out var isHit);
      if (isHit) return true;
    }

    return false;
  }

  public virtual void Add(EasyUI_Element element)
  {
    Children.Add(element);
  }

  public void Clear()
  {
    foreach (var rd in RenderData) rd.Clear();
    RenderData.Clear();
  }

  public virtual EasyUI_BuildOut Build(EasyUI_BuildIn buildArgs)
  {
    Clear();
    if (!IsVisible) return new EasyUI_BuildOut();

    // Получаем bounding box текущего объекта в локальных координатах
    var elementBoundingBox = BoundingBox();

    // Смещаем относительно родителя
    elementBoundingBox += buildArgs.ParentPosition;

    // Инициализация коллизии
    var hasCollision = Events.OnMouseDown != null || Events.OnMouseUp != null || Events.OnMouseOver != null ||
                       Events.OnMouseOut != null || Events.OnClick != null;
    if (hasCollision)
    {
      InitCollision(elementBoundingBox);
      if (CheckCollision())
      {
        if (!_isMouseOver)
        {
          _isMouseOver = true;
          Events?.OnMouseOver?.Invoke();
        }

        if (Mouse.IsKeyDown(MouseKey.Left) && !_isMouseDown)
        {
          _isMouseDown = true;
          Events?.OnMouseDown?.Invoke();
        }

        if (Mouse.IsKeyUp(MouseKey.Left) && _isMouseDown)
        {
          _isMouseDown = false;
          Events?.OnMouseUp?.Invoke();
          Events?.OnClick?.Invoke();
        }
      }
      else
      {
        if (_isMouseOver)
        {
          _isMouseOver = false;
          Events?.OnMouseOut?.Invoke();
        }

        if (Mouse.IsKeyUp(MouseKey.Left) && _isMouseDown)
        {
          _isMouseDown = false;
          Events?.OnMouseUp?.Invoke();
        }
      }
    }

    // Проходимся по чилдам
    if (Children.Count > 0)
    {
      var rList = new List<EasyUI_RenderData>();
      for (var i = 0; i < Children.Count; i++)
      {
        Children[i].Build(new EasyUI_BuildIn
        {
          Parent = this,
          FontData = buildArgs.FontData,
          ParentPosition = buildArgs.ParentPosition + Position()
        });

        // Копируем содержимое чилда
        rList.AddRange(Children[i].RenderData);
      }

      // Добавляем в основной список
      RenderData.AddRange(rList);
    }

    // Бэкграунд по bounding box
    var background = new EasyUI_RenderData();
    background.DrawRectangle(elementBoundingBox, BackgroundColor());
    RenderData.Insert(0, background);

    if (Text != "")
    {
      var textData = new EasyUI_RenderData();
      textData.DrawText(
        Text,
        Style.TextAlign,
        buildArgs.FontData,
        new Vector4(1, 1, 1, 1),
        elementBoundingBox
      );
      RenderData.Add(textData);
    }

    // Событие рендера
    Events?.OnRender?.Invoke(buildArgs.Delta);

    return new EasyUI_BuildOut();
  }

  public float Width()
  {
    return Style.Width switch
    {
      float v => v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => 0
    };
  }

  public float Height()
  {
    return Style.Height switch
    {
      float v => v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => 0
    };
  }

  public Vector2 Position()
  {
    var o = new Vector2();

    o.X = Style.X switch
    {
      float v => v,
      double v => (float)v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => o.X
    };

    o.Y = Style.Y switch
    {
      float v => v,
      double v => (float)v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => o.Y
    };

    return o;
  }

  public Vector2 Size()
  {
    return new Vector2
    {
      X = Width(),
      Y = Height()
    };
  }

  public Rectangle BoundingBox()
  {
    var p = Position();
    var s = Size();
    return Rectangle.FromLeftTopWidthHeight(p.X, p.Y, s.X, s.Y);
  }

  public Vector4 TextColor()
  {
    if (Style.TextColor is Vector4 v) return v;
    return Style.TextColor switch
    {
      "white" => new Vector4(1, 1, 1, 1),
      "black" => new Vector4(0, 0, 0, 1),
      "red" => new Vector4(1, 0, 0, 1),
      "green" => new Vector4(0, 1, 0, 1),
      "blue" => new Vector4(0, 0, 1, 1),
      _ => new Vector4(0, 0, 0, 0)
    };
  }

  public Vector4 BackgroundColor()
  {
    if (Style.BackgroundColor is Vector4 v)
    {
      return v;
    }

    return Style.BackgroundColor switch
    {
      "white" => new Vector4(1, 1, 1, 1),
      "black" => new Vector4(0, 0, 0, 1),
      "red" => new Vector4(1, 0, 0, 1),
      "green" => new Vector4(0, 1, 0, 1),
      "blue" => new Vector4(0, 0, 1, 1),
      _ => new Vector4(0, 0, 0, 0)
    };
  }
}

public class EasyUI_Button : EasyUI_Element
{
  public EasyUI_Button()
  {
    var baseColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
    Style.BackgroundColor = baseColor;
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlign = "center";

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

public class EasyUI_Spin : EasyUI_Element
{
  // public Action<float> OnChange;
  private EasyUI_Element _center;

  public EasyUI_Spin()
  {
    var buttonWidth = 24;

    var leftButton = new EasyUI_Button();
    leftButton.Style.Width = buttonWidth;
    leftButton.Text = "-";
    leftButton.Events.OnClick += () => { Events.OnChange?.Invoke(-1); };

    _center = new EasyUI_Element();
    _center.Style.Width = buttonWidth * 2;
    _center.Style.Height = 24;
    _center.Style.X = buttonWidth;
    _center.Style.TextAlign = "center";
    _center.Text = $"0";

    var rightButton = new EasyUI_Button();
    rightButton.Style.Width = buttonWidth;
    rightButton.Style.X = buttonWidth + buttonWidth * 2;
    rightButton.Text = "+";
    rightButton.Events.OnClick += () => { Events.OnChange?.Invoke(1); };

    Children.Add(leftButton);
    Children.Add(_center);
    Children.Add(rightButton);
  }

  public void UpdateLabel(string text)
  {
    _center.Text = text;
  }
}

public class EasyUI_Check : EasyUI_Element
{
  private EasyUI_Element _text;
  // public bool Value;

  public EasyUI_Check()
  {
    var buttonWidth = 24;

    var leftCheck = new EasyUI_Button();
    leftCheck.Style.Width = 24;
    leftCheck.Style.Height = 24;
    leftCheck.Style.BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
    Children.Add(leftCheck);
    leftCheck.Events.OnClick += () =>
    {
      if (Value is bool v) Value = !v;
    };

    var check = new EasyUI_Element();
    check.Style.Width = 24 - 8;
    check.Style.Height = 24 - 8;
    check.Style.X = 4;
    check.Style.Y = 4;
    check.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
    Children.Add(check);

    _text = new EasyUI_Element();
    _text.Style.Width = buttonWidth * 2;
    _text.Style.Height = 24;
    _text.Style.X = buttonWidth;
    _text.Style.TextAlign = "center";
    _text.Text = $"0";
    Children.Add(_text);

    // Чекаем обновления
    Events.OnRender += (d) =>
    {
      if (Value is bool v) check.IsVisible = v;
    };
  }

  public void UpdateLabel(string text)
  {
    _text.Text = text;
  }
}

public class EasyUI_Slider : EasyUI_Element
{
  public float Min = 0;
  public float Max = 1;

  public EasyUI_Slider()
  {
    Style.Width = 128;
    Style.Height = 24;
    Style.BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);

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

    Events.OnRender += d =>
    {
      if (!isDrag) return;
      bar.Style.X = bar.Position().X + Mouse.ClientDelta.X;
      if (bar.Position().X < 0) bar.Style.X = 0;
      if (bar.Position().X > Size().X - 8) bar.Style.X = Size().X - 8;

      var percentage = bar.Position().X / (Size().X - 8);
      Value = percentage.Remap(0, 1, Min, Max);
      Events.OnChange(Value);
    };
  }
}

public class EasyUI_Window : EasyUI_Element
{
  private EasyUI_Element _header;
  private EasyUI_Element _body;
  private EasyUI_Element _minimize;

  public EasyUI_Window()
  {
    _header = new EasyUI_Element();
    _header.Style.Y = -20;
    _header.Style.Width = 80;
    _header.Style.Height = 20;
    _header.Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1f);
    _header.Text = "Window";

    var isDrag = false;
    _header.Events.OnMouseDown += () => { isDrag = true; };
    _header.Events.OnMouseUp += () => { isDrag = false; };
    Children.Add(_header);

    _body = new EasyUI_Element();
    _body.Style.Y = 0;
    _body.Style.Width = 80;
    _body.Style.Height = 40;
    _body.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1f);
    Children.Add(_body);

    _minimize = new EasyUI_Element();
    _minimize.Style.X = 80 - 16;
    _minimize.Style.Y = -20;
    _minimize.Style.Width = 16;
    _minimize.Style.Height = 20;
    _minimize.Style.BackgroundColor = new Vector4(0.25f, 0.0f, 0.0f, 1f);
    _minimize.Text = "_";
    _minimize.Events.OnClick += () => { _body.IsVisible = !_body.IsVisible; };
    Children.Add(_minimize);

    Events.OnRender += delta =>
    {
      if (!isDrag) return;
      Mouse.Cursor = MouseCursor.Move;
      Style.X = Position().X + Mouse.ClientDelta.X;
      Style.Y = Position().Y + Mouse.ClientDelta.Y;
      if (Position().X < 0) Style.X = 0;
      if (Position().Y < 20) Style.Y = 20;
    };
  }

  public void SetSize(float width, float height)
  {
    _header.Style.Width = width;
    _minimize.Style.X = width - 16;
    _body.Style.Width = width;
    _body.Style.Height = height;
    Style.Width = width;
    Style.Height = height;
  }

  public override void Add(EasyUI_Element element)
  {
    _body.Add(element);
  }
}