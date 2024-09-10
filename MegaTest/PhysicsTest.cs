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
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaTest.Render;
using NUnit.Framework;

namespace MegaTest;

internal class TestScene : Render_Scene
{
  private float _time;
  private float _x = -1;
  private float _scaleX = 1;
  private VerletPoint _vp = new(new Vector3(0, 1, 0), 1.0f);
  private VerletPoint _vp2 = new(new Vector3(), 1.0f);
  private DistanceConstraint _constraint;
  private VerletLine _vl;
  private SphereCollider _sphereCollider = new();
  private BoxCollider _boxCollider = new();

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
    AddLayer("dynamicPoint", new Layer_Point() { });
    AddLayer("dynamicLine", new Layer_Line() { LineWidth = 2f });

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

      //t.Events.OnMouseOver = () => { t.Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1); };
      //t.Events.OnMouseOut = () => { t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1); };

      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1) * 1.2f;
        t.Events.OnClick = () => { _scaleX += 0.1f; };
      });
      imgui.Add<IMGUI_Element>(t =>
      {
        t.Style.Width = 80;
        t.Style.Height = 30;
        t.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1) * 1.2f;
        t.Events.OnClick = () => { _scaleX -= 0.1f; };
      });
    });

    _vp.IsStatic = true;
    _constraint = new DistanceConstraint(_vp, _vp2, 0);
    _sphereCollider = new SphereCollider();
    _sphereCollider.Radius = 0.5f;

    _boxCollider = new BoxCollider();
    _boxCollider.Size = new Vector3(1, 1, 1);

    _vl = new VerletLine(new Vector3(0, 1, 0), new Vector3(0, -1, 0), 32, 1, true, false);
  }

  public override void OnBeforeUpdate(float delta)
  {
    _time += delta;
    // _x = (float)Math.Sin(_time / 2f) * 1;
    // _scaleX = Math.Abs((float)Math.Sin(_time) * 2).Clamp(0.2f, 1);

    Camera.BasicMovement(delta);

    if (Keyboard.IsKeyDown(KeyboardKey.I)) _x -= delta;
    if (Keyboard.IsKeyDown(KeyboardKey.O)) _x += delta;

    VerletLine(delta);
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

  private void VerletPoint(float delta)
  {
    var point = GetLayer<Layer_Point>("dynamicPoint");

    point.Draw(_vp, new RGBA<float>(1, 0, 0, 1), 4);
    point.Draw(_vp2, new RGBA<float>(1, 0, 0, 1), 4);

    _vp.Position = new Vector3(_x, 1, 0);
    _vp2.ApplyForce(new Vector3(0, -9.8f, 0));

    _vp2.Tick(delta);
    _constraint.Tick();
  }

  private void VerletLine(float delta)
  {
    var layerLine = GetLayer<Layer_Line>("dynamicLine");


    _vl.Start.Position = new Vector3(_x, 1, 0);
    _vl.ApplyForce(new Vector3(0, -9.8f, 0));
    _vl.Tick(delta);

    _sphereCollider.Transform.Scale = new Vector3(_scaleX, 1, 1);
    // _boxCollider.Transform.Rotation = Quaternion.FromEuler(0, _time * 45f, 0, "deg");

    for (var i = 0; i < _vl.Points.Count; i++)
    {
      _vl.Points[i].Color = new RGBA<float>(1, 0, 0, 1);

      //_boxCollider.ResolveCollision(_vl.Points[i]);
      //_boxCollider.ResolveCollision(_vl.Points[i]);

      _sphereCollider.ResolveCollision(_vl.Points[i]);

      /*var ray = new Ray(_vl.Points[i].PreviousPosition, _vl.Points[i].Position);
      _sphereCollider.RayIntersection(ray, out var hitPoint, out var isHit);
      if (isHit)
      {
        _vl.Points[i].Position = hitPoint;
        _vl.Points[i].PreviousPosition = hitPoint;
      }*/

      /*_sphereCollider.PointIntersection(_vl.Points[i].Position, out var isHit);
      if (isHit)
      {
        _vl.Points[i].Color = new RGBA<float>(0, 1, 1, 1);
      }
      else
      {
        _vl.Points[i].Color = new RGBA<float>(1, 0, 0, 1);
      }*/
    }

    layerLine.Draw(_sphereCollider, new RGBA<float>(0, 1, 0, 1));
    layerLine.Draw(_vl);

    /*_vp.Position = new Vector3(_x, 1, 0);
    _vp2.ApplyForce(new Vector3(0, -9.8f, 0));

    _vp2.Tick(delta);
    _constraint.Tick();*/
  }

  private void Sphere()
  {
    var line = GetLayer<Layer_Line>("dynamicLine");
    var sc = new SphereCollider();
    sc.Radius = 1f / 2f;
    sc.Transform.Position = new Vector3(-_x, 0, 0);
    sc.Transform.Scale = new Vector3(_scaleX, 1, 1);
    sc.Transform.Rotation = Quaternion.FromEuler(_time * 45f, _time * 45f, _time * 45f, "deg");
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
    box.Transform.Scale = new Vector3(_scaleX, 1, 1);
    box.Transform.Rotation = Quaternion.FromEuler(_time * 45f, _time * 45f, 0, "deg");
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