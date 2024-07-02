using System.Collections.Generic;
using System.IO;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Text;

public struct BitmapSymbol
{
  public Rectangle Area;
  public int Width;
}

public class BitmapFont
{
  public Texture_2D<RGBA<byte>> Texture = new(1, 1);

  public int Width => Texture.RAW.Width;
  public int Height => Texture.RAW.Height;

  private Dictionary<string, BitmapSymbol> _symbolMap = new();

  public BitmapSymbol GetSymbol(char chr)
  {
    return _symbolMap.TryGetValue(chr + "", out var value) ? value : new BitmapSymbol();
  }

  public void Load(string path)
  {
    var data = File.ReadAllBytes(path);
    var s = new MemoryStream(data);
    var r = new BinaryReader(s);

    var version = r.ReadByte();
    var symbolByteSize = r.ReadByte();
    var totalChars = r.ReadUInt16();
    Console.WriteLine(version);
    Console.WriteLine(symbolByteSize);
    Console.WriteLine(totalChars);

    for (var i = 0; i < totalChars; i++)
    {
      var symbolCode = r.ReadUInt32();
      var x = r.ReadUInt16();
      var y = r.ReadUInt16();
      var width = r.ReadUInt16();
      var height = r.ReadUInt16();
      var character = char.ConvertFromUtf32((int)symbolCode)[0] + "";

      _symbolMap[character] = new BitmapSymbol
      {
        Area = Rectangle.FromLeftTopWidthHeight(x, y, width, height)
      };
      Console.WriteLine($"{symbolCode} {character} - {x} {y} {width} {height}");
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
      Texture.RAW[p] = new RGBA<byte>(255, 255, 255, pixel);
      p += 1;
    }

    // for (var i = 0; i < bitmapWidth * bitmapHeight; i++) Console.WriteLine(r.ReadByte());
  }
}