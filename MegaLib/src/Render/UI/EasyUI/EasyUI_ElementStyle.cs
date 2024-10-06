using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ElementStyle
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
      _x = value;
      _isBackgroundChanged = true;
    }
  }

  public float Y
  {
    get => _y;
    set
    {
      _y = value;
      _isBackgroundChanged = true;
    }
  }

  public float Width
  {
    get => _width;
    set
    {
      _width = value;
      _isBackgroundChanged = true;
    }
  }

  public float Height
  {
    get => _height;
    set
    {
      _height = value;
      _isBackgroundChanged = true;
    }
  }

  public Vector4 BorderRadius
  {
    get => _borderRadius;
    set
    {
      _borderRadius = value;
      _isBackgroundChanged = true;
    }
  }

  public Vector4 BackgroundColor
  {
    get => _backgroundColor;
    set
    {
      _backgroundColor = value;
      _isBackgroundChanged = true;
    }
  }

  public bool IsBackgroundChanged
  {
    get => _isBackgroundChanged;
    set => _isBackgroundChanged = value;
  }

  public Vector2 Position => new(X, Y);
  public Vector2 Size => new(Width, Height);
  public Rectangle BoundingBox => Rectangle.FromLeftTopWidthHeight(X, Y, Width, Height);

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
  private Vector4[] _borderColor;
  private bool _isBorderChanged;

  public Vector4 BorderWidth
  {
    get => _borderWidth;
    set
    {
      _borderWidth = value;
      _isBorderChanged = true;
    }
  }

  public Vector4[] BorderColor
  {
    get => _borderColor;
    set
    {
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
    BorderColor = [color, color, color, color];
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

  public Vector4 TextColor
  {
    get => _textColor;
    set
    {
      _textColor = value;
      _isTextChanged = true;
    }
  }

  public TextAlignment TextAlignment
  {
    get => _textAlignment;
    set
    {
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
        9 => RGBA<float>.FromHex(hex).Vector4,
        _ => default
      };
    }

    return default;
  }
}