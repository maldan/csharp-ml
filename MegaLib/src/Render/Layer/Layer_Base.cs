using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Layer;

public interface ILayerRenderer
{
  public void Init();
  public void Render();
  public void BeforeRender();
  public void AfterRender();
}

public class Layer_Capture : Layer_Base
{
  public List<string> LayerNames = [];
}

public class Layer_Base
{
  //private readonly List<RO_Base> _objectList = new();
  public string Name;

  public virtual int Count => 0;
  public ILayerRenderer LayerRenderer;

  /*public void Add<T>(T obj)
  {
  }

  public virtual void Remove<T>(T obj)
  {
  }*/

  public virtual void Clear()
  {
  }

  /*public void Add(RO_Base obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Base obj)
  {
    _objectList.Remove(obj);
  }*/

  /*public void ForEach<T>(Action<T> fn) where T : RO_Base
  {
    foreach (var obj in _objectList.Where(obj => obj.IsVisible)) fn((T)obj);
  }*/


  public virtual void Init()
  {
    LayerRenderer?.Init();
  }

  /*public virtual void Update(float delta)
  {
    foreach (var obj in _objectList) obj.Update(delta);
  }*/

  public void Render()
  {
    LayerRenderer?.Render();
  }
}