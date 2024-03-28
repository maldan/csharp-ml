using System;
using System.Collections.Generic;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Core.Layer
{
  public class RL_StaticLine : RL_Base
  {
    public float LineWidth = 1.0f;
    public bool IsSmooth = true;
  }

  public class RL_StaticMesh : RL_Base
  {
  }

  public class RL_SkinnedMesh : RL_Base
  {
  }

  public class RL_Skybox : RL_Base
  {
  }

  public class RL_Base
  {
    private readonly List<RO_Base> _objectList = new();
    public string Name;

    public int Count => _objectList.Count;

    public void Add(RO_Base obj)
    {
      _objectList.Add(obj);
    }

    public void Remove(RO_Base obj)
    {
      _objectList.Remove(obj);
    }

    public void ForEach<T>(Action<T> fn) where T : RO_Base
    {
      foreach (var obj in _objectList) fn((T)obj);
    }

    public void Clear()
    {
      _objectList.Clear();
    }
  }
}