using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject
{
  public class RO_Line : RO_Base
  {
    public Vector3 From;
    public Vector3 To;

    public RO_Line()
    {
    }

    public RO_Line(Vector3 from, Vector3 to)
    {
      From = from;
      To = to;
    }
  }
}