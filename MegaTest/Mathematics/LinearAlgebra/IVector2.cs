using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics.LinearAlgebra;

public class IVector2Tests
{
  [Test]
  public void VectorMultiplication()
  {
    // Arrange
    var vectorA = new IVector2(2, 3);
    var vectorB = new IVector2(4, 5);

    // Act
    var result = vectorA * vectorB;

    // Assert
    ClassicAssert.AreEqual(new IVector2(8, 15), result);
  }

  [Test]
  public void VectorMultiplicationByScalar()
  {
    // Arrange
    var vector = new IVector2(3, 4);
    var scalar = 2;

    // Act
    var result = vector * scalar;

    // Assert
    ClassicAssert.AreEqual(new IVector2(6, 8), result);
  }

  [Test]
  public void MultiplicationByZeroVector()
  {
    // Arrange
    var vectorA = new IVector2(5, 7);
    var zeroVector = new IVector2(0, 0);

    // Act
    var result = vectorA * zeroVector;

    // Assert
    ClassicAssert.AreEqual(new IVector2(0, 0), result);
  }

  [Test]
  public void MultiplicationByNegativeVector()
  {
    // Arrange
    var vectorA = new IVector2(3, 4);
    var vectorB = new IVector2(-2, -1);

    // Act
    var result = vectorA * vectorB;

    // Assert
    ClassicAssert.AreEqual(new IVector2(-6, -4), result);
  }

  [Test]
  public void MultiplicationByUnitVector()
  {
    // Arrange
    var vector = new IVector2(5, 7);
    var oneVector = new IVector2(1, 1);

    // Act
    var result = vector * oneVector;

    // Assert
    ClassicAssert.AreEqual(vector, result);
  }
}