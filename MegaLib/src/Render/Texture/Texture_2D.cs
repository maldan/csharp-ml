using System.IO;
using System.Runtime.InteropServices;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;

namespace MegaLib.Render.Texture;

public class Texture_2D<T>
{
  public ImageGPU<T> RAW { get; private set; }
  public TextureOptions Options;
  public ulong Id;

  public Texture_2D(ImageGPU<T> raw)
  {
    RAW = raw;
    Options.FiltrationMode = TextureFiltrationMode.Linear;
    Options.WrapMode = TextureWrapMode.Clamp;
    Options.UseMipMaps = true;
    Id = TextureId.NextId();

    AutoFormat();
  }

  public Texture_2D(int width, int height)
  {
    RAW = new ImageGPU<T>(width, height);
    Options.FiltrationMode = TextureFiltrationMode.Linear;
    Options.WrapMode = TextureWrapMode.Clamp;
    Options.UseMipMaps = true;
    Id = TextureId.NextId();

    AutoFormat();
  }

  private void AutoFormat()
  {
    Options.Format = RAW switch
    {
      ImageGPU<byte> => TextureFormat.R8,
      ImageGPU<RGB8> => TextureFormat.RGB8,
      ImageGPU<RGBA8> => TextureFormat.RGBA8,
      ImageGPU<float> => TextureFormat.R32F,
      _ => Options.Format
    };
  }

  public void SaveToBMP(string outputPath)
  {
    if (Options.Format != TextureFormat.RGBA8) return;

    // Создание BMP файла вручную
    using var fileStream = new FileStream(outputPath, FileMode.Create);
    using var writer = new BinaryWriter(fileStream);

    // Пишем заголовок BMP файла
    writer.Write((ushort)0x4D42); // Тип файла 'BM' (тип: ushort, 2 байта)
    writer.Write((uint)(54 + RAW.Width * RAW.Height * 4)); // Размер файла (тип: uint, 4 байта)
    writer.Write((ushort)0); // Зарезервировано (тип: ushort, 2 байта)
    writer.Write((ushort)0); // Зарезервировано (тип: ushort, 2 байта)
    writer.Write((uint)54); // Смещение пиксельных данных (тип: uint, 4 байта)

    // Заголовок BITMAPINFOHEADER
    writer.Write((uint)40); // Размер заголовка (тип: uint, 4 байта)
    writer.Write((int)RAW.Width); // Ширина изображения (тип: int, 4 байта)
    writer.Write((int)-RAW.Height); // Высота изображения (тип: int, 4 байта)
    writer.Write((ushort)1); // Плоскости (тип: ushort, 2 байта)
    writer.Write((ushort)32); // Бит на пиксель (тип: ushort, 2 байта)
    writer.Write((uint)0); // BI_RGB, без сжатия (тип: uint, 4 байта)
    writer.Write((uint)(RAW.Width * RAW.Height * 4)); // Размер изображения (тип: uint, 4 байта)
    writer.Write((int)0); // Горизонтальное разрешение (тип: int, 4 байта)
    writer.Write((int)0); // Вертикальное разрешение (тип: int, 4 байта)
    writer.Write((uint)0); // Использованные цвета (тип: uint, 4 байта)
    writer.Write((uint)0); // Важные цвета (тип: uint, 4 байта)

    // Пишем пиксельные данные
    var pixelData = new byte[RAW.Width * RAW.Height * 4];
    Marshal.Copy(RAW.DataPtr, pixelData, 0, pixelData.Length);
    writer.Write(pixelData);
  }
}