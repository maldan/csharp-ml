using System;
using System.Collections.Generic;
using System.Diagnostics;
using MegaLib.Runtime;
using NUnit.Framework;

namespace MegaTest.Runtime;

public class RuntimeExpressionTest
{
  [Test]
  public void TestBasic()
  {
    var o = new Dictionary<string, object>()
    {
      { "IsJump", true },
      { "A", 1 },
      { "B", 2 }
    };
    var re = RuntimeExpression.Compile("A + B", o);

    var t = new Stopwatch();
    t.Start();
    for (var i = 0; i < 100000; i++) re.Invoke(o);

    t.Stop();
    Console.WriteLine(t.ElapsedMilliseconds);
    // Console.WriteLine();
  }

  [Test]
  public void TestAddition()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 1 },
      { "B", 2 }
    };
    var re = RuntimeExpression.Compile("A + B", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(3f));
  }

  [Test]
  public void TestBooleanAnd()
  {
    var o = new Dictionary<string, object>()
    {
      { "IsJump", true },
      { "IsRun", false }
    };
    var re = RuntimeExpression.Compile("IsJump && IsRun", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(false));
  }

  [Test]
  public void TestBooleanOr()
  {
    var o = new Dictionary<string, object>()
    {
      { "IsJump", true },
      { "IsRun", false }
    };
    var re = RuntimeExpression.Compile("IsJump || IsRun", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(true));
  }

  [Test]
  public void TestMultiplication()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 5 },
      { "B", 4 }
    };
    var re = RuntimeExpression.Compile("A * B", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(20f));
  }

  [Test]
  public void TestSubtraction()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 10 },
      { "B", 3 }
    };
    var re = RuntimeExpression.Compile("A - B", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(7f));
  }

  [Test]
  public void TestDivision()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 20 },
      { "B", 5 }
    };
    var re = RuntimeExpression.Compile("A / B", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(4f));
  }

  [Test]
  public void TestComplexExpression()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 2 },
      { "B", 3 },
      { "C", 5 }
    };
    var re = RuntimeExpression.Compile("(A + B) * C", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(25f));
  }

  [Test]
  public void TestNestedExpression()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 1 },
      { "B", 2 },
      { "C", 3 }
    };
    var re = RuntimeExpression.Compile("(A + (B * C))", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(7f));
  }

  [Test]
  public void TestBooleanWithMath()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 10 },
      { "B", 20 }
    };
    var re = RuntimeExpression.Compile("A < B && B > 15", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(true));
  }

  [Test]
  public void TestComplexBooleanWithMath()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 10 },
      { "B", 5 },
      { "C", 2 }
    };
    var re = RuntimeExpression.Compile("A > B && (A + B) > C", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(true));
  }

  [Test]
  public void TestDeepNestedExpression()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 1 },
      { "B", 2 },
      { "C", 3 },
      { "D", 4 }
    };
    var re = RuntimeExpression.Compile("((A + B) * (C + D))", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(21f));
  }

  [Test]
  public void TestMixedTypes()
  {
    var o = new Dictionary<string, object>()
    {
      { "A", 10 },
      { "B", 5 },
      { "IsTrue", true },
      { "IsFalse", false }
    };
    var re = RuntimeExpression.Compile("A > B && IsTrue || IsFalse", o);
    var result = re.Invoke(o);
    Assert.That(result, Is.EqualTo(true));
  }
}