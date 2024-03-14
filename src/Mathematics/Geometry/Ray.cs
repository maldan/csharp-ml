using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry
{
  public struct Ray
  {
    public Vector3 Position;
    public Vector3 Direction;
    public float Length;

    public Ray(Vector3 from, Vector3 to)
    {
      Position = from;
      Direction = (to - from).Normalized;
      Length = Vector3.Distance(from, to);
    }

    public Vector3 Start => Position;
    public Vector3 End => Position + Direction * Length;
  }
}