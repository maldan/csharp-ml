using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MegaLib.Ext;
using MegaLib.OS.Api;
using Timer = System.Timers.Timer;

namespace MegaLib.Audio;

public class AudioOutput
{
  public int SampleRate { get; private set; }
  public bool IsStereo { get; private set; }
  public int BufferSize { get; private set; }

  private short[] _buffer;

  private IntPtr _hWaveOut;

  private GCHandle _hBuffer;
  private GCHandle _hWaveHdr;

  private WAVEHDR _waveHdr;

  private int _bufferId;
  private int _bufferMax = 32;

  private WAVEFORMATEX _format;

  private Mutex _mu = new();

  public AudioOutput(int bufferSize, int sampleRate, bool isStereo)
  {
    SampleRate = sampleRate;
    IsStereo = isStereo;
    _buffer = new short[bufferSize * _bufferMax];
    BufferSize = bufferSize;
  }

  public void Open()
  {
    var numberOfChannels = IsStereo ? 2 : 1;
    _format = new WAVEFORMATEX
    {
      wFormatTag = WinMM.WAVE_FORMAT_PCM,
      nChannels = (ushort)numberOfChannels,
      nSamplesPerSec = (uint)SampleRate,
      wBitsPerSample = 16,
      nBlockAlign = (ushort)(numberOfChannels * 16 / 8),
      nAvgBytesPerSec = (uint)(SampleRate * numberOfChannels * 16 / 8),
      cbSize = 0
    };

    var result = WinMM.WaveOutOpen(
      out _hWaveOut,
      0xFFFFFFFF,
      ref _format,
      null,
      IntPtr.Zero, WinMM.CALLBACK_NULL);

    if (result != 0)
      throw result switch
      {
        4 => // MMSYSERR_ALLOCATED
          new Exception("The device is already in use."),
        2 => // MMSYSERR_BADDEVICEID
          new Exception("The specified device ID is invalid."),
        6 => // MMSYSERR_NODRIVER
          new Exception("The driver is not installed."),
        7 => // MMSYSERR_NOMEM
          new Exception("There is not enough memory available for this operation."),
        32 => // WAVERR_BADFORMAT
          new Exception("The specified format is not supported."),
        _ => new Exception("Unknown error: " + result)
      };

    // Аллоцируем в управляемую память и закрепляем чтобы GC не переместил данные
    _hBuffer = GCHandle.Alloc(_buffer, GCHandleType.Pinned);


    // Подготавливаем проигрывание звука
    _waveHdr = new WAVEHDR
    {
      lpData = _hBuffer.AddrOfPinnedObject(),
      dwBufferLength = (uint)(_buffer.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };
    _hWaveHdr = GCHandle.Alloc(_waveHdr, GCHandleType.Pinned);

    Console.WriteLine($"Delay must be {(int)(BufferSize / (float)SampleRate * 500f)}");
    // WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr, (uint)Marshal.SizeOf(_waveHdr));
  }

  public void Tick()
  {
    // Обязательно надо создавать новый хедер и подготавливать его. Иначе обрывки начинаются
    /*_waveHdr = new WAVEHDR
    {
      lpData = _hBuffer.AddrOfPinnedObject(),
      dwBufferLength = (uint)(_buffer.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };*/
    //_waveHdr.lpData = _hBuffer.AddrOfPinnedObject();
    //_waveHdr.dwBufferLength = (uint)(_buffer.Length * sizeof(short));

    _waveHdr.dwFlags = 0;
    _waveHdr.dwLoops = 0;

    var result = WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr, (uint)Marshal.SizeOf(_waveHdr));
    if (result != 0)
    {
      Console.WriteLine($"WaveOutPrepareHeader failed with error {result}");
      return;
    }

    result = WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr, (uint)Marshal.SizeOf(_waveHdr));
    if (result != 0)
    {
      Console.WriteLine($"WaveOutWrite failed with error {result}");
      return;
    }

    var x = new Stopwatch();
    x.Start();
    var ms = (int)(_buffer.Length / (float)SampleRate * 500f) - 16;
    Thread.Sleep(ms);
    x.Stop();
    Console.WriteLine($"{x.ElapsedMilliseconds} - {ms} = {x.ElapsedMilliseconds - ms}");

    // Ждем завершения воспроизведения
    Console.WriteLine(_waveHdr.dwFlags);
    Console.WriteLine(_waveHdr.dwLoops);

    /*result = WinMM.WaveOutUnprepareHeader(_hWaveOut, ref _waveHdr, (uint)Marshal.SizeOf(_waveHdr));
    if (result != 0)
    {
      Console.WriteLine($"WaveOutUnprepareHeader failed with error {result}");
      return;
    }*/

    //_hBuffer.Free();
  }

  public void Fill(float[] buffer)
  {
    _mu.WaitOne();
    for (var i = 0; i < BufferSize; i++)
      _buffer[i + _bufferId * BufferSize] = (short)(buffer[i] * 32768);
    _bufferId += 1;
    if (_bufferId >= _bufferMax) _bufferId = 0;
    _mu.ReleaseMutex();
  }

  public void Close()
  {
    WinMM.WaveOutClose(_hWaveOut);
  }

  /*~AudioOutput()
  {
    WinMM.WaveOutClose(_hWaveOut);
  }*/
}