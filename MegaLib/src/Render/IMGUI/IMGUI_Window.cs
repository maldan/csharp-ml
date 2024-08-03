using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Window : IMGUI_Element
{
  public string Title;
  public List<IMGUI_Element> Elements = [];
  private bool _isDrag = false;
  private bool _isClick = false;
  private Vector2 _startDrag;
  private Vector3 _startPos;

  public bool IsVerticalContent = true;
  public float Padding = 5;
  public float HeaderHeight = 20;

  public override uint Build(uint indexOffset = 0)
  {
    Clear();

    if (_isDrag)
    {
      var v = Mouse.ClientClamped - _startDrag;
      Position = _startPos + v;
    }

    if (HeaderHeight > 0)
    {
      var headerColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);

      InitCollision(Rectangle.FromLeftTopWidthHeight(Position.X, Position.Y, Size.X, HeaderHeight));
      var isHit = CheckCollision();

      if (isHit)
      {
        if (Mouse.IsKeyDown(MouseKey.Left))
          if (!_isClick)
          {
            _isClick = true;
            _isDrag = true;
            _startDrag = Mouse.ClientClamped;
            _startPos = Position;
          }

        headerColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
      }

      // Заголовок
      indexOffset = DoRectangle(
        new Vector3(Position.X, Position.Y, 0),
        new Vector2(Size.X, HeaderHeight),
        headerColor,
        indexOffset);

      // Текст заголовка
      indexOffset = DoText(Position, Title, indexOffset);

      if (!Mouse.IsKeyDown(MouseKey.Left))
      {
        _isClick = false;
        _isDrag = false;
      }
    }

    // Body
    indexOffset = DoRectangle(new Vector3(Position.X, Position.Y + HeaderHeight, 0), Size,
      new Vector4(0.2f, 0.2f, 0.2f, 1),
      indexOffset);

    if (IsVerticalContent)
    {
      var p = new Vector3(Position.X + Padding, Position.Y + Padding + HeaderHeight, 0.0001f);
      var totalH = 0f;
      for (var i = 0; i < Elements.Count; i++)
      {
        Elements[i].Position = p;
        var h = Elements[i].Size.Y > 0 ? Elements[i].Size.Y : 20;
        Elements[i].Size = new Vector2(Size.X - Padding * 2, h);
        indexOffset = Elements[i].Build(indexOffset);
        p.Y += h;
        p.Y += 5;
        totalH += h + 5;

        Vertices.AddRange(Elements[i].Vertices);
        UV.AddRange(Elements[i].UV);
        Colors.AddRange(Elements[i].Colors);
        Indices.AddRange(Elements[i].Indices);
      }

      Size.Y = HeaderHeight + totalH + Padding * 2;
    }
    else
    {
      var eachItemWidth = (Size.X - Padding * 2) / Elements.Count;
      var p = new Vector3(Position.X + Padding, Position.Y + Padding + HeaderHeight, 0.0001f);
      for (var i = 0; i < Elements.Count; i++)
      {
        Elements[i].Position = p;
        Elements[i].Size = new Vector2(eachItemWidth, 20);
        indexOffset = Elements[i].Build(indexOffset);
        p.X += eachItemWidth;

        Vertices.AddRange(Elements[i].Vertices);
        UV.AddRange(Elements[i].UV);
        Colors.AddRange(Elements[i].Colors);
        Indices.AddRange(Elements[i].Indices);
      }
      // Console.WriteLine("X");
    }

    return indexOffset;
  }
}