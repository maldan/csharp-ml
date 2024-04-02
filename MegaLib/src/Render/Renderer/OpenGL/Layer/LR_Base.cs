using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class LR_Base : ILayerRenderer
  {
    protected readonly OpenGL_Context Context;
    protected readonly RL_Base Layer;
    protected readonly Render_Scene Scene;
    protected readonly OpenGL_Shader Shader = new();

    public LR_Base(OpenGL_Context context, RL_Base layer, Render_Scene scene)
    {
      Context = context;
      Layer = layer;
      Scene = scene;
    }

    public void Init()
    {
    }

    public void Render()
    {
    }
  }
}