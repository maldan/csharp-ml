using System;
using System.Runtime.InteropServices;
using System.Threading;
using MegaLib.Asm;
using MegaLib.Render.Color;

namespace MegaLib.Render.Buffer;

internal static class ImageGPUId
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

public class ImageGPU<T>
{
  public ulong Id { get; private set; }

  public ushort Width { get; private set; }
  public ushort Height { get; private set; }
  public bool IsChanged { get; private set; }

  private T[] _pixelData;

  public event EventHandler<ulong> OnDestroy;
  public Action<T[]> OnSync;

  public ImageGPU(int width, int height)
  {
    Id = ImageGPUId.NextId();
    Width = (ushort)width;
    Height = (ushort)height;
    _pixelData = new T[width * height];
  }

  ~ImageGPU()
  {
    OnDestroy?.Invoke(this, Id);
  }

  public void Resize(int width, int height)
  {
    Width = (ushort)width;
    Height = (ushort)height;
    _pixelData = new T[width * height];
    IsChanged = true;
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

  /*public byte[] GetBytes()
  {
    if (_pixelData is RGB<byte>[] p)
    {
      var b = new byte[Width * Height * 3];
      var pp = 0;
      for (var i = 0; i < p.Length; i++)
      {
        b[pp++] = p[i].R;
        b[pp++] = p[i].G;
        b[pp++] = p[i].B;
      }

      return b;
    }

    if (_pixelData is RGBA<byte>[] p2)
    {
      var b = new byte[Width * Height * 4];
      var pp = 0;
      for (var i = 0; i < p2.Length; i++)
      {
        b[pp++] = p2[i].R;
        b[pp++] = p2[i].G;
        b[pp++] = p2[i].B;
        b[pp++] = p2[i].A;
      }

      return b;
    }

    return null;
  }*/

  public void SetPixels(T[] list)
  {
    Array.Copy(list, _pixelData, list.Length);
    IsChanged = true;
  }

  public void SetRaw(IntPtr ptr, int size)
  {
    var memcpy = AsmRuntime.MemCopy();
    memcpy(Marshal.UnsafeAddrOfPinnedArrayElement(_pixelData, 0), ptr, size);
  }

  public void Sync()
  {
    if (!IsChanged) return;

    //var handle = GCHandle.Alloc(_pixelData, GCHandleType.Pinned);
    OnSync?.Invoke(_pixelData);
    IsChanged = false;
    //handle.Free();
  }
}