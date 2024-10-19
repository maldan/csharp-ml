using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ImageMagick;
using MegaLib.Asm;
using MegaLib.Render.Color;

namespace MegaLib.Media.Image;

public class BitmapData<T> where T : struct
{
  public T[] Pixels { get; }
  public int Width { get; }
  public int Height { get; }
  public byte ChannelAmount { get; private set; }
  public int ByteSize => Width * Height * ChannelAmount;

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
    ChannelAmount = Pixels switch
    {
      byte[] => 1,
      RGB8[] => 3,
      RGBA8[] => 4,
      _ => 0
    };
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

  public void SetRaw(IntPtr ptr, int size)
  {
    var memcpy = AsmRuntime.MemCopy();
    memcpy(Marshal.UnsafeAddrOfPinnedArrayElement(Pixels, 0), ptr, size);
  }

  public void CopyTo(IntPtr destPtr, int size)
  {
    var memcpy = AsmRuntime.MemCopy();
    memcpy(destPtr, Marshal.UnsafeAddrOfPinnedArrayElement(Pixels, 0), size);
  }
}

public static class BitmapData
{
  public static BitmapData<T> FromFile<T>(string path) where T : struct
  {
    var d = File.ReadAllBytes(path);
    return FromFile<T>(d);
  }

  public static BitmapData<T> FromFile<T>(byte[] data) where T : struct
  {
    using var image = new MagickImage(data);
    var targetBitmap = new BitmapData<T>((int)image.Width, (int)image.Height);
    byte[] pixels = [];

    if (typeof(T) == typeof(byte))
    {
      var pixelsSource = image.GetPixels().ToByteArray(PixelMapping.RGB);

      pixels = new byte[pixelsSource.Length / 3];
      var p = 0;
      for (var i = 0; i < pixelsSource.Length; i += 3)
      {
        pixels[p] = (byte)(0.3 * pixelsSource[i] + 0.59 * pixelsSource[i + 1] + 0.11 * pixelsSource[i + 2]);
        p++;
      }

      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      targetBitmap.SetRaw(ptr, pixels.Length);
    }

    if (typeof(T) == typeof(RGBA8))
    {
      pixels = image.GetPixels().ToByteArray(PixelMapping.RGBA);
      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      targetBitmap.SetRaw(ptr, pixels.Length);
    }

    if (typeof(T) == typeof(RGB8))
    {
      pixels = image.GetPixels().ToByteArray(PixelMapping.RGB);
      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      targetBitmap.SetRaw(ptr, pixels.Length);
    }

    return targetBitmap;
  }

  public static BitmapData<T> FromFileX<T>(string path) where T : struct
  {
    var d = File.ReadAllBytes(path);
    return FromFileX<T>(d);
  }

  public static BitmapData<T> FromFileX<T>(byte[] data) where T : struct
  {
    if (data == null)
      throw new Exception("Empty texture data");

    using var memoryStream = new MemoryStream(data);
    var bmp = new Bitmap(memoryStream);

    // Определяем количество каналов в исходном изображении
    var sourceChannels = GetChannelsCount(bmp.PixelFormat);

    // Создаем объект BitmapData с нужным форматом
    var targetBitmap = new BitmapData<T>(bmp.Width, bmp.Height);
    var bitmapRect = new Rectangle(0, 0, bmp.Width, bmp.Height);

    // Определяем целевой формат по типу T
    var targetChannels = targetBitmap.ChannelAmount;

    /*// Если в картинке 3 канала, а нам надо 4
    if (sourceChannels == 3 && targetChannels == 3)
    {
      var bmpRgba = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
      using var g = Graphics.FromImage(bmpRgba);
      g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
      bmp = bmpRgba;
      sourceChannels = GetChannelsCount(bmp.PixelFormat);
    }*/

    // Если в картинке 3 канала, а нам надо 4
    if (sourceChannels == 3 && targetChannels == 4)
    {
      var bmpRgba = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
      using var g = Graphics.FromImage(bmpRgba);
      g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
      bmp = bmpRgba;
      sourceChannels = GetChannelsCount(bmp.PixelFormat);
    }

    // Копируем пиксели из Bitmap в массив байтов
    var bitmapData = bmp.LockBits(bitmapRect, ImageLockMode.ReadOnly, bmp.PixelFormat);
    var pixelBuffer = new byte[Math.Abs(bitmapData.Stride) * bmp.Height];
    Marshal.Copy(bitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
    bmp.UnlockBits(bitmapData);

    if (sourceChannels == targetChannels)
    {
      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(pixelBuffer, 0);
      targetBitmap.SetRaw(ptr, pixelBuffer.Length);
      bmp.Dispose();
      return targetBitmap;
    }

    // Преобразование пикселей в нужный формат
    var sourceStride = Math.Abs(bitmapData.Stride);

    for (var y = 0; y < bmp.Height; y++)
    {
      for (var x = 0; x < bmp.Width; x++)
      {
        var sourceOffset = y * sourceStride + x * sourceChannels;

        // Чтение пикселей в зависимости от исходного формата
        var r = pixelBuffer[sourceOffset];
        var g = pixelBuffer[sourceOffset + 1];
        var b = pixelBuffer[sourceOffset + 2];
        var a = sourceChannels == 4 ? pixelBuffer[sourceOffset + 3] : (byte)255;

        if (typeof(T) == typeof(byte))
        {
          var color = (byte)((r + g + b) / 3f);
          targetBitmap.SetPixel(x, y, (T)(object)color);
        }
        else

          // Преобразование в целевой формат
        if (typeof(T) == typeof(RGB8))
        {
          // Если целевой формат RGB, игнорируем альфа-канал
          var color = new RGB8(b, g, r);
          targetBitmap.SetPixel(x, y, (T)(object)color);
        }
        else if (typeof(T) == typeof(RGBA8))
        {
          // Если целевой формат RGBA, добавляем альфа-канал
          var color = new RGBA8(b, g, r, a);
          targetBitmap.SetPixel(x, y, (T)(object)color);
        }
        else
        {
          throw new NotSupportedException($"Unsupported target format: {typeof(T)}");
        }
      }
    }

    bmp.Dispose();
    return targetBitmap;
  }

  public static void ToFile<T>(this BitmapData<T> bitmapData, string filePath) where T : struct
  {
    // Пример данных изображения
    byte[] imageBytes; // Ваш массив сырых байтов

    // Предположим, что у нас массив пикселей RGB (3 канала на пиксель)
    imageBytes = new byte[bitmapData.ByteSize]; // Пример массива для RGB

    var ptr1 = Marshal.UnsafeAddrOfPinnedArrayElement(imageBytes, 0);
    bitmapData.CopyTo(ptr1, imageBytes.Length);

    // Создаем объект MagickImage из сырых байтов
    using var image = new MagickImage();

    // Чтение пикселей в зависимости от формата
    if (bitmapData.ChannelAmount == 3)
    {
      // Если у вас RGB
      image.ReadPixels(
        imageBytes,
        0, (uint)imageBytes.Length,
        new PixelReadSettings((uint)bitmapData.Width, (uint)bitmapData.Height, StorageType.Char, PixelMapping.RGB));
    }
    else if (bitmapData.ChannelAmount == 4)
    {
      // Если у вас RGBA
      image.ReadPixels(
        imageBytes,
        0, (uint)imageBytes.Length,
        new PixelReadSettings((uint)bitmapData.Width, (uint)bitmapData.Height, StorageType.Char, PixelMapping.RGBA));
    }
    else if (bitmapData.ChannelAmount == 1)
    {
      var rgbBytes = new byte[imageBytes.Length * 3];
      var p = 0;
      foreach (var b in imageBytes)
      {
        rgbBytes[p] = b;
        rgbBytes[p + 1] = b;
        rgbBytes[p + 2] = b;
        p += 3;
      }

      image.ReadPixels(
        rgbBytes,
        0, (uint)rgbBytes.Length,
        new PixelReadSettings(
          (uint)bitmapData.Width, (uint)bitmapData.Height,
          StorageType.Char, PixelMapping.RGB
        )
      );
      image.ColorSpace = ColorSpace.Gray;
    }
    else
    {
      throw new NotSupportedException("Unsupported channel count.");
    }

    image.Write(filePath);
  }

  public static void ToFileX<T>(this BitmapData<T> bitmapData, string filePath, ImageFormat format) where T : struct
  {
    int targetChannels = bitmapData.ChannelAmount;

    // Определяем нужный пиксельный формат для Bitmap в зависимости от количества каналов
    var pixelFormat = targetChannels switch
    {
      1 => PixelFormat.Format8bppIndexed, // 1-канальный (грейскейл)
      3 => PixelFormat.Format24bppRgb, // 3-канальный (RGB)
      4 => PixelFormat.Format32bppArgb, // 4-канальный (RGBA)
      _ => throw new NotSupportedException($"Unsupported channel count: {targetChannels}")
    };

    // Создаем Bitmap с нужным размером и форматом пикселей
    using var bitmap = new Bitmap(bitmapData.Width, bitmapData.Height, pixelFormat);

    if (targetChannels == 1)
    {
      // Для 8-битного изображения требуется вручную настроить палитру (грейскейл)
      var palette = bitmap.Palette;
      for (var i = 0; i < palette.Entries.Length; i++)
      {
        palette.Entries[i] = Color.FromArgb(i, i, i);
      }

      bitmap.Palette = palette;
    }

    // Заполняем Bitmap данными из BitmapData
    var rect = new Rectangle(0, 0, bitmapData.Width, bitmapData.Height);
    var bitmapDataInternal = bitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);

    // Копируем пиксели в буфер
    var pixelBuffer = new byte[Math.Abs(bitmapDataInternal.Stride) * bitmap.Height];
    var stride = Math.Abs(bitmapDataInternal.Stride);

    for (var y = 0; y < bitmapData.Height; y++)
    {
      for (var x = 0; x < bitmapData.Width; x++)
      {
        var targetOffset = y * stride + x * targetChannels;

        // Преобразование пикселей в зависимости от типа T и количества каналов
        if (targetChannels == 1 && typeof(T) == typeof(byte)) // 1-канальный (грейскейл)
        {
          var grayValue = (byte)(object)bitmapData[x, y];
          pixelBuffer[targetOffset] = grayValue;
        }
        else if (targetChannels == 3 && typeof(T) == typeof(RGB8)) // 3-канальный (RGB)
        {
          var pixel = (RGB8)(object)bitmapData[x, y];
          pixelBuffer[targetOffset] = pixel.R;
          pixelBuffer[targetOffset + 1] = pixel.G;
          pixelBuffer[targetOffset + 2] = pixel.B;
        }
        else if (targetChannels == 4 && typeof(T) == typeof(RGBA8)) // 4-канальный (RGBA)
        {
          var pixel = (RGBA8)(object)bitmapData[x, y];
          pixelBuffer[targetOffset] = pixel.R;
          pixelBuffer[targetOffset + 1] = pixel.G;
          pixelBuffer[targetOffset + 2] = pixel.B;
          pixelBuffer[targetOffset + 3] = pixel.A;
        }
        else
        {
          throw new NotSupportedException($"Unsupported format: {typeof(T)}");
        }
      }
    }

    // Копируем данные обратно в Bitmap
    Marshal.Copy(pixelBuffer, 0, bitmapDataInternal.Scan0, pixelBuffer.Length);
    bitmap.UnlockBits(bitmapDataInternal);

    // Сохраняем Bitmap в файл с нужным форматом
    bitmap.Save(filePath, format);
  }

  private static int GetChannelsCount(PixelFormat pixelFormat)
  {
    return pixelFormat switch
    {
      PixelFormat.Format24bppRgb => 3,
      PixelFormat.Format32bppArgb or PixelFormat.Format32bppPArgb => 4,
      _ => throw new NotSupportedException("Unsupported image format")
    };
  }
}