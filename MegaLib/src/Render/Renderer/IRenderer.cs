using System.Collections.Generic;
using MegaLib.Render.Core;
using MegaLib.Render.Light;
using MegaLib.VR;

namespace MegaLib.Render.Renderer;

public interface IRenderer
{
  public byte[] GetScreen();

  public void SetConfig(RendererConfig config);

  public void Tick(float delta, int updateIteration);

  public VrRuntime StartVrSession(Dictionary<string, object> args);

  public Render_Scene Scene { get; set; }
}