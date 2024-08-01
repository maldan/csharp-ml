using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.IO;

public class VrSide
{
  public Matrix4x4 Projection = Matrix4x4.Identity;
  public Matrix4x4 View = Matrix4x4.Identity;
  public VrController Controller;
  private VrHeadset _headset;

  public VrSide(VrHeadset headset)
  {
    _headset = headset;
    Controller = new VrController(headset);
  }
}

public class VrController
{
  public Matrix4x4 Transform = Matrix4x4.Identity;
  private VrHeadset _headset;
  public Vector2 JoystickDirection;
  public float Trigger;
  public float Grab;
  public bool IsA;
  public bool IsB;

  public VrController(VrHeadset headset)
  {
    _headset = headset;
  }
}

public class VrHeadset
{
  public Matrix4x4 Transform = Matrix4x4.Identity;
  public VrSide Left;
  public VrSide Right;
  public Vector3 PositionOffset;
  public Quaternion RotationOffset = Quaternion.Identity;

  public Matrix4x4 OffsetMatrix = Matrix4x4.Identity;

  public VrHeadset()
  {
    Left = new VrSide(this);
    Right = new VrSide(this);
  }

  public void OffsetPosition(Vector3 dir)
  {
    var hh = RotationOffset.Inverted * Transform.Rotation;
    var head = Matrix4x4.Identity.Rotate(hh);

    var dirNew = dir.AddW(1.0f) * head;
    PositionOffset += dirNew.DropW();
  }

  public void OffsetRotation(Vector3 dir)
  {
    RotationOffset *= Quaternion.FromEuler(dir, "rad");
  }

  public void CalculateOffset()
  {
    var offsetTransform = Matrix4x4.Identity
      .Rotate(RotationOffset)
      .Translate(PositionOffset.Inverted);
    OffsetMatrix = offsetTransform;

    Left.View *= offsetTransform;
    Right.View *= offsetTransform;
  }
}

public static class VrInput
{
  public static VrHeadset Headset = new();
}