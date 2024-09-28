using System;
using MegaLib.Render.Color;

namespace MegaLib.Media.Image;

public class BitmapData<T> where T : struct
{
  public T[] Pixels { get; }
  public int Width { get; }
  public int Height { get; }
  public int BytesPerPixel { get; private set; }

  public BitmapData(int w, int h)
  {
    Width = w;
    Height = h;
    Pixels = new T[w * h];
    AutoFormat();
  }

  public BitmapData(int w, int h, T[] data)
  {
    Width = w;
    Height = h;
    Pixels = data;
    AutoFormat();
  }

  private void AutoFormat()
  {
    BytesPerPixel = Pixels switch
    {
      RGB<byte>[] => 3,
      RGBA<byte>[] => 4,
      _ => 0
    };
    Console.WriteLine(BytesPerPixel);
  }

  public void SetPixel(int x, int y, T color)
  {
    var index = y * Width + x;
    Pixels[index] = color;
  }

  public T GetPixel(int x, int y)
  {
    var index = y * Width + x;
    return Pixels[index];
  }

  public T this[int x, int y]
  {
    get => GetPixel(x, y);
    set => SetPixel(x, y, value);
  }

  public T this[int index]
  {
    get => Pixels[index];
    set => Pixels[index] = value;
  }
}