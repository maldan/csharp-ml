using System.Collections.Generic;

namespace MegaLib.Audio;

public class AudioMixer
{
  private Dictionary<string, AudioChannel> _channels = new();

  public void AddChannel(string name)
  {
  }

  public AudioChannel GetChannel(string name)
  {
    return _channels.GetValueOrDefault(name);
  }
}