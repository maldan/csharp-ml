using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaLib.Asm;

namespace MegaLib.ECS;

public delegate void RefAction<T>(ref T item, int index);

public class ECS_ComponentChunk
{
  private int _elementSize;
  private int _capacity;
  private IntPtr _bufferPtr;
  private int _bufferSize;

  public int Count { get; private set; }

  public ECS_ComponentChunk(Type t, int capacity)
  {
    _elementSize = Marshal.SizeOf(t);
    _capacity = capacity;
    _bufferSize = _capacity * _elementSize;
    _bufferPtr = Marshal.AllocHGlobal(_capacity * _elementSize);
  }

  private void Resize()
  {
    var newBuffer = Marshal.AllocHGlobal(_capacity * _elementSize);
    var memcpy = AsmRuntime.MemCopy();
    memcpy(_bufferPtr, newBuffer, _bufferSize);
    _bufferSize = _capacity * _elementSize;
  }

  public void Add()
  {
    if (Count == _capacity)
    {
      _capacity *= 2;
      Resize();
    }

    Count++;
  }

  private unsafe T* GetPointer<T>(int index) where T : unmanaged
  {
    return (T*)(_bufferPtr + index * _elementSize);
  }

  public unsafe ref T Get<T>(int index) where T : unmanaged
  {
    var tt = GetPointer<T>(index);
    return ref *tt;
  }

  public unsafe void ForEach<T>(RefAction<T> fn) where T : unmanaged
  {
    for (var i = 0; i < _capacity; i++)
    {
      var p = GetPointer<T>(i);
      fn(ref *p, i);
    }
  }
}