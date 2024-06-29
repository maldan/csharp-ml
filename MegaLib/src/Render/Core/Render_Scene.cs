using System;
using System.Collections.Generic;
using System.Threading;
using MegaLib.Render.Camera;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Core;

public class Render_Scene
{
  public Camera_Base Camera = new Camera_Perspective();
  public readonly List<RL_Base> Pipeline = new();
  public Texture_Cube Skybox;
  private Mutex _mutex = new();

  public void AddLayer(string name, RL_Base layer)
  {
    _mutex.WaitOne();
    layer.Name = name;
    Pipeline.Add(layer);
    _mutex.ReleaseMutex();
  }

  public bool Add(string layerName, RO_Base obj)
  {
    var added = true;
    _mutex.WaitOne();
    var layer = Pipeline.Find(x => x.Name == layerName);
    if (layer == null) added = false;
    layer?.Add(obj);
    _mutex.ReleaseMutex();
    return added;
  }

  public void Delete(string layerName, RO_Base obj)
  {
    _mutex.WaitOne();
    var layer = Pipeline.Find(x => x.Name == layerName);
    layer?.Remove(obj);
    _mutex.ReleaseMutex();
  }

  public virtual void OnInit()
  {
  }

  public virtual void OnLoad()
  {
  }

  public virtual void OnBeforeRender()
  {
  }

  public virtual void OnBeforeUpdate(float delta)
  {
  }

  public virtual void OnAfterUpdate(float delta)
  {
  }

  public void Update(float delta)
  {
    OnBeforeUpdate(delta);
    foreach (var layer in Pipeline) layer.Update(delta);
    OnAfterUpdate(delta);
  }

  public void Render()
  {
    _mutex.WaitOne();

    OnBeforeRender();
    foreach (var layer in Pipeline) layer.Render();

    _mutex.ReleaseMutex();
  }
}