using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.Renderer.OpenGL.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class Renderer_OpenGL : IRenderer
  {
    private readonly Context_OpenGL _context = new();
    private Render_Scene _scene;
    private readonly Dictionary<string, GLRL_Base> _glLayer = new();

    public void SetScene(Render_Scene scene)
    {
      _scene = scene;

      foreach (var layer in scene.Pipeline)
      {
        switch (layer)
        {
          case RL_StaticLine:
            _glLayer[layer.Name] = new GLRL_Line(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          case RL_StaticMesh:
            _glLayer[layer.Name] = new GLRL_StaticMesh(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          case RL_SkinnedMesh:
            _glLayer[layer.Name] = new GLRL_SkinnedMesh(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          case RL_Skybox:
            _glLayer[layer.Name] = new GLRL_Skybox(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          default:
            throw new Exception("Unsupported layer type");
        }
      }

      // Init cube map
      _context.CreateCubeTexture("main", new[]
      {
        _scene.Skybox.GPU_RIGHT,
        _scene.Skybox.GPU_LEFT,

        _scene.Skybox.GPU_TOP,
        _scene.Skybox.GPU_BOTTOM,

        _scene.Skybox.GPU_BACK,
        _scene.Skybox.GPU_FRONT,
      }, _scene.Skybox.Options);
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
        if (_glLayer.ContainsKey(layer.Name)) _glLayer[layer.Name].Render();
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