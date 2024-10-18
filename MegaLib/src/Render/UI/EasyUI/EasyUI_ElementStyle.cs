using System;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.UI.EasyUI;

/*public class EasyUI_ElementStyle
{
  public object Width;
  public object Height;
  public object TextColor = new Vector4(1, 1, 1, 1);
  public object BackgroundColor;
  public object X;
  public object Y;
  public TextAlignment TextAlign;

  public object BorderWidth;
  public object BorderColor;

  public object BorderRadius;

  public void SetArea(float x, float y, float width, float height)
  {
    X = x;
    Y = y;
    Width = width;
    Height = height;
  }
}*/

public struct BorderColor
{
  public Vector4 Top;
  public Vector4 Left;
  public Vector4 Bottom;
  public Vector4 Right;

  public static bool operator ==(BorderColor v1, BorderColor v2)
  {
    return v1.Top == v2.Top && v1.Left == v2.Left && v1.Bottom == v2.Bottom && v1.Right == v2.Right;
  }

  public static bool operator !=(BorderColor v1, BorderColor v2)
  {
    return !(v1 == v2);
  }
}

public struct ElementStyle
{
  #region Основная область

  // Основная область
  private float _x;
  private float _y;
  private float _width;
  private float _height;
  private Vector4 _borderRadius;
  private Vector4 _backgroundColor;
  private bool _isBackgroundChanged;

  public float X
  {
    get => _x;
    set
    {
      if (MathF.Abs(_x - value) < float.Epsilon) return;
      _x = value;
      _isBackgroundChanged = true;
    }
  }

  public float Y
  {
    get => _y;
    set
    {
      if (MathF.Abs(_y - value) < float.Epsilon) return;
      _y = value;
      _isBackgroundChanged = true;
    }
  }

  public float Width
  {
    get => _width;
    set
    {
      if (MathF.Abs(_width - value) < float.Epsilon) return;
      _width = value;
      _isBackgroundChanged = true;
    }
  }

  public float Height
  {
    get => _height;
    set
    {
      if (MathF.Abs(_height - value) < float.Epsilon) return;
      _height = value;
      _isBackgroundChanged = true;
    }
  }

  public Vector4 BorderRadius
  {
    get => _borderRadius;
    set
    {
      if (_borderRadius == value) return;
      _borderRadius = value;
      _isBackgroundChanged = true;
    }
  }

  public Vector4 BackgroundColor
  {
    get => _backgroundColor;
    set
    {
      if (_backgroundColor == value) return;
      _backgroundColor = value;
      _isBackgroundChanged = true;
    }
  }

  public bool IsBackgroundChanged
  {
    get => _isBackgroundChanged;
    set => _isBackgroundChanged = value;
  }

  public Vector2 Position => new(_x, _y);
  public Vector2 Size => new(_width, _height);
  public Rectangle BoundingBox => Rectangle.FromLeftTopWidthHeight(_x, _y, _width, _height);

  public void SetArea(float x, float y, float width, float height)
  {
    X = x;
    Y = y;
    Width = width;
    Height = height;
  }

  // Установка цвета из строки
  public void SetBackgroundColor(string color)
  {
    BackgroundColor = color[0] == '#' ? HexToColor(color) : WordToColor(color);
  }

  public void SetBorderRadius(float value)
  {
    BorderRadius = new Vector4(value, value, value, value);
  }

  #endregion

  #region Обводка

  private Vector4 _borderWidth;
  private BorderColor _borderColor;
  private bool _isBorderChanged;

  public Vector4 BorderWidth
  {
    get => _borderWidth;
    set
    {
      if (_borderWidth == value) return;
      _borderWidth = value;
      _isBorderChanged = true;
    }
  }

  public BorderColor BorderColor
  {
    get => _borderColor;
    set
    {
      if (_borderColor == value) return;
      _borderColor = value;
      _isBorderChanged = true;
    }
  }

  public bool IsBorderChanged
  {
    get => _isBorderChanged;
    set => _isBorderChanged = value;
  }

  // Установка цвета из строки
  public void SetBorderColor(Vector4 color)
  {
    BorderColor = new BorderColor
    {
      Top = color,
      Right = color,
      Bottom = color,
      Left = color
    };
  }

  public void SetBorderColor(Vector4 top, Vector4 right, Vector4 bottom, Vector4 left)
  {
    BorderColor = new BorderColor
    {
      Top = top,
      Right = right,
      Bottom = bottom,
      Left = left
    };
  }

  public void SetBorderColor(string color)
  {
    SetBorderColor(color[0] == '#' ? HexToColor(color) : WordToColor(color));
  }

  public void SetBorderWidth(float value)
  {
    BorderWidth = new Vector4(value, value, value, value);
  }

  #endregion

  #region Текст

  private TextAlignment _textAlignment;
  private Vector4 _textColor;
  private bool _isTextChanged;
  private Vector2 _textOffset;

  public Vector2 TextOffset
  {
    get => _textOffset;
    set
    {
      if (_textOffset == value) return;
      _textOffset = value;
      _isTextChanged = true;
    }
  }

  public Vector4 TextColor
  {
    get => _textColor;
    set
    {
      if (_textColor == value) return;
      _textColor = value;
      _isTextChanged = true;
    }
  }

  public TextAlignment TextAlignment
  {
    get => _textAlignment;
    set
    {
      if (_textAlignment == value) return;
      _textAlignment = value;
      _isTextChanged = true;
    }
  }

  public bool IsTextChanged
  {
    get => _isTextChanged;
    set => _isTextChanged = value;
  }

  public void SetTextColor(string color)
  {
    TextColor = color[0] == '#' ? HexToColor(color) : WordToColor(color);
  }

  #endregion

  public ElementStyle()
  {
    _textAlignment = TextAlignment.Left;
    _textColor = new Vector4(1, 1, 1, 1);
    _isBackgroundChanged = true;
    _isBorderChanged = true;
    _isTextChanged = true;
  }

  private Vector4 WordToColor(string word)
  {
    return word switch
    {
      "white" => new Vector4(1, 1, 1, 1),
      "black" => new Vector4(0, 0, 0, 1),
      "red" => new Vector4(1, 0, 0, 1),
      "green" => new Vector4(0, 1, 0, 1),
      "blue" => new Vector4(0, 0, 1, 1),
      _ => default
    };
  }

  private Vector4 HexToColor(string hex)
  {
    if (hex[0] == '#')
    {
      return hex.Length switch
      {
        7 => RGB<float>.FromHex(hex).Vector3.AddW(1.0f),
        9 => (Vector4)RGBA32F.FromHex(hex),
        _ => default
      };
    }

    return default;
  }
}