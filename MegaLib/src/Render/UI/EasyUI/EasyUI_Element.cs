using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Element
{
  public bool IsVisible = true;
  public bool IsOutsideVisibleArea;

  public EasyUI_ElementStyle Style = new();
  public List<EasyUI_RenderData> RenderData = [];
  public EasyUI_ElementEvents Events = new();
  public List<Triangle> Collision = [];
  public List<EasyUI_Element> Children = [];
  public string Text;
  private bool _isClick;
  private bool _isMouseDown;
  private bool _isMouseDownIgnore;
  private bool _isMouseOver;
  public object Value; // Пользовательское значение
  protected FontData CurrentFontData;
  public Rectangle StencilRectangle;

  private float _lastClickTime;
  private int _clickCount;
  private float _timer;

  public EasyUI_Element Parent;
  public Layer_EasyUI LayerEasyUi;

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

  public virtual void Remove(EasyUI_Element element)
  {
    Children.Remove(element);
  }

  public void Clear()
  {
    foreach (var rd in RenderData) rd.Clear();
    RenderData.Clear();
  }

  protected void HandleMouseEvents()
  {
    var isHovering = CheckCollision();

    // Если курсор над элементом. Вызываем OnMouseOver
    if (isHovering && !_isMouseOver)
    {
      _isMouseOver = true;
      Events?.OnMouseOver?.Invoke();
    }

    // Если курсор покинул элемент
    if (!isHovering && _isMouseOver)
    {
      _isMouseOver = false;
      Events?.OnMouseOut?.Invoke();
    }

    // Сбрасываем статус игнора если мышь отпущена
    if (Mouse.IsKeyUp(MouseKey.Left))
    {
      _isMouseDownIgnore = false;
    }

    // Нажал мышь за пределами элемента
    if (!_isMouseOver && Mouse.IsKeyDown(MouseKey.Left)) _isMouseDownIgnore = true;

    // Нажал мышь над элементом
    if (_isMouseOver && Mouse.IsKeyDown(MouseKey.Left) && !_isMouseDownIgnore && !_isMouseDown)
    {
      _isMouseDown = true;
      Events?.OnMouseDown?.Invoke();
    }

    // Клик только если мышь опущена на элементе
    if (_isMouseDown && _isMouseOver && Mouse.IsKeyUp(MouseKey.Left))
    {
      var currentTime = _timer; // Предположим, что у вас есть способ получить текущее время
      if (currentTime - _lastClickTime <= 0.3f)
      {
        _clickCount++;
      }
      else
      {
        _clickCount = 1; // Сброс количества кликов
      }

      _lastClickTime = currentTime;

      if (_clickCount == 2)
      {
        Events?.OnDoubleClick?.Invoke();
        _clickCount = 0; // Сброс после двойного клика
      }
      else
      {
        Events?.OnClick?.Invoke();
      }
    }

    // Отпустил мышь с нажатого элемента, причем неважно, где курсор
    if (_isMouseDown && Mouse.IsKeyUp(MouseKey.Left))
    {
      _isMouseDown = false;
      Events?.OnMouseUp?.Invoke();
    }
  }

  public virtual EasyUI_BuildOut Build(EasyUI_BuildIn buildArgs)
  {
    _timer += buildArgs.Delta;
    LayerEasyUi = buildArgs.LayerEasyUi;
    Parent = buildArgs.Parent;

    Clear();
    if (!IsVisible || IsOutsideVisibleArea) return new EasyUI_BuildOut();

    var stencilRectangleStack = new List<Rectangle>();
    if (buildArgs.StencilRectangleStack != null)
    {
      stencilRectangleStack.AddRange(buildArgs.StencilRectangleStack);
    }

    // buildArgs.StencilRectangleStack ??= [];

    CurrentFontData = buildArgs.FontData;

    // Событие до основного рендера
    Events?.OnBeforeRender?.Invoke(buildArgs.Delta);

    // Получаем bounding box текущего объекта в локальных координатах
    var elementBoundingBox = BoundingBox();

    // Смещаем относительно родителя
    elementBoundingBox += buildArgs.ParentPosition;

    // Чекаем если элемент за пределами масок
    for (var i = 0; i < stencilRectangleStack.Count; i++)
    {
      if (!elementBoundingBox.IsIntersects(stencilRectangleStack[i]))
      {
        return new EasyUI_BuildOut();
      }
    }

    // Инициализация коллизии
    var hasCollision = Events.OnMouseDown != null || Events.OnMouseUp != null || Events.OnMouseOver != null ||
                       Events.OnMouseOut != null || Events.OnClick != null;
    if (hasCollision)
    {
      InitCollision(elementBoundingBox);
      HandleMouseEvents();
    }

    if (!StencilRectangle.IsEmpty)
    {
      var newMask = StencilRectangle + buildArgs.ParentPosition;
      for (var i = 0; i < stencilRectangleStack.Count; i++)
      {
        newMask = newMask.GetIntersection(stencilRectangleStack[i]);
      }

      stencilRectangleStack.Add(newMask);
    }

    // Установка стенсила если есть
    var stencilId = stencilRectangleStack.Count;

    if (!StencilRectangle.IsEmpty)
    {
      // Начинаем стенсил
      var stencil = new EasyUI_RenderData
      {
        IsStencilStart = true,
        StencilId = stencilId
      };
      stencil.DrawRectangle(stencilRectangleStack.Last(), new Vector4());
      RenderData.Add(stencil);
    }

    // Бэкграунд по bounding box
    var bgColor = BackgroundColor();
    if (bgColor.A > 0)
    {
      var background = new EasyUI_RenderData();
      background.DrawRectangle(elementBoundingBox, BackgroundColor());
      background.BorderRadius = BorderRadius();
      RenderData.Add(background);
    }

    if (Style.BorderWidth != null)
    {
      var outline = new EasyUI_RenderData();
      outline.DrawOutline(elementBoundingBox, BorderWidth(), BorderColor());
      RenderData.Add(outline);
    }

    if (Text != "")
    {
      var textData = new EasyUI_RenderData
      {
        IsText = true
      };
      textData.DrawText(
        Text,
        Style.TextAlign,
        buildArgs.FontData,
        TextColor(),
        elementBoundingBox
      );
      RenderData.Add(textData);
    }

    // Проходимся по чилдам
    if (Children.Count > 0)
    {
      var rList = new List<EasyUI_RenderData>();
      for (var i = 0; i < Children.Count; i++)
      {
        var child = Children[i];
        child.Build(buildArgs with
        {
          Parent = this,
          ParentPosition = buildArgs.ParentPosition + Position(),
          StencilRectangleStack = stencilRectangleStack.ToArray().ToList()
        });

        // Копируем содержимое чилда
        rList.AddRange(child.RenderData);
      }

      // Добавляем в основной список
      RenderData.AddRange(rList);
    }

    // Завершаем стенцил если установлен
    if (!StencilRectangle.IsEmpty)
    {
      var stencil = new EasyUI_RenderData
      {
        IsStencilStop = true,
        StencilId = stencilId
      };
      RenderData.Add(stencil);
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
      //string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => 0
    };
  }

  public float Height()
  {
    return Style.Height switch
    {
      float v => v,
      int v => v,
      //string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
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

    o.X = MathF.Floor(o.X);
    o.Y = MathF.Floor(o.Y);

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

    if (Style.TextColor is string s && s[0] == '#')
    {
      switch (s.Length)
      {
        case 7:
          return RGB<float>.FromHex(s).Vector3.AddW(1.0f);
        case 9:
          return RGBA<float>.FromHex(s).Vector4;
      }
    }

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
    if (Style.BackgroundColor is Vector4 v) return v;

    if (Style.BackgroundColor is string s && s[0] == '#')
    {
      switch (s.Length)
      {
        case 7:
          return RGB<float>.FromHex(s).Vector3.AddW(1.0f);
        case 9:
          return RGBA<float>.FromHex(s).Vector4;
      }
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

  public Vector4 BorderRadius()
  {
    return Style.BorderRadius switch
    {
      Vector4 v1 => v1,
      int i => new Vector4(i, i, i, i),
      float i => new Vector4(i, i, i, i),
      double i => new Vector4((float)i, (float)i, (float)i, (float)i),
      _ => new Vector4()
    };
  }

  public Vector4[] BorderColor()
  {
    if (Style.BorderColor is string s && s[0] == '#')
    {
      switch (s.Length)
      {
        case 7:
          var v = RGB<float>.FromHex(s).Vector3.AddW(1.0f);
          return [v, v, v, v];
        case 9:
          var vv = RGBA<float>.FromHex(s).Vector4;
          return [vv, vv, vv, vv];
      }
    }

    return Style.BorderColor switch
    {
      Vector4 v1 => [v1, v1, v1, v1],
      Vector4[] { Length: 1 } vv => [vv[0], vv[0], vv[0], vv[0]],
      Vector4[] { Length: 4 } v => v,
      _ => [Vector4.One, Vector4.One, Vector4.One, Vector4.One]
    };
  }

  public float[] BorderWidth()
  {
    return Style.BorderWidth switch
    {
      float[] v => v,
      Vector4 v => [v.X, v.Y, v.Z, v.W],
      int v => [v, v, v, v],
      float v => [v, v, v, v],
      double v => [(float)v, (float)v, (float)v, (float)v],
      _ => [0, 0, 0, 0]
    };
  }
}