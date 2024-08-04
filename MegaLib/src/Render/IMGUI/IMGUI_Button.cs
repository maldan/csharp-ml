using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Button : IMGUI_Element
{
  public string Text;
  public Action OnClick;
  private bool _isClick;
  public Action<IMGUI_Button> OnTick;

  public bool IsClicked => _isClick;

  public override IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return new IMGUI_BuildOut { IndexOffset = buildArgs.IndexOffset };
    FontData = buildArgs.FontData;

    InitCollision(Rectangle.FromLeftTopWidthHeight(Position.X, Position.Y, Size.X, Size.Y));
    var isHit = CheckCollision();
    if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
      {
        if (!_isClick)
        {
          _isClick = true;
          OnClick?.Invoke();
        }

        buildArgs.IndexOffset = DoRectangle(Position, Size, new Vector4(0.7f, 0.4f, 0.4f, 1), buildArgs.IndexOffset);
      }
      else
      {
        buildArgs.IndexOffset = DoRectangle(Position, Size, new Vector4(0.4f, 0.4f, 0.4f, 1), buildArgs.IndexOffset);
      }
    }
    else
    {
      buildArgs.IndexOffset = DoRectangle(Position, Size, new Vector4(0.3f, 0.3f, 0.3f, 1), buildArgs.IndexOffset);
    }

    var textSize = GetTextSize(Text) * 0.5f;
    var center = Size * 0.5f;
    // System.Console.WriteLine(textSize);
    buildArgs.IndexOffset = DoText(
      Position + new Vector3(0, 0, 0.0001f) + center + new Vector2(-textSize.X, -textSize.Y),
      Text,
      new Vector4(0.75f, 0.75f, 0.75f, 1),
      buildArgs.IndexOffset);

    /*if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
      {
        if (!_isClick)
        {
          _isClick = true;
          OnClick?.Invoke();
        }

        for (var i = 0; i < Colors.Count; i++)
          Colors[i] = new Vector4(0.8f, 0.2f, 0.2f, 1.0f);
      }
      else
      {
        for (var i = 0; i < Colors.Count; i++)
          Colors[i] = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
      }
    }
    else
    {
      for (var i = 0; i < Colors.Count; i++)
        Colors[i] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
    }*/

    if (!Mouse.IsKeyDown(MouseKey.Left)) _isClick = false;

    // Вызываем каждый кадр
    OnTick?.Invoke(this);

    return new IMGUI_BuildOut { IndexOffset = buildArgs.IndexOffset };
  }
}