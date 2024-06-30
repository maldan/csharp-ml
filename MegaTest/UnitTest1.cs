using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaLib.Asm;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Texture;
using NUnit.Framework;

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
    Assert.AreEqual((int)v3.Euler.ToDegrees.X, 90);
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
    Assert.AreEqual(result.Equals(expectedResult), true);
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
    Assert.AreEqual(result.Equals(expectedResult), true);
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
}