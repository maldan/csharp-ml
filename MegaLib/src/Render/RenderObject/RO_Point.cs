using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.RenderObject;

public class RO_Point : RO_Base
{
  public Vector3 Position;
  public RGBA<float> Color = new(1, 1, 1, 1);
  public float Size = 1;

  public RO_Point()
  {
  }

  public RO_Point(Vector3 position, RGBA<float> color, float size = 1f)
  {
    Position = position;
    Color = color;
    Size = size;
  }

  public RO_Point(float x, float y, float z, float size = 1f)
  {
    Position = new Vector3(x, y, z);
    Size = size;
  }
}