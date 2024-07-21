using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.Renderer.OpenGL.Layer;
using MegaLib.VR;

namespace MegaLib.Render.Renderer.OpenGL;

public class OpenGL_Renderer : IRenderer
{
  private readonly OpenGL_Context _context = new();
  private Render_Scene _scene;
  private VrRuntime _vrRuntime;

  private void Clear()
  {
    OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT);
    OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
  }

  private void Render()
  {
    _scene.Render();
    OpenGL32.wglSwapBuffers(OpenGL32.wglGetCurrentDC());

    // Подчищаем удаленные ресурсы
    _context.Clean();
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

  public void Tick(float delta, int updateIteration)
  {
    for (var i = 0; i < updateIteration; i++) _scene.Update(delta);
    if (_vrRuntime != null)
    {
      _vrRuntime.Tick();
    }
    else
    {
      Clear();
      Render();
    }
  }

  public VrRuntime StartVrSession(Dictionary<string, object> args)
  {
    _vrRuntime = new VrRuntime();
    _vrRuntime.InitSession((IntPtr)args["dc"], (IntPtr)args["glrc"]);
    _vrRuntime.OnRender = (pose, fov) =>
    {
      Clear();

      _scene.Camera.IsZInverted = true;
      _scene.Camera.Position = new Vector3(pose.Position.X, pose.Position.Y, pose.Position.Z);
      _scene.Camera.Position += new Vector3(VrInput.Headset.PositionOffset.X, VrInput.Headset.PositionOffset.Y,
        -VrInput.Headset.PositionOffset.Z);

      _scene.Camera.Rotation = new Quaternion(
        pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z, pose.Orientation.W
      ).Inverted;
      ((Camera_Perspective)_scene.Camera).XrFOV = new Vector4(
        fov.AngleLeft, fov.AngleUp, fov.AngleRight, fov.AngleDown
      );

      Render();
    };
    return _vrRuntime;
  }

  public Render_Scene Scene
  {
    get => _scene;
    set
    {
      _scene = value;

      _context.MapTexture(_scene.Skybox);

      foreach (var layer in value.Pipeline)
      {
        layer.LayerRenderer = layer switch
        {
          RL_Line => new LR_Line(_context, layer, _scene),
          RL_Point => new LR_Point(_context, layer, _scene),
          RL_StaticMesh => new LR_Mesh(_context, layer, _scene),
          RL_Sprite => new LR_Sprite(_context, layer, _scene),
          RL_UI => new LR_UI(_context, layer, _scene),
          RL_SkinnedMesh => new LR_Skin(_context, layer, _scene),
          RL_Skybox => new LR_Skybox(_context, layer, _scene),
          RL_BitmapText => new GL_Layer_BitmapText(_context, layer, _scene),
          _ => throw new Exception("Unsupported layer type")
        };

        layer.Init();
      }
    }
  }
}