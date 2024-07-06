using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api;

[StructLayout(LayoutKind.Sequential)]
public struct WAVEFORMATEX
{
  public ushort wFormatTag;
  public ushort nChannels;
  public uint nSamplesPerSec;
  public uint nAvgBytesPerSec;
  public ushort nBlockAlign;
  public ushort wBitsPerSample;
  public ushort cbSize;
}

[StructLayout(LayoutKind.Sequential)]
public struct WAVEHDR
{
  public IntPtr lpData;
  public uint dwBufferLength;
  public uint dwBytesRecorded;
  public uint dwUser;
  public uint dwFlags;
  public uint dwLoops;
  public IntPtr lpNext;
  public IntPtr reserved;
}

[StructLayout(LayoutKind.Sequential)]
public struct WAVEFORMATEXTENSIBLE
{
  public WAVEFORMATEX Format;
  public ushort wValidBitsPerSample;
  public uint dwChannelMask;
  public Guid SubFormat;
}

public static class WinMM
{
  public static readonly Guid KSDATAFORMAT_SUBTYPE_IEEE_FLOAT = new("00000003-0000-0010-8000-00AA00389B71");
  public static readonly Guid KSDATAFORMAT_SUBTYPE_PCM = new("00000001-0000-0010-8000-00AA00389B71");
  public const int WAVE_FORMAT_PCM = 1;
  public const int CALLBACK_FUNCTION = 0x00030000;
  public const int CALLBACK_NULL = 0x00000000;
  public const int WOM_DONE = 0x3BD; // сообщение, посылаемое по завершении проигрывания буфера
  public const int WAVE_ALLOWSYNC = 0x0002;
  public const int WHDR_BEGINLOOP = 0x00000004; // Начало цикла
  public const int WHDR_ENDLOOP = 0x00000008; // Конец цикла

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutOpen")]
  public static extern int WaveOutOpen(out IntPtr hWaveOut, uint uDeviceID, ref WAVEFORMATEX lpFormat,
    WaveOutProc dwCallback, IntPtr dwInstance, uint fdwOpen);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutPrepareHeader")]
  public static extern int WaveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, uint uSize);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutWrite")]
  public static extern int WaveOutWrite(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, uint uSize);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutUnprepareHeader")]
  public static extern int WaveOutUnprepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, uint uSize);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutClose")]
  public static extern int WaveOutClose(IntPtr hWaveOut);

  public delegate void WaveOutProc(IntPtr hWaveOut, uint uMsg, IntPtr dwInstance, ref WAVEHDR lpWaveHdr,
    IntPtr dwParam2);
}