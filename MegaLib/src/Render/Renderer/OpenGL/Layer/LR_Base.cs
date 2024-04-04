using System;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class LR_Base : ILayerRenderer
  {
    protected readonly OpenGL_Context Context;
    protected readonly RL_Base Layer;
    protected readonly Render_Scene Scene;
    protected readonly OpenGL_Shader Shader;

    public LR_Base(OpenGL_Context context, RL_Base layer, Render_Scene scene)
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
  }
}