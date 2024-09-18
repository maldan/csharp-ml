using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics.LinearAlgebra;

public class IVector3Tests
{
    // Тестирование конструктора
    [Test]
    public void Constructor_ShouldInitializeFields()
    {
        var vector = new IVector3(1, 2, 3);
        ClassicAssert.AreEqual(1, vector.X);
        ClassicAssert.AreEqual(2, vector.Y);
        ClassicAssert.AreEqual(3, vector.Z);
    }

    // Тестирование сложения
    [Test]
    public void Add_Operation_ShouldReturnCorrectResult()
    {
        var a = new IVector3(1, 2, 3);
        var b = new IVector3(4, 5, 6);
        var result = a + b;
        ClassicAssert.AreEqual(new IVector3(5, 7, 9), result);
    }

    // Тестирование вычитания
    [Test]
    public void Subtract_Operation_ShouldReturnCorrectResult()
    {
        var a = new IVector3(5, 6, 7);
        var b = new IVector3(2, 2, 2);
        var result = a - b;
        ClassicAssert.AreEqual(new IVector3(3, 4, 5), result);
    }

    // Тестирование умножения векторов
    [Test]
    public void Multiply_Vectors_ShouldReturnCorrectResult()
    {
        var a = new IVector3(1, 2, 3);
        var b = new IVector3(4, 5, 6);
        var result = a * b;
        ClassicAssert.AreEqual(new IVector3(4, 10, 18), result);
    }

    // Тестирование умножения вектора на скаляр
    [Test]
    public void Multiply_VectorByScalar_ShouldReturnCorrectResult()
    {
        var a = new IVector3(2, 3, 4);
        var scalar = 2;
        var result = a * scalar;
        ClassicAssert.AreEqual(new IVector3(4, 6, 8), result);
    }

    // Тестирование деления векторов
    [Test]
    public void Divide_Vectors_ShouldReturnCorrectResult()
    {
        var a = new IVector3(4, 6, 8);
        var b = new IVector3(2, 3, 4);
        var result = a / b;
        ClassicAssert.AreEqual(new IVector3(2, 2, 2), result);
    }

    // Тестирование деления вектора на скаляр
    [Test]
    public void Divide_VectorByScalar_ShouldReturnCorrectResult()
    {
        var a = new IVector3(4, 6, 8);
        var scalar = 2;
        var result = a / scalar;
        ClassicAssert.AreEqual(new IVector3(2, 3, 4), result);
    }

    // Тестирование унарного оператора +
    [Test]
    public void UnaryPlus_ShouldReturnCorrectResult()
    {
        var a = new IVector3(1, 2, 3);
        var result = +a;
        ClassicAssert.AreEqual(a, result);
    }

    // Тестирование унарного оператора -
    [Test]
    public void UnaryMinus_ShouldReturnCorrectResult()
    {
        var a = new IVector3(1, 2, 3);
        var result = -a;
        ClassicAssert.AreEqual(new IVector3(-1, -2, -3), result);
    }

    // Тестирование оператора равенства
    [Test]
    public void Equal_Operator_ShouldReturnTrueForEqualVectors()
    {
        var a = new IVector3(1, 2, 3);
        var b = new IVector3(1, 2, 3);
        ClassicAssert.IsTrue(a == b);
    }

    // Тестирование оператора неравенства
    [Test]
    public void NotEqual_Operator_ShouldReturnTrueForDifferentVectors()
    {
        var a = new IVector3(1, 2, 3);
        var b = new IVector3(4, 5, 6);
        ClassicAssert.IsTrue(a != b);
    }

    // Тестирование метода Equals
    [Test]
    public void Equals_Method_ShouldReturnTrueForEqualVectors()
    {
        var a = new IVector3(1, 2, 3);
        var b = new IVector3(1, 2, 3);
        ClassicAssert.IsTrue(a.Equals(b));
    }

    // Тестирование метода GetHashCode
    [Test]
    public void GetHashCode_ShouldReturnSameHashForEqualVectors()
    {
        var a = new IVector3(1, 2, 3);
        var b = new IVector3(1, 2, 3);
        ClassicAssert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    // Тестирование метода ToString
    [Test]
    public void ToString_ShouldReturnCorrectFormat()
    {
        var a = new IVector3(1, 2, 3);
        ClassicAssert.AreEqual("IVector3(1, 2, 3)", a.ToString());
    }
}