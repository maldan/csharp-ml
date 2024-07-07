using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = MegaLib.IO.Console;

namespace MegaLib.Audio;

public class AudioManager
{
  public AudioOutput _audioOutput;
  public AudioMixer Mixer { get; private set; }
  private Dictionary<string, AudioSample> _samples = new();

  public int SampleRate { get; private set; }
  public bool IsStereo { get; private set; }
  public int BufferSize { get; private set; }

  private Mutex _mu = new();

  public AudioManager(int bufferSize, int sampleRate, bool isStereo)
  {
    SampleRate = sampleRate;
    BufferSize = bufferSize;
    IsStereo = isStereo;
    Mixer = new AudioMixer(BufferSize);

    _audioOutput = new AudioOutput(BufferSize, sampleRate, isStereo);
    _audioOutput.Open();
  }

  public void LoadSample(string path, string name)
  {
    var sample = new AudioSample();
    sample.FromFile(path);
    sample.Align(BufferSize);
    _samples[name] = sample;

    Console.WriteLine($"Sample Rate: {sample.SampleRate}");
    Console.WriteLine($"Channels: {sample.NumberOfChannels}");
    Console.WriteLine($"Bits Per Sample: {sample.BitsPerSample}");
    Console.WriteLine($"Data Length: {sample.Buffer.Length}");
  }

  public void PlaySample(string name, string channelName)
  {
    _mu.WaitOne();
    var sample = _samples[name];
    var channel = Mixer.GetChannel(channelName);
    channel.Queue(sample);
    Console.WriteLine($"Queue sample {name} -> {channelName}");
    _mu.ReleaseMutex();
  }

  public void Run()
  {
    Task.Run(() =>
    {
      while (true)
      {
        _mu.WaitOne();
        Mixer.Tick();
        _audioOutput.Fill(Mixer.Buffer);
        _mu.ReleaseMutex();

        var ms = (int)(_audioOutput.BufferSize / (float)_audioOutput.SampleRate * 500f);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < ms) Thread.SpinWait(1);
      }
    });

    Thread.Sleep(32);

    Task.Run(() =>
    {
      while (true) _audioOutput.Tick();
    });
  }
}