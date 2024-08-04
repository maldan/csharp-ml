using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Check : IMGUI_Element
{
  public string Text;
  public Func<bool, bool> OnClick;
  private bool _isClick;
  public bool IsActive;

  public override uint Build(uint indexOffset = 0)
  {
    Clear();
    if (!IsVisible) return indexOffset;

    var checkSize = new Vector2(Size.Y, Size.Y);

    InitCollision(Rectangle.FromLeftTopWidthHeight(Position.X, Position.Y, Size.Y, Size.Y));
    var isHit = CheckCollision();
    if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
      {
        if (!_isClick)
        {
          _isClick = true;
          IsActive = OnClick.Invoke(!IsActive);
        }

        indexOffset = DoRectangle(Position, checkSize, new Vector4(0.7f, 0.4f, 0.4f, 1), indexOffset);
      }
      else
      {
        indexOffset = DoRectangle(Position, checkSize, new Vector4(0.4f, 0.4f, 0.4f, 1), indexOffset);
      }
    }
    else
    {
      indexOffset = DoRectangle(Position, checkSize, new Vector4(0.3f, 0.3f, 0.3f, 1), indexOffset);
    }

    if (IsActive)
      indexOffset = DoRectangle(Position + new Vector2(4, 4), checkSize + new Vector2(-8, -8),
        new Vector4(0.235f, 0.63f, 1f, 1), indexOffset);

    indexOffset = DoText(Position + new Vector3(Size.Y + 5, 0, 0.0001f), Text, indexOffset);

    if (!Mouse.IsKeyDown(MouseKey.Left)) _isClick = false;

    return indexOffset;
  }
}