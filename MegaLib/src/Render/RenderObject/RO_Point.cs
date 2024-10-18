using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.RenderObject;

public struct RO_Point
{
  public Vector3 Position;
  public RGBA32F Color = new(1, 1, 1, 1);
  public float Size = 1;

  public RO_Point()
  {
  }

  public RO_Point(Vector3 position, RGBA32F color, float size = 1f)
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