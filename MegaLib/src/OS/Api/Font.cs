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
  }

  public static FontData Generate(string fontName, int fontSize)
  {
    return Generate(
      " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя",
      fontName, fontSize);
  }

  public static FontData Generate(string charset, string fontName, int fontSize)
  {
    var info = GetGlyphInfo(charset, fontName, fontSize);
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
    var hFont = GDI32.CreateFont(fontSize, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,
      GDI32.ANTIALIASED_QUALITY,
      0, fontName);
    GDI32.SelectObject(hdcMem, hFont);

    // Рендерим текст
    var offsetX = startOffset;
    foreach (var (k, v) in info)
    {
      var g = info[k];
      g.TextureArea = Rectangle.FromLeftTopWidthHeight(offsetX, 0, g.Width + padding, g.Height + padding);
      GDI32.TextOut(hdcMem, offsetX, 0, k.ToString(), 1);
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
        texture.RAW[x, y] = new RGBA<byte>(p.R, p.G, p.B, 0);
      else
        texture.RAW[x, y] = new RGBA<byte>(p.R, p.G, p.B, 255);
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

  /*public static void RenderCharacter(char c, string fontName, int fontSize)
  {
    var hdc = User32.GetDC(IntPtr.Zero);
    var hdcMem = GDI32.CreateCompatibleDC(hdc);

    var bmi = new GDI32.BITMAPINFO2();
    bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(GDI32.BITMAPINFOHEADER));
    bmi.bmiHeader.biWidth = 100;
    bmi.bmiHeader.biHeight = -100; // Отрицательное значение для правильного порядка строк
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
    var pixelData = new byte[100 * 100 * 4];
    Marshal.Copy(bits, pixelData, 0, pixelData.Length);
    for (var i = 0; i < pixelData.Length; i += 4) pixelData[i + 3] = 0; // Устанавливаем альфа-канал в 0
    Marshal.Copy(pixelData, 0, bits, pixelData.Length);

    // Создаем шрифт
    var hFont = GDI32.CreateFont(fontSize, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, fontName);
    GDI32.SelectObject(hdcMem, hFont);

    // Устанавливаем цвет текста на черный
    //GDI32.SetTextColor(hdcMem, 0xFFFFFFFF); // Черный текст

    // Рендерим текст
    GDI32.TextOut(hdcMem, 0, 0, c.ToString(), 1);

    // Сохранение в BMP
    SaveToBMP(bits, 100, 100, "sas.bmp");

    // Освобождаем ресурсы
    GDI32.DeleteObject(hFont);
    GDI32.DeleteObject(hBitmap);
    GDI32.DeleteDC(hdcMem);
    User32.ReleaseDC(IntPtr.Zero, hdc);
  }*/

  /*public static GDI32.SIZE GetGlyphSize(char c, string fontName, int fontSize)
  {
    var hdc = User32.GetDC(IntPtr.Zero);
    var hdcMem = GDI32.CreateCompatibleDC(hdc);

    var hFont = GDI32.CreateFont(fontSize, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, fontName);
    GDI32.SelectObject(hdcMem, hFont);

    GDI32.SIZE size;
    GDI32.GetTextExtentPoint32(hdcMem, c.ToString(), 1, out size);

    // Освобождение ресурсов
    GDI32.DeleteObject(hFont);
    GDI32.DeleteDC(hdcMem);
    User32.ReleaseDC(IntPtr.Zero, hdc);

    return size;
  }*/

  public static Dictionary<char, GlyphInfo> GetGlyphInfo(string charset, string fontName, int fontSize)
  {
    var d = new Dictionary<char, GlyphInfo>();

    var hdc = User32.GetDC(IntPtr.Zero);
    var hdcMem = GDI32.CreateCompatibleDC(hdc);

    var hFont = GDI32.CreateFont(fontSize, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, fontName);
    GDI32.SelectObject(hdcMem, hFont);

    foreach (var ch in charset)
    {
      GDI32.GetTextExtentPoint32(hdcMem, ch.ToString(), 1, out var size);
      d[ch] = new GlyphInfo
      {
        Width = size.cx,
        Height = size.cy
      };
    }

    // Освобождение ресурсов
    GDI32.DeleteObject(hFont);
    GDI32.DeleteDC(hdcMem);
    User32.ReleaseDC(IntPtr.Zero, hdc);

    return d;
  }

  /*private static void SaveToBMP(IntPtr bits, int width, int height, string outputPath)
  {
    // Создание BMP файла вручную
    using (var fileStream = new FileStream(outputPath, FileMode.Create))
    using (var writer = new BinaryWriter(fileStream))
    {
      // Пишем заголовок BMP файла
      writer.Write((ushort)0x4D42); // Тип файла 'BM'
      writer.Write(54 + width * height * 4); // Размер файла
      writer.Write((ushort)0); // Зарезервировано
      writer.Write((ushort)0); // Зарезервировано
      writer.Write(54); // Смещение пиксельных данных

      // Заголовок BITMAPINFOHEADER
      writer.Write(40); // Размер заголовка
      writer.Write(width); // Ширина изображения
      writer.Write(-height); // Высота изображения
      writer.Write((ushort)1); // Плоскости
      writer.Write((ushort)32); // Бит на пиксель
      writer.Write(0); // BI_RGB, без сжатия
      writer.Write(width * height * 4); // Размер изображения
      writer.Write(0); // Горизонтальное разрешение
      writer.Write(0); // Вертикальное разрешение
      writer.Write(0); // Использованные цвета
      writer.Write(0); // Важные цвета

      // Пишем пиксельные данные
      var pixelData = new byte[width * height * 4];
      Marshal.Copy(bits, pixelData, 0, pixelData.Length);
      writer.Write(pixelData);
    }
  }*/
}