using System;
using System.IO;
using System.Reflection;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Shader;
using NUnit.Framework;

namespace MegaTest.Shader;

public class AShaderProgramTest
{
  [Test]
  public void Basic()
  {
    var x = ShaderProgram.Compile("Mesh");
    File.WriteAllText("D:/csharp_lib/MegaLib/backup/vertex.glsl", x["vertex"]);
    File.WriteAllText("D:/csharp_lib/MegaLib/backup/fragment.glsl", x["fragment"]);
  }

  /*[Test]
  public void Basic2()
  {
    var x = ShaderProgram.Compile2("Mesh");
    File.WriteAllText("D:/csharp_lib/MegaLib/backup/vertex2.glsl", x["vertex"]);
    File.WriteAllText("D:/csharp_lib/MegaLib/backup/fragment2.glsl", x["fragment"]);
  }*/
}