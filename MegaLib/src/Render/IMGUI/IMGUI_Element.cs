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

  // Очищаем вершины и прочий контент
  public void Clear()
  {
    Vertices.Clear();
    Colors.Clear();
    UV.Clear();
    Indices.Clear();
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
    Indices.AddRange([0, 1, 2, 0, 2, 3]);
  }

  public void DrawText(
    string text,
    FontData fontData,
    Vector2 position,
    Vector4 color,
    Rectangle drawArea
  )
  {
    if (text == null) return;
    if (text.Length == 0) return;

    var maxLineHeight = 0f;
    var offset = new Vector2(0, 0);
    uint indexOffset = 0;

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

      Vertices.AddRange([
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
        0 + indexOffset, 1 + indexOffset, 2 + indexOffset, 0 + indexOffset, 2 + indexOffset, 3 + indexOffset
      ]);
      indexOffset += 4;

      offset.X += area.Width;
      maxLineHeight = Math.Max(maxLineHeight, area.TextureArea.Height);
    }
  }
}

public class ElementStyle
{
  public object Width;
  public object Height;
  public string Color;
  public object BackgroundColor;
  public string Padding;
  public string Margin;
  public string Gap;
  public string Position;
  public object Left;
  public object Top;
  public object Right;
  public object Bottom;
  public string FlexDirection;

  public Vector4 BackgroundColorValue()
  {
    if (BackgroundColor is Vector4 v)
    {
      return v;
    }

    return BackgroundColor switch
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
    var bg = Style.BackgroundColorValue();
    var re = new RenderData();

    var boundingBox = BoundingBox(buildArgs.DrawArea);
    boundingBox += new Vector2(buildArgs.DrawArea.FromX, buildArgs.DrawArea.FromY);
    // area += pa;

    re.DrawRectangle(boundingBox, bg);
    RenderData.Add(re);

    // Проходимся по чилдам
    var yOffset = 0f;
    for (var i = 0; i < Children.Count; i++)
    {
      var buildOut = Children[i].Build(new BuildIn()
      {
        Parent = this,
        DrawArea = boundingBox + new Vector2(0, yOffset),
        Delta = buildArgs.Delta
      });
      yOffset += buildOut.DrawArea.Height;

      // Копируем содержимое чилда
      RenderData.AddRange(Children[i].RenderData);
    }

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

    return Style.Width switch
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
    var o = new Vector2();

    o.X = Width(parentArea);

    o.Y = Style.Height switch
    {
      float v => v,
      int v => v,
      string v => string.IsNullOrEmpty(v) ? 0 : float.Parse(v),
      _ => o.Y
    };

    return o;
  }

  public Rectangle BoundingBox(Rectangle parentArea = default)
  {
    var p = Position();
    var s = Size(parentArea);
    return Rectangle.FromLeftTopWidthHeight(p.X, p.Y, s.X, s.Y);
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