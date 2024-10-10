using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Layer;

public class Layer_Point : Layer_Base
{
  public override int Count => _objectList.Count;
  private List<RO_Point> _objectList = [];

  public void Add(RO_Point obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Point obj)
  {
    _objectList.Remove(obj);
  }

  public void ForEach(Action<RO_Point> fn)
  {
    for (var i = 0; i < _objectList.Count; i++) fn(_objectList[i]);
  }

  public override void Clear()
  {
    _objectList.Clear();
  }

  public void Draw(VerletPoint point, RGBA<float> color, float size = 1.0f)
  {
    Add(new RO_Point()
    {
      Position = point.Position,
      Color = color,
      Size = size
    });
  }

  public void Draw(Vector3 point, RGBA<float> color, float size = 1.0f)
  {
    Add(new RO_Point()
    {
      Position = point,
      Color = color,
      Size = size
    });
  }
}