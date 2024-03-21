using MegaLib.Render.Core;

namespace MegaLib.Render.Renderer
{
  public interface IRenderer
  {
    public void SetScene(Render_Scene scene);
    public void Render();
  }
}