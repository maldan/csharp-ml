using System;
using System.Threading.Tasks;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Layer;
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
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
    AddLayer("line", new Layer_Line()
    {
      Camera = new Camera_Orthographic()
      // LineWidth = 2
    });

    var imgui = GetLayer<Layer_IMGUI>();

    var rnd = new Random();
    var sex = 0f;

    imgui.Add<IMGUI_Element>(t =>
    {
      // t.Scrollable = true;
      t.IsDebug = true;

      t.Style.Padding = new Vector4(5, 5, 5, 5);
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.Gap = 5;
      t.Style.Width = 60;
      t.Style.Height = 90;
      t.Style.Left = 10;
      t.Style.Top = 10;

      t.Events.OnMouseOver = () => { t.Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1); };
      t.Events.OnMouseOut = () => { t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1); };

      t.Text = "GAS";

      /*t.Events.OnRender = f =>
      {
        sex -= f;
        t.Scroll.Y = sex;
      };*/

      /*imgui.Add<IMGUI_Element>(t =>
      {
        // t.IsDebug = true;

        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.9f, 0.1f, 0.1f, 1);

        //t.Events.OnMouseOver = () => { t.Style.BackgroundColor = new Vector4(1.0f, 0.5f, 0.5f, 1); };
        //t.Events.OnMouseOut = () => { t.Style.BackgroundColor = new Vector4(0.0f, 0.5f, 0.5f, 1); };
        //t.Events.OnClick = () => { t.Style.BackgroundColor = new Vector4(1.0f, 1.5f, 1.5f, 1); };
      });*/

      /*var colors = new Vector4[]
      {
        new(0.1f, 0.5f, 0.1f, 2),
        new(0.5f, 0.1f, 0.1f, 2),
        new(0.5f, 0.5f, 0.1f, 2),
        new(0.8f, 0.5f, 0.3f, 2)
      };

      for (var i = 0; i < colors.Length; i++)
      {
        var i1 = i;

        imgui.Add<IMGUI_Element>(t =>
        {
          t.Text = "GAS";
          // t.IsDebug = true;

          t.Style.Width = 80;
          t.Style.Height = 30;
          t.Style.BackgroundColor = colors[i1];

          t.Events.OnMouseOver = () => { t.Style.BackgroundColor = colors[i1] * 1.2f; };
          t.Events.OnMouseOut = () => { t.Style.BackgroundColor = colors[i1]; };

          //t.Events.OnClick = () => { t.Style.BackgroundColor = new Vector4(1.0f, 1.5f, 1.5f, 1); };
        });
      }*/

      /*imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
        t.Style.BorderWidth = 1;

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

      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
        t.Style.BorderWidth = 1;
        t.Style.Color = new Vector4(1, 1, 1, 1);
        t.Text = "SEX";
      });*/

      // t.Events.OnRender = f => { t.Scroll.Y = (float)Math.Sin(sex++ / 32f) * 60f; };
    });
  }

  /*public override void OnBeforeRender()
  {
    var l = GetLayer<Layer_Line>();

    var a = new Rectangle(10, 10, 244, 244);
    var b = new Rectangle(80, 20, 280, 60);

    l.DrawRectangle(a, new RGBA<float>(1, 1, 1, 1));
    l.DrawRectangle(b, new RGBA<float>(1, 1, 1, 1));
    if (b.IsIntersects(a))
    {
      l.DrawRectangle(a.GetIntersection(b), new RGBA<float>(1, 0, 0, 1));
    }
  }*/
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
      OnResize = (win) =>
      {
        var w = win.ClientWidth;
        var h = win.ClientHeight;

        if (scene.Camera is Camera_Perspective p)
        {
          p.AspectRatio = w / (float)h;
        }

        var c1 = scene.GetLayer<Layer_IMGUI>();
        if (c1 != null)
        {
          c1.Camera.LeftBorder = 0;
          c1.Camera.TopBorder = 0;
          c1.Camera.RightBorder = w;
          c1.Camera.BottomBorder = h;
        }

        var c2 = scene.GetLayer<Layer_Line>();
        if (c2 != null)
        {
          c2.Camera.LeftBorder = 0;
          c2.Camera.TopBorder = 0;
          c2.Camera.RightBorder = w;
          c2.Camera.BottomBorder = h;
        }

        OpenGL32.glViewport(0, 0, w, h);
      }
    };

    win.InitOpenGL();

    // scene.OnInit();

    if (scene.Camera is Camera_Orthographic c)
    {
      c.LeftBorder = 0;
      c.TopBorder = 0;
      c.RightBorder = 1280 / 4;
      c.BottomBorder = 720 / 4;
    }

    renderer.Scene = scene;
    Task.Run(() => { scene.OnLoad(); });

    win.Center();
    win.Show();
    win.Run();
  }
}