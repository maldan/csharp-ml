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

  public List<int> Sex = new();

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
    _samples[name] = sample;

    Console.WriteLine($"Sample Rate: {sample.SampleRate}");
    Console.WriteLine($"Channels: {sample.NumberOfChannels}");
    Console.WriteLine($"Bits Per Sample: {sample.BitsPerSample}");
    Console.WriteLine($"Data Length: {sample.Buffer.Length}");
  }

  public void PlaySample(string name, string channelName)
  {
    var sample = _samples[name];
    var channel = Mixer.GetChannel(channelName);
    channel.Queue(sample);
  }

  public void Run()
  {
    /*_audioOutput.Tick();
    _audioOutput.OnSex = () =>
    {
      Mixer.Tick();
      _audioOutput.Fill(Mixer.Buffer);
      _audioOutput.Tick();
    };*/

    /*Task.Run(() =>
    {
      for (var i = 0; i < 32; i++)
      {
        Mixer.Tick();
        _audioOutput.Fill(Mixer.Buffer);
        _audioOutput.Tick();
      }
    }).Wait();*/


    Task.Run(() =>
    {
      while (true)
      {
        var tt = Stopwatch.StartNew();
        Mixer.Tick();
        _audioOutput.Fill(Mixer.Buffer);

        var ms = (int)(_audioOutput.BufferSize / (float)_audioOutput.SampleRate * 500f);
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < ms) Thread.SpinWait(1);
        Sex.Add((int)tt.ElapsedMilliseconds);
      }
    });

    Thread.Sleep(48);

    Task.Run(() =>
    {
      while (true) _audioOutput.Tick();
    });


    /*for (var i = 0; i < 8; i++)
    {
      Mixer.Tick();
      _audioOutput.Fill(Mixer.Buffer);
    }*/

    /*_audioOutput.OnDoneBuffer = () =>
    {



      Mixer.Tick();
      _audioOutput.Fuck2(Mixer.Buffer);
    };*/

    /*// Запускаем в фоне вывод звука
    /*Task.Run(() =>
    {
      while (true) _audioOutput.Tick();
    });#1#

    while (true)
    {
      Mixer.Tick();
      _audioOutput.Fuck(Mixer.Buffer);

      /*Array.Copy(Mixer.Buffer, _audioOutput.Buffer, Mixer.Buffer.Length);

      // Записываем
      for (var i = 0; i < _audioOutput.BufferOut.Length; i++)
        _audioOutput.BufferOut[i] = (short)(_audioOutput.Buffer[i] * 32768);

      _audioOutput.Tick();
      Console.WriteLine("GAS");#1#
    }*/
  }
  /*public void Tick()
  {

  }*/
}