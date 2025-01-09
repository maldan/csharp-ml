using System.Collections.Generic;
using System.Threading;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;
using MegaLib.Render.Light;
using MegaLib.Render.Renderer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Scene;

public class Render_Scene
{
  // Главная камера
  public Camera_Base Camera = new Camera_Perspective();

  public readonly List<Layer_Base> Pipeline = [];
  public Texture_Cube Skybox;
  public List<LightBase> Lights = [];
  private Mutex _mutex = new();
  public bool UseSSAO;
  public bool UsePostprocess;
  public bool IsVrMode { get; set; }
  public float DeltaTime;
  public RGBA32F BackgroundColor = new();

  public Layer_PostProcess PostProcessLayer = new();

  public Texture_2D<float> LightTexture;

  public IRenderer Renderer { get; private set; }

  public Render_Scene()
  {
    LightTexture = new Texture_2D<float>(64, 64);
    LightTexture.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    LightTexture.Options.WrapMode = TextureWrapMode.Clamp;
    LightTexture.Options.Format = TextureFormat.R32F;
    LightTexture.Options.UseMipMaps = false;
  }

  public void SetRenderer(IRenderer renderer)
  {
    Renderer = renderer;
  }

  public void AddLayer(string name, Layer_Base layer)
  {
    _mutex.WaitOne();
    layer.Name = name;
    Pipeline.Add(layer);
    _mutex.ReleaseMutex();
  }

  public T GetLayer<T>(string name) where T : Layer_Base
  {
    _mutex.WaitOne();
    var layer = Pipeline.Find(x => x.Name == name);
    _mutex.ReleaseMutex();
    return (T)layer;
  }

  public T GetLayer<T>() where T : Layer_Base
  {
    _mutex.WaitOne();
    var layerIndex = Pipeline.FindIndex(x => x is T);
    _mutex.ReleaseMutex();
    if (layerIndex == -1) return null;
    return (T)Pipeline[layerIndex];
  }

  /*public bool Add(string layerName, RO_Base obj)
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
  }*/

  public void DeleteAll(string layerName)
  {
    _mutex.WaitOne();
    var layer = Pipeline.Find(x => x.Name == layerName);
    layer?.Clear();
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
    // foreach (var layer in Pipeline) layer.Update(delta);
    OnAfterUpdate(delta);
  }

  private void CalculateLight()
  {
    var id = 0;

    // Количество источников света
    LightTexture.RAW[id++] = Lights.Count;

    foreach (var light in Lights)
    {
      if (light is LightDirection ld)
      {
        LightTexture.RAW[id++] = 1;
        LightTexture.RAW[id++] = -ld.Direction.X;
        LightTexture.RAW[id++] = -ld.Direction.Y;
        LightTexture.RAW[id++] = ld.Direction.Z;
        LightTexture.RAW[id++] = 0;
      }

      if (light is LightPoint lp)
      {
        LightTexture.RAW[id++] = 2;
        LightTexture.RAW[id++] = light.Position.X;
        LightTexture.RAW[id++] = light.Position.Y;
        LightTexture.RAW[id++] = light.Position.Z;
        LightTexture.RAW[id++] = lp.Radius;
      }

      LightTexture.RAW[id++] = light.Intensity;

      LightTexture.RAW[id++] = light.Color.R;
      LightTexture.RAW[id++] = light.Color.G;
      LightTexture.RAW[id++] = light.Color.B;
    }
  }

  public void Render()
  {
    _mutex.WaitOne();

    OnBeforeRender();

    if (UsePostprocess) PostProcessLayer.LayerRenderer.BeforeRender();

    CalculateLight();
    foreach (var layer in Pipeline)
    {
      if (layer is Layer_EasyUI) continue;
      layer.Render();
    }
    
    if (UsePostprocess)
    {
      PostProcessLayer.LayerRenderer.AfterRender();
      PostProcessLayer.Render();
    }

    // Render UI at last
    foreach (var layer in Pipeline)
    {
      if (layer is Layer_EasyUI) layer.Render();
    }

    _mutex.ReleaseMutex();
  }
}