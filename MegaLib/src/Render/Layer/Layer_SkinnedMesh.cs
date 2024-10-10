using System;
using System.Collections.Generic;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Layer;

public class Layer_SkinnedMesh : Layer_Base
{
  public override int Count => _objectList.Count;
  private List<RO_Skin> _objectList = [];

  public void Add(RO_Skin obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Skin obj)
  {
    _objectList.Remove(obj);
  }

  public void ForEach(Action<RO_Skin> fn)
  {
    for (var i = 0; i < _objectList.Count; i++) fn(_objectList[i]);
  }

  public override void Clear()
  {
    _objectList.Clear();
  }
}