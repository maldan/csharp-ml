using System;
using System.Threading;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject;
/*public enum RO_DataType
{
  Buffer,
  Texture
}*/

internal static class RoId
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

public class RO_Base
{
  public Transform Transform;
  public ulong Id;
  public bool IsVisible = true;

  protected RO_Base()
  {
    Id = RoId.NextId();
  }

  public virtual void Update(float delta)
  {
  }

  public virtual void CalculateBoundingBox()
  {
  }
}