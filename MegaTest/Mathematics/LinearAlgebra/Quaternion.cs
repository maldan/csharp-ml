using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics.LinearAlgebra;

public class QuaternionTest
{
  // Тест для углов (0, 0, 0)
  [Test]
  public void TestEulerToQuaternion_ZeroRotation()
  {
    var eulerAngles = new Vector3(0, 0, 0);
    var expected = new Quaternion(0, 0, 0, 1); // кватернион идентичности

    var result = Quaternion.FromEuler(eulerAngles, "deg");

    ClassicAssert.AreEqual(expected, result);
  }

  // Тест для углов (90, 0, 0) - поворот на 90 градусов по оси X
  [Test]
  public void TestEulerToQuaternion_Rotation90DegreesX()
  {
    var eulerAngles = new Vector3(90, 0, 0);
    var expected = new Quaternion(0.7071f, 0, 0, 0.7071f); // Ожидаемый кватернион

    var result = Quaternion.FromEuler(eulerAngles, "deg");

    ClassicAssert.AreEqual(expected, result);
  }

  [Test]
  public void Test2()
  {
    //var dir = new Vector3(0, -1, 0);
    //var q = Quaternion.LookRotation(Vector3.Down, Vector3.Up);
    //Console.WriteLine(q.Euler.ToDegrees);
  }

  // Тест для углов (0, 90, 0) - поворот на 90 градусов по оси Y
  /*[Test]
  public void TestEulerToQuaternion_Rotation90DegreesY()
  {
    var eulerAngles = new Vector3(0, 90, 0);
    var expected = new Quaternion(0, 0.7071f, 0, 0.7071f); // Ожидаемый кватернион

    var result = EulerToQuaternion(eulerAngles);

    ClassicAssert.AreEqual(QuaternionsAreEqual(expected, result), true);
  }

  // Тест для углов (0, 0, 90) - поворот на 90 градусов по оси Z
  [Test]
  public void TestEulerToQuaternion_Rotation90DegreesZ()
  {
    var eulerAngles = new Vector3(0, 0, 90);
    var expected = new Quaternion(0, 0, 0.7071f, 0.7071f); // Ожидаемый кватернион

    var result = EulerToQuaternion(eulerAngles);

    ClassicAssert.AreEqual(QuaternionsAreEqual(expected, result), true);
  }*/

  // Вспомогательная функция для сравнения кватернионов с учетом плавающей точности
  private bool QuaternionsAreEqual(Quaternion q1, Quaternion q2, float tolerance = 0.0001f)
  {
    return MathF.Abs(q1.X - q2.X) < tolerance &&
           MathF.Abs(q1.Y - q2.Y) < tolerance &&
           MathF.Abs(q1.Z - q2.Z) < tolerance &&
           MathF.Abs(q1.W - q2.W) < tolerance;
  }
}