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

  public float[] Buffer;
  public short[] BufferOut1;
  public short[] BufferOut2;

  private IntPtr _hWaveOut;

  private GCHandle _hBuffer1;
  private GCHandle _hBuffer2;
  private WAVEHDR _waveHdr1;
  private WAVEHDR _waveHdr2;

  private int _bufferId;
  private int _bufferMax = 32;

  public Action OnSex;

  private WAVEFORMATEX _format;

  public Stopwatch X = new();
  public List<int> Sex = new();

  private Mutex _mu = new();

  /*public AudioOutput(int bufferSize)
  {
    SampleRate = 44100;
    IsStereo = true;
    Buffer = new float[bufferSize];
    BufferOut1 = new short[bufferSize];
    BufferOut2 = new short[bufferSize];
  }*/

  public AudioOutput(int bufferSize, int sampleRate, bool isStereo)
  {
    SampleRate = sampleRate;
    IsStereo = isStereo;
    Buffer = new float[bufferSize * _bufferMax];
    BufferOut1 = new short[bufferSize * _bufferMax];
    BufferOut2 = new short[bufferSize * _bufferMax];
    BufferSize = bufferSize;
  }

  public void Open()
  {
    // Закрепляем текущий экземпляр, чтобы получить IntPtr для передачи в колбэк
    var thisHandle = GCHandle.Alloc(this);

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

    /*var result = WinMM.WaveOutOpen(
      out _hWaveOut,
      0xFFFFFFFF,
      ref _format,
      WaveOutCallback,
      GCHandle.ToIntPtr(thisHandle), WinMM.CALLBACK_FUNCTION | WinMM.WAVE_ALLOWSYNC);
      */

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
    _hBuffer1 = GCHandle.Alloc(BufferOut1, GCHandleType.Pinned);
    _hBuffer2 = GCHandle.Alloc(BufferOut2, GCHandleType.Pinned);

    // Подготавливаем проигрывание звука
    _waveHdr1 = new WAVEHDR
    {
      lpData = _hBuffer1.AddrOfPinnedObject(),
      dwBufferLength = (uint)(BufferOut1.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };

    _waveHdr2 = new WAVEHDR
    {
      lpData = _hBuffer2.AddrOfPinnedObject(),
      dwBufferLength = (uint)(BufferOut2.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };

    // Switch();

    // Установка таймера
    /*var timer = new Timer((int)(BufferOut1.Length / (float)SampleRate * 500f));
    timer.Elapsed += OnTimedEvent;
    timer.AutoReset = true;
    timer.Enabled = true;*/

    Console.WriteLine($"Delay must be {(int)(BufferSize / (float)SampleRate * 500f)}");
    //WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));
    //WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));
  }

  private void OnTimedEvent(object source, ElapsedEventArgs e)
  {
    OnSex.Invoke();
    Task.Run(() =>
    {
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

      WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));
      WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

      WinMM.WaveOutUnprepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

      WinMM.WaveOutClose(_hWaveOut);
    });
  }

  public void Tick()
  {
    /*if (_bufferId == 0)
    {
      WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

      _bufferId = 1;
    }
    else
    {
      WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr2, (uint)Marshal.SizeOf(_waveHdr2));
      WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr2, (uint)Marshal.SizeOf(_waveHdr2));
      _bufferId = 0;
    }*/

    // 

    _hBuffer1 = GCHandle.Alloc(BufferOut1, GCHandleType.Pinned);

    _waveHdr1 = new WAVEHDR
    {
      lpData = _hBuffer1.AddrOfPinnedObject(),
      dwBufferLength = (uint)(BufferOut1.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };

    WinMM.WaveOutPrepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

    WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

    var x = new Stopwatch();
    x.Start();
    var ms = (int)(BufferOut1.Length / (float)SampleRate * 500f) - 16;
    Thread.Sleep(ms);
    x.Stop();
    Console.WriteLine($"{x.ElapsedMilliseconds} - {ms} = {x.ElapsedMilliseconds - ms}");

    WinMM.WaveOutUnprepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

    _hBuffer1.Free();
  }

  public void OldTick()
  {
    //var x = new Stopwatch();
    //x.Start();

    // x.Stop();
    // Console.WriteLine($"Gazovik {x.ElapsedMilliseconds}");

    //var ms = (int)(BufferOut1.Length / (float)(SampleRate * 2 * 16 / 8f) * 1000f);
    /*var ms = 1000; //(int)((float)BufferOut1.Length / (float)SampleRate * 1000f);
    Console.WriteLine(ms);
    var stopwatch = Stopwatch.StartNew();
    while (stopwatch.ElapsedMilliseconds < ms) Thread.SpinWait(1);*/

    // Освобождаем ресурсы
    // WinMM.WaveOutUnprepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

    // Thread.Sleep();

    // Засыпаем пока не проиграется звук
    // Console.WriteLine((int)(BufferOut.Length / (float)SampleRate * 500f));

    // Thread.Sleep((int)(BufferOut1.Length / (float)SampleRate * 500f) - 2);
    //var stopwatch = Stopwatch.StartNew();
    //while (stopwatch.ElapsedMilliseconds < 24) Thread.SpinWait(1);

    // Дождаться завершения воспроизведения
    //while ((waveHdr.dwFlags & 0x00000001) == 0) // WHDR_DONE
    //  Thread.Sleep(10);

    // Освобождаем ресурсы
    // WinMM.WaveOutUnprepareHeader(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

    // Очищаем ресурсы
    // hBuffer.Free();

    X = new Stopwatch();
    X.Start();

    WinMM.WaveOutWrite(_hWaveOut, ref _waveHdr1, (uint)Marshal.SizeOf(_waveHdr1));

    /*var x = new Stopwatch();
    x.Start();
    var ms = (int)(BufferOut1.Length / (float)SampleRate * 500f);
    Thread.Sleep(ms);
    x.Stop();*/

    /*var fuck = -(x.ElapsedMilliseconds - ms);
    if (fuck <= 0) fuck = 0;
    var stopwatch = Stopwatch.StartNew();
    while (stopwatch.ElapsedMilliseconds < fuck) Thread.SpinWait(1);

    Console.WriteLine($"Y: {x.ElapsedMilliseconds} {x.ElapsedMilliseconds - ms} {ms}");*/
  }

  /*private static void WaveOutCallback(IntPtr hWaveOut, uint uMsg, IntPtr dwInstance, ref WAVEHDR lpWaveHdr,
    IntPtr dwParam2)
  {
    if (uMsg == WinMM.WOM_DONE)
    {
      // Преобразуем указатель на экземпляр класса обратно в объект AudioPlayer и сигнализируем об окончании воспроизведения
      var player = (AudioOutput)GCHandle.FromIntPtr(dwInstance).Target;

      player.X.Stop();

      var ms = (int)(player.BufferOut1.Length / (float)player.SampleRate * 500f);
      player.Sex.Add((int)player.X.ElapsedMilliseconds - ms);

      player?.OnSex?.Invoke();

      // player._playbackCompleted.Set();
    }
  }*/

  public void Fill(float[] buffer)
  {
    for (var i = 0; i < BufferSize; i++)
      BufferOut1[i + _bufferId * BufferSize] = (short)(buffer[i] * 32768);
    _bufferId += 1;
    if (_bufferId >= _bufferMax) _bufferId = 0;
  }

  public void Close()
  {
    WinMM.WaveOutClose(_hWaveOut);
  }

  ~AudioOutput()
  {
    WinMM.WaveOutClose(_hWaveOut);
  }
}