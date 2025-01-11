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

  public void AddSystem(ECS_System system)
  {
    SystemList.Add(system);
  }

  public ECS_Entity CreateEntity(ECS_Archetype archetype)
  {
    _entityId++;
    var componentIndex = archetype.AddComponents();
    return new ECS_Entity
    {
      Id = _entityId,
      ComponentIndex = componentIndex,
      Archetype = archetype
    };
  }

  public int ComponentGetBit(Type type)
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
    var archetype = new ECS_Archetype();
    for (var i = 0; i < componentTypes.Length; i++)
    {
      archetype.Id |= ComponentGetMask(componentTypes[i]);
      archetype.CreateChunk(componentTypes[i]);
    }

    return archetype;
  }

  public virtual void Tick(float delta)
  {
    SystemList.ForEach(system => { system.Tick(delta); });
  }
}