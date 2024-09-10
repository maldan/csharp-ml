using MegaLib.Physics;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Core.Layer;

public class Layer_Point : Layer_Base
{
  public void Draw(VerletPoint point, RGBA<float> color, float size = 1.0f)
  {
    Add(new RO_Point()
    {
      Position = point.Position,
      Color = color,
      Size = size
    });
  }
}