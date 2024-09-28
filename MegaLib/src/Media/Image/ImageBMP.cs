using System;
using System.IO;
using MegaLib.Render.Color;

namespace MegaLib.Media.Image;

public class ImageBMP
{
  public static object Decode(byte[] bmpData)
  {
    using var ms = new MemoryStream(bmpData);
    using var reader = new BinaryReader(ms);

    // Чтение заголовка BMP
    var header = new string(reader.ReadChars(2));
    if (header != "BM") throw new InvalidDataException("Неправильный BMP файл.");

    reader.ReadInt32(); // Размер файла (неиспользуемый)
    reader.ReadInt32(); // Зарезервировано
    var pixelArrayOffset = reader.ReadInt32(); // Смещение до массива пикселей

    // Чтение информации о заголовке изображения
    if (reader.ReadInt32() != 40) throw new NotSupportedException("Неподдерживаемый формат DIB.");
    var width = reader.ReadInt32();
    var height = reader.ReadInt32();
    reader.ReadUInt16(); // Плоскости
    var bitsPerPixel = reader.ReadUInt16();

    if (bitsPerPixel != 24 && bitsPerPixel != 32)
      throw new NotSupportedException("Поддерживаются только 24-битный и 32-битный BMP.");

    var bytesPerPixel = bitsPerPixel / 8;

    // Пропуск ненужных данных
    reader.ReadInt32(); // Сжатие
    reader.ReadInt32(); // Размер изображения
    reader.ReadInt32(); // Горизонтальное разрешение
    reader.ReadInt32(); // Вертикальное разрешение
    reader.ReadInt32(); // Количество цветов
    reader.ReadInt32(); // Важные цвета

    // Чтение массива пикселей
    ms.Seek(pixelArrayOffset, SeekOrigin.Begin);
    var stride = (width * bytesPerPixel + 3) / 4 * 4; // Выравнивание до 4 байт
    var pixels = new RGB<byte>[width * height];

    for (var y = 0; y < height; y++)
    {
      var row = new byte[stride];
      reader.Read(row, 0, stride);

      for (var x = 0; x < width; x++)
      {
        var pixelIndex = x * bytesPerPixel;
        RGB<byte> color;

        // Меняем местами B и R для каждого пикселя
        if (bitsPerPixel == 24)
        {
          color = new RGB<byte>(row[pixelIndex + 2], row[pixelIndex + 1], row[pixelIndex]);
        }
        else // bitsPerPixel == 32
        {
          color = new RGB<byte>(row[pixelIndex + 2], row[pixelIndex + 1], row[pixelIndex]);
        }

        pixels[(height - 1 - y) * width + x] = color; // Сохранение с учетом порядка
      }
    }

    return new BitmapData<RGB<byte>>(width, height, pixels);
  }

  public static byte[] Encode(object data)
  {
    var bitmapData = (BitmapData<RGB<byte>>)data;

    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms);

    writer.Write(new char[] { 'B', 'M' }); // Заголовок
    writer.Write((int)(ms.Length + 54 +
                       bitmapData.Width * bitmapData.Height * bitmapData.BytesPerPixel)); // Общий размер
    writer.Write(0); // Зарезервировано
    writer.Write(54); // Смещение до массива пикселей

    // Заголовок DIB
    writer.Write(40); // Размер заголовка
    writer.Write(bitmapData.Width); // Ширина 
    writer.Write(bitmapData.Height); // Высота
    writer.Write((ushort)1); // Плоскость
    writer.Write((ushort)(bitmapData.BytesPerPixel * 8)); // Биты на пиксель
    writer.Write(0); // Сжатие
    writer.Write(0); // Размер изображения
    writer.Write(0); // Горизонтальное разрешение
    writer.Write(0); // Вертикальное разрешение
    writer.Write(0); // Количество цветов
    writer.Write(0); // Важные цвета

    // Запись массива пикселей
    var stride = (bitmapData.Width * bitmapData.BytesPerPixel + 3) / 4 * 4; // Выравнивание до 4 байт
    var paddedRow = new byte[stride];

    for (var y = bitmapData.Height - 1; y >= 0; y--)
    {
      for (var x = 0; x < bitmapData.Width; x++)
      {
        // Меняем местами B и R
        var pixelIndex = x;
        paddedRow[x * bitmapData.BytesPerPixel] = bitmapData.Pixels[y * bitmapData.Width + x].B;
        paddedRow[x * bitmapData.BytesPerPixel + 1] = bitmapData.Pixels[y * bitmapData.Width + x].G;
        paddedRow[x * bitmapData.BytesPerPixel + 2] = bitmapData.Pixels[y * bitmapData.Width + x].R;

        if (bitmapData.BytesPerPixel == 4)
        {
          paddedRow[x * bitmapData.BytesPerPixel + 3] = 0; // Альфа-канал по умолчанию 0
        }
      }

      // Заполнение оставшихся байтов 0
      writer.Write(paddedRow);
    }

    return ms.ToArray(); // Возвращаем байтовый массив
  }
}