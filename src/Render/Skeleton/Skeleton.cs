using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Skeleton
{
  public class Skeleton
  {
    public Bone[] BoneList = { };
    public Vector3 Position;
    public Quaternion Rotation = Quaternion.Identity;
  }
}