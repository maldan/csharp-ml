using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Skeleton
{
  public class Bone
  {
    public int Id;
    public string Name;

    public Vector3 Position;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;
  }
}