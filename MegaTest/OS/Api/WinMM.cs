using System;
using System.Runtime.InteropServices;
using MegaLib.Asm;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Text;
using NUnit.Framework;

namespace MegaTest.OS.Api;

public class WinMMTests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public void TestSound()
  {
    var sampleRate = 44100;
    var durationInSeconds = 1;
    var frequency = 440;
    var amplitude = 32760;
    var channels = 1;
    var samples = sampleRate * durationInSeconds;

    var buffer = new short[samples];
    for (var i = 0; i < samples; i++)
      buffer[i] = (short)(amplitude * Math.Sin(2 * Math.PI * frequency * i / sampleRate));

    var format = new WAVEFORMATEX
    {
      wFormatTag = 1,
      nChannels = (ushort)channels,
      nSamplesPerSec = (uint)sampleRate,
      nAvgBytesPerSec = (uint)(sampleRate * channels * sizeof(short)),
      nBlockAlign = (ushort)(channels * sizeof(short)),
      wBitsPerSample = 16,
      cbSize = 0
    };

    IntPtr hWaveOut;
    WinMM.WaveOutOpen(out hWaveOut, 0xFFFFFFFF, ref format, IntPtr.Zero, IntPtr.Zero, 0x00030000);

    var hBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

    var waveHdr = new WAVEHDR
    {
      lpData = hBuffer.AddrOfPinnedObject(),
      dwBufferLength = (uint)(buffer.Length * sizeof(short)),
      dwFlags = 0,
      dwLoops = 0
    };

    WinMM.WaveOutPrepareHeader(hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));
    WinMM.WaveOutWrite(hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));

    System.Threading.Thread.Sleep(durationInSeconds * 1000);

    WinMM.WaveOutUnprepareHeader(hWaveOut, ref waveHdr, (uint)Marshal.SizeOf(waveHdr));
    WinMM.WaveOutClose(hWaveOut);

    hBuffer.Free();
  }
}