using System;
using MegaLib.Compiler.Sharp;
using MegaLib.Render.Shader;
using NUnit.Framework;

namespace MegaTest.Compiler.Sharp;

public class SharpCompilerTest
{
  [Test]
  public void Basic()
  {
    var xx = new SharpCompiler();
    // language=C#
    xx.AddCode(@"
      public class X {
        public string Test = "";

        public void Hello() {
        }
      }
      
      public class Y : X {
        public string Rock = "";
            
        [MyAttribute]
        public void Test() {
            var x = 5.0f;
            if (x > 4.0f) {
                Hello();
            }
        }
      }
    ");
    xx.Parse();
    foreach (var classInfo in xx.ClassList)
    {
      foreach (var attribute in classInfo.AttributeList)
      {
        Console.WriteLine($"[{attribute.Name}()]");
      }

      Console.WriteLine($"Class {classInfo.Name} : {classInfo.Parent?.Name}");

      foreach (var field in classInfo.FieldList)
      {
        foreach (var attribute in field.AttributeList)
        {
          Console.WriteLine($"[{attribute.Name}()]");
        }

        Console.WriteLine($"-- {field.Type} {field.Name}");
      }

      foreach (var method in classInfo.MethodList)
      {
        foreach (var attribute in method.AttributeList)
        {
          Console.WriteLine($"[{attribute.Name}()]");
        }

        Console.WriteLine($"{method.AA}");
        Console.WriteLine($"-- {method.ReturnType} {method.Name}() {{");
        foreach (var statement in method.StatementList)
        {
          Console.WriteLine($"---- {statement}");
        }

        Console.WriteLine("}");
      }
    }
  }
}