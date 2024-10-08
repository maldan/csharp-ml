using MegaLib.Render.Layer;
using MegaLib.Render.Scene;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Base : ILayerRenderer
{
  protected readonly OpenGL_Context Context;
  protected readonly Layer_Base Layer;
  protected readonly Render_Scene Scene;
  protected readonly OpenGL_Shader Shader;

  public LR_Base(OpenGL_Context context, Layer_Base layer, Render_Scene scene)
  {
    Context = context;
    Layer = layer;
    Scene = scene;
    Shader = new OpenGL_Shader
    {
      Context = Context
    };
  }

  public virtual void Init()
  {
  }

  public virtual void Render()
  {
  }

  public virtual void BeforeRender()
  {
  }

  public virtual void AfterRender()
  {
  }
}