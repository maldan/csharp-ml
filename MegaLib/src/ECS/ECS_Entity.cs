using System;

namespace MegaLib.ECS;

public class ECS_Entity
{
  public int Id;
  public int ComponentIndex = -1;
  public ECS_Archetype Archetype;
  public bool IsDead;
  public ECS_World World;

  public ref T GetComponentData<T>() where T : unmanaged
  {
    var chunk = Archetype.GetComponentChunk(typeof(T));
    return ref chunk.Get<T>(ComponentIndex);
  }

  public byte[] GetRawComponentData(Type t)
  {
    var chunk = Archetype.GetComponentChunk(t);
    return chunk.GetRaw(ComponentIndex);
  }

  public void SetRawComponentData<T>(Type t, T value) where T : unmanaged
  {
    var chunk = Archetype.GetComponentChunk(t);
    chunk.SetRaw(ComponentIndex, value);
  }

  public void AddComponent(Type t)
  {
    var oldAt = Archetype;

    //Console.WriteLine($"AT - {Archetype.Mask}");
    // Move to new archetype
    var newAt = World.ExtendArchetype(Archetype, t);
    //Console.WriteLine($"AT - {newAt.Mask}");
    newAt.AddEntity(this);

    oldAt.RemoveEntity(this);
  }

  public void AddComponent<T>(T value) where T : unmanaged
  {
    AddComponent(typeof(T));
    SetRawComponentData(typeof(T), value);
  }

  public bool HasComponent(Type t)
  {
    return Archetype.HasComponent(t);
  }

  public void RemoveComponent(Type t)
  {
    var oldAt = Archetype;

    // Move to new archetype
    var newAt = World.ReduceArchetype(Archetype, t);
    newAt.AddEntity(this);

    oldAt.RemoveEntity(this);
  }

  public void SoftDestroy()
  {
    World.AddEntityToRemoveQueue(this);
  }

  public void Destroy()
  {
    Archetype.RemoveEntity(this);
  }
}