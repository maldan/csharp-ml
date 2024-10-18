using System;
using System.Collections.Generic;
using System.IO;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Text;

public class AsciiEscapeCode
{
  public const string Reset = "\x1B[0m";
  public const string Bold = "\x1B[1m";
  public const string Italic = "\x1B[3m";

  public const string FontSize6 = "\x1B[6s";
  public const string FontSize8 = "\x1B[8s";
  public const string FontSize12 = "\x1B[12s";
  public const string FontSize14 = "\x1B[14s";
  public const string FontSize16 = "\x1B[16s";

  public const string White = "\x1B[38;2;255;255;255m";
  public const string Red = "\x1B[38;2;255;0;0m";
  public const string Blue = "\x1B[38;2;0;0;255m";
  public const string Green = "\x1B[38;2;0;255;0m";
  public const string Yellow = "\x1B[38;2;255;210;0m";
}

public struct BitmapSymbol
{
  public Rectangle Area;
  public int Width;
}

public struct BitmapFontStyle
{
  public BitmapFontStyleType Style;
  public int Size;
}

public enum BitmapFontStyleType
{
  Normal = 0,
  Bold = 1,
  Italic = 2
}

public class BitmapFont
{
  public Texture_2D<RGBA8> Texture = new(1, 1);

  public int Width => Texture.RAW.Width;
  public int Height => Texture.RAW.Height;

  private BitmapFontStyle _currentStyle;
  public BitmapFontStyle DefaultStyle { get; private set; }

  private Dictionary<BitmapFontStyle, Dictionary<char, BitmapSymbol>> _symbolMap = new();

  public BitmapSymbol GetSymbol(char chr)
  {
    if (_symbolMap.ContainsKey(_currentStyle))
      if (_symbolMap[_currentStyle].ContainsKey(chr))
        return _symbolMap[_currentStyle][chr];
    return new BitmapSymbol();
  }

  public BitmapFontStyleType FontStyle
  {
    set => _currentStyle.Style = value;
  }

  public int FontSize
  {
    set => _currentStyle.Size = value;
  }

  public void Load(string path)
  {
    var data = File.ReadAllBytes(path);
    var s = new MemoryStream(data);
    var r = new BinaryReader(s);

    var version = r.ReadByte();
    var symbolByteSize = r.ReadByte();
    var totalStyles = r.ReadByte();

    for (var i = 0; i < totalStyles; i++)
    {
      var style = r.ReadByte();
      var fontSize = r.ReadByte();
      var totalChars = r.ReadUInt16();

      // Выбираем первый шрифт по дефолту
      if (DefaultStyle.Size == 0)
        DefaultStyle = new BitmapFontStyle { Style = (BitmapFontStyleType)style, Size = fontSize };

      _currentStyle = new BitmapFontStyle { Style = (BitmapFontStyleType)style, Size = fontSize };
      _symbolMap[_currentStyle] = new Dictionary<char, BitmapSymbol>();

      for (var j = 0; j < totalChars; j++)
      {
        var symbolCode = r.ReadUInt16();
        var x = r.ReadUInt16();
        var y = r.ReadUInt16();
        var width = r.ReadByte();
        var height = r.ReadByte();
        var symbolWidth = r.ReadByte();
        var character = char.ConvertFromUtf32(symbolCode)[0];

        _symbolMap[_currentStyle][character] = new BitmapSymbol
        {
          Area = Rectangle.FromLeftTopWidthHeight(x, y, width, height),
          Width = symbolWidth
        };
        Console.WriteLine($"{symbolCode} {character} - {x} {y} {width} {height}");
      }
    }

    var bitmapWidth = r.ReadUInt16();
    var bitmapHeight = r.ReadUInt16();
    Texture.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    Texture.RAW.Resize(bitmapWidth, bitmapHeight);

    Console.WriteLine($"{bitmapWidth}x{bitmapHeight}");
    var p = 0;
    for (var y = 0; y < bitmapHeight; y++)
    for (var x = 0; x < bitmapWidth; x++)
    {
      var pixel = r.ReadByte();
      // if (pixel < 10) pixel = 10;
      Texture.RAW[p] = new RGBA8(255, 255, 255, pixel);
      p += 1;
    }

    _currentStyle = DefaultStyle;
    // for (var i = 0; i < bitmapWidth * bitmapHeight; i++) Console.WriteLine(r.ReadByte());
  }
}