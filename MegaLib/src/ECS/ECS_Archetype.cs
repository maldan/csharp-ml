using System;
using System.Collections.Generic;

namespace MegaLib.ECS;

public class ECS_Archetype
{
  public ulong Mask;
  public Dictionary<Type, ECS_ComponentChunk> Components = new();

  public void CreateChunk(Type t)
  {
    Components.Add(t, new ECS_ComponentChunk(t, 10));
  }

  public int AddComponents()
  {
    var count = Components.Count;
    foreach (var (type, component) in Components) component.Add();
    return count;
  }

  public ECS_ComponentChunk Get(Type t)
  {
    return Components[t];
  }

  public void RemoveComponentData(int index)
  {
    foreach (var (type, c) in Components) c.Remove(index);
  }

  public unsafe void ForEach<T1>(RefAction<T1> fn) where T1 : unmanaged
  {
    var c = Get(typeof(T1));
    for (var i = 0; i < c.Count; i++)
    {
      var v = c.GetPointer<T1>(i);
      fn(ref *v, i);
    }
  }

  public unsafe void ForEach<T1, T2>(RefAction<T1, T2> fn) where T1 : unmanaged where T2 : unmanaged
  {
    var c1 = Get(typeof(T1));
    var c2 = Get(typeof(T2));
    for (var i = 0; i < c1.Count; i++)
    {
      var v1 = c1.GetPointer<T1>(i);
      var v2 = c2.GetPointer<T2>(i);
      fn(ref *v1, ref *v2, i);
    }
  }
}