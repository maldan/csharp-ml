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

  public override IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };

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
      buildArgs.IndexOffset = DoRectangle(
        new Vector3(Position.X, Position.Y, 0),
        new Vector2(Size.X, HeaderHeight),
        headerColor,
        buildArgs.IndexOffset);

      // Текст заголовка
      var textSize = GetTextSize(Title) * 0.5f;
      buildArgs.IndexOffset = DoText(
        Position + new Vector3(5, HeaderHeight / 2f, 0) + new Vector3(0, -textSize.Y, 0),
        Title,
        new Vector4(0.5f, 0.5f, 0.5f, 1),
        buildArgs.IndexOffset);

      if (!Mouse.IsKeyDown(MouseKey.Left))
      {
        _isClick = false;
        _isDrag = false;
      }
    }

    // Body
    buildArgs.IndexOffset = DoRectangle(
      new Vector3(Position.X, Position.Y + HeaderHeight, 0),
      new Vector2(Size.X, Height),
      new Vector4(0.2f, 0.2f, 0.2f, 1),
      buildArgs.IndexOffset);

    // Билдим контент
    Content.Position = Position + new Vector2(0, HeaderHeight) + new Vector2(Padding, Padding);
    Content.Size = Size - new Vector2(Padding, Padding) * 2;
    var buildOut = Content.Build(buildArgs);
    buildArgs.IndexOffset = buildArgs.IndexOffset;
    Height = buildOut.Height + Padding;

    CopyRenderDataFrom(Content);

    return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };
  }
}