using System;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_TextInput : EasyUI_Element
{
  public EasyUI_TextInput()
  {
    var baseColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
    Style.BackgroundColor = baseColor;
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlign = TextAlignment.Left | TextAlignment.VerticalCenter;
    Value = "";
    Text = $"{Value}";

    var cursor = new EasyUI_Element();
    cursor.Style.Width = 2;
    cursor.Style.Height = 16;
    cursor.Style.X = 4;
    cursor.Style.Y = 4;
    cursor.Style.BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
    Children.Add(cursor);

    var cursorPosition = 0;

    var isOver = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1);
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.BackgroundColor = baseColor;
    };
    Events.OnMouseDown += () => { Style.BackgroundColor = new Vector4(0.5f, 0.2f, 0.2f, 1); };
    Events.OnMouseUp += () => { Style.BackgroundColor = baseColor; };

    var timer = 0f;
    var keyState = new bool[] { false, false, false, false };
    var keyPreviousState = new bool[] { false, false, false, false };
    var isBackspace = 0;
    var isLeftArrow = 1;
    var isRightArrow = 2;

    Events.OnRender += (delta) =>
    {
      timer += delta * 4f;

      keyState[isBackspace] = Keyboard.IsKeyDown(KeyboardKey.Backspace);
      keyState[isLeftArrow] = Keyboard.IsKeyDown(KeyboardKey.ArrowLeft);
      keyState[isRightArrow] = Keyboard.IsKeyDown(KeyboardKey.ArrowRight);

      if (isOver) Mouse.Cursor = MouseCursor.TextInput;
      var x = Keyboard.GetCurrentInput();
      if (x != "")
      {
        var currentString = (string)Value;
        if (currentString == "")
        {
          Value = $"{Value}{x}";
        }
        else
        {
          // Разделяем строку на две части
          var beforeCursor = currentString.Substring(0, cursorPosition);
          var afterCursor = currentString.Substring(cursorPosition);

          // Вставляем новый символ
          Value = $"{beforeCursor}{x}{afterCursor}";
        }

        // Увеличиваем позицию курсора на 1
        cursorPosition += 1;
      }

      if (keyState[isBackspace] && !keyPreviousState[isBackspace])
      {
        var str = (string)Value;
        if (str != "" && cursorPosition > 0)
        {
          cursorPosition -= 1; // Понижаем позицию курсора
          // Удаляем символ перед курсором
          str = str.Remove(cursorPosition, 1);
          Value = str;
          Console.WriteLine(str);
        }
      }

      if (keyState[isLeftArrow] && !keyPreviousState[isLeftArrow])
      {
        cursorPosition -= 1;
        if (cursorPosition <= 0) cursorPosition = 0;
      }

      if (keyState[isRightArrow] && !keyPreviousState[isRightArrow])
      {
        cursorPosition += 1;
        if (cursorPosition > ((string)Value).Length) cursorPosition = ((string)Value).Length;
      }

      if ((int)timer % 2 == 0)
      {
        cursor.Style.BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 0.0f);
      }
      else
      {
        cursor.Style.BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
      }

      if (CurrentFontData != null)
      {
        var currentString = (string)Value;
        var finalOffset = 0;
        for (var i = 0; i < cursorPosition; i++)
        {
          if (i >= currentString.Length) continue;
          finalOffset += CurrentFontData.GetGlyph(currentString[i]).Width;
        }

        if (finalOffset <= 0) finalOffset = 0;

        cursor.Style.X = finalOffset;
      }

      Text = $"{Value}";

      // Предыдущее состояние клавиш
      for (var i = 0; i < keyState.Length; i++) keyPreviousState[i] = keyState[i];
    };
  }
}