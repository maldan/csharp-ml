using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Button : IMGUI_Element
{
  public string Text;
  public Action OnClick;
  private bool _isClick;

  public override uint Build(uint indexOffset = 0)
  {
    Clear();

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

        indexOffset = DoRectangle(Position, Size, new Vector4(0.7f, 0.4f, 0.4f, 1), indexOffset);
      }
      else
      {
        indexOffset = DoRectangle(Position, Size, new Vector4(0.4f, 0.4f, 0.4f, 1), indexOffset);
      }
    }
    else
    {
      indexOffset = DoRectangle(Position, Size, new Vector4(0.3f, 0.3f, 0.3f, 1), indexOffset);
    }

    indexOffset = DoText(Position + new Vector3(0, 0, 0.0001f), Text, indexOffset);

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

    return indexOffset;
  }
}