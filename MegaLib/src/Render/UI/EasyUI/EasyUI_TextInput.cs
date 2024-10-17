using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_TextInput : EasyUI_Element
{
  private int _cursorPosition;
  private int _fromCursorPosition;

  public int MinSelection => (int)MathF.Min(_cursorPosition, _fromCursorPosition);
  public int MaxSelection => (int)MathF.Max(_cursorPosition, _fromCursorPosition);

  private float _timer;
  private Dictionary<byte, bool> _keyState = new();
  private Dictionary<byte, bool> _keyPreviousState = new();
  private float _deleteTimer;
  private float _deleteTimer2;
  private bool _isSelectionMode;
  private object _lastValue;

  private EasyUI_Element _cursor;
  private EasyUI_Element _selection;
  private EasyUI_Element _textContent;

  public int MaxCharacters;
  public TextInputType InputType = TextInputType.Text;

  public float ValueStep = 0.1f;

  public bool IsFocused
  {
    get
    {
      if (LayerEasyUi?.FocusedElement == null) return false;
      return LayerEasyUi.FocusedElement == this;
    }
  }

  public EasyUI_TextInput()
  {
    var baseColor = "#232323";
    Style.SetBackgroundColor(baseColor);
    Style.SetArea(0, 0, 64, 24);
    Style.TextAlignment = TextAlignment.Left | TextAlignment.VerticalCenter;
    Style.SetBorderWidth(2f);
    Style.SetBorderColor("#fe000000");
    Style.SetBorderRadius(2);

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
    _textContent.Style.TextAlignment = TextAlignment.Left | TextAlignment.VerticalCenter;
    _textContent.Style.SetTextColor("#c0c0c0");

    Children.Add(_textContent);
    Children.Add(_cursor);
    Children.Add(_selection);

    var isOver = false;
    var gas = false;

    Events.OnMouseOver += () =>
    {
      isOver = true;
      Style.SetBackgroundColor("#292929");
    };
    Events.OnMouseOut += () =>
    {
      isOver = false;
      Style.SetBackgroundColor(baseColor);
    };
    Events.OnMouseDown += () => { };
    Events.OnClick += () =>
    {
      _isSelectionMode = false;

      if (LayerEasyUi.FocusedElement == this) return;
      LayerEasyUi.FocusedElement = this;
      Events.OnFocus?.Invoke();
    };
    Events.OnDoubleClick += () =>
    {
      _cursorPosition = 0;
      _fromCursorPosition = $"{Value}".Length;
      _isSelectionMode = true;
    };
    Events.OnMouseUp += () => { Style.SetBackgroundColor(baseColor); };

    DrawCursor();

    _keyState[(byte)KeyboardKey.Backspace] = false;
    _keyState[(byte)KeyboardKey.ArrowLeft] = false;
    _keyState[(byte)KeyboardKey.ArrowRight] = false;
    foreach (var (key, value) in _keyState) _keyPreviousState[key] = value;

    Events.OnBeforeRender += _ =>
    {
      if (Mouse.IsKeyDown(MouseKey.Left) && !isOver)
      {
        LoseFocus();
      }
    };

    Events.OnBeforeRender += delta =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.TextInput;

      _textContent.Style.Width = Style.Width - 16;
      _textContent.Style.Height = Style.Height;
      _textContent.Style.X = 4;

      _timer += delta * 4f;
      if (IsFocused)
      {
        Style.SetBorderColor("#ae5c00");
        HandleInput(delta);
      }
      else
      {
        Style.SetBorderColor(new Vector4(0, 0, 0, 0.5f));
      }

      DrawCursor();
      DrawSelection();

      _textContent.Text = $"{Value}";
    };

    InitKeys();
  }

  public void OnRead(Func<float> read)
  {
    var oldF = 0f;
    Events.OnBeforeRender += (_) =>
    {
      if (IsFocused) return;

      var v = read();
      if (MathF.Abs(v - oldF) > 0.0001f)
      {
        oldF = v;
        Value = $"{v}".Replace(",", ".");
      }
    };
  }

  public void OnWrite(Action<float> write)
  {
    Events.OnChange += o => { write(GetFloatValue()); };
  }

  public void OnRead(Func<string> read)
  {
    Events.OnBeforeRender += (_) =>
    {
      if (IsFocused) return;
      Value = $"{read()}";
    };
  }

  public void OnWrite(Action<string> write)
  {
    Events.OnChange += o => { write($"{Value}"); };
  }

  public void OnRead<T>(Func<T> read) where T : struct
  {
    Events.OnBeforeRender += (_) =>
    {
      if (IsFocused) return;

      Value = $"{read()}";
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

  public float GetFloatValue()
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
    if (LayerEasyUi.FocusedElement != this) return;
    LayerEasyUi.FocusedElement = null;
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
    _fromCursorPosition = 0;
    _isSelectionMode = false;
  }

  private void InitKeys()
  {
    _keyState[(byte)KeyboardKey.Backspace] = Keyboard.IsKeyDown(KeyboardKey.Backspace);
    _keyState[(byte)KeyboardKey.ArrowLeft] = Keyboard.IsKeyDown(KeyboardKey.ArrowLeft);
    _keyState[(byte)KeyboardKey.ArrowRight] = Keyboard.IsKeyDown(KeyboardKey.ArrowRight);
    _keyState[(byte)KeyboardKey.ArrowUp] = Keyboard.IsKeyDown(KeyboardKey.ArrowUp);
    _keyState[(byte)KeyboardKey.ArrowDown] = Keyboard.IsKeyDown(KeyboardKey.ArrowDown);
    _keyState[(byte)KeyboardKey.Enter] = Keyboard.IsKeyDown(KeyboardKey.Enter);
    _keyState[(byte)KeyboardKey.Shift] = Keyboard.IsKeyDown(KeyboardKey.Shift);

    // Предыдущее состояние клавиш
    foreach (var (key, value) in _keyState) _keyPreviousState[key] = value;
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
      // Если есть выделение и что-то печатаем, сначала удаляем выделение
      if (_isSelectionMode)
      {
        RemoveCharacter();
        _isSelectionMode = false;
      }

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

    // _isSelectionMode = Keyboard.IsKeyDown(KeyboardKey.Shift);

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
      _isSelectionMode = false;
    }

    if (_keyState[(byte)KeyboardKey.ArrowRight] && !_keyPreviousState[(byte)KeyboardKey.ArrowRight])
    {
      _cursorPosition += 1;
      if (_cursorPosition > ((string)Value).Length) _cursorPosition = ((string)Value).Length;
      _isSelectionMode = false;
    }

    // Увеличить значение
    var offsetValue = ValueStep;
    if (Keyboard.IsKeyDown(KeyboardKey.Shift)) offsetValue *= 10f;

    var arrowUp = _keyState[(byte)KeyboardKey.ArrowUp] && !_keyPreviousState[(byte)KeyboardKey.ArrowUp];
    arrowUp = arrowUp || Keyboard.KeyDownTimer(KeyboardKey.ArrowUp) > 0.5f;
    if (arrowUp)
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
            ? vv + offsetValue
            : 0;
          Value = $"{v:F2}".Replace(",", ".");
          break;
        }
      }

      EmitChange();
    }

    var arrowDown = _keyState[(byte)KeyboardKey.ArrowDown] && !_keyPreviousState[(byte)KeyboardKey.ArrowDown];
    arrowDown = arrowDown || Keyboard.KeyDownTimer(KeyboardKey.ArrowDown) > 0.5f;
    if (arrowDown)
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
            ? vv - offsetValue
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
  }

  private void DrawSelection()
  {
    if (!_isSelectionMode)
    {
      _selection.Style.BackgroundColor = new Vector4(0, 0, 0, 0);
      return;
    }

    var currentString = (string)Value;
    var minOffset = 0;
    for (var i = 0; i < MinSelection; i++)
    {
      if (i >= currentString.Length) continue;
      minOffset += CurrentFontData.GetGlyph(currentString[i]).Width;
    }

    var maxOffset = 0;
    for (var i = MinSelection; i < MaxSelection; i++)
    {
      if (i >= currentString.Length) continue;
      maxOffset += CurrentFontData.GetGlyph(currentString[i]).Width;
    }

    _selection.Style.X = _textContent.Style.Position.X + minOffset;
    _selection.Style.Width = maxOffset - minOffset;
    _selection.Style.BackgroundColor = new Vector4(0.0f, 0.8f, 0.0f, 0.25f);
  }

  private void RemoveCharacter()
  {
    var str = (string)Value;
    if (str == "") return;

    if (_isSelectionMode && MaxSelection - MinSelection > 0)
    {
      str = str.Remove(MinSelection, MaxSelection - MinSelection);
      Value = str;
      _cursorPosition = MinSelection;
    }

    if (_cursorPosition > 0)
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

  private void DrawValue()
  {
  }
}