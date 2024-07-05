using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MegaLib.Ext;
using MegaLib.OS.Api;

namespace MegaLib.Audio;

public class AudioOutput
{
  public int SampleRate = 44100;
  public int NumberOfChannels = 2;
  private IntPtr _hWaveOut;

  public List<short> Buffer = [];


  public AudioOutput()
  {
  }

  public void Open()
  {
    var format = new WAVEFORMATEX
    {
      wFormatTag = 1,
      nChannels = (ushort)NumberOfChannels,
      nSamplesPerSec = (uint)SampleRate,
      nAvgBytesPerSec = (uint)(SampleRate * NumberOfChannels * sizeof(short)),
      nBlockAlign = (ushort)(NumberOfChannels * sizeof(short)),
      wBitsPerSample = 16,
      cbSize = 0
    };

    WinMM.WaveOutOpen(out _hWaveOut, 0xFFFFFFFF, ref format, IntPtr.Zero, IntPtr.Zero, 0x00030000);
  }

  public void Run()
  {
    while (true)
    {
      if (Buffer.Count == 0) continue;

      // Берем из буффера послендие 0.25 секунд
      var buffer = Buffer.Pop(44100 / 4);

      // Аллоцируем в управляемую память и закрепляем чтобы GC не переместил данные
      var hBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

      // Подготавливаем проигрывание звука
      var waveHdr = new WAVEHDR
      {
        lpData = hBuffer.AddrOfPinnedObject(),
        dwBufferLength = (uint)(buffer.Count * sizeof(short)),
        dwFlags = 0,
        dwLoops = 0
      };
      WinMM.WaveOutPrepareHeader(_hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));

      // Начинаем вывод
      WinMM.WaveOutWrite(_hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));

      // Засыпаем пока не прогирается звук
      Thread.Sleep(SampleRate / buffer.Count * 1000);

      // Освобождаем ресурсы
      WinMM.WaveOutUnprepareHeader(_hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));

      // Очищаем ресурсы
      hBuffer.Free();
    }
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