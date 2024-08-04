using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Tree : IMGUI_Element
{
  public string Title;
  public IMGUI_Container Content = new();
  public float HeaderHeight = 20;
  private bool _isClick = false;
  private bool _isShowContent = true;

  public override IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };

    InitCollision(Rectangle.FromLeftTopWidthHeight(Position.X, Position.Y, Size.X, HeaderHeight));
    var isHit = CheckCollision();

    if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
        if (!_isClick)
        {
          _isClick = true;
          _isShowContent = !_isShowContent;
        }
    }

    if (!Mouse.IsKeyDown(MouseKey.Left))
    {
      _isClick = false;
    }

    // Хедер
    buildArgs.IndexOffset = DoRectangle(Position, Size, new Vector4(0.3f, 0.3f, 0.3f, 1), buildArgs.IndexOffset);

    // Текст заголовка
    var textSize = GetTextSize(Title) * 0.5f;
    buildArgs.IndexOffset = DoText(
      Position + new Vector3(5, HeaderHeight / 2f, 0) + new Vector3(0, -textSize.Y, 0),
      Title,
      new Vector4(0.5f, 0.5f, 0.5f, 1),
      buildArgs.IndexOffset);

    // Инфа
    if (_isShowContent)
    {
      Content.Position = Position + new Vector2(0, HeaderHeight + Padding);
      Content.Size = Size;
      buildArgs.IndexOffset = Content.Build(buildArgs).IndexOffset;
      CopyRenderDataFrom(Content);
    }

    return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };
  }
}