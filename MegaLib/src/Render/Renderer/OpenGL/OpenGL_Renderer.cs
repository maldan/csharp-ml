using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Layer;
using MegaLib.Render.Renderer.OpenGL.Layer;
using MegaLib.Render.Scene;
using MegaLib.VR;

namespace MegaLib.Render.Renderer.OpenGL;

public class OpenGL_Renderer : IRenderer
{
  private readonly OpenGL_Context _context = new();
  private Render_Scene _scene;
  private VrRuntime _vrRuntime;
  private RendererConfig _rendererConfig;
  private Rectangle _viewport = new(0, 0, 800, 600);

  private void Clear()
  {
    OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT | OpenGL32.GL_STENCIL_BUFFER_BIT);
    if (_scene != null)
    {
      OpenGL32.glClearColor(
        _scene.BackgroundColor.R,
        _scene.BackgroundColor.G,
        _scene.BackgroundColor.B,
        _scene.BackgroundColor.A);
    }
    else
    {
      OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    }
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

  public void SetConfig(RendererConfig config)
  {
    _rendererConfig = config;
  }

  public void Tick(float delta, int updateIteration)
  {
    _scene.DeltaTime = delta;
    for (var i = 0; i < updateIteration; i++) _scene.Update(delta);
    if (_vrRuntime != null)
    {
      _scene.IsVrMode = true;
      _vrRuntime.Tick();
    }
    else
    {
      _scene.IsVrMode = false;
      Clear();
      Render();
    }
  }

  public VrRuntime StartVrSession(Dictionary<string, object> args)
  {
    _vrRuntime = new VrRuntime();
    _vrRuntime.InitSession((IntPtr)args["dc"], (IntPtr)args["glrc"]);
    _vrRuntime.OnRender = (predictedTime, pose, fov) =>
    {
      Clear();

      // _scene.Camera.IsZInverted = true;
      _scene.Camera.Position = new Vector3(pose.Position.X, pose.Position.Y, pose.Position.Z);

      /*_scene.Camera.Position += new Vector3(VrInput.Headset.PositionOffset.X, VrInput.Headset.PositionOffset.Y,
        -VrInput.Headset.PositionOffset.Z);*/

      _scene.Camera.Rotation = new Quaternion(
        pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z, pose.Orientation.W
      );

      /*var mx = Matrix4x4.Identity;
      mx = mx.Rotate(new Quaternion(
        pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z, pose.Orientation.W
      ).Inverted);
      mx = mx.Translate(pose.Position.X, pose.Position.Y, -pose.Position.Z);*/

      ((Camera_Perspective)_scene.Camera).XrFOV = new Vector4(
        fov.AngleLeft, fov.AngleUp, fov.AngleRight, fov.AngleDown
      );

      //var wt = VrInput.Headset.WorldTransform;
      //wt = wt.Scale(new Vector3(-1, 0, 0))
      //_scene.Camera.ViewMatrix = _scene.Camera.ViewMatrix.Scale(new Vector3(-1, 1, 1));
      _scene.Camera.ViewMatrix = VrInput.Headset.WorldTransform * _scene.Camera.ViewMatrix;

      // Invert by x (I don't know why)
      _scene.Camera.ViewMatrix = _scene.Camera.ViewMatrix.Scale(new Vector3(-1, 1, 1));

      //_scene.Camera.Position += VrInput.Headset.WorldTransform.Position;
      //_scene.Camera.Rotation *= VrInput.Headset.WorldTransform.Rotation;

      SetViewport(0, 0, (ushort)_vrRuntime.ViewWidth, (ushort)_vrRuntime.ViewHeight);

      Render();
    };

    return _vrRuntime;
  }

  public void SetViewport(ushort x, ushort y, ushort width, ushort height)
  {
    var old = _viewport;
    _viewport = Rectangle.FromLeftTopWidthHeight(x, y, width, height);
    OpenGL32.glViewport(0, 0, width, height);

    // Поменялся
    if (old != _viewport) (_scene.PostProcessLayer.LayerRenderer as LR_PostProcess).Framebuffer.Resize(width, height);
  }

  public Render_Scene Scene
  {
    get => _scene;
    set
    {
      _scene = value;
      _scene.OnInit();
      _context.MapTexture(_scene.Skybox);

      foreach (var layer in value.Pipeline)
      {
        layer.LayerRenderer = layer switch
        {
          Layer_Line => new LR_Line(_context, layer, _scene),
          Layer_Point => new LR_Point(_context, layer, _scene),
          Layer_StaticMesh => new LR_Mesh(_context, layer, _scene),
          Layer_Voxel => new LR_Voxel(_context, layer, _scene),
          Layer_Sprite => new LR_Sprite(_context, layer, _scene),
          // Layer_UI => new LR_UI(_context, layer, _scene),
          //Layer_IMGUI => new LR_IMGUI(_context, layer, _scene),
          Layer_EasyUI => new LR_EasyUI(_context, layer, _scene),
          Layer_SkinnedMesh => new LR_SkinnedMesh(_context, layer, _scene),
          Layer_Skybox => new LR_Skybox(_context, layer, _scene),
          //Layer_BitmapText => new GL_Layer_BitmapText(_context, layer, _scene),
          Layer_Capture => new LR_Capture(_context, layer, _scene),
          Layer_PostProcess => new LR_PostProcess(_context, layer, _scene),
          _ => throw new Exception("Unsupported layer type")
        };

        layer.Init();
      }

      _scene.PostProcessLayer.LayerRenderer = new LR_PostProcess(_context, _scene.PostProcessLayer, _scene);
      _scene.PostProcessLayer.Init();
    }
  }
}