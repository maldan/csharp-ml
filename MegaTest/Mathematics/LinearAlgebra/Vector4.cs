using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace MegaTest.Mathematics.LinearAlgebra;

public class Vector4Tests
{
  // Тестирование метода ToString
  [Test]
  public void Testx()
  {
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(1, 0, 0, 0)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(1, 1, 0, 0)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(1, 1, 1, 0)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(1, 1, 1, 1)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(1, 0, 0, 1)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(1, 1, 0, 1)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(0, 0, 0, 0)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(0, 1, 0, 0)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(0, 0, 1, 0)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(0, 0, 1, 1)));
    Console.WriteLine(Vector4.NormalizeWeights(new Vector4(0, 0, 0.5f, 1)));
  }
}