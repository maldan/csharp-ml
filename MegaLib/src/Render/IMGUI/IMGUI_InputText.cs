using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

/*public class IMGUI_InputText : IMGUI_Element
{
  public string Text;
  public Func<string, string> OnChange;
  private bool _isFocused = false;

  public override uint Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return buildArgs.IndexOffset;

    // Коллизиция
    InitCollision(Rectangle.FromLeftTopWidthHeight(Position.X, Position.Y, Size.X, Size.Y));
    var isHit = CheckCollision();

    // Поле
    buildArgs.IndexOffset = DoRectangle(Position, Size, new Vector4(0.15f, 0.15f, 0.15f, 1), buildArgs.IndexOffset);

    // Текст
    buildArgs.IndexOffset = DoText(Position, Text, new Vector4(1, 1, 1, 1), buildArgs.IndexOffset);

    if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
      {
        _isFocused = true;
      }
    }
    else
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
        _isFocused = false;
    }

    if (_isFocused)
    {
      buildArgs.IndexOffset = DoRectangle(Position + new Vector2(Text.Length * 12, 0), new Vector2(2, Size.Y),
        new Vector4(0.7f, 0.7f, 0.7f, 1), buildArgs.IndexOffset);

      if (Keyboard.IsKeyPressed(KeyboardKey.Backspace))
      {
        if (Text.Length > 0) Text = OnChange.Invoke(Text[..^1]);
      }
      else
      {
        var ch = Keyboard.GetCurrentInput();
        if (ch != "") Text = OnChange.Invoke(Text + ch);
      }
    }

    return buildArgs.IndexOffset;
  }
}*/