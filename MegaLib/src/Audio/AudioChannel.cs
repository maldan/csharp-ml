using System;

namespace MegaLib.Audio;

public class AudioChannel
{
  private AudioSample _currentSample;
  public float[] Buffer { get; private set; }
  private int _sampleOffset;

  public AudioChannel(int bufferSize)
  {
    Buffer = new float[bufferSize];
  }

  public void Tick()
  {
    // Семпла нет
    if (_currentSample == null)
    {
      for (var i = 0; i < Buffer.Length; i++)
        Buffer[i] = 0;
    }
    else
    {
      // Копируем из семлпа в буффер
      Array.Copy(
        _currentSample.Buffer,
        _sampleOffset,
        Buffer,
        0,
        Buffer.Length);

      // Смещаемся по буфферу
      _sampleOffset += Buffer.Length;

      // Проиграли до конца семпл, значит удаляем
      if (_sampleOffset >= _currentSample.Buffer.Length)
      {
        _sampleOffset = 0;
        _currentSample = null;
      }
    }
  }

  public void Queue(AudioSample sample)
  {
    _currentSample = sample;
    _sampleOffset = 0;
  }
}