using System;
using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics.LinearAlgebra;

public class Matrix3x3Tests
{
  private Matrix3x3 matrix;

  [SetUp]
  public void Setup()
  {
    matrix = new Matrix3x3(
      1, 2, 3,
      4, 5, 6,
      7, 8, 9
    );
  }

  [Test]
  public void TestGetIndex()
  {
    ClassicAssert.AreEqual(1, matrix[0]);
    ClassicAssert.AreEqual(2, matrix[1]);
    ClassicAssert.AreEqual(3, matrix[2]);
    ClassicAssert.AreEqual(4, matrix[3]);
    ClassicAssert.AreEqual(5, matrix[4]);
    ClassicAssert.AreEqual(6, matrix[5]);
    ClassicAssert.AreEqual(7, matrix[6]);
    ClassicAssert.AreEqual(8, matrix[7]);
    ClassicAssert.AreEqual(9, matrix[8]);
  }

  [Test]
  public void TestSetIndex()
  {
    matrix[0] = 10;
    matrix[4] = 11;
    matrix[8] = 12;

    ClassicAssert.AreEqual(10, matrix[0]);
    ClassicAssert.AreEqual(11, matrix[4]);
    ClassicAssert.AreEqual(12, matrix[8]);
  }

  [Test]
  public void TestGetInvalidIndex()
  {
    ClassicAssert.AreEqual(0, matrix[9]); // Проверка выхода за пределы
    ClassicAssert.AreEqual(0, matrix[-1]); // Проверка отрицательного индекса
  }

  [Test]
  public void TestSetInvalidIndex()
  {
    matrix[9] = 100; // Наблюдается, но не проверяется
    matrix[-1] = 200; // Наблюдается, но не проверяется
    // Здесь нет явной проверки, но можно проверить, что значения не изменились
    ClassicAssert.AreEqual(1, matrix[0]);
  }

  [Test]
  public void TestCount()
  {
    for (var i = 0; i < matrix.Count; i++)
    {
      matrix[i] = i;
      ClassicAssert.AreEqual(i, matrix[i]);
    }

    for (var i = 0; i < matrix.Count; i++)
    {
      matrix[i] = 228;
    }

    for (var i = 0; i < 9; i++)
    {
      ClassicAssert.AreEqual(228, matrix[i]);
    }

    Console.WriteLine(matrix);
  }
}