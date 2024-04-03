using System;
using System.Runtime.InteropServices;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.Renderer.OpenGL.Layer;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class OpenGL_Renderer : IRenderer
  {
    private readonly OpenGL_Context _context = new();
    private Render_Scene _scene;

    public void SetScene(Render_Scene scene)
    {
      _scene = scene;

      foreach (var layer in scene.Pipeline)
      {
        switch (layer)
        {
          case RL_StaticLine:
            layer.LayerRenderer = new LR_Line(_context, layer, _scene);
            break;
          default:
            throw new Exception("Unsupported layer type");
        }

        layer.Init();
      }

      // Init cube map
    }

    public void Clear()
    {
      OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT);
      OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public void Render()
    {
      foreach (var layer in _scene.Pipeline)
      {
        layer.Render();
      }
    }

    public byte[] GetScreen()
    {
      var width = 800;
      var height = 600;
      var pixels = new byte[width * height * 4];
      var pixelsPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      OpenGL32.glReadPixels(0, 0, width, height, OpenGL32.GL_RGBA, OpenGL32.GL_UNSIGNED_BYTE, pixelsPtr);

      return pixels;
    }
  }
}