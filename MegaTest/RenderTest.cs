using System;
using System.Threading.Tasks;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Physics;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Layer;
using MegaLib.Render.Light;
using MegaLib.Render.Mesh;
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Texture;
using MegaTest.Render;
using NUnit.Framework;
using Console = System.Console;

namespace MegaTest;

internal class RenderTestScene : Render_Scene
{
  private float _x;
  private RO_Mesh _cube;
  private RO_Mesh _sphere;
  private float _cameraOrbitRadius = 10f;
  private Vector3 _cameraFocalPoint = new();

  public override void OnInit()
  {
    // Инициализируем камеру
    var _camera = new Camera_Perspective
    {
      AspectRatio = 800f / 600f
    };
    _camera.Position += new Vector3(0, 0, -5);
    _camera.Rotation = Quaternion.FromEuler(0, 0, 0, "deg");
    Camera = _camera;

    // Добавляем слои
    AddLayer("imgui", new Layer_IMGUI()
    {
      Camera = new Camera_Orthographic()
    });
    AddLayer("dynamicPoint", new Layer_Point() { });
    AddLayer("dynamicLine", new Layer_Line() { });
    AddLayer("staticMesh", new Layer_StaticMesh() { });
    AddLayer("skybox", new Layer_Skybox() { });

    Skybox = new Texture_Cube();
    Skybox.Options.FiltrationMode = TextureFiltrationMode.Nearest;

    // 
    /*var gltf = GLTF.FromFile("C:\\Users\\black\\Desktop\\untitled.gltf");
    foreach (var gltfMesh in gltf.MeshList)
    {
      foreach (var primitive in gltfMesh.Primitives)
      {
        var mm = new RO_Mesh();
        mm.FromGLTF(primitive);
        mm.InitDefaultTextures();
        Add("staticMesh", mm);
      }
    }*/

    var cubeMesh = MeshGenerator.Cube(1);
    _cube = new RO_Mesh();
    _cube.FromMesh(cubeMesh);
    Add("staticMesh", _cube);
    _cube.InitDefaultTextures();
    _cube.AlbedoTexture = new Texture_2D<RGBA<byte>>(1, 1);
    _cube.AlbedoTexture.RAW[0] = new RGBA<byte>(0, 0, 0, 255);
    _cube.RoughnessTexture.RAW[0] = new RGBA<byte>(0, 128, 128, 128);
    _cube.MetallicTexture.RAW[0] = new RGBA<byte>(255, 0, 0, 128);
    _cube.Transform.Position = new Vector3(0, 0.5f, 0);

    var gridMesh = new RO_Mesh();
    gridMesh.FromMesh(MeshGenerator.Grid(2, 2, 4));
    gridMesh.InitDefaultTextures();
    Add("staticMesh", gridMesh);

    _sphere = new RO_Mesh();
    _sphere.FromMesh(MeshGenerator.UVSphere(16, 16, 0.5f));
    _sphere.InitDefaultTextures();
    _sphere.Transform.Position = new Vector3(1, 0.5f, -1f);
    _sphere.AlbedoTexture = new Texture_2D<RGBA<byte>>(1, 1);
    _sphere.AlbedoTexture.RAW[0] = new RGBA<byte>(255, 255, 255, 255);
    _sphere.RoughnessTexture.RAW[0] = new RGBA<byte>(0, 128, 128, 128);
    _sphere.MetallicTexture.RAW[0] = new RGBA<byte>(0, 0, 0, 128);
    Add("staticMesh", _sphere);

    /*var ld = new LightPoint();
    ld.Position = new Vector3(-1, 1, -1);
    ld.Color = new RGBA<float>(1, 1, 1, 1);
    ld.Intensity = 2.1f;
    ld.Radius = 1.6f;
    Lights.Add(ld);

    ld = new LightPoint();
    ld.Position = new Vector3(1, 1, -1);
    ld.Color = new RGBA<float>(1, 1, 1, 1);
    ld.Intensity = 2.1f;
    ld.Radius = 1f;
    Lights.Add(ld);*/

    var ld2 = new LightDirection();
    ld2.Direction = new Vector3(0, 1, 1);
    ld2.Color = new RGBA<float>(1, 1, 1, 1);
    ld2.Intensity = 1;
    Lights.Add(ld2);

    ld2 = new LightDirection();
    ld2.Direction = new Vector3(0, 1, -1);
    ld2.Color = new RGBA<float>(1, 0, 1, 1);
    ld2.Intensity = 1;
    Lights.Add(ld2);

    var imgui = GetLayer<Layer_IMGUI>();
    imgui.Add<IMGUI_Element>(t =>
    {
      t.Scrollable = true;
      t.IsDebug = true;

      t.Style.Padding = new Vector4(5, 5, 5, 5);
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.Gap = 5;
      t.Style.Width = 60;
      t.Style.Height = 90;
      t.Style.Left = 10;
      t.Style.Top = 10;

      t.Text = "SAS";
    });

    var id = 0;
    Console.WriteLine($"ID: {id++}");
    Console.WriteLine($"ID: {id++}");
  }

  public override void OnBeforeUpdate(float delta)
  {
    Camera.OrbitalCameraMovement(delta, ref _cameraOrbitRadius, ref _cameraFocalPoint);
    // Camera.BasicMovement(delta);
    _x += delta;

    _cube.Transform.Rotation = Quaternion.FromEuler(0, _x * 5.0f, 0, "deg");
    _sphere.Transform.Rotation = Quaternion.FromEuler(_x * 45.0f, 0, 0, "deg");

    // Console.WriteLine($"{Camera.Rotation.Euler.ToDegrees}");
  }

  public override void OnBeforeRender()
  {
    /*for (var i = 0; i < 8; i++)
    {
      Add("dynamicLine",
        new RO_Line(new Vector3(i - 3.5f, 0f, -4f), new Vector3(i - 3.5f, 0f, 4f)));
      Add("dynamicLine",
        new RO_Line(new Vector3(i - 3.5f, 4f, -4f), new Vector3(i - 3.5f, 4f, 4f)));
    }

    for (var i = 0; i < 8; i++)
    {
      Add("dynamicLine",
        new RO_Line(new Vector3(-4, 0, i - 3.5f), new Vector3(4, 0, i - 3.5f)));
      Add("dynamicLine",
        new RO_Line(new Vector3(-4, 4, i - 3.5f), new Vector3(4, 4, i - 3.5f)));
    }*/

    var lineLayer = GetLayer<Layer_Line>("dynamicLine");
    // lineLayer.DrawSphere();
    foreach (var light in Lights)
    {
      if (light is LightPoint lp)
      {
        lineLayer.DrawSphere(lp.Position, lp.Radius, new RGBA<float>(1, 0, 0, 1));
      }
    }
    /**/
  }
}

public class RenderTest
{
  [Test]
  public void TestBasic()
  {
    var renderer = new OpenGL_Renderer();
    var scene = new RenderTestScene();

    Mouse.Init();

    var win = new Window
    {
      Title = "Mazel Game",
      Width = 1280,
      Height = 720,
      OnPaint = (win, delta) =>
      {
        Keyboard.Update();
        Mouse.Update();
        renderer.Tick(delta, 1);
      },
      OnResize = (w, h) =>
      {
        if (scene.Camera is Camera_Perspective p)
        {
          p.AspectRatio = w / (float)h;
        }

        var c1 = scene.GetLayer<Layer_IMGUI>();
        if (c1 != null)
        {
          c1.Camera.Left = 0;
          c1.Camera.Top = 0;
          c1.Camera.Right = w;
          c1.Camera.Bottom = h;
        }

        OpenGL32.glViewport(0, 0, w, h);
      }
    };

    win.InitOpenGL();

    // scene.OnInit();
    Task.Run(() => { scene.OnLoad(); });
    if (scene.Camera is Camera_Orthographic c)
    {
      c.Left = 0;
      c.Top = 0;
      c.Right = 1280 / 4;
      c.Bottom = 720 / 4;
    }

    renderer.Scene = scene;

    win.Center();
    win.Show();
    win.Run();
  }
}