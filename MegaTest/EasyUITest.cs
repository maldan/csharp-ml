using System;
using System.Threading.Tasks;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Layer;
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.Scene;
using MegaLib.Render.UI.EasyUI;
using NUnit.Framework;
using Random = MegaLib.Mathematics.Random;

namespace MegaTest;

internal class TestScene3 : Render_Scene
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
    AddLayer("easyui", new Layer_EasyUI()
    {
      Camera = new Camera_Orthographic()
    });
    AddLayer("line", new Layer_Line()
    {
      Camera = new Camera_Orthographic()
      // LineWidth = 2
    });

    var easyUi = GetLayer<Layer_EasyUI>();

    var rnd = new Random();
    var sex = 0f;

    // Основной контейнер
    easyUi.Add<EasyUI_Element>(t =>
    {
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.SetArea(10, 10, 60, 90);

      // Кнопка
      easyUi.Add<EasyUI_Button>(t =>
      {
        t.Style.X = 10;
        t.Style.Y = 10;
        t.Text = "Click";
      });

      // Кнопка
      easyUi.Add<EasyUI_Spin>(t =>
      {
        var f = 1f;
        t.Style.X = 10;
        t.Style.Y = 40;
        t.UpdateLabel($"{f:F}");
        t.Events.OnChange = o =>
        {
          if (o is int i)
          {
            if (i == -1) f -= 1;
            if (i == 1) f += 1;
          }

          t.UpdateLabel($"{f:F}");
        };
      });

      // Кнопка
      easyUi.Add<EasyUI_Check>(t =>
      {
        t.Style.X = 10;
        t.Style.Y = 65;
        t.Value = true;
      });

      // Кнопка
      easyUi.Add<EasyUI_Slider>(t =>
      {
        t.Style.X = 0;
        t.Style.Y = 90;
        t.Value = 0;
        t.Events.OnChange = f =>
        {
          if (f is float ff)
          {
            t.Style.BackgroundColor = new Vector4(ff, ff, ff, 1f);
          }
        };
      });
    });

    // Кнопка
    easyUi.Add<EasyUI_Window>(t =>
    {
      t.Style.X = 120;
      t.Style.Y = 90;

      // Кнопка
      easyUi.Add<EasyUI_Button>(t =>
      {
        t.Style.X = 0;
        t.Style.Y = 0;
        t.Text = "Click";
      });
    });

    // Кнопка
    easyUi.Add<EasyUI_Window>(t =>
    {
      t.Style.X = 220;
      t.Style.Y = 90;

      t.SetSize(128, 240);

      // Кнопка
      easyUi.Add<EasyUI_Button>(t =>
      {
        t.Style.X = 0;
        t.Style.Y = 0;
        t.Text = "Click";
      });
    });
  }
}

public class EasyUITest
{
  [Test]
  public void TestBasic()
  {
    var renderer = new OpenGL_Renderer();
    var scene = new TestScene3();

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

        var c1 = scene.GetLayer<Layer_EasyUI>();
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

    renderer.Scene = scene;
    Task.Run(() => { scene.OnLoad(); });

    win.Center();
    win.Show();
    win.Run();
  }
}