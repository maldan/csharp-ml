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

public static class WinMM
{
  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutOpen")]
  public static extern int WaveOutOpen(out IntPtr hWaveOut, uint uDeviceID, ref WAVEFORMATEX lpFormat,
    IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutPrepareHeader")]
  public static extern int WaveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, uint uSize);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutWrite")]
  public static extern int WaveOutWrite(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, uint uSize);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutUnprepareHeader")]
  public static extern int WaveOutUnprepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, uint uSize);

  [DllImport("winmm.dll", SetLastError = true, EntryPoint = "waveOutClose")]
  public static extern int WaveOutClose(IntPtr hWaveOut);
}