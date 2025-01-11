﻿namespace MegaLib.ECS;

public class ECS_Entity
{
  public int Id;
  public int ComponentIndex;
  public ECS_Archetype Archetype;

  public ref T GetComponent<T>() where T : unmanaged
  {
    var chunk = Archetype.Get(typeof(T));
    return ref chunk.Get<T>(ComponentIndex);
  }
}