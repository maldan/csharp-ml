using System;
using System.Collections;
using System.Collections.Generic;

namespace MegaLib.Render.Buffer
{
  public class ListGPU<T> : IEnumerable<T>
  {
    private static ulong _nextId = 1;

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
      Id = _nextId++;
    }

    public ListGPU(int capacity)
    {
      _capacity = capacity;
      _array = new T[_capacity];
      _count = 0;
      Id = _nextId++;
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
        if (index < 0 || index >= _count)
        {
          throw new IndexOutOfRangeException();
        }

        return _array[index];
      }
      set
      {
        if (index < 0 || index >= _count)
        {
          throw new IndexOutOfRangeException();
        }

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
      for (var i = 0; i < _count; i++)
      {
        yield return _array[i];
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Sync()
    {
      OnSync?.Invoke(_array);
      IsChanged = false;
    }
  }
}