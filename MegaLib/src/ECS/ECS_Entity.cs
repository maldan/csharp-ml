using System;

namespace MegaLib.ECS;

public class ECS_Entity
{
  public int Id;
  public int ComponentIndex = -1;
  public ECS_Archetype Archetype;
  public bool IsDead;
  public ECS_World World;

  public ref T GetComponent<T>() where T : unmanaged
  {
    var chunk = Archetype.GetComponentChunk(typeof(T));
    return ref chunk.Get<T>(ComponentIndex);
  }

  public byte[] GetRawComponentData(Type t)
  {
    var chunk = Archetype.GetComponentChunk(t);
    return chunk.GetRaw(ComponentIndex);
  }

  public void AddComponent<T>() where T : unmanaged
  {
    Console.WriteLine($"AT - {Archetype.Mask}");
    // Move to new archetype
    var newAt = World.ExtendArchetype(Archetype, typeof(T));
    Console.WriteLine($"AT - {newAt.Mask}");
    newAt.AddEntity(this);
    Archetype.RemoveEntity(this);
  }

  public void RemoveComponent<T>() where T : unmanaged
  {
    // Move to new archetype
    var newAt = World.ReduceArchetype(Archetype, typeof(T));
    newAt.AddEntity(this);
    Archetype.RemoveEntity(this);
  }

  public void Destroy()
  {
    Archetype.RemoveEntity(this);
  }
}