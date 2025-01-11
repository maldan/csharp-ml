using System;
using System.Collections.Generic;

namespace MegaLib.ECS;

public class ECS_Archetype
{
  public ulong Id;
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
}