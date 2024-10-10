using System;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Texture;
using NUnit.Framework;

namespace MegaTest;

public class OpenGlTest
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public void OpenGL_GenerateBuffers()
  {
    var win = new Window();
    win.InitOpenGL();

    uint bufferId = 0;
    OpenGL32.glGenBuffers(1, ref bufferId);
    Console.WriteLine(bufferId);
    OpenGL32.glDeleteBuffers(1, [bufferId]);
  }

  [Test]
  public void OpenGL_GenerateTextures()
  {
    var win = new Window();
    win.InitOpenGL();

    uint id = 0;
    OpenGL32.glGenTextures(1, ref id);
    OpenGL32.glDeleteTextures([id]);
  }

  [Test]
  public void OpenGL_Textures()
  {
    var win = new Window();
    win.InitOpenGL();
    OpenGL32.glEnable(OpenGL32.GL_DEBUG_OUTPUT);
    OpenGL32.PrintGlError("OpenGL32.GL_DEBUG_OUTPUT");

    var scene = new Render_Scene();
    scene.AddLayer("main", new Layer_Sprite());

    for (var i = 0; i < 10; i++)
    {
      var sprite = new RO_Sprite();
      sprite.Texture = new Texture_2D<RGBA<byte>>(32, 32);
      sprite.Width = 32;
      sprite.Height = 32;
      scene.GetLayer<Layer_Sprite>().Add(sprite);
    }

    var renderer = new OpenGL_Renderer();
    renderer.Scene = scene;
    renderer.Tick(0.016f, 1);

    scene.DeleteAll("main");

    GC.Collect();
    GC.WaitForPendingFinalizers();
  }

  [Test]
  public void OpenGL_RenderObject()
  {
    var s1 = new RO_Sprite();
    var s2 = new RO_Mesh();
    var s3 = new RO_Skin();
    Console.WriteLine(s1.Id);
    Console.WriteLine(s2.Id);
    Console.WriteLine(s3.Id);
  }
}