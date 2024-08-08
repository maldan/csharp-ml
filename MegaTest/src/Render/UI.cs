using System;
using System.Threading.Tasks;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Renderer.OpenGL;
using NUnit.Framework;
using Random = MegaLib.Mathematics.Random;

namespace MegaTest.Render;

internal class TestScene2 : Render_Scene
{
  public override void OnInit()
  {
    // Инициализируем камеру
    var _camera = new Camera_Perspective
    {
      AspectRatio = 800f / 600f
    };
    _camera.Position += new Vector3(0, 4, -5);
    _camera.Rotation = Quaternion.FromEuler(45, 0, 0, "deg");
    Camera = _camera;

    // Добавляем слои
    AddLayer("imgui", new Layer_IMGUI()
    {
      Camera = new Camera_Orthographic()
    });

    var imgui = GetLayer<Layer_IMGUI>();

    var rnd = new Random();
    var sex = 0f;

    imgui.Add<IMGUI_Element>(t =>
    {
      t.Style.Padding = new Vector4(5, 5, 5, 5);
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.Gap = 5;

      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.35f, 0.35f, 0.35f, 1);

        t.Events.OnMouseOver = () => { t.Style.BackgroundColor = new Vector4(1.0f, 0.5f, 0.5f, 1); };
        t.Events.OnMouseOut = () => { t.Style.BackgroundColor = new Vector4(0.0f, 0.5f, 0.5f, 1); };
        t.Events.OnClick = () => { t.Style.BackgroundColor = new Vector4(1.0f, 1.5f, 1.5f, 1); };
      });

      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);

        t.Events.OnMouseOver = () =>
        {
          System.Console.WriteLine("OVER");
          t.Style.BackgroundColor = new Vector4(1.0f, 0.5f, 0.5f, 1);
        };
        t.Events.OnMouseOut = () =>
        {
          System.Console.WriteLine("OUT");
          t.Style.BackgroundColor = new Vector4(0.0f, 0.5f, 0.5f, 1);
        };
        t.Events.OnClick = () =>
        {
          System.Console.WriteLine("CICK");
          t.Style.BackgroundColor = new Vector4(1.0f, 1.5f, 1.5f, 1);
          t.Style.Color = new Vector4(0.0f, 0.0f, 0.0f, 1);
        };
        t.Style.Color = new Vector4(1, 1, 1, 1);
        t.Text = "GAY";
        t.Style.Margin = new Vector4(0, 5, 0, 0);
      });
    });
  }
}

internal class TestScene : Render_Scene
{
  public override void OnInit()
  {
    // Инициализируем камеру
    var _camera = new Camera_Perspective
    {
      AspectRatio = 800f / 600f
    };
    _camera.Position += new Vector3(0, 4, -5);
    _camera.Rotation = Quaternion.FromEuler(45, 0, 0, "deg");
    Camera = _camera;

    // Добавляем слои
    AddLayer("imgui", new Layer_IMGUI()
    {
      Camera = new Camera_Orthographic()
    });

    var imgui = GetLayer<Layer_IMGUI>();

    var rnd = new Random();
    var sex = 0f;

    imgui.Add<IMGUI_Element>(t =>
    {
      t.Style.Width = 80;
      t.Style.Height = 30;
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.Left = 4;
      t.Style.Top = 4;

      t.Style.Color = new Vector4(1, 1, 1, 1);

      t.Events.OnRender = f =>
      {
        t.Text = $"{t.Style.Left}";
        t.Style.Left = Math.Sin(sex * 3f) * 32;
        t.Style.Top = -Math.Cos(sex * 3f) * 32;
        sex += f;
      };

      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = "50%";
        t.Style.Height = 10;
        t.Style.BackgroundColor = "blue";
        t.Style.Left = 5;
        t.Style.Top = 5;
      });

      /*imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = "100%";
        t.Style.Height = 10;
        t.Style.BackgroundColor = "green";

        /*t.Style.Left = 5;
        t.Style.Top = 5;#1#
        t.Style.Top = 1;
      });

      var sex = 0f;
      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = "50%";
        t.Style.Height = 10;
        t.Style.BackgroundColor = new Vector4(1, 1, 1, 1) * new Vector4(0.5f, 0.5f, 0.5f, 1f);

        /*t.Style.Left = 5;
        t.Style.Top = 5;#1#
        t.Style.Top = 1;

        t.Events.OnRender = f =>
        {
          t.Style.Left = Math.Sin(sex * 10f) * 10;
          sex += f;
        };
      });*/
    });

    /*imgui.BeginWindow("Hello");
    imgui.Button("Hello", (btn) => { btn.OnClick = () => { Console.WriteLine("GAS"); }; });
    imgui.Button("Hello", (btn) => { btn.OnClick = () => { Console.WriteLine("GAS"); }; });

    // Дерево
    imgui.Add<IMGUI_Tree>((s) =>
    {
      s.Title = "Sasa";
      imgui.Button("Hello", (btn) => { btn.OnClick = () => { Console.WriteLine("GAS"); }; });

      imgui.Add<IMGUI_Tree>((s2) =>
      {
        s2.Title = "Rock";
        imgui.Button("Sas", (btn) => { btn.OnClick = () => { Console.WriteLine("GAS"); }; });
      });
    });

    imgui.EndWindow();*/
  }
}

public class UITest
{
  [Test]
  public void TestBasic()
  {
    var renderer = new OpenGL_Renderer();
    var scene = new TestScene2();

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