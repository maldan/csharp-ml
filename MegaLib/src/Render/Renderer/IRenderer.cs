using MegaLib.Render.Core;
using MegaLib.VR;

namespace MegaLib.Render.Renderer;

public interface IRenderer
{
  public byte[] GetScreen();

  public void Tick(float delta, int updateIteration);

  public VrRuntime StartVrSession();

  public Render_Scene Scene { get; set; }
}