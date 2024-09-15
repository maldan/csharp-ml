using System;
using System.IO;
using System.Reflection;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Shader;
using NUnit.Framework;

namespace MegaTest.Shader;

public class ShaderProgramTest
{
  [Test]
  public void Basic()
  {
    var x = ShaderProgram.Compile("Mesh");
    Console.WriteLine(x);
  }
}