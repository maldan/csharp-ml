using System;
using System.IO;
using System.Reflection;
using MegaLib.AssetLoader;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Shader;
using NUnit.Framework;

namespace MegaTest.Shader;

public class VoxLoaderTest
{
  [Test]
  public void Basic()
  {
    var voxLoader = new VoxLoader();
    voxLoader.Parse(@"C:\Software\MagicaVoxel-0.99.7.1-win64\vox\monu2.vox");
    voxLoader.PrintInfo();
  }
}