using System;
using System.Threading.Tasks;
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
    AddLayer("dynamicLine", new Layer_Line() { LineWidth = 2f });
    AddLayer("staticMesh", new Layer_StaticMesh() { });

    var cubeMesh = MeshGenerator.Cube(1);
    var roCubeMesh = new RO_Mesh();
    roCubeMesh.FromMesh(cubeMesh);
    Add("staticMesh", roCubeMesh);

    roCubeMesh.InitDefaultTextures();

    var ld = new LightDirection();
    ld.Direction = new Vector3(0, 0, 1);
    ld.Color = new RGBA<float>(1, 1, 1, 1);
    ld.Intensity = 1;
    Lights.Add(ld);

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
  }

  public override void OnBeforeUpdate(float delta)
  {
    Camera.BasicMovement(delta);

    Console.WriteLine($"{Camera.Rotation.Euler.ToDegrees}");
  }

  public override void OnBeforeRender()
  {
    for (var i = 0; i < 8; i++)
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
      OnPaint = (delta) =>
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

    scene.OnInit();
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