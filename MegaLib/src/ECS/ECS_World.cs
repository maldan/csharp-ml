using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaLib.ECS;

public class ECS_World
{
  public List<ECS_System> SystemList = [];
  public Dictionary<int, List<object>> EntityList = [];

  public void AddSystem(ECS_System system)
  {
    SystemList.Add(system);
  }

  /*public List<T> GetEntityList<T>(int tag)
  {
    if (!EntityList.ContainsKey(tag)) return new List<T>();
    return (List<T>)EntityList[tag];
  }*/

  public T FirstEntity<T>(int tag)
  {
    var entityList = EntityList.ContainsKey(tag) ? EntityList[tag] : [];
    if (entityList.Count == 0) return default;
    return (T)entityList[0];
  }

  public void EachEntity<T>(int tag, Func<T, T> fn)
  {
    var entityList = EntityList.ContainsKey(tag) ? EntityList[tag] : [];
    for (var i = 0; i < entityList.Count; i++)
    {
      entityList[i] = fn((T)entityList[i]);
    }
  }

  public void ClearEntitiesBy<T>(int tag, Func<T, bool> fn)
  {
    var entityList = EntityList.ContainsKey(tag) ? EntityList[tag] : [];
    EntityList[tag] = entityList.Where(x => !fn((T)x)).ToList();
  }

  public void AddEntity(int tag, object value)
  {
    if (!EntityList.ContainsKey(tag)) EntityList[tag] = [];
    EntityList[tag].Add(value);
  }

  public virtual void Tick(float delta)
  {
    SystemList.ForEach(system => { system.Tick(delta); });
  }
}