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

  private List<ECS_Entity> _removeQueue = new();

  public void AddSystem(ECS_System system)
  {
    system.World = this;
    SystemList.Add(system);
  }

  public ECS_Entity CreateEntity(ECS_Archetype archetype)
  {
    _entityId++;
    var e = new ECS_Entity
    {
      Id = _entityId,
      Archetype = archetype,
      World = this
    };
    archetype.AddEntity(e);
    return e;
  }

  private int GetComponentBit(Type type)
  {
    if (!_typeToBit.TryGetValue(type, out var bit))
    {
      bit = _nextBit++;
      _typeToBit[type] = bit;
    }

    return bit;
  }

  public ulong GetComponentMask(params Type[] componentTypes)
  {
    ulong mask = 0;
    foreach (var type in componentTypes) mask |= 1UL << GetComponentBit(type);
    return mask;
  }

  public ECS_Archetype CreateArchetype(params Type[] componentTypes)
  {
    ulong mask = 0;
    foreach (var t in componentTypes) mask |= GetComponentMask(t);
    if (_archetypes.ContainsKey(mask)) return _archetypes[mask];

    var archetype = new ECS_Archetype
    {
      Mask = mask
    };
    foreach (var t in componentTypes) archetype.AddType(t);
    _archetypes.Add(mask, archetype);

    return archetype;
  }

  public void AddEntityToRemoveQueue(ECS_Entity entity)
  {
    entity.IsDead = true;
    _removeQueue.Add(entity);
  }

  public void Flush()
  {
    foreach (var entity in _removeQueue) entity.Destroy();
    _removeQueue.Clear();
  }

  public virtual void Tick(float delta)
  {
    SystemList.ForEach(system => { system.Tick(delta); });
    Flush();
  }

  public List<ECS_Archetype> SelectArchetypes(ulong mask)
  {
    var list = new List<ECS_Archetype>();
    foreach (var (bitmask, at) in _archetypes)
      if ((mask & bitmask) == mask)
        list.Add(at);
    return list;
  }

  // Create a new archetype with additional type
  public ECS_Archetype ExtendArchetype(ECS_Archetype archetype, Type type)
  {
    // Collect types
    var types = new HashSet<Type>();
    foreach (var (t, _) in archetype.Components) types.Add(t);
    types.Add(type);

    return CreateArchetype(types.ToArray());
  }

  // Create a new archetype without specified type
  public ECS_Archetype ReduceArchetype(ECS_Archetype archetype, Type type)
  {
    // Collect types
    var types = new HashSet<Type>();
    foreach (var (t, _) in archetype.Components) types.Add(t);
    types.Remove(type);

    return CreateArchetype(types.ToArray());
  }

  public void ForEach<T1>(ECS_RefAction<T1> fn) where T1 : unmanaged
  {
    var list = SelectArchetypes(GetComponentMask(typeof(T1)));
    foreach (var at in list) at.ForEach(fn);
  }

  public void ForEach<T1, T2>(ECS_RefAction<T1, T2> fn) where T1 : unmanaged where T2 : unmanaged
  {
    var list = SelectArchetypes(GetComponentMask(typeof(T1), typeof(T2)));
    foreach (var at in list) at.ForEach(fn);
  }
}