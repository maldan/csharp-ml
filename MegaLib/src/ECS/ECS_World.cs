using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaLib.ECS;

public class ECS_World
{
  public readonly List<ECS_System> SystemList = [];

  // For archetype mask
  private Dictionary<Type, int> _typeToBit = new();
  private int _nextBit;
  private int _entityId;
  private Dictionary<ulong, ECS_Archetype> _archetypes = new();

  // Entities
  private List<ECS_Entity> _entityList = [];

  public void AddSystem(ECS_System system)
  {
    SystemList.Add(system);
  }

  public ECS_Entity CreateEntity(ECS_Archetype archetype)
  {
    _entityId++;
    var componentIndex = archetype.AddComponents();
    var e = new ECS_Entity
    {
      Id = _entityId,
      ComponentIndex = componentIndex,
      Archetype = archetype
    };
    _entityList.Add(e);
    return e;
  }

  public void DestroyEntity(ECS_Entity entity)
  {
    _entityList.Remove(entity);
    entity.Destroy();
  }

  private int ComponentGetBit(Type type)
  {
    if (!_typeToBit.TryGetValue(type, out var bit))
    {
      bit = _nextBit++;
      _typeToBit[type] = bit;
    }

    return bit;
  }

  public ulong ComponentGetMask(params Type[] componentTypes)
  {
    ulong mask = 0;
    foreach (var type in componentTypes) mask |= 1UL << ComponentGetBit(type);
    return mask;
  }

  public ECS_Archetype CreateArchetype(params Type[] componentTypes)
  {
    ulong mask = 0;
    foreach (var t in componentTypes) mask |= ComponentGetMask(t);
    if (_archetypes.ContainsKey(mask)) return _archetypes[mask];

    var archetype = new ECS_Archetype
    {
      Mask = mask
    };
    foreach (var t in componentTypes) archetype.CreateChunk(t);
    _archetypes.Add(mask, archetype);

    return archetype;
  }

  public virtual void Tick(float delta)
  {
    SystemList.ForEach(system => { system.Tick(delta); });
  }

  public List<ECS_Archetype> SelectArchetypes(ulong mask)
  {
    var list = new List<ECS_Archetype>();
    foreach (var (bitmask, at) in _archetypes)
      if ((mask & bitmask) == mask)
        list.Add(at);
    return list;
  }

  public void ForEach<T1>(ulong mask, RefAction<T1> fn) where T1 : unmanaged
  {
    var list = SelectArchetypes(mask);
    foreach (var at in list) at.ForEach(fn);
  }

  public void ForEach<T1, T2>(ulong mask, RefAction<T1, T2> fn) where T1 : unmanaged where T2 : unmanaged
  {
    var list = SelectArchetypes(mask);
    foreach (var at in list) at.ForEach(fn);
  }
}