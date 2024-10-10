using System;
using System.Collections.Generic;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Layer;

public class Layer_Sprite : Layer_Base
{
  public bool IsYInverted = false;

  public override int Count => _objectList.Count;
  private List<RO_Sprite> _objectList = [];

  public void Add(RO_Sprite obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Sprite obj)
  {
    _objectList.Remove(obj);
  }

  public void ForEach(Action<RO_Sprite> fn)
  {
    for (var i = 0; i < _objectList.Count; i++) fn(_objectList[i]);
  }

  public override void Clear()
  {
    _objectList.Clear();
  }
}