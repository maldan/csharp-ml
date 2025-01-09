using System.Collections.Generic;
using MegaLib.Mathematics.Geometry;
using MegaLib.Render.Scene;
using MegaLib.VR;

namespace MegaLib.Render.Renderer;

public interface IRenderer
{
  public Rectangle Viewport { get; }

  public void Scale(float scale);
  public void ScaleViewport(float scale);

  public byte[] GetScreen();

  public void SetConfig(RendererConfig config);

  public void Tick(float delta, int updateIteration);

  public VrRuntime StartVrSession(Dictionary<string, object> args);

  public void SetViewport(ushort x, ushort y, ushort width, ushort height);

  public Render_Scene Scene { get; set; }
}