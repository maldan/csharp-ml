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

  private short[] _buffer0;
  private short[] _buffer1;
  private GCHandle _hBuffer0;
  private GCHandle _hBuffer1;
  private GCHandle _hWaveHdr0;
  private GCHandle _hWaveHdr1;
  private WAVEHDR _waveHdr0;
  private WAVEHDR _waveHdr1;

  private IntPtr _hWaveOut;


  private int _currentBufferId;
  private int _bufferOffset;
  private int _bufferMax = 32;

  private WAVEFORMATEX _format;

  private Mutex _mu = new();
  private bool _canFuck;

  public int BufferPlayTimeMS
  {
    get
    {
      if (IsStereo) return (int)(BufferSize / (float)SampleRate * 500f);
      return (int)(BufferSize / (float)SampleRate * 1000f);
    }
  }

  public bool IsFirstBuffer => _bufferOffset == 0;
  public bool IsLastBuffer => _bufferOffset == _bufferMax - 1;

  public AudioOutput(int bufferSize, int sampleRate, bool isStereo)
  {
    SampleRate = sampleRate;
    IsStereo = isStereo;
    _buffer0 = new short[bufferSize * _bufferMax];
    _buffer1 = new short[bufferSize * _bufferMax];
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

    var thisClass = GCHandle.Alloc(this);

    var result = WinMM.WaveOutOpen(
      out _hWaveOut,
      0xFFFFFFFF,
      ref _format,
      WaveOutProc,
      GCHandle.ToIntPtr(thisClass), WinMM.CALLBACK_FUNCTION);

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
    _hBuffer0 = GCHandle.Alloc(_buffer0, GCHandleType.Pinned);
    _hBuffer1 = GCHandle.Alloc(_buffer1, GCHandleType.Pinned);

    // Подготавливаем проигрывание звука
    _waveHdr0 = new WAVEHDR
    {
      lpData = _hBuffer0.AddrOfPinnedObject(),
      dwBufferLength = (uint)(_buffer0.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };
    _hWaveHdr0 = GCHandle.Alloc(_waveHdr0, GCHandleType.Pinned);

    _waveHdr1 = new WAVEHDR
    {
      lpData = _hBuffer1.AddrOfPinnedObject(),
      dwBufferLength = (uint)(_buffer1.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };
    _hWaveHdr1 = GCHandle.Alloc(_waveHdr1, GCHandleType.Pinned);

    Console.WriteLine($"Delay must be {BufferPlayTimeMS}");
    // WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr, (uint)Marshal.SizeOf(_waveHdr));
  }

  public void Fuck()
  {
    _mu.WaitOne();
    _canFuck = true;
    _mu.ReleaseMutex();
  }

  public void UnFuck()
  {
    _mu.WaitOne();
    _canFuck = false;
    _mu.ReleaseMutex();
  }

  public bool CanFuck()
  {
    var b = false;
    _mu.WaitOne();
    b = _canFuck;
    _mu.ReleaseMutex();
    return b;
  }

  public void Tick()
  {
    _mu.WaitOne();
    Console.WriteLine($"Audio Output Tick {_currentBufferId}");
    if (_currentBufferId == 0)
    {
      _waveHdr0.dwFlags = 0;
      _waveHdr0.dwLoops = 0;
      var result = WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr0, (uint)Marshal.SizeOf(_waveHdr0));
      if (result != 0)
      {
        Console.WriteLine($"WaveOutPrepareHeader0 failed with error {result}");
        return;
      }

      result = WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr0, (uint)Marshal.SizeOf(_waveHdr0));
      if (result != 0)
      {
        Console.WriteLine($"WaveOutWrite0 failed with error {result}");
        return;
      }
    }
    else
    {
      _waveHdr1.dwFlags = 0;
      _waveHdr1.dwLoops = 0;
      var result = WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));
      if (result != 0)
      {
        Console.WriteLine($"WaveOutPrepareHeader1 failed with error {result}");
        return;
      }

      result = WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));
      if (result != 0)
      {
        Console.WriteLine($"WaveOutWrite1 failed with error {result}");
        return;
      }
    }

    _mu.ReleaseMutex();
  }

  public void OldTick()
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

    /*_waveHdr.dwFlags = 0;
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
    Console.WriteLine(_waveHdr.dwLoops);*/

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
    if (_currentBufferId == 0)
      for (var i = 0; i < BufferSize; i++)
        _buffer0[i + _bufferOffset * BufferSize] = (short)(buffer[i] * 32768);
    else
      for (var i = 0; i < BufferSize; i++)
        _buffer1[i + _bufferOffset * BufferSize] = (short)(buffer[i] * 32768);
    _bufferOffset += 1;
    if (_bufferOffset >= _bufferMax) _bufferOffset = 0;
    _mu.ReleaseMutex();
  }

  public void SwitchBuffer()
  {
    _mu.WaitOne();
    _currentBufferId = _currentBufferId == 0 ? 1 : 0;
    _mu.ReleaseMutex();
  }

  public void Close()
  {
    WinMM.WaveOutClose(_hWaveOut);
  }

  private static void WaveOutProc(IntPtr hWaveOut, uint uMsg, IntPtr dwInstance, ref WAVEHDR lpWaveHdr, IntPtr dwParam2)
  {
    if (uMsg == WinMM.WOM_DONE)
    {
      var handle = GCHandle.FromIntPtr(dwInstance);
      var output = (AudioOutput)handle.Target;
      output?.ClearBuffer(ref lpWaveHdr);
    }
  }

  public void ClearBuffer(ref WAVEHDR waveHdr)
  {
    if (waveHdr.lpData == _waveHdr0.lpData)
      Console.WriteLine($"CLEAR BUFFER 0");
    else if (waveHdr.lpData == _waveHdr1.lpData)
      Console.WriteLine($"CLEAR BUFFER 1");
    else
      Console.WriteLine($"CLEAR NIGGA WHAT???");

    Console.WriteLine($"dwFlags {waveHdr.dwFlags} {waveHdr.dwLoops} {waveHdr.dwUser}");
    Console.WriteLine($"dwBufferLength {waveHdr.dwBufferLength} {waveHdr.dwBytesRecorded}");

    var result = WinMM.WaveOutUnprepareHeader(_hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));
    if (result != 0) Console.WriteLine($"WaveOutUnprepareHeader failed with error {result}");
  }

  /*~AudioOutput()
  {
    WinMM.WaveOutClose(_hWaveOut);
  }*/
}