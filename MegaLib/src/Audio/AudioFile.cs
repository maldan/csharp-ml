using System;
using System.IO;

namespace MegaLib.Audio;

public class AudioFile
{
  public int NumberOfChannels { get; private set; }
  public int SampleRate { get; private set; }
  public int BitsPerSample { get; private set; }
  public float[] Buffer { get; private set; }

  public void ReadWave(string path)
  {
    var wavData = File.ReadAllBytes(path);

    using var ms = new MemoryStream(wavData);
    using var reader = new BinaryReader(ms);

    // Чтение заголовка RIFF
    var chunkID = new string(reader.ReadChars(4));
    if (chunkID != "RIFF")
      throw new FormatException("Invalid file format");

    reader.ReadInt32(); // Chunk size
    var format = new string(reader.ReadChars(4));
    if (format != "WAVE")
      throw new FormatException("Invalid file format");

    // Чтение заголовка fmt 
    var subChunk1ID = new string(reader.ReadChars(4));
    if (subChunk1ID != "fmt ")
      throw new FormatException("Invalid file format");

    var subChunk1Size = reader.ReadInt32();
    var audioFormat = reader.ReadInt16();
    NumberOfChannels = reader.ReadInt16();
    SampleRate = reader.ReadInt32();
    reader.ReadInt32(); // Byte rate
    reader.ReadInt16(); // Block align
    BitsPerSample = reader.ReadInt16();

    // Чтение данных
    var subChunk2ID = new string(reader.ReadChars(4));
    if (subChunk2ID != "data")
      throw new FormatException("Invalid file format");

    var subChunk2Size = reader.ReadInt32();
    var audioData = reader.ReadBytes(subChunk2Size);

    // Конвертация байтов в шорты
    var sampleCount = subChunk2Size / (BitsPerSample / 8);

    var buffer = new short[sampleCount];
    System.Buffer.BlockCopy(audioData, 0, buffer, 0, subChunk2Size);

    // Заполняем финальный буффер значениями от -1 до 1
    Buffer = new float[buffer.Length];
    for (var i = 0; i < buffer.Length; i++) Buffer[i] = buffer[i] / 32768.0f;
  }

  /*static short[] MonoToStereo(short[] monoBuffer)
  {
      short[] stereoBuffer = new short[monoBuffer.Length * 2];
      for (int i = 0; i < monoBuffer.Length; i++)
      {
          stereoBuffer[2 * i] = monoBuffer[i];      // Левый канал
          stereoBuffer[2 * i + 1] = monoBuffer[i];  // Правый канал
      }
      return stereoBuffer;
  }*/
}