using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MegaLib.Asm;
using MegaLib.Render.Color;

namespace MegaLib.Media.Image;

public class ImagePNG
{
  // PNG сигнатура (8 байт)
  private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

  public enum ColorType
  {
    Grayscale = 0,
    RGB = 2,
    IndexedColor = 3,
    GrayscaleWithAlpha = 4,
    RGBWithAlpha = 6
  }

  public enum BitDepth
  {
    Depth1 = 1,
    Depth2 = 2,
    Depth4 = 4,
    Depth8 = 8,
    Depth16 = 16
  }

  public static bool Check(byte[] pngData)
  {
    using var ms = new MemoryStream(pngData);
    using var reader = new BinaryReader(ms);
    var signature = reader.ReadBytes(8);
    return CheckSignature(signature);
  }

  public static object Decode(byte[] pngData)
  {
    if (!Check(pngData)) return null;

    using var ms = new MemoryStream(pngData);
    using var reader = new BinaryReader(ms);

    // Проверка сигнатуры PNG
    var signature = reader.ReadBytes(8);

    object bitmapData = null;

    // Пример: Создание списка для хранения байтов
    var totalChunkData = new List<byte>();

    // Чтение чанков
    while (reader.BaseStream.Position < reader.BaseStream.Length)
    {
      // Размер чанка (4 байта)
      var length = BitConverter.ToInt32(ReadBigEndian(reader, 4), 0);

      // Тип чанка (4 байта)
      var chunkType = Encoding.ASCII.GetString(reader.ReadBytes(4));

      // Данные чанка
      var chunkData = reader.ReadBytes(length);

      // CRC (4 байта)
      var crc = reader.ReadBytes(4);

      // Обработка отдельных типов чанков
      if (chunkType == "IHDR")
      {
        var (width, height, bitDepth, colorType) = ProcessIHDR(chunkData);
        if (colorType == ColorType.RGB && bitDepth == BitDepth.Depth8)
        {
          bitmapData = new BitmapData<RGB<byte>>(width, height);
        }
      }
      else if (chunkType == "IDAT")
      {
        // Добавляем данные чанка в список
        totalChunkData.AddRange(chunkData);
      }
      else if (chunkType == "IEND")
      {
        var uncompressedData = DecompressIDATData(totalChunkData.ToArray());
        Console.WriteLine("Конец PNG файла.");
        Console.WriteLine($"Bytes total {totalChunkData.Count}");
        Console.WriteLine($"Bytes total {uncompressedData.Length}");

        FillBitmapData(bitmapData, uncompressedData);

        break;
      }
    }

    return bitmapData;
  }

  private static bool CheckSignature(byte[] signature)
  {
    if (signature.Length != PngSignature.Length) return false;
    return !PngSignature.Where((t, i) => signature[i] != t).Any();
  }

  private static byte[] ReadBigEndian(BinaryReader reader, int byteCount)
  {
    var bytes = reader.ReadBytes(byteCount);
    Array.Reverse(bytes); // Переводим из Big-Endian в Little-Endian
    return bytes;
  }

  private static (int width, int height, BitDepth bitDepth, ColorType colorType) ProcessIHDR(byte[] data)
  {
    var width = BitConverter.ToInt32(ReadBigEndian(new BinaryReader(new MemoryStream(data)), 4), 0);
    var height = BitConverter.ToInt32(ReadBigEndian(new BinaryReader(new MemoryStream(data)), 4), 0);

    var bitDepth = (BitDepth)data[8];
    var colorType = (ColorType)data[9];

    Console.WriteLine($"Размер изображения: {width}x{height}");
    Console.WriteLine($"Глубина цвета: {bitDepth}");
    Console.WriteLine($"Тип цвета: {colorType}");

    return (width, height, bitDepth, colorType);
  }

  private static byte[] DecompressIDATData(byte[] chunkData)
  {
    // Удаляем Zlib заголовок, если он есть
    var dataWithoutHeader = new byte[chunkData.Length - 2];
    Array.Copy(chunkData, 2, dataWithoutHeader, 0, dataWithoutHeader.Length);

    using var inputStream = new MemoryStream(dataWithoutHeader);
    using var outputStream = new MemoryStream();
    using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
    deflateStream.CopyTo(outputStream);

    return outputStream.ToArray();
  }

  private static void FillBitmapData(object data, byte[] uncompressedData)
  {
    var bitmapData = (BitmapData<RGB<byte>>)data;
    var memcpy = AsmRuntime.MemCopy();

    var width = bitmapData.Width;
    var height = bitmapData.Height;
    var bytesPerPixel = bitmapData.BytesPerPixel;
    var stride = width * bytesPerPixel;

    var previousRow = new byte[stride];
    var currentRow = new byte[stride];

    var offset = 0;

    for (var y = 0; y < height; y++)
    {
      var filterType = uncompressedData[offset];
      offset++;

      Buffer.BlockCopy(uncompressedData, offset, currentRow, 0, stride);
      offset += stride;

      switch (filterType)
      {
        case 0:
          // No filter
          break;
        case 1:
          ApplySubFilter(currentRow, bytesPerPixel);
          break;
        case 2:
          ApplyUpFilter(currentRow, previousRow);
          break;
        case 3:
          ApplyAverageFilter(currentRow, previousRow, bytesPerPixel);
          break;
        case 4:
          ApplyPaethFilter(currentRow, previousRow, bytesPerPixel);
          break;
        default:
          throw new NotSupportedException($"Unsupported filter type {filterType}");
      }

      // Копируем строку в bitmapData

      var fromPtr = Marshal.UnsafeAddrOfPinnedArrayElement(currentRow, 0);
      var toPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bitmapData.Pixels, 0);
      memcpy(toPtr + y * stride, fromPtr, stride);

      // Меняем строки местами
      (previousRow, currentRow) = (currentRow, previousRow);
    }
  }

  private static void ApplySubFilter(byte[] row, int bytesPerPixel)
  {
    for (var i = bytesPerPixel; i < row.Length; i++)
    {
      row[i] = (byte)((row[i] + row[i - bytesPerPixel]) & 0xFF);
    }
  }

  private static void ApplyUpFilter(byte[] row, byte[] previousRow)
  {
    for (var i = 0; i < row.Length; i++)
    {
      row[i] = (byte)((row[i] + previousRow[i]) & 0xFF);
    }
  }

  private static void ApplyAverageFilter(byte[] row, byte[] previousRow, int bytesPerPixel)
  {
    for (var i = 0; i < row.Length; i++)
    {
      var left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
      int up = previousRow[i];
      row[i] = (byte)((row[i] + ((left + up) >> 1)) & 0xFF);
    }
  }

  private static void ApplyPaethFilter(byte[] row, byte[] previousRow, int bytesPerPixel)
  {
    for (var i = 0; i < row.Length; i++)
    {
      var left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
      int up = previousRow[i];
      var upLeft = i >= bytesPerPixel ? previousRow[i - bytesPerPixel] : 0;

      row[i] = (byte)((row[i] + PaethPredictor(left, up, upLeft)) & 0xFF);
    }
  }

  private static byte PaethPredictor(int left, int up, int upLeft)
  {
    var p = left + up - upLeft;
    var pa = Math.Abs(p - left);
    var pb = Math.Abs(p - up);
    var pc = Math.Abs(p - upLeft);

    if (pa <= pb && pa <= pc) return (byte)left;
    return pb <= pc ? (byte)up : (byte)upLeft;
  }
}