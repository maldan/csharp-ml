using System;
using System.Runtime.InteropServices;

namespace MegaLib.Render.Buffer
{
  public class ImageGPU<T>
  {
    private static ulong _nextId = 1;
    public ulong Id { get; private set; }

    public ushort Width { get; private set; }
    public ushort Height { get; private set; }
    public bool IsChanged { get; private set; }

    private T[] _pixelData;

    public event EventHandler<ulong> OnDestroy;
    public Action<T[]> OnSync;

    public ImageGPU(int width, int height)
    {
      Id = _nextId++;
      Width = (ushort)width;
      Height = (ushort)height;
      _pixelData = new T[width * height];
    }

    ~ImageGPU()
    {
      OnDestroy?.Invoke(this, Id);
    }

    public T this[int x, int y]
    {
      get
      {
        var index = (y * Width + x) * 1;
        return _pixelData[index];
      }
      set
      {
        var index = (y * Width + x) * 1;
        if (index < 0) return;
        if (index > _pixelData.Length - 1) return;
        _pixelData[index] = value;
        IsChanged = true;
      }
    }

    public T this[int index]
    {
      get => _pixelData[index];
      set
      {
        _pixelData[index] = value;
        IsChanged = true;
      }
    }

    public void SetPixels(T[] list)
    {
      Array.Copy(list, _pixelData, list.Length);
      IsChanged = true;
    }

    public void Sync()
    {
      OnSync?.Invoke(_pixelData);
      IsChanged = false;
    }
  }
}