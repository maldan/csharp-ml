using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MegaLib.Asm;
using MegaLib.Audio;
using MegaLib.Ext;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Text;
using NUnit.Framework;
using Random = MegaLib.Mathematics.Random;

namespace MegaTest;

public class Tests
{
  [SetUp]
  public void Setup()
  {
  }

  /*[Test]
  public void Test1()
  {
    var v1 = new Vector3(1.0f, 0.0f, 0.0f);
    var v2 = new Vector3(1.0f, 0.0f, 0.0f);
    var v3 = v1 + v2;
    Assert.AreEqual(v3.X, 2.0f);
  }*/

  [Test]
  public void Test2()
  {
    var v1 = Quaternion.FromEuler(45.0f, 0.0f, 0.0f, "deg");
    var v2 = Quaternion.FromEuler(45.0f, 0.0f, 0.0f, "deg");
    var v3 = v1 * v2;
    Assert.Equals((int)v3.Euler.ToDegrees.X, 90);
  }

  [Test]
  public void Test3()
  {
    // Первая матрица
    var a = new Matrix4x4
    {
      M00 = 1, M01 = 2, M02 = 3, M03 = 4,
      M10 = 5, M11 = 6, M12 = 7, M13 = 8,
      M20 = 9, M21 = 10, M22 = 11, M23 = 12,
      M30 = 13, M31 = 14, M32 = 15, M33 = 16
    };

    // Вторая матрица
    var b = new Matrix4x4
    {
      M00 = 17, M01 = 18, M02 = 19, M03 = 20,
      M10 = 21, M11 = 22, M12 = 23, M13 = 24,
      M20 = 25, M21 = 26, M22 = 27, M23 = 28,
      M30 = 29, M31 = 30, M32 = 31, M33 = 32
    };

    // Выполняем умножение вручную
    var expectedResult = new Matrix4x4
    {
      M00 = 250, M01 = 260, M02 = 270, M03 = 280,
      M10 = 618, M11 = 644, M12 = 670, M13 = 696,
      M20 = 986, M21 = 1028, M22 = 1070, M23 = 1112,
      M30 = 1354, M31 = 1412, M32 = 1470, M33 = 1528
    };

    // Выполняем умножение матриц с использованием оператора *
    var result = a * b;

    // Сравниваем результаты
    Assert.Equals(result.Equals(expectedResult), true);
  }

  [Test]
  public void Test4()
  {
    // Первая матрица
    var a = new System.Numerics.Matrix4x4
    {
      M11 = 1, M12 = 2, M13 = 3, M14 = 4,
      M21 = 5, M22 = 6, M23 = 7, M24 = 8,
      M31 = 9, M32 = 10, M33 = 11, M34 = 12,
      M41 = 13, M42 = 14, M43 = 15, M44 = 16
    };

    // Вторая матрица
    var b = new System.Numerics.Matrix4x4
    {
      M11 = 17, M12 = 18, M13 = 19, M14 = 20,
      M21 = 21, M22 = 22, M23 = 23, M24 = 24,
      M31 = 25, M32 = 26, M33 = 27, M34 = 28,
      M41 = 29, M42 = 30, M43 = 31, M44 = 32
    };

    // Выполняем умножение вручную
    var expectedResult = new System.Numerics.Matrix4x4
    {
      M11 = 250, M12 = 260, M13 = 270, M14 = 280,
      M21 = 618, M22 = 644, M23 = 670, M24 = 696,
      M31 = 986, M32 = 1028, M33 = 1070, M34 = 1112,
      M41 = 1354, M42 = 1412, M43 = 1470, M44 = 1528
    };

    // Выполняем умножение матриц с использованием оператора *
    var result = a * b;

    // Сравниваем результаты
    Assert.Equals(result.Equals(expectedResult), true);
  }

  // Прототип функции, которую будем вызывать
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate int MyFunctionDelegate(int x, int y);

  [Test]
  public void TestAsm()
  {
    var asm = new AsmRuntime();
    var code = new byte[] { 0x8B, 0xC1, 0x03, 0xD0, 0xC3 };
    var fn = asm.AllocateFunction<MyFunctionDelegate>("add", code);
    Console.WriteLine(fn(5, 10));
  }

  [Test]
  public void TestAsmMemCopy()
  {
    var memcpy = AsmRuntime.MemCopy();

    var code1 = new byte[] { 0, 0, 0, 0, 0 };
    var code2 = new byte[] { 1, 2, 3, 4, 5 };

    Console.WriteLine(string.Join(", ", code1));
    Console.WriteLine(string.Join(", ", code2));

    memcpy(
      Marshal.UnsafeAddrOfPinnedArrayElement(code1, 0),
      Marshal.UnsafeAddrOfPinnedArrayElement(code2, 0),
      3
    );

    Console.WriteLine(string.Join(", ", code1));
    Console.WriteLine(string.Join(", ", code2));
  }

  [Test]
  public void TestRectangle()
  {
    var rect1 = new Rectangle(0, 0, -5, -5);
    var rect2 = new Rectangle(-1, -1, -10, -10);

    Console.WriteLine(rect1.IsCollide(rect2)); // True
  }

  [Test]
  public void TestRectangleUV()
  {
    Console.WriteLine(new Rectangle(0, 0, 256, 256).ToUV(256, 256));
    Console.WriteLine(new Rectangle(0, 0, 128, 128).ToUV(256, 256));
    Console.WriteLine(new Rectangle(128, 128, 256, 256).ToUV(256, 256));
  }

  [Test]
  public void TestBitmapFont()
  {
    var bmp = new BitmapFont();
    bmp.Load("C:/Users/black/Desktop/font.bin");
  }

  [Test]
  public void TestLastList()
  {
    var list = new List<int> { 1, 2, 3, 4, 5 };
    var x = list.Pop(3);
    for (var i = 0; i < x.Count; i++) Console.WriteLine(x[i]);
    Console.WriteLine("Last");
    for (var i = 0; i < list.Count; i++) Console.WriteLine(list[i]);
  }

  [Test]
  public void TestWav()
  {
    var filePath = "C:/Users/black/Desktop/Battle Cry.wav";
    var audioFile = new AudioSample();
    audioFile.FromFile(filePath);

    // var wavData = File.ReadAllBytes(filePath);
    // short[] buffer = Wav.ReadWavFile(wavData, out int sampleRate, out short channels, out short bitsPerSample);

    Console.WriteLine($"Sample Rate: {audioFile.SampleRate}");
    Console.WriteLine($"Channels: {audioFile.NumberOfChannels}");
    Console.WriteLine($"Bits Per Sample: {audioFile.BitsPerSample}");
    Console.WriteLine($"Data Length: {audioFile.Buffer.Length}");

    /*// Пример обработки каналов: конвертация моно в стерео
    if (channels == 1)
    {
      buffer = MonoToStereo(buffer);
      channels = 2;
    }*/
  }

  [Test]
  public void TestPlaySound()
  {
    Console.WriteLine("XXX");
    var filePath = "C:/Users/black/Desktop/Battle Cry.wav";
    var am = new AudioManager(48000 / 32, 48000, true);
    am.LoadSample(filePath, "music");
    am.Mixer.CreateChannel("bgm");
    am.PlaySample("music", "bgm");
    Console.WriteLine("X");
    am.Run();
    Thread.Sleep(4000);

    // for (var i = 0; i < am.Sex.Count; i++) Console.WriteLine(am.Sex[i]);
  }

  [Test]
  public void TestPlayShortSound()
  {
    Console.WriteLine("XXX");
    var filePath = "D:/csharp/Mazel Game/Mazel Game/asset/audio/punch_1.wav";
    var am = new AudioManager(48000 / 32, 48000, true);
    am.LoadSample(filePath, "music");
    am.Mixer.CreateChannel("bgm");
    am.Run();

    for (var i = 0; i < 120; i++)
    {
      am.PlaySample("music", "bgm");
      Console.WriteLine("X");
      Thread.Sleep(10);
      GC.Collect();
    }


    // for (var i = 0; i < am.Sex.Count; i++) Console.WriteLine(am.Sex[i]);
  }

  [Test]
  public void TestRandom()
  {
    var rnd = new Random();
    for (var i = 0; i < 100; i++) Console.WriteLine(rnd.RangeInt(0, 1));
  }

  [Test]
  public void TestRandomFloat()
  {
    var rnd = new Random();
    for (var i = 0; i < 100; i++) Console.WriteLine(rnd.RangeFloat(0, 1f));
  }
}