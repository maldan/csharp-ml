using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject
{
  public class RenderObject_Line : RenderObject_Base
  {
    public Vector3 From;
    public Vector3 To;

    public RenderObject_Line()
    {
    }

    public RenderObject_Line(Vector3 from, Vector3 to)
    {
      From = from;
      To = to;
    }
  }
}