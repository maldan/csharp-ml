using System.Collections.Generic;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class GLRL_Base
  {
    protected readonly Context_OpenGL Context;
    protected readonly RL_Base Layer;
    protected readonly Render_Scene Scene;
    protected readonly OpenGL_Shader Shader = new();

    public GLRL_Base(Context_OpenGL context, RL_Base layer, Render_Scene scene)
    {
      Context = context;
      Layer = layer;
      Scene = scene;
    }

    public virtual void Init()
    {
    }

    public virtual void Render()
    {
    }

    public virtual void Destroy()
    {
    }
  }
}