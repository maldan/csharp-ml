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
        t.Style.TextColor = new Vector4(1, 0, 0, 1);
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
            // t.Style.BackgroundColor = new Vector4(ff, ff, ff, 1f);
          }
        };
      });

      easyUi.Add<EasyUI_Slider>(t =>
      {
        t.Direction = Direction.Vertical;

        t.Style.X = 0;
        t.Style.Y = 120;
        t.Value = 0;
        t.ShowText = false;
      });
    });

    // Кнопка
    easyUi.Add<EasyUI_Window>(t =>
    {
      t.Style.X = 120;
      t.Style.Y = 90;
      t.SetSize(120, 120);

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

      t.SetSize(160, 128);

      easyUi.Add<EasyUI_ScrollPane>(scrollPane =>
      {
        scrollPane.Style.SetArea(2, 1, 160 - 4, 124);
        //scrollPane.Style.BorderWidth = 1;
        //scrollPane.Style.BorderColor = "#00ff00";

        easyUi.Add<EasyUI_Layout>(layout =>
        {
          layout.Style.Width = scrollPane.ContentWidth() - 5;
          layout.Style.Height = 10;
          layout.Gap = 5;
          layout.Style.X = 2;
          layout.Style.Y = 2;
          //layout.Style.BorderWidth = 1;
          //layout.Style.BorderColor = "#fe0000";

          // Кнопка
          EasyUI_Button button = null;
          easyUi.Add<EasyUI_Button>(t =>
          {
            t.Style.X = 0;
            t.Style.Y = 0;
            t.Text = "Click";
            button = t;

            t.Events.OnMouseOver += () => { Console.WriteLine("A"); };
            t.Events.OnMouseOut += () => { Console.WriteLine("B"); };
          });

          easyUi.Add<EasyUI_VectorInput>(vi => { vi.Style.Width = 128; });

          easyUi.Label("Int");
          easyUi.Add<EasyUI_TextInput>(input =>
          {
            input.Style.X = 0;
            input.Style.Y = 32;
            input.Style.Width = 128;
            input.InputType = TextInputType.Integer;
          });

          easyUi.Label("Float");
          easyUi.Add<EasyUI_TextInput>(input =>
          {
            input.Style.X = 0;
            input.Style.Y = 32 + 24 + 2;
            input.Style.Width = 128;
            input.InputType = TextInputType.Float;
          });

          easyUi.Label("Text");
          easyUi.Add<EasyUI_TextInput>(input =>
          {
            input.Style.X = 0;
            input.Style.Y = 32 + 24 + 24 + 2;
            input.Style.Width = 128;
            input.InputType = TextInputType.Text;
            input.Events.OnChange += o => { button.Text = $"{o}"; };
          });

          easyUi.Label("Gaayyy");
          easyUi.Add<EasyUI_Check>(check => { });

          easyUi.Label("Furry sex");
          easyUi.Add<EasyUI_Slider>(check => { });

          easyUi.Label("Text XX");
          easyUi.Add<EasyUI_ScrollPane>(scrollPane2 =>
          {
            scrollPane2.Style.SetArea(0, 0, 128, 128);

            easyUi.Add<EasyUI_Layout>(layout2 =>
            {
              layout2.Style.Width = scrollPane2.ContentWidth();
              layout2.Gap = 5;
              layout2.Style.Y = 0;

              for (var i = 0; i < 32; i++)
              {
                easyUi.Label($"Text {i}");
              }

              easyUi.Label("Text Rock");

              easyUi.Label("Text XX");
              easyUi.Add<EasyUI_ScrollPane>(scrollPane3 =>
              {
                scrollPane3.Style.SetArea(0, 0, 128 - 32, 128);

                easyUi.Add<EasyUI_Layout>(layout3 =>
                {
                  layout3.Style.Width = scrollPane3.ContentWidth();
                  layout3.Gap = 5;
                  layout3.Style.Y = 0;

                  for (var i = 0; i < 32; i++)
                  {
                    easyUi.Label($"Text {i}");
                  }

                  easyUi.Label("Text Rock");

                  easyUi.Label("Int");
                  easyUi.Add<EasyUI_TextInput>(input =>
                  {
                    input.Style.X = 0;
                    input.Style.Y = 32;
                    input.InputType = TextInputType.Integer;
                  });
                  easyUi.Label("Int");
                });
              });
            });
          });

          easyUi.Label("Text Sex");
          easyUi.Label("Text");
          easyUi.Label("Text");
        });
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