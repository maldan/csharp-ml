using System;
using System.Threading.Tasks;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Physics;
using MegaLib.Physics.Collider;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;
using MegaLib.Render.Renderer.OpenGL;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.UI.EasyUI;
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

  private PhysicsWorld _physicsWorld = new();
  // private RigidBody _rigidBody;

  private float _cameraOrbitRadius = 10f;
  private Vector3 _cameraFocalPoint = new();

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
    AddLayer("easyui", new Layer_EasyUI()
    {
      Camera = new Camera_Orthographic()
    });

    var easyUi = GetLayer<Layer_EasyUI>();
    easyUi.Add<EasyUI_Element>(t =>
    {
      t.Style.BackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1);
      t.Style.SetArea(10, 10, 240, 120);

      easyUi.Add<EasyUI_Slider>(t =>
      {
        t.Value = 0;
        t.Min = -1;
        t.Max = 1;
        t.Events.OnChange = o =>
        {
          if (o is float f) _physicsWorld.RigidBodies[0].Position = new Vector3(0, f, 0);
        };
      });

      easyUi.Add<EasyUI_Slider>(t =>
      {
        t.Style.Y = 32;
        t.Value = 0;
        t.Min = -45;
        t.Max = 45;
        t.Events.OnChange = o =>
        {
          if (o is float f)
            _physicsWorld.RigidBodies[2].Rotation = Quaternion.FromEuler(f, 0, 0, "deg");
        };
      });

      easyUi.Add<EasyUI_Button>(t =>
      {
        t.Text = "FUCK1";
        t.Style.Y = 60;
        t.Events.OnClick += () =>
        {
          //_physicsWorld.RigidBodies[0].AddImpulse(new Vector3(3f, 0, 0));
          _physicsWorld.RigidBodies[0].ApplyForceAtPoint(new Vector3(300f, 0, 0), new Vector3(0, 0, 0.4f));
        };
      });

      easyUi.Add<EasyUI_Button>(t =>
      {
        t.Text = "FUCK2";
        t.Style.Y = 90;
        t.Events.OnClick += () =>
        {
          // _physicsWorld.RigidBodies[1].ApplyForceAtPoint(new Vector3(-300f, 0, 0), new Vector3(0, 0, 0.4f));
          _physicsWorld.RigidBodies[1].AddImpulse(new Vector3(-3f, 0, 0));
        };
      });

      /*t.Events.OnRender = (delta) =>
      {
        t.Text = string.Join("\n", [
          $"Pos: {_rigidBody.Position}",
          $"Rot: {_rigidBody.Rotation}",
          $"Vel: {_rigidBody.Velocity}",
          $"AngVel: {_rigidBody.AngularVelocity}",
          "2"
        ]);
      };*/

      /*easyUi.Add<EasyUI_Button>(t =>
      {
        t.Text = "FUCK";
        t.Style.Y = 100;
        t.Events.OnClick += () => { _rigidBody.ApplyForceAtPoint(new Vector3(0, 0, 10f), new Vector3(-0.5f, 0, 0)); };
      });*/

      //
    });

    _vp.IsStatic = true;
    _constraint = new DistanceConstraint(_vp, _vp2, 0);
    _sphereCollider = new SphereCollider();
    _sphereCollider.Radius = 0.5f;

    _boxCollider = new BoxCollider();
    _boxCollider.Size = new Vector3(1, 1, 1);

    _vl = new VerletLine(new Vector3(0, 1, 0), new Vector3(0, -1, 0), 32, 1, true, false);

    //_rigidBody = new RigidBody(1);
    //_rigidBody.UseGravity = false;

    var rb1 = new RigidBody(1);
    rb1.UseGravity = true;
    var sc1 = new SphereCollider();
    rb1.Position = new Vector3(0, 1, 0);
    sc1.Radius = 0.5f;
    rb1.Colliders.Add(sc1);
    _physicsWorld.Add(rb1);

    /*var rb2 = new RigidBody(1);
    rb2.Position = new Vector3(0.2f, 2, 0);
    rb2.UseGravity = true;
    var sc2 = new SphereCollider();
    sc2.Radius = 0.5f;
    rb2.Colliders.Add(sc2);
    _physicsWorld.Add(rb2);*/

    var rb3 = new RigidBody(1);
    rb3.IsKinematic = true;
    rb3.SetMass(0);
    rb3.Rotation = Quaternion.FromEuler(0, 0, 0, "deg");
    var sc3 = new PlaneCollider();
    rb3.Colliders.Add(sc3);
    _physicsWorld.Add(rb3);
  }

  public override void OnBeforeUpdate(float delta)
  {
    _time += delta;
    // _x = (float)Math.Sin(_time / 2f) * 1;
    // _scaleX = Math.Abs((float)Math.Sin(_time) * 2).Clamp(0.2f, 1);

    Camera.OrbitalCameraMovement(delta, ref _cameraOrbitRadius, ref _cameraFocalPoint);

    if (Keyboard.IsKeyDown(KeyboardKey.I)) _x -= delta;
    if (Keyboard.IsKeyDown(KeyboardKey.O)) _x += delta;

    Sphere2(delta);
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

    var line = GetLayer<Layer_Line>("dynamicLine");
    line.DrawGrid(4, 1f, new RGBA<float>(1, 1, 1, 0.5f), 1, new RGBA<float>(1, 1, 1, 0.25f));
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
      //_vl.Points[i].Color = new RGBA<float>(1, 0, 0, 1);

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

    //layerLine.Draw(_sphereCollider, new RGBA<float>(0, 1, 0, 1));
    //layerLine.Draw(_vl);

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
      //line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
      GetLayer<Layer_Point>("dynamicPoint").Add(new RO_Point()
      {
        Position = point,
        Size = 8,
        Color = new RGBA<float>(0, 1, 1, 1)
      });
    }
    else
    {
      //line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
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
    line.DrawBoxCollider(box, new RGBA<float>(0, 1, 0, 1));

    var ray = new Ray(new Vector3(_x, 0, -1), new Vector3(_x, 0, 1));
    box.RayIntersection(ray, out var point, out var isHit);
    if (isHit)
    {
      ray = new Ray(new Vector3(_x, 0, -1), point);
      //line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
      GetLayer<Layer_Point>("dynamicPoint").Add(new RO_Point()
      {
        Position = point,
        Size = 8,
        Color = new RGBA<float>(0, 1, 0, 1)
      });
    }
    else
    {
      //line.Draw(ray, new RGBA<float>(1, 0, 0, 1));
    }
  }

  private void Sphere2(float delta)
  {
    var line = GetLayer<Layer_Line>("dynamicLine");
    var pointLayer = GetLayer<Layer_Point>("dynamicPoint");

    _physicsWorld.Update(delta);

    //_rigidBody.ApplyForce(new Vector3(0, -1, 0), new Vector3(-0.4f, 0.4f, 0));

    // _rigidBody.Update(delta);

    // Console.WriteLine(_rigidBody.Position);

    //Console.WriteLine(_rigidBody.AngularVelocity);

    /*var sc = new SphereCollider();
    sc.Radius = 1f / 2f;
    sc.Transform.Position = _rigidBody.Position;
    sc.Transform.Scale = new Vector3(1, 1, 1);
    sc.Transform.Rotation = _rigidBody.Rotation;
    line.Draw(sc, new RGBA<float>(0, 1, 0, 1));*/

    foreach (var body in _physicsWorld.RigidBodies)
    {
      line.DrawRigidBody(body, new RGBA<float>(1, 0, 0, 1));
    }

    foreach (var collisionData in _physicsWorld.Collisions)
    {
      pointLayer.Draw(collisionData.ContactPoint, new RGBA<float>(1, 0, 0, 1), 8f);
    }

    /*var line = GetLayer<Layer_Line>("dynamicLine");
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
    }*/
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
      OnPaint = (win, delta) =>
      {
        if (win.IsFocused) Keyboard.Update();
        else Keyboard.ResetAll();
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