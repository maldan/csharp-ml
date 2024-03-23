using System;
using System.Collections.Generic;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Core
{
  public enum Render_LayerType
  {
    StaticLine,
    DynamicLine,
    StaticMesh,
    SkinnedMesh,
    Skybox,
  }

  public class Render_Layer
  {
    private readonly List<RenderObject_Base> _objectList = new();
    public string Name;
    public Render_LayerType Type;

    public int Count => _objectList.Count;

    public void Add(RenderObject_Base obj)
    {
      _objectList.Add(obj);
    }

    public void Remove(RenderObject_Base obj)
    {
      _objectList.Remove(obj);
    }

    public void ForEach<T>(Action<T> fn) where T : RenderObject_Base
    {
      foreach (var obj in _objectList) fn((T)obj);
    }

    public void Clear()
    {
      _objectList.Clear();
    }
  }
}