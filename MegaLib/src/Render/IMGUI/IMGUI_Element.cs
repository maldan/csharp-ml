using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Color;

namespace MegaLib.Render.IMGUI;

public class RenderData
{
  public List<Vector3> Vertices = [];
  public List<Vector4> Colors = [];
  public List<Vector2> UV = [];
  public List<uint> Indices = [];
  private uint _indexOffset;

  // Очищаем вершины и прочий контент
  public void Clear()
  {
    Vertices.Clear();
    Colors.Clear();
    UV.Clear();
    Indices.Clear();
    _indexOffset = 0;
  }

  public void DrawOutline(Rectangle area, float[] width, Vector4[] color)
  {
    DrawRectangle(
      new Rectangle(area.FromX, area.FromY, area.FromX + width[0], area.ToY),
      color[0]
    );

    DrawRectangle(
      new Rectangle(area.FromX, area.FromY, area.ToX, area.FromY + width[1]),
      color[1]
    );

    DrawRectangle(
      new Rectangle(area.FromX + area.Width - width[2], area.FromY, area.FromX + area.Width, area.ToY),
      color[2]
    );

    DrawRectangle(
      new Rectangle(area.FromX, area.FromY + area.Height, area.ToX, area.FromY + area.Height - width[3]),
      color[3]
    );
  }

  public void DrawRectangle(Rectangle area, Vector4 color)
  {
    var pivot = new Vector3(0.5f, 0.5f, 0);
    var size = new Vector3(area.Width, area.Height, 1);
    var offset = new Vector2(area.FromX, area.FromY);

    Vertices.AddRange([
      (new Vector3(-0.5f, -0.5f, 0) + pivot) * size + offset,
      (new Vector3(-0.5f, 0.5f, 0) + pivot) * size + offset,
      (new Vector3(0.5f, 0.5f, 0) + pivot) * size + offset,
      (new Vector3(0.5f, -0.5f, 0) + pivot) * size + offset
    ]);
    UV.AddRange([
      new Vector2(0, 0) + new Vector2(-1, -1),
      new Vector2(0, 1) + new Vector2(-1, -1),
      new Vector2(1, 1) + new Vector2(-1, -1),
      new Vector2(1, 0) + new Vector2(-1, -1)
    ]);
    Colors.AddRange([
      color, color, color, color
    ]);
    Indices.AddRange([
      0 + _indexOffset, 1 + _indexOffset, 2 + _indexOffset,
      0 + _indexOffset, 2 + _indexOffset, 3 + _indexOffset
    ]);
    _indexOffset += 4;
  }

  public Rectangle DrawText(
    string text,
    FontData fontData,
    Vector2 position,
    Vector4 color,
    Rectangle drawArea
  )
  {
    if (text == null) return new Rectangle();
    if (text.Length == 0) return new Rectangle();

    var maxLineHeight = 0f;
    var offset = new Vector2(0, 0);
    // uint indexOffset = 0;

    var textSize = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
    var vectorList = new List<Vector3>();

    for (var i = 0; i < text.Length; i++)
    {
      var symbol = text[i];
      if (symbol == '\n')
      {
        offset.X = 0;
        offset.Y += maxLineHeight;
        maxLineHeight = 0;
        continue;
      }

      var area = fontData.GetGlyph(text[i]);
      if (area.TextureArea.IsEmpty) continue;

      var lt = new Vector2(0, 0) * new Vector2(area.TextureArea.Width, area.TextureArea.Height) + offset + position;
      var rt = new Vector2(1, 1) * new Vector2(area.TextureArea.Width, area.TextureArea.Height) + offset + position;
      var symbolArea = new Rectangle(lt.X, lt.Y, rt.X, rt.Y);

      // Если символ находится за пределами области рисования
      if (!drawArea.IsInsideOrIntersects(symbolArea))
      {
        continue;
      }

      // Высчитываем размер текста
      textSize.FromX = Math.Min(textSize.FromX, symbolArea.FromX);
      textSize.FromY = Math.Min(textSize.FromY, symbolArea.FromY);
      textSize.ToX = Math.Max(textSize.ToX, symbolArea.ToX);
      textSize.ToY = Math.Max(textSize.ToY, symbolArea.ToY);

      vectorList.AddRange([
        new Vector3(0, 0, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position,
        new Vector3(1, 0, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position,
        new Vector3(1, 1, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position,
        new Vector3(0, 1, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position
      ]);

      Colors.AddRange([color, color, color, color]);

      var uv = area.TextureArea.ToUV(fontData.Texture.RAW.Width, fontData.Texture.RAW.Height);
      UV.AddRange([
        new Vector2(uv.FromX, uv.FromY),
        new Vector2(uv.ToX, uv.FromY),
        new Vector2(uv.ToX, uv.ToY),
        new Vector2(uv.FromX, uv.ToY)
      ]);
      Indices.AddRange([
        0 + _indexOffset, 1 + _indexOffset, 2 + _indexOffset,
        0 + _indexOffset, 2 + _indexOffset, 3 + _indexOffset
      ]);
      _indexOffset += 4;

      offset.X += area.Width;
      maxLineHeight = Math.Max(maxLineHeight, area.TextureArea.Height);
    }

    for (var i = 0; i < vectorList.Count; i++)
    {
      vectorList[i] += new Vector3(drawArea.Width / 2f, drawArea.Height / 2f, 0);
      vectorList[i] -= new Vector3(textSize.Width / 2f, textSize.Height / 2f, 0);
    }

    Vertices.AddRange(vectorList);

    return textSize;
  }
}

public class ElementStyle
{
  public object Width = "auto";
  public object Height = "auto";
  public object Color = new Vector4(1, 1, 1, 1);
  public object BackgroundColor;
  public object Padding;
  public object Margin;
  public object Gap;
  public string Position;
  public object Left;
  public object Top;
  public object Right;
  public object Bottom;
  public string FlexDirection;
}

public class ElementEvents
{
  public Action OnClick;
  public Action OnMouseOver;
  public Action OnMouseOut;
  public Action<float> OnRender;
}

public struct BuildIn
{
  public Rectangle DrawArea;
  public FontData FontData;
  public float ZIndex;
  public IMGUI_Element Parent;
  public float Delta;
}

public struct BuildOut
{
  public Rectangle DrawArea;
  public float ZIndex;
}

public class IMGUI_Element
{
  public string Id;
  public bool IsVisible = true;
  public ElementStyle Style = new();
  public List<RenderData> RenderData = [];
  public ElementEvents Events = new();
  public List<Triangle> Collision = [];
  public List<IMGUI_Element> Children = [];
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

  public virtual BuildOut Build(BuildIn buildArgs)
  {
    Clear();

    // Рисуем основное тело
    var bg = BackgroundColor();
    var re = new RenderData();

    var boundingBox = BoundingBox(buildArgs.DrawArea);
    boundingBox += new Vector2(buildArgs.DrawArea.FromX, buildArgs.DrawArea.FromY);
    // area += pa;

    // Инициализация коллизии
    var hasCollision = Events.OnMouseOver != null || Events.OnMouseOut != null || Events.OnClick != null;
    if (hasCollision)
    {
      InitCollision(boundingBox);
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
    var yOffset = 0f;
    var maxX = 0f;
    var parentPadding = Padding();
    var parentMargin = Margin();
    var parentGap = Gap();
    var marginOffsetY = 0f;
    for (var i = 0; i < Children.Count; i++)
    {
      var childMargin = Children[i].Margin();

      var localBB = boundingBox;
      localBB.FromX += parentPadding.X;
      localBB.FromY += parentPadding.Y;
      localBB.ToX -= parentPadding.X * 2;
      localBB.ToY -= parentPadding.Y * 2;

      localBB.FromY += childMargin.Y;
      localBB.ToY += childMargin.Y;
      marginOffsetY += childMargin.Y;

      var buildOut = Children[i].Build(new BuildIn()
      {
        Parent = this,
        DrawArea = localBB + new Vector2(0, yOffset),
        Delta = buildArgs.Delta,
        FontData = buildArgs.FontData
      });
      yOffset += buildOut.DrawArea.Height;
      maxX = Math.Max(maxX, buildOut.DrawArea.Width);

      if (i < Children.Count - 1) yOffset += parentGap;

      // Копируем содержимое чилда
      RenderData.AddRange(Children[i].RenderData);
    }

    // Добавляем текстовый контент
    if (!string.IsNullOrEmpty(Text))
    {
      var rd = new RenderData();
      var textSize = rd.DrawText(
        Text,
        buildArgs.FontData,
        new Vector2(boundingBox.FromX, boundingBox.FromY),
        TextColor(),
        boundingBox);
      RenderData.Add(rd);

      /*var rd2 = new RenderData();
      rd2.DrawRectangle(textSize + new Vector2(boundingBox.FromX, boundingBox.FromY), new Vector4(1, 1, 0, 0.5f));
      RenderData.Add(rd2);*/
    }

    // Рисуем основной квадрат. Ставим его на первое место
    if (Style.Height is "auto")
    {
      boundingBox.ToY += yOffset + parentPadding.Y * 2 + marginOffsetY;
    }

    if (Style.Width is "auto")
    {
      boundingBox.ToX += maxX + parentPadding.X * 2;
    }

    re.DrawRectangle(boundingBox, bg);
    re.DrawOutline(boundingBox, [1, 2, 3, 4], [
      new Vector4(1, 1, 1, 1),
      new Vector4(0, 1, 1, 1),
      new Vector4(0, 0, 1, 1),
      new Vector4(1, 0, 1, 1)
    ]);

    RenderData.Insert(0, re);

    Events?.OnRender?.Invoke(buildArgs.Delta);

    return new BuildOut
    {
      ZIndex = buildArgs.ZIndex + 1,
      DrawArea = boundingBox + new Vector2(0, yOffset)
    };
  }

  public float Width(Rectangle parentArea = default)
  {
    if (Style.Width is string s)
    {
      if (s.EndsWith('%'))
      {
        var v = float.Parse(s[..^1]);
        return parentArea.Width * (v / 100f);
      }
    }

    if (Style.Width is "auto") return 0;

    return Style.Width switch
    {
      float v => v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => 0
    };
  }

  public float Height(Rectangle parentArea = default)
  {
    if (Style.Height is string s)
    {
      if (s.EndsWith('%'))
      {
        var v = float.Parse(s[..^1]);
        return parentArea.Height * (v / 100f);
      }
    }

    if (Style.Height is "auto") return 0;

    return Style.Height switch
    {
      float v => v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => 0
    };
  }

  public float Gap()
  {
    return Style.Gap switch
    {
      float v => v,
      int v => v,
      double v => (float)v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => 0
    };
  }

  public Vector4 Padding()
  {
    return Style.Padding switch
    {
      Vector4 v => v,
      _ => new Vector4()
    };
  }

  public Vector4 Margin()
  {
    return Style.Margin switch
    {
      Vector4 v => v,
      _ => new Vector4()
    };
  }

  public Vector2 Position()
  {
    var o = new Vector2();

    o.X = Style.Left switch
    {
      float v => v,
      double v => (float)v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => o.X
    };

    o.Y = Style.Top switch
    {
      float v => v,
      double v => (float)v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => o.Y
    };

    return o;
  }

  public Vector2 Size(Rectangle parentArea = default)
  {
    return new Vector2
    {
      X = Width(parentArea),
      Y = Height(parentArea)
    };
  }

  public Rectangle BoundingBox(Rectangle parentArea = default)
  {
    var p = Position();
    var s = Size(parentArea);
    return Rectangle.FromLeftTopWidthHeight(p.X, p.Y, s.X, s.Y);
  }

  public Vector4 TextColor()
  {
    if (Style.Color is Vector4 v) return v;
    return Style.Color switch
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

/*public class IMGUI_Element
{
  public string Id;
  public Vector3 Position;
  public Vector2 Size;
  public List<Triangle> Collision = [];

  public List<Vector3> Vertices = [];
  public List<Vector4> Colors = [];
  public List<Vector2> UV = [];
  public List<uint> Indices = [];

  public FontData FontData;

  public bool IsVisible = true;

  // Дизайн
  public float Padding;
  public float Margin;
  public Vector4 BackgroundColor;
  public Vector4 TextColor;
  public float Height;

  // События
  public Action OnClick;
  public Action OnMouseOver;
  public Action OnMouseOut;

  // Очищаем вершины и прочий контент
  public void Clear()
  {
    Vertices.Clear();
    Colors.Clear();
    UV.Clear();
    Indices.Clear();
  }

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

  public virtual IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    return new IMGUI_BuildOut { IndexOffset = buildArgs.IndexOffset };
  }

  protected Vector2 GetTextSize(string text)
  {
    if (text == null) return new Vector2();
    if (text.Length == 0) return new Vector2();

    var o = new Vector2(0, FontData.GetGlyph(text[0]).Height);
    var offset = new Vector2(0, 0);
    var maxLineHeight = 0f;

    for (var i = 0; i < text.Length; i++)
    {
      var symbol = text[i];
      if (symbol == '\n')
      {
        offset.X = 0;
        offset.Y += maxLineHeight;
        maxLineHeight = 0;
        continue;
      }

      var area = FontData.GetGlyph(text[i]);
      if (area.TextureArea.IsEmpty) continue;

      offset.X += area.Width;
      maxLineHeight = Math.Max(maxLineHeight, area.TextureArea.Height);

      // Вычисляем максимальный размер
      o.X = Math.Max(o.X, offset.X);
      o.Y = Math.Max(o.Y, offset.Y);
    }

    return o;
  }

  protected uint DoTextCenter(Vector3 position, Vector2 areaSize, string text, Vector4 color, uint indexOffset = 0)
  {
    var textSize = GetTextSize(text) * 0.5f;
    var center = areaSize * 0.5f;

    return DoText(
      position + new Vector3(0, 0, 0.0001f) + center + new Vector2(-textSize.X, -textSize.Y),
      text,
      color,
      indexOffset);
  }

  protected uint DoText(Vector3 position, string text, Vector4 color, uint indexOffset = 0)
  {
    if (text == null) return indexOffset;
    if (text.Length == 0) return indexOffset;

    // var color = new Vector4(1, 1, 1, 1);
    var maxLineHeight = 0f;
    var offset = new Vector2(0, 0);

    // Сначала надо рассчитать

    for (var i = 0; i < text.Length; i++)
    {
      var symbol = text[i];
      if (symbol == '\n')
      {
        offset.X = 0;
        offset.Y += maxLineHeight;
        maxLineHeight = 0;
        continue;
      }

      var area = FontData.GetGlyph(text[i]);
      if (area.TextureArea.IsEmpty) continue;

      Vertices.AddRange([
        new Vector3(0, 0, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position,
        new Vector3(1, 0, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position,
        new Vector3(1, 1, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position,
        new Vector3(0, 1, 0) * new Vector3(area.TextureArea.Width, area.TextureArea.Height, 1) + offset + position
      ]);
      Colors.AddRange([color, color, color, color]);

      var uv = area.TextureArea.ToUV(FontData.Texture.RAW.Width, FontData.Texture.RAW.Height);
      UV.AddRange([
        new Vector2(uv.FromX, uv.FromY),
        new Vector2(uv.ToX, uv.FromY),
        new Vector2(uv.ToX, uv.ToY),
        new Vector2(uv.FromX, uv.ToY)
      ]);
      Indices.AddRange([
        0 + indexOffset, 1 + indexOffset, 2 + indexOffset, 0 + indexOffset, 2 + indexOffset, 3 + indexOffset
      ]);
      indexOffset += 4;

      offset.X += area.Width;
      maxLineHeight = Math.Max(maxLineHeight, area.TextureArea.Height);
    }

    return indexOffset;
  }

  protected uint DoRectangle(Vector3 offset, Vector2 size, Vector4 color, uint indexOffset = 0)
  {
    var pivot = new Vector3(0.5f, 0.5f, 0);
    var size2 = new Vector3(size.X, size.Y, 1);

    Vertices.AddRange([
      (new Vector3(-0.5f, -0.5f, 0) + pivot) * size2 + offset,
      (new Vector3(-0.5f, 0.5f, 0) + pivot) * size2 + offset,
      (new Vector3(0.5f, 0.5f, 0) + pivot) * size2 + offset,
      (new Vector3(0.5f, -0.5f, 0) + pivot) * size2 + offset
    ]);
    UV.AddRange([
      new Vector2(0, 0) + new Vector2(-1, -1),
      new Vector2(0, 1) + new Vector2(-1, -1),
      new Vector2(1, 1) + new Vector2(-1, -1),
      new Vector2(1, 0) + new Vector2(-1, -1)
    ]);
    Colors.AddRange([
      color, color, color, color
    ]);
    Indices.AddRange([
      0 + indexOffset, 1 + indexOffset, 2 + indexOffset,
      0 + indexOffset, 2 + indexOffset, 3 + indexOffset
    ]);
    return indexOffset + 4;
  }

  protected void CopyRenderDataFrom(IMGUI_Element element)
  {
    Vertices.AddRange(element.Vertices);
    UV.AddRange(element.UV);
    Colors.AddRange(element.Colors);
    Indices.AddRange(element.Indices);
  }
}*/