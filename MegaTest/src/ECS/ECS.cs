using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MegaLib.ECS;
using MegaLib.Mathematics.LinearAlgebra;
using NUnit.Framework;

namespace MegaTest.ECS;

public struct Transform
{
  public Vector3 Position;
  public Quaternion Rotation;
  public Vector3 Scale;

  public override string ToString()
  {
    return $"P: {Position}, R: {Rotation}, S: {Scale}";
  }
}

public struct Gay
{
  public Vector4 Position;
}

public class ECSTest
{
  [Test]
  public unsafe void IterateUnmanaged()
  {
    var t = new Stopwatch();
    t.Start();
    var size = 1_000_000;
    var elementSize = Marshal.SizeOf<Transform>();

    // Allocate unmanaged memory
    var unmanagedMemory = Marshal.AllocHGlobal(size * elementSize);

    for (var i = 0; i < size; i++)
    {
      var charPtr = (Transform*)(unmanagedMemory + i * elementSize);
      X(ref *charPtr, i);
    }

    var totalX = 0f;
    for (var i = 0; i < size; i++)
    {
      var charPtr = (Transform*)(unmanagedMemory + i * elementSize);
      totalX += (*charPtr).Position.X;
    }

    t.Stop();
    Console.WriteLine(t.ElapsedTicks);
    Console.WriteLine(totalX);
  }

  [Test]
  public void IterateNative()
  {
    var t = new Stopwatch();
    t.Start();
    var size = 1_000_000;

    // Allocate unmanaged memory
    var unmanagedMemory = new Transform[size];

    for (var i = 0; i < size; i++)
    {
      X(ref unmanagedMemory[i], i);
    }

    var totalX = 0f;
    for (var i = 0; i < size; i++)
    {
      totalX += unmanagedMemory[i].Position.X;
    }

    t.Stop();
    Console.WriteLine(t.ElapsedTicks);
    Console.WriteLine($"MS {t.ElapsedMilliseconds}");
    Console.WriteLine(totalX);
  }

  public void X(ref Transform t, int index)
  {
    t.Position = new Vector3(index, 5, 5);
  }

  [Test]
  public void Sus()
  {
    var world = new ECS_World();

    var t = new Stopwatch();
    var at = world.CreateArchetype(typeof(Transform), typeof(Gay), typeof(Vector3));
    var ee = world.CreateEntity(at);

    ee.GetComponentData<Transform>().Position = new Vector3(1, 2, 3);
    Console.WriteLine(ee.GetComponentData<Transform>().Position);

    // at.Get(typeof(Transform)).Get<Transform>(0).Position = Vector3.One;

    Console.WriteLine(at.Mask);
    var transformChunk = at.GetComponentChunk(typeof(Transform));

    t.Start();
    var totalX = 0f;
    var cnt = 0;

    transformChunk.Get<Transform>(0).Position.X = 228;
    transformChunk.Get<Transform>(1).Position.X = 228;

    /*transformChunk.ForEach((ref Transform vv, int index) => { X(ref vv, index); });
    transformChunk.ForEach((ref Transform vv, int index) =>
    {
      totalX += vv.Position.X;
      cnt++;
    });*/

    t.Stop();
    Console.WriteLine(t.ElapsedTicks);
    Console.WriteLine($"MS {t.ElapsedMilliseconds}");
    Console.WriteLine(totalX);
    Console.WriteLine(cnt);
    //transformChunk.ForEach((ref Transform vv, int index) => { Console.WriteLine(vv.Position); });
  }

  [Test]
  public void Sus2()
  {
    var world = new ECS_World();

    var at1 = world.CreateArchetype(typeof(Transform), typeof(Gay), typeof(Vector3));
    var at2 = world.CreateArchetype(typeof(Transform));
    var ee1 = world.CreateEntity(at1);
    var ee2 = world.CreateEntity(at2);

    ee1.GetComponentData<Transform>().Position = new Vector3(1, 2, 3);

    world.ForEach((ref Transform vv, ref Gay g, ECS_Entity e) =>
    {
      vv.Position.X = 1;
      vv.Position.Y = 2;
      vv.Position.Z = 3;
      g.Position = new Vector4(5, 5, 6, 7);
    });
    world.ForEach(
      (ref Transform vv, ref Gay g, ECS_Entity e) => { Console.WriteLine($"{e} - {vv.Position} {g.Position}"); });
    Console.WriteLine("----");
    world.ForEach(
      (ref Transform vv, ECS_Entity e) => { Console.WriteLine($"{e} - {vv.Position}"); });

    //world.DestroyEntity(ee1);
    //world.DestroyEntity(ee2);
    ee1.Destroy();
    ee2.Destroy();

    Console.WriteLine("----");
    world.ForEach(
      (ref Transform vv, ECS_Entity e) => { Console.WriteLine($"{e} - {vv.Position}"); });
  }

  [Test]
  public void Sus3()
  {
    var world = new ECS_World();

    var at1 = world.CreateArchetype(typeof(Transform), typeof(Gay), typeof(Vector3));
    var ee1 = world.CreateEntity(at1);

    ee1.GetComponentData<Transform>().Position = new Vector3(1, 2, 3);

    var bytes = at1.GetComponentChunk(typeof(Transform)).GetRaw(0);
    ee1.GetComponentData<Transform>().Position = new Vector3(5, 5, 5);
    Console.WriteLine(ee1.GetComponentData<Transform>().Position);
    at1.GetComponentChunk(typeof(Transform)).SetRaw(0, bytes);
    Console.WriteLine(ee1.GetComponentData<Transform>().Position);

    at1.GetComponentChunk(typeof(Transform)).SetRaw(0, new Transform() { Position = new Vector3(4, 8, 16) });
    Console.WriteLine(ee1.GetComponentData<Transform>().Position);
  }

  [Test]
  public void Sus4()
  {
    var world = new ECS_World();

    var at1 = world.CreateArchetype(typeof(Transform));
    var at2 = world.CreateArchetype(typeof(Transform), typeof(Gay));
    var ee1 = world.CreateEntity(at1);

    ee1.GetComponentData<Transform>().Position = new Vector3(1, 2, 3);
    Console.WriteLine(ee1.Archetype.Mask);
    ee1.AddComponent(typeof(Gay));
    Console.WriteLine(ee1.Archetype.Mask);

    Console.WriteLine($"A - {at1.GetComponentChunk(typeof(Transform)).Count}");
    Console.WriteLine($"B - {at2.GetComponentChunk(typeof(Transform)).Count}");

    Console.WriteLine(ee1.GetComponentData<Transform>().Position);
  }
}