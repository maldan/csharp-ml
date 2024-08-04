using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public enum FlexDirection
{
  Row,
  Column
}

public class IMGUI_Window : IMGUI_Element
{
  public string Title;

  // public List<IMGUI_Element> Elements = [];
  private bool _isDrag = false;
  private bool _isClick = false;
  private Vector2 _startDrag;
  private Vector3 _startPos;

  public bool IsDraggable = true;
  public bool IsClosable;
  public FlexDirection FlexDirection = FlexDirection.Column;
  public float HeaderHeight = 20;
  public float Gap = 5;

  public IMGUI_Container Content = new();

  public IMGUI_Window()
  {
    Padding = 5;
  }

  public override uint Build(uint indexOffset = 0)
  {
    Clear();
    if (!IsVisible) return indexOffset;

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

    // Билдим контент
    Content.Position = Position + new Vector2(0, HeaderHeight);
    Content.Size = Size;
    indexOffset = Content.Build(indexOffset);
    Vertices.AddRange(Content.Vertices);
    UV.AddRange(Content.UV);
    Colors.AddRange(Content.Colors);
    Indices.AddRange(Content.Indices);

    /*if (FlexDirection == IMGUI_FlexDirection.Column)
    {
      var p = new Vector3(Position.X + Padding, Position.Y + Padding + HeaderHeight, 0.0001f);
      var totalH = 0f;
      for (var i = 0; i < Elements.Count; i++)
      {
        // Пропускаем невидимые
        if (!Elements[i].IsVisible) continue;

        Elements[i].Position = p;
        var h = Elements[i].Size.Y > 0 ? Elements[i].Size.Y : 20;
        Elements[i].Size = new Vector2(Size.X - Padding * 2, h);
        indexOffset = Elements[i].Build(indexOffset);
        p.Y += h;
        p.Y += Gap;
        totalH += h + Gap;

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
    }*/

    return indexOffset;
  }
}