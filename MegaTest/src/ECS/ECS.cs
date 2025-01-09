using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

public delegate void RefAction<T>(ref T item, int index);

public class ComponentChunk
{
  private int _elementSize;
  private int _count;
  private int _capacity;
  private IntPtr _bufferPtr;

  public ComponentChunk(Type t, int capacity)
  {
    _elementSize = Marshal.SizeOf(t);
    _capacity = capacity;

    // Allocate unmanaged memory
    _bufferPtr = Marshal.AllocHGlobal(capacity * _elementSize);
  }

  public unsafe T* Get<T>(int index) where T : struct
  {
    return (T*)(_bufferPtr + index * _elementSize);
  }

  public unsafe void ForEach<T>(RefAction<T> fn) where T : struct
  {
    for (var i = 0; i < _capacity; i++)
    {
      var p = Get<T>(i);
      fn(ref *p, i);
    }
  }

  /*public unsafe void It<T>(Action<T> fn) where T : struct
  {
    for (var i = 0; i < _capacity; i++)
    {
      var p = Get<T>(i);
      fn(p);
    }
  }*/
  // public float this[int index] { }
}

public class Arche
{
  public ulong Id;
  public Dictionary<Type, ComponentChunk> Components = new();

  public void CreateChunk(Type t)
  {
    Components.Add(t, new ComponentChunk(t, 1_000_000));
  }

  public ComponentChunk Get(Type t)
  {
    return Components[t];
  }
}

public static class ComponentTypeRegistry
{
  private static Dictionary<Type, int> _typeToBit = new();
  private static int _nextBit = 0;

  public static int GetBit(Type type)
  {
    if (!_typeToBit.TryGetValue(type, out var bit))
    {
      bit = _nextBit++;
      _typeToBit[type] = bit;
    }

    return bit;
  }

  public static ulong GetMask(params Type[] componentTypes)
  {
    ulong mask = 0;
    foreach (var type in componentTypes)
    {
      mask |= 1UL << GetBit(type);
    }

    return mask;
  }
}

public class ECSTest
{
  [Test]
  public unsafe void IterateUnmanaged()
  {
    var t = new Stopwatch();
    t.Start();
    var size = 1000000;
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
    var t = new Stopwatch();
    var at = CreateArchetype(typeof(Transform), typeof(Gay), typeof(Vector3));
    Console.WriteLine(at.Id);
    var transformChunk = at.Get(typeof(Transform));

    t.Start();
    var totalX = 0f;
    var cnt = 0;
    transformChunk.ForEach((ref Transform vv, int index) => { X(ref vv, index); });
    transformChunk.ForEach((ref Transform vv, int index) =>
    {
      totalX += vv.Position.X;
      //vv.Position.X = index;
      // vv.Position.Y = index;
      //vv.Position.Z = index;
      cnt++;
    });
    t.Stop();
    Console.WriteLine(t.ElapsedTicks);
    Console.WriteLine($"MS {t.ElapsedMilliseconds}");
    Console.WriteLine(totalX);
    Console.WriteLine(cnt);
    // transformChunk.ForEach((ref Transform vv, int index) => { Console.WriteLine(vv.Position); });
  }

  public Arche CreateArchetype(params Type[] componentTypes)
  {
    var archetype = new Arche();

    for (var i = 0; i < componentTypes.Length; i++)
    {
      archetype.Id |= ComponentTypeRegistry.GetMask(componentTypes[i]);
      archetype.CreateChunk(componentTypes[i]);
      // Console.WriteLine($"{componentTypes[i]} - {ComponentTypeRegistry.GetMask(componentTypes[i])}");
    }

    return archetype;
  }
}