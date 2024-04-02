using System;
using System.Collections.Generic;
using MegaLib.Render.Camera;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Core
{
  public class Render_Scene
  {
    public Camera_Base Camera = new Camera_Perspective();
    public readonly List<RL_Base> Pipeline = new();
    public Texture_Cube Skybox;

    public void AddLayer(string name, RL_Base layer)
    {
      layer.Name = name;
      Pipeline.Add(layer);
    }

    public void Add(string layerName, RO_Base obj)
    {
      var layer = Pipeline.Find(x => x.Name == layerName);
      if (layer == null) throw new Exception("Layer not found");
      layer.Add(obj);
    }

    public void Delete(string layerName, RO_Base obj)
    {
      var layer = Pipeline.Find(x => x.Name == layerName);
      if (layer == null) throw new Exception("Layer not found");
      layer.Remove(obj);
    }

    public void Render()
    {
    }
  }
}