using System;
using System.Threading.Tasks;
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
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.RenderObject;
using MegaTest.Render;
using NUnit.Framework;

namespace MegaTest;

internal class TestScene : Render_Scene
{
  private float _time;
  private float _x;

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
    AddLayer("dynamicPoint", new Layer_Point() { });
    AddLayer("dynamicLine", new Layer_Line() { });
  }

  public override void OnBeforeUpdate(float delta)
  {
    _time += delta;
    _x = (float)Math.Sin(_time) * 1;

    Camera.BasicMovement(delta);
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

    Box();
  }

  private void Sphere()
  {
    var line = GetLayer<Layer_Line>("dynamicLine");
    var sc = new SphereCollider();
    sc.Radius = 1f / 2f;
    sc.Transform.Position = new Vector3(-_x, 0, 0);
    sc.Transform.Rotation = Quaternion.FromEuler(0, _time * 45f, 0, "deg");
    line.Draw(sc, new RGBA<float>(0, 1, 0, 1));

    var ray = new Ray(new Vector3(_x, 0, -1), new Vector3(_x, 0, 1));
    sc.RayIntersection(ray, out var point, out var isHit);
    if (isHit)
    {
      ray = new Ray(new Vector3(_x, 0, -1), point);
      line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
      Add("dynamicPoint", new RO_Point()
      {
        Position = point,
        Size = 8,
        Color = new RGBA<float>(0, 1, 1, 1)
      });
    }
    else
    {
      line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
    }
  }

  private void Box()
  {
    var line = GetLayer<Layer_Line>("dynamicLine");

    var box = new BoxCollider();
    box.Size = new Vector3(1, 1, 1);
    box.Transform.Position = new Vector3(-_x, 0, 0);
    box.Transform.Rotation = Quaternion.FromEuler(0, _time * 45f, 0, "deg");
    line.Draw(box, new RGBA<float>(0, 1, 0, 1));

    var ray = new Ray(new Vector3(_x, 0, -1), new Vector3(_x, 0, 1));
    box.RayIntersection(ray, out var point, out var isHit);
    if (isHit)
    {
      ray = new Ray(new Vector3(_x, 0, -1), point);
      line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
      Add("dynamicPoint", new RO_Point()
      {
        Position = point,
        Size = 8,
        Color = new RGBA<float>(0, 1, 0, 1)
      });
    }
    else
    {
      line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
    }
  }
}

public class PhysicsTest
{
  [Test]
  public void TestBasic()
  {
    var renderer = new OpenGL_Renderer();
    var scene = new TestScene();

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