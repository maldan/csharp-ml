using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;
using Rectangle = MegaLib.Mathematics.Geometry.Rectangle;

namespace MegaLib.OS.Api;

public class FontData
{
  public Texture_2D<RGBA<byte>> Texture;
  public Dictionary<char, Font.GlyphInfo> GlyphInfo;

  public Font.GlyphInfo GetGlyph(char chr)
  {
    if (GlyphInfo.ContainsKey(chr))
      return GlyphInfo[chr];
    return new Font.GlyphInfo();
  }
}

public static class Font
{
  public struct GlyphInfo
  {
    public Rectangle TextureArea;
    public int Width;
    public int Height;
    public float ScaleFactor;
  }

  public static FontData Generate(string fontName, int fontSize, float scaleFactor)
  {
    return Generate(
      " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя",
      fontName, fontSize, scaleFactor);
  }

  public static FontData Generate(string charset, string fontName, int fontSize, float scaleFactor)
  {
    var info = GetGlyphInfo(charset, fontName, fontSize, scaleFactor);

    var startOffset = 1;
    var width = startOffset; // Начальный оффсет
    var height = 0;
    var padding = 1;
    foreach (var (k, v) in info)
    {
      width += v.Width + padding;
      height = Math.Max(height, v.Height);
    }

    width += startOffset;
    height += padding;

    var hdc = User32.GetDC(IntPtr.Zero);
    var hdcMem = GDI32.CreateCompatibleDC(hdc);

    var bmi = new GDI32.BITMAPINFO2();
    bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(GDI32.BITMAPINFOHEADER));
    bmi.bmiHeader.biWidth = width;
    bmi.bmiHeader.biHeight = -height; // Отрицательное значение для правильного порядка строк
    bmi.bmiHeader.biPlanes = 1;
    bmi.bmiHeader.biBitCount = 32;
    bmi.bmiHeader.biCompression = 0; // BI_RGB

    IntPtr bits;
    var hBitmap = GDI32.CreateDIBSection(hdcMem, ref bmi, GDI32.DIB_RGB_COLORS, out bits, IntPtr.Zero, 0);
    GDI32.SelectObject(hdcMem, hBitmap);

    // Устанавливаем прозрачный фон для текста
    GDI32.SetBkMode(hdcMem, GDI32.TRANSPARENT);
    //GDI32.SetBkColor(hdcMem, 0x00000000); // Белый фон
    GDI32.SetTextColor(hdcMem, 0x00FFFFFF); // Черный текст

    // Заполняем альфа-канал нулями, чтобы фон был прозрачным
    var pixelData = new byte[width * height * 4];
    Marshal.Copy(bits, pixelData, 0, pixelData.Length);
    /*for (var i = 0; i < pixelData.Length; i += 4) pixelData[i + 3] = 0; // Устанавливаем альфа-канал в 0
    Marshal.Copy(pixelData, 0, bits, pixelData.Length);*/

    // Создаем шрифт
    var hFont = GDI32.CreateFont((int)(fontSize * scaleFactor), 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,
      GDI32.ANTIALIASED_QUALITY,
      0, fontName);
    GDI32.SelectObject(hdcMem, hFont);

    // Рендерим текст
    var offsetX = startOffset;
    foreach (var (k, v) in info)
    {
      var g = info[k];
      g.TextureArea = Rectangle.FromLeftTopWidthHeight(offsetX, 0, g.Width + padding, g.Height + padding);
      GDI32.TextOut(hdcMem, (int)offsetX, 0, k.ToString(), 1);
      offsetX += v.Width + padding;
      info[k] = g;
    }

    // Сохранение в BMP
    // SaveToBMP(bits, width, height, "sas.bmp");
    var texture = new Texture_2D<RGBA<byte>>(width, height);
    texture.RAW.SetRaw(bits, pixelData.Length);
    // texture.SaveToBMP("gas.bmp");

    for (var y = 0; y < texture.RAW.Height; y++)
    for (var x = 0; x < texture.RAW.Width; x++)
    {
      var p = texture.RAW[x, y];
      if (p is { R: 0, B: 0, G: 0 })
        texture.RAW[x, y] = new RGBA<byte>(0, 0, 0, 0);
      else
        texture.RAW[x, y] = new RGBA<byte>(p.R, p.G, p.B, p.R);
    }

    // Освобождаем ресурсы
    GDI32.DeleteObject(hFont);
    GDI32.DeleteObject(hBitmap);
    GDI32.DeleteDC(hdcMem);
    User32.ReleaseDC(IntPtr.Zero, hdc);

    return new FontData
    {
      Texture = texture,
      GlyphInfo = info
    };
  }

  public static Dictionary<char, GlyphInfo> GetGlyphInfo(string charset, string fontName, int fontSize,
    float scaleFactor)
  {
    var d = new Dictionary<char, GlyphInfo>();

    var hdc = User32.GetDC(IntPtr.Zero);
    var hdcMem = GDI32.CreateCompatibleDC(hdc);

    var hFont = GDI32.CreateFont(
      (int)(fontSize * scaleFactor), 0,
      0, 0, 0, 0, 0, 0, 1, 0, 0,
      GDI32.ANTIALIASED_QUALITY,
      0, fontName);
    GDI32.SelectObject(hdcMem, hFont);

    foreach (var ch in charset)
    {
      GDI32.GetTextExtentPoint32(hdcMem, ch.ToString(), 1, out var size);
      d[ch] = new GlyphInfo
      {
        Width = size.cx,
        Height = size.cy,
        ScaleFactor = scaleFactor
      };
    }

    // Освобождение ресурсов
    GDI32.DeleteObject(hFont);
    GDI32.DeleteDC(hdcMem);
    User32.ReleaseDC(IntPtr.Zero, hdc);

    return d;
  }
}