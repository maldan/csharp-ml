using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.IMGUI;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ElementEvents
{
  public Action OnClick;
  public Action OnMouseOver;
  public Action OnMouseOut;
  public Action<float> OnRender;
}

public struct EasyUI_BuildIn
{
  public FontData FontData;
  public EasyUI_Element Parent;
  public float Delta;
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

  public void Clear()
  {
    foreach (var rd in RenderData) rd.Clear();
    RenderData.Clear();
  }

  public virtual EasyUI_BuildOut Build(EasyUI_BuildIn buildArgs)
  {
    Clear();

    // Получаем bounding box текущего объекта в локальных координатах
    var elementBoundingBox = BoundingBox();

    // Смещаем относительно родителя
    if (buildArgs.Parent != null) elementBoundingBox += buildArgs.Parent.Position();

    // Инициализация коллизии
    var hasCollision = Events.OnMouseOver != null || Events.OnMouseOut != null || Events.OnClick != null;
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
          // Events?.OnClick?.Invoke();
        }

        if (Mouse.IsKeyUp(MouseKey.Left) && _isMouseDown)
        {
          _isMouseDown = false;
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

        if (Mouse.IsKeyUp(MouseKey.Left))
        {
          _isMouseDown = false;
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
          FontData = buildArgs.FontData
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

    return new EasyUI_BuildOut
    {
    };
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