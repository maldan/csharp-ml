using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;

namespace MegaLib.Render.IMGUI;

public struct IMGUI_BuildArgs
{
  public uint IndexOffset;
  public FontData FontData;
}

public struct IMGUI_BuildOut
{
  public uint IndexOffset;
  public float Height;
}

public class IMGUI_Element
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
}