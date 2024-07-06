using System;
using System.Collections.Generic;

namespace MegaLib.Audio;

public class AudioMixer
{
  private Dictionary<string, AudioChannel> _channels = new();
  public float[] Buffer { get; private set; }
  private float[] _emptyBuffer;

  public AudioMixer(int bufferSize)
  {
    Buffer = new float[bufferSize];
    _emptyBuffer = new float[bufferSize];
  }

  public void CreateChannel(string name)
  {
    _channels[name] = new AudioChannel(Buffer.Length);
  }

  public AudioChannel GetChannel(string name)
  {
    return _channels.GetValueOrDefault(name);
  }

  public void Tick()
  {
    // Обновляем все каналы
    foreach (var (name, channel) in _channels) channel.Tick();

    // Очищаем буффер нулями
    Array.Copy(_emptyBuffer, Buffer, Buffer.Length);

    // Миксуем звук из всех каналов
    for (var i = 0; i < Buffer.Length; i++)
      foreach (var (name, channel) in _channels)
        Buffer[i] += channel.Buffer[i];
  }
}