using System;
using System.Collections.Generic;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.UI.EasyUI;

/*
public struct StencilRectangle
{
  public Rectangle Rectangle;
}
*/

public class EasyUI_RenderData
{
  public List<Vector3> Vertices = [];
  public List<Vector4> Colors = [];
  public List<Vector2> UV = [];
  public List<uint> Indices = [];
  public List<RO_Line> Lines = [];
  private uint _indexOffset;
  public bool IsText;
  public bool IsLine;
  public bool IsStencilStart;
  public bool IsStencilStop;
  public bool IsIgnore;
  public int StencilId;
  public Vector4 BorderRadius;

  private bool _isChanged;
  private Rectangle _boundingBox;

  public Rectangle BoundingBox
  {
    get
    {
      if (!_isChanged) return _boundingBox;
      CalculateBoundingBox();
      _isChanged = false;
      return _boundingBox;
    }
  }

  // Очищаем вершины и прочий контент
  public void Clear()
  {
    _isChanged = true;

    Vertices.Clear();
    Colors.Clear();
    UV.Clear();
    Indices.Clear();
    _indexOffset = 0;
  }

  public void DrawOutline(Rectangle area, float[] width, Vector4[] color)
  {
    _isChanged = true;

    if (width[0] > 0)
    {
      DrawRectangle(
        new Rectangle(area.FromX, area.FromY, area.FromX + width[0], area.ToY),
        color[0]
      );
    }

    if (width[1] > 0)
    {
      DrawRectangle(
        new Rectangle(area.FromX, area.FromY, area.ToX, area.FromY + width[1]),
        color[1]
      );
    }

    if (width[2] > 0)
    {
      DrawRectangle(
        new Rectangle(area.FromX + area.Width - width[2], area.FromY, area.FromX + area.Width, area.ToY),
        color[2]
      );
    }

    if (width[3] > 0)
    {
      DrawRectangle(
        new Rectangle(area.FromX, area.FromY + area.Height, area.ToX, area.FromY + area.Height - width[3]),
        color[3]
      );
    }
  }

  public void DrawRectangle(Rectangle area, Vector4 color)
  {
    _isChanged = true;

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
    TextAlignment textAlign,
    FontData fontData,
    Vector4 color,
    Rectangle drawArea
  )
  {
    _isChanged = true;
    if (text == null) return new Rectangle();
    if (text.Length == 0) return new Rectangle();

    var maxLineHeight = 0f;
    var offset = new Vector2(0, 0);
    var position = new Vector2(drawArea.FromX, drawArea.FromY);
    // uint indexOffset = 0;

    var textSize = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
    var vectorList = new List<Vector3>(text.Length * 4);

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
      var size = new Vector2(area.TextureArea.Width, area.TextureArea.Height) / area.ScaleFactor;

      var lt = new Vector2(0, 0) * size + offset + position;
      var rt = new Vector2(1, 1) * size + offset + position;
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
        new Vector3(0, 0, 0) * new Vector3(size.X, size.Y, 1) + offset + position,
        new Vector3(1, 0, 0) * new Vector3(size.X, size.Y, 1) + offset + position,
        new Vector3(1, 1, 0) * new Vector3(size.X, size.Y, 1) + offset + position,
        new Vector3(0, 1, 0) * new Vector3(size.X, size.Y, 1) + offset + position
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

      offset.X += area.Width / area.ScaleFactor;
      maxLineHeight = Math.Max(maxLineHeight, area.TextureArea.Height);
    }

    // Строго по центру
    if ((textAlign & TextAlignment.Center) != 0)
    {
      for (var i = 0; i < vectorList.Count; i++)
      {
        vectorList[i] += new Vector3(drawArea.Width / 2f, drawArea.Height / 2f, 0);
        vectorList[i] -= new Vector3(textSize.Width / 2f, textSize.Height / 2f, 0);
      }
    }

    // Вертикальное выравнивание
    if ((textAlign & TextAlignment.VerticalCenter) != 0)
    {
      for (var i = 0; i < vectorList.Count; i++)
      {
        vectorList[i] += new Vector3(0, drawArea.Height / 2f, 0);
        vectorList[i] -= new Vector3(0, textSize.Height / 2f, 0);
      }
    }

    for (var i = 0; i < vectorList.Count; i++)
    {
      vectorList[i] = vectorList[i].Floor();
    }

    Vertices.AddRange(vectorList);

    return textSize;
  }

  private void CalculateBoundingBox()
  {
    _boundingBox.FromX = float.MaxValue;
    _boundingBox.FromY = float.MaxValue;
    _boundingBox.ToX = float.MinValue;
    _boundingBox.ToY = float.MinValue;

    for (var i = 0; i < Vertices.Count; i++)
    {
      _boundingBox.FromX = Math.Min(Vertices[i].X, _boundingBox.FromX);
      _boundingBox.FromY = Math.Min(Vertices[i].Y, _boundingBox.FromY);
      _boundingBox.ToX = Math.Max(Vertices[i].X, _boundingBox.ToX);
      _boundingBox.ToY = Math.Max(Vertices[i].Y, _boundingBox.ToY);
    }
  }
}