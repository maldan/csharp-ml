using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MegaLib.Render.Buffer;

internal static class ListGPUId
{
  private static ulong _nextId = 1;
  private static readonly Mutex Mutex = new();

  public static ulong NextId()
  {
    ulong id;
    Mutex.WaitOne();
    try
    {
      id = _nextId;
      _nextId += 1;
    }
    finally
    {
      Mutex.ReleaseMutex();
    }

    return id;
  }
}

public class ListGPU<T> : IEnumerable<T>
{
  public ulong Id { get; private set; }
  public bool IsChanged { get; private set; }

  private T[] _array;
  private int _capacity;
  private int _count;

  public Action<T[]> OnSync;
  public event EventHandler<ulong> OnDestroy;

  public ListGPU()
  {
    _capacity = 4;
    _array = new T[_capacity];
    _count = 0;
    Id = ListGPUId.NextId();
  }

  public ListGPU(int capacity)
  {
    _capacity = capacity;
    _array = new T[_capacity];
    _count = 0;
    Id = ListGPUId.NextId();
  }

  public ListGPU(List<T> arr)
  {
    _capacity = arr.Capacity;
    _array = arr.ToArray();
    _count = arr.Count;
    IsChanged = true;
    Id = ListGPUId.NextId();
  }

  ~ListGPU()
  {
    OnDestroy?.Invoke(this, Id);
  }

  public void Add(T item)
  {
    if (_count == _capacity)
    {
      _capacity *= 2;
      Array.Resize(ref _array, _capacity);
    }

    _array[_count++] = item;
    IsChanged = true;
  }

  public T this[int index]
  {
    get
    {
      if (index < 0 || index >= _count) throw new IndexOutOfRangeException();

      return _array[index];
    }
    set
    {
      if (index < 0 || index >= _count) throw new IndexOutOfRangeException();

      _array[index] = value;
      IsChanged = true;
    }
  }

  public int Count => _count;

  public void Clear()
  {
    Array.Clear(_array, 0, _count);
    _count = 0;
    IsChanged = true;
  }

  public IEnumerator<T> GetEnumerator()
  {
    for (var i = 0; i < _count; i++) yield return _array[i];
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  public void Sync()
  {
    if (!IsChanged) return;
    //var handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
    OnSync?.Invoke(_array);
    IsChanged = false;
    //handle.Free();
  }
}