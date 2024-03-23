using System;
using System.Collections.Generic;
using MegaLib.Render.Camera;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Core
{
  public class Render_Scene
  {
    public Camera_Base Camera = new Camera_Perspective();
    public readonly List<Render_Layer> Pipeline = new();
    public Texture_Cube Skybox;

    public void AddLayer(string name, Render_LayerType type)
    {
      Pipeline.Add(new Render_Layer { Name = name, Type = type });
    }

    public void Add(string layerName, RenderObject_Base obj)
    {
      var layer = Pipeline.Find(x => x.Name == layerName);
      if (layer == null) throw new Exception("Layer not found");
      layer.Add(obj);
    }

    public void Delete(string layerName, RenderObject_Base obj)
    {
      var layer = Pipeline.Find(x => x.Name == layerName);
      if (layer == null) throw new Exception("Layer not found");
      layer.Remove(obj);
    }
  }
}