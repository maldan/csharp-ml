using System;
using System.Collections.Generic;
using System.Globalization;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public enum TextInputType
{
  Text,
  Integer,
  Float
}

public class EasyUI_TextInput : EasyUI_Element
{
  private int _cursorPosition;
  private int _fromCursorPosition;

  public int MinSelection => (int)MathF.Min(_cursorPosition, _cursorPosition);
  public int MaxSelection => (int)MathF.Max(_cursorPosition, _cursorPosition);

  private float _timer;
  private Dictionary<byte, bool> _keyState = new();
  private Dictionary<byte, bool> _keyPreviousState = new();
  private float _deleteTimer;
  private float _deleteTimer2;
  private bool _isSelectionMode;

  private EasyUI_Element _cursor;
  private EasyUI_Element _selection;
  private EasyUI_Element _textContent;

  public int MaxCharacters;
  public TextInputType InputType = TextInputType.Text;

  public bool IsFocused => EasyUI_GlobalState.FocusedElement == this;

  public EasyUI_TextInput()
  {
    var baseColor = "#232323";
    Style.BackgroundColor = baseColor;
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlign = TextAlignment.Left | TextAlignment.VerticalCenter;
    Style.BorderWidth = 1f;
    Style.BorderColor = "#fe000000";
    Value = "";
    Text = "";

    _cursor = new EasyUI_Element
    {
      Style =
      {
        Width = 2,
        Height = 16,
        X = 4,
        Y = 4,
        BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f)
      }
    };
    _selection = new EasyUI_Element
    {
      Style =
      {
        Width = 0,
        Height = 16,
        X = 4,
        Y = 4,
        BackgroundColor = new Vector4(0.0f, 0.8f, 0.0f, 0.25f)
      }
    };

    _textContent = new EasyUI_Element();
    _textContent.Style.TextAlign = TextAlignment.Left | TextAlignment.VerticalCenter;
    _textContent.Style.TextColor = "#c0c0c0";

    Children.Add(_textContent);
    Children.Add(_cursor);
    Children.Add(_selection);

    var isOver = false;
    var gas = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.BackgroundColor = "#292929";
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.BackgroundColor = baseColor;
    };
    Events.OnMouseDown += () => { };
    Events.OnClick += () =>
    {
      if (EasyUI_GlobalState.FocusedElement == this) return;
      EasyUI_GlobalState.FocusedElement = this;
      Events.OnFocus?.Invoke();
    };
    Events.OnMouseUp += () => { Style.BackgroundColor = baseColor; };

    DrawCursor();

    _keyState[(byte)KeyboardKey.Backspace] = false;
    _keyState[(byte)KeyboardKey.ArrowLeft] = false;
    _keyState[(byte)KeyboardKey.ArrowRight] = false;
    foreach (var (key, value) in _keyState) _keyPreviousState[key] = value;

    Events.OnBeforeRender += _ =>
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
      {
        LoseFocus();
      }
    };

    Events.OnRender += delta =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.TextInput;

      _textContent.Style.Width = Width() - 16;
      _textContent.Style.Height = Height();
      _textContent.Style.X = 4;

      _timer += delta * 4f;
      if (IsFocused)
      {
        Style.BorderColor = "#ae5c00";
        HandleInput(delta);
      }
      else
      {
        Style.BorderColor = new Vector4(0, 0, 0, 0);
      }

      DrawCursor();

      _textContent.Text = $"{Value}";
    };
  }

  public void OnRead<T>(Func<T> read) where T : struct
  {
    Events.OnBeforeRender += (_) =>
    {
      if (!IsFocused) Value = $"{read()}".Replace(",", ".");
    };
  }

  public void OnWrite<T>(Action<T> write) where T : struct
  {
    Events.OnChange += o =>
    {
      if (typeof(T) == typeof(float))
      {
        write((T)(object)GetFloatValue());
      }
      else
      {
        write((T)Value);
      }
    };
  }

  private float GetFloatValue()
  {
    return float.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
      ? f
      : 0.0f;
  }

  private void EmitChange()
  {
    switch (InputType)
    {
      case TextInputType.Integer:
      {
        Events.OnChange?.Invoke(int.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
          ? f
          : 0);
        break;
      }
      case TextInputType.Float:
      {
        Events.OnChange?.Invoke(float.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
          ? f
          : 0.0f);
        break;
      }
      default:
        Events.OnChange?.Invoke(Value);
        break;
    }
  }

  private void LoseFocus()
  {
    if (EasyUI_GlobalState.FocusedElement != this) return;
    EasyUI_GlobalState.FocusedElement = null;
    Events.OnBlur?.Invoke();

    switch (InputType)
    {
      case TextInputType.Integer:
      {
        if (float.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
        {
          Value = $"{MathF.Floor(f)}";
        }
        else
        {
          Value = int.TryParse($"{Value}", out var x) ? $"{x}" : "0";
        }

        break;
      }
      case TextInputType.Float:
      {
        Value = float.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
          ? $"{x}".Replace(",", ".")
          : "0.0";
        break;
      }
      case TextInputType.Text:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    EmitChange();
    _cursorPosition = ((string)Value).Length;
  }

  private void HandleInput(float delta)
  {
    _keyState[(byte)KeyboardKey.Backspace] = Keyboard.IsKeyDown(KeyboardKey.Backspace);
    _keyState[(byte)KeyboardKey.ArrowLeft] = Keyboard.IsKeyDown(KeyboardKey.ArrowLeft);
    _keyState[(byte)KeyboardKey.ArrowRight] = Keyboard.IsKeyDown(KeyboardKey.ArrowRight);
    _keyState[(byte)KeyboardKey.ArrowUp] = Keyboard.IsKeyDown(KeyboardKey.ArrowUp);
    _keyState[(byte)KeyboardKey.ArrowDown] = Keyboard.IsKeyDown(KeyboardKey.ArrowDown);
    _keyState[(byte)KeyboardKey.Enter] = Keyboard.IsKeyDown(KeyboardKey.Enter);
    _keyState[(byte)KeyboardKey.Shift] = Keyboard.IsKeyDown(KeyboardKey.Shift);

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
        var beforeCursor = currentString[.._cursorPosition];
        var afterCursor = currentString[_cursorPosition..];

        // Вставляем новый символ
        Value = $"{beforeCursor}{x}{afterCursor}";
      }

      // Увеличиваем позицию курсора на 1
      _cursorPosition += 1;
    }

    if (_keyState[(byte)KeyboardKey.Backspace] && !_keyPreviousState[(byte)KeyboardKey.Backspace])
    {
      RemoveCharacter();
    }

    if (_keyState[(byte)KeyboardKey.Shift] && !_keyPreviousState[(byte)KeyboardKey.Shift])
    {
      _fromCursorPosition = _cursorPosition;
    }

    _isSelectionMode = Keyboard.IsKeyDown(KeyboardKey.Shift);

    if (Keyboard.IsKeyDown(KeyboardKey.Backspace))
    {
      _deleteTimer += delta;
      if (_deleteTimer > 0.4f)
      {
        _deleteTimer2 += delta;
      }

      if (_deleteTimer2 > 0.04f)
      {
        _deleteTimer2 = 0;
        RemoveCharacter();
      }
    }
    else
    {
      _deleteTimer = 0;
      _deleteTimer2 = 0;
    }

    if (_keyState[(byte)KeyboardKey.ArrowLeft] && !_keyPreviousState[(byte)KeyboardKey.ArrowLeft])
    {
      _cursorPosition -= 1;
      if (_cursorPosition <= 0) _cursorPosition = 0;
    }

    if (_keyState[(byte)KeyboardKey.ArrowRight] && !_keyPreviousState[(byte)KeyboardKey.ArrowRight])
    {
      _cursorPosition += 1;
      if (_cursorPosition > ((string)Value).Length) _cursorPosition = ((string)Value).Length;
    }

    // Увеличить значение
    if (_keyState[(byte)KeyboardKey.ArrowUp] && !_keyPreviousState[(byte)KeyboardKey.ArrowUp])
    {
      switch (InputType)
      {
        case TextInputType.Integer:
        {
          var v = int.TryParse($"{Value}", out var vv) ? vv + 1 : 0;
          Value = $"{v}";
          break;
        }
        case TextInputType.Float:
        {
          var v = float.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var vv)
            ? vv + 0.1f
            : 0;
          Value = $"{v:F2}".Replace(",", ".");
          break;
        }
      }

      EmitChange();
    }

    if (_keyState[(byte)KeyboardKey.ArrowDown] && !_keyPreviousState[(byte)KeyboardKey.ArrowDown])
    {
      switch (InputType)
      {
        case TextInputType.Integer:
        {
          var v = int.TryParse($"{Value}", out var vv) ? vv - 1 : 0;
          Value = $"{v}";
          break;
        }
        case TextInputType.Float:
        {
          var v = float.TryParse($"{Value}", NumberStyles.Float, CultureInfo.InvariantCulture, out var vv)
            ? vv - 0.1f
            : 0;
          Value = $"{v:F2}".Replace(",", ".");
          break;
        }
      }

      EmitChange();
    }

    // Клик на ентер
    if (_keyState[(byte)KeyboardKey.Enter] && !_keyPreviousState[(byte)KeyboardKey.Enter])
    {
      LoseFocus();
    }

    // Предыдущее состояние клавиш
    foreach (var (key, value) in _keyState) _keyPreviousState[key] = value;

    /*_selection.Style.X = MinSelection * 4;
    _selection.Style.Width = (MinSelection + MaxSelection) * 4;*/
  }


  private void RemoveCharacter()
  {
    var str = (string)Value;
    if (str != "" && _cursorPosition > 0)
    {
      _cursorPosition -= 1; // Понижаем позицию курсора
      // Удаляем символ перед курсором
      str = str.Remove(_cursorPosition, 1);
      Value = str;
      Console.WriteLine(str);
    }
  }

  private void DrawCursor()
  {
    if (!IsFocused)
    {
      _cursor.Style.BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 0.0f);
      return;
    }

    _cursor.Style.BackgroundColor =
      (int)_timer % 2 == 0
        ? new Vector4(0.7f, 0.7f, 0.7f, 0.0f)
        : new Vector4(0.7f, 0.7f, 0.7f, 1.0f);

    if (CurrentFontData != null)
    {
      var currentString = (string)Value;
      var finalOffset = 0;
      for (var i = 0; i < _cursorPosition; i++)
      {
        if (i >= currentString.Length) continue;
        finalOffset += CurrentFontData.GetGlyph(currentString[i]).Width;
      }

      if (finalOffset <= 0) finalOffset = 0;

      _cursor.Style.X = 4 + finalOffset;
    }
  }
}