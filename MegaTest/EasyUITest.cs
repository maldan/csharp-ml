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
      Camera = new Camera_Orthographic(),
      LineWidth = 2
    });

    var easyUi = GetLayer<Layer_EasyUI>();

    var rnd = new Random();
    var sex = 0f;

    easyUi.Add<EasyUI_Element>(t =>
    {
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.SetArea(10, 10, 60, 90);
      t.Style.TextAlign = "center";

      t.Events.OnMouseOver = () => { t.Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1); };
      t.Events.OnMouseOut = () => { t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1); };

      t.Text = "GAS";

      easyUi.Add<EasyUI_Element>(t =>
      {
        t.Style.BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 1);
        t.Style.SetArea(10, 10, 32, 32);
        t.Text = "XX";
        t.Style.TextAlign = "center";
      });
    });

    easyUi.Add<EasyUI_Element>(t =>
    {
      t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
      t.Style.SetArea(100, 10, 60, 90);

      t.Events.OnMouseOver = () => { t.Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1); };
      t.Events.OnMouseOut = () => { t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1); };

      t.Text = "BLA BLA BL\nDFDFDF\nXX";
    });
  }
}

public class EaseUITest
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

        var c1 = scene.GetLayer<Layer_EasyUI>();
        if (c1 != null)
        {
          c1.Camera.Left = 0;
          c1.Camera.Top = 0;
          c1.Camera.Right = w;
          c1.Camera.Bottom = h;
        }

        var c2 = scene.GetLayer<Layer_Line>();
        if (c2 != null)
        {
          c2.Camera.Left = 0;
          c2.Camera.Top = 0;
          c2.Camera.Right = w;
          c2.Camera.Bottom = h;
        }

        OpenGL32.glViewport(0, 0, w, h);
      }
    };

    win.InitOpenGL();

    // scene.OnInit();

    if (scene.Camera is Camera_Orthographic c)
    {
      c.Left = 0;
      c.Top = 0;
      c.Right = 1280 / 4;
      c.Bottom = 720 / 4;
    }

    renderer.Scene = scene;
    Task.Run(() => { scene.OnLoad(); });

    win.Center();
    win.Show();
    win.Run();
  }
}