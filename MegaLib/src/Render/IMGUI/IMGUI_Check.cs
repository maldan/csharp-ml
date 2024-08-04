using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

/*public class IMGUI_Check : IMGUI_Element
{
  public string Text;
  public Func<bool, bool> OnClick;
  private bool _isClick;
  public bool IsActive;

  public override uint Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return buildArgs.IndexOffset;

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

        buildArgs.IndexOffset =
          DoRectangle(Position, checkSize, new Vector4(0.7f, 0.4f, 0.4f, 1), buildArgs.IndexOffset);
      }
      else
      {
        buildArgs.IndexOffset =
          DoRectangle(Position, checkSize, new Vector4(0.4f, 0.4f, 0.4f, 1), buildArgs.IndexOffset);
      }
    }
    else
    {
      buildArgs.IndexOffset = DoRectangle(Position, checkSize, new Vector4(0.3f, 0.3f, 0.3f, 1), buildArgs.IndexOffset);
    }

    if (IsActive)
      buildArgs.IndexOffset = DoRectangle(Position + new Vector2(4, 4), checkSize + new Vector2(-8, -8),
        new Vector4(0.235f, 0.63f, 1f, 1), buildArgs.IndexOffset);

    buildArgs.IndexOffset = DoText(Position + new Vector3(Size.Y + 5, 0, 0.0001f), Text, new Vector4(1, 1, 1, 1),
      buildArgs.IndexOffset);

    if (!Mouse.IsKeyDown(MouseKey.Left)) _isClick = false;

    return buildArgs.IndexOffset;
  }
}*/