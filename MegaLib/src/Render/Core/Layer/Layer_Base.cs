using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Core.Layer;

public interface ILayerRenderer
{
  public void Init();
  public void Render();
  public void BeforeRender();
  public void AfterRender();
}

public class Layer_Sprite : Layer_Base
{
  public bool IsYInverted = false;
}

public class Layer_BitmapText : Layer_Base
{
}

public class Layer_StaticMesh : Layer_Base
{
}

public class Layer_SkinnedMesh : Layer_Base
{
}

public class Layer_Skybox : Layer_Base
{
}

public class Layer_Point : Layer_Base
{
}

public class Layer_Capture : Layer_Base
{
  public List<string> LayerNames = [];
}

public class Layer_PostProcess : Layer_Base
{
}

public class Layer_Base
{
  private readonly List<RO_Base> _objectList = new();
  public string Name;
  public int Count => _objectList.Count;
  public ILayerRenderer LayerRenderer;

  public void Add(RO_Base obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Base obj)
  {
    _objectList.Remove(obj);
  }

  public void ForEach<T>(Action<T> fn) where T : RO_Base
  {
    foreach (var obj in _objectList.Where(obj => obj.IsVisible)) fn((T)obj);
  }

  public void Clear()
  {
    _objectList.Clear();
  }

  public void Init()
  {
    LayerRenderer?.Init();
  }

  public void Update(float delta)
  {
    foreach (var obj in _objectList) obj.Update(delta);
  }

  public void Render()
  {
    LayerRenderer?.Render();
  }
}