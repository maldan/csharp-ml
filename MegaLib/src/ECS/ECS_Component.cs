using System;
using System.Runtime.InteropServices;
using MegaLib.Asm;

namespace MegaLib.ECS;

public delegate void ECS_RefAction<T1>(ref T1 item, ECS_Entity entity);

public delegate void ECS_RefAction<T1, T2>(ref T1 item1, ref T2 item2, ECS_Entity entity);

public delegate void ECS_RefAction<T1, T2, T3>(ref T1 item1, ref T2 item2, ref T3 item3, ECS_Entity entity);

public class ECS_ComponentChunk
{
  private int _elementSize;
  private int _capacity;
  private IntPtr _bufferPtr;
  private int _bufferSize;

  public int Count { get; private set; }

  private AsmRuntime.MemCopyDelegate _memcpy;

  public ECS_ComponentChunk(Type t, int capacity)
  {
    _elementSize = Marshal.SizeOf(t);
    _capacity = capacity;
    _bufferSize = _capacity * _elementSize;
    _bufferPtr = Marshal.AllocHGlobal(_capacity * _elementSize);

    _memcpy = AsmRuntime.MemCopy();
  }

  private void Resize()
  {
    _capacity *= 2;

    var oldBufferPtr = _bufferPtr;
    var newBuffer = Marshal.AllocHGlobal(_capacity * _elementSize);
    _memcpy(newBuffer, oldBufferPtr, _bufferSize);

    // Set new buffer size
    _bufferSize = _capacity * _elementSize;

    // Remove old
    Marshal.FreeHGlobal(oldBufferPtr);
  }

  public void Add()
  {
    if (Count == _capacity) Resize();
    Count++;
  }

  public void Add(byte[] data)
  {
    if (Count == _capacity) Resize();
    SetRaw(Count, data);
    Count++;
  }

  public void Remove(int index)
  {
    if (Count == 1)
    {
      Count -= 1;
      return;
    }

    var src = _bufferPtr + index * _elementSize;
    var last = _bufferPtr + (Count - 1) * _elementSize;

    _memcpy(src, last, _elementSize);
    Count -= 1;
  }

  public byte[] GetRaw(int index)
  {
    var bb = new byte[_elementSize];
    Marshal.Copy(_bufferPtr + index * _elementSize, bb, 0, _elementSize);
    return bb;
  }

  public void SetRaw(int index, byte[] value)
  {
    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(value, 0);
    _memcpy(_bufferPtr + index * _elementSize, ptr, _elementSize);
  }

  public unsafe void SetRaw<T>(int index, T cmp) where T : unmanaged
  {
    var ptr = (nint)(&cmp);
    _memcpy(_bufferPtr + index * _elementSize, ptr, _elementSize);
  }

  public unsafe T* GetPointer<T>(int index) where T : unmanaged
  {
    return (T*)(_bufferPtr + index * _elementSize);
  }

  public unsafe ref T Get<T>(int index) where T : unmanaged
  {
    return ref *(T*)(_bufferPtr + index * _elementSize);
  }

  /*public unsafe void ForEach<T>(ECS_RefAction<T> fn) where T : unmanaged
  {
    for (var i = 0; i < Count; i++)
    {
      var p = GetPointer<T>(i);
      fn(ref *p, i);
    }
  }*/
}