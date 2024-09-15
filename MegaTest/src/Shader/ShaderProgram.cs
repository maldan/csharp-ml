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
    File.WriteAllText("D:/csharp_lib/MegaLib/backup/vertex.glsl", x["vertex"]);
    File.WriteAllText("D:/csharp_lib/MegaLib/backup/fragment.glsl", x["fragment"]);
  }
}