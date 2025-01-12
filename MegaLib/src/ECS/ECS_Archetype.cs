using System;
using System.Collections.Generic;

namespace MegaLib.ECS;

public class ECS_Archetype
{
  public ulong Mask;
  private List<ECS_Entity> _entities = [];
  public Dictionary<Type, ECS_ComponentChunk> Components = new();

  public void AddType(Type t)
  {
    // If already have
    if (Components.ContainsKey(t)) return;
    Components.Add(t, new ECS_ComponentChunk(t, 1024));
  }

  public ECS_ComponentChunk GetComponentChunk(Type t)
  {
    return Components[t];
  }

  public bool HasComponent(Type t)
  {
    return Components.ContainsKey(t);
  }

  public void AddEntity(ECS_Entity entity)
  {
    var count = _entities.Count;
    _entities.Add(entity);

    // If fresh created entity
    if (entity.ComponentIndex == -1)
    {
      foreach (var (type, component) in Components) component.Add();
    }
    else
    {
      foreach (var (type, component) in Components)
      {
        if (entity.HasComponent(type)) component.Add(entity.GetRawComponentData(type));
        else component.Add();
      }
    }

    entity.Archetype = this;
    entity.ComponentIndex = count;
  }

  public void RemoveEntity(ECS_Entity entity)
  {
    var index = _entities.IndexOf(entity);
    if (index == -1) return;
    _entities.RemoveAt(index);

    // Remove components
    foreach (var (type, c) in Components)
    {
      Console.WriteLine(type);
      c.Remove(index);
    }
  }

  public unsafe void ForEach<T1>(ECS_RefAction<T1> fn) where T1 : unmanaged
  {
    var c = GetComponentChunk(typeof(T1));
    for (var i = 0; i < c.Count; i++)
    {
      if (_entities[i].IsDead) continue;
      var v = c.GetPointer<T1>(i);
      fn(ref *v, _entities[i]);
    }
  }

  public unsafe void ForEach<T1, T2>(ECS_RefAction<T1, T2> fn) where T1 : unmanaged where T2 : unmanaged
  {
    var c1 = GetComponentChunk(typeof(T1));
    var c2 = GetComponentChunk(typeof(T2));
    for (var i = 0; i < c1.Count; i++)
    {
      if (_entities[i].IsDead) continue;

      var v1 = c1.GetPointer<T1>(i);
      var v2 = c2.GetPointer<T2>(i);
      fn(ref *v1, ref *v2, _entities[i]);
    }
  }
}