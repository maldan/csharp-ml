using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.IO;

public class VrSide
{
  public Matrix4x4 Projection = Matrix4x4.Identity;

  public Matrix4x4 View = Matrix4x4.Identity;

  // public VrController Controller;
  private VrHeadset _headset;

  public VrSide(VrHeadset headset)
  {
    _headset = headset;
    // Controller = new VrController(headset);
  }
}

public class VrController
{
  public Matrix4x4 GripTransform = Matrix4x4.Identity;
  public Matrix4x4 AimTransform = Matrix4x4.Identity;

  public Vector2 ThumbstickDirection;

  public float TriggerValue;
  public float GripValue;

  public bool IsPressA;
  public bool IsPressB;
  public bool IsPressTrigger => TriggerValue > 0;
  public bool IsPressGrip => GripValue > 0;
  public bool IsPressThumbstick;

  public bool IsHoverA;
  public bool IsHoverB;
  public bool IsHoverTrigger;
  public bool IsHoverGrip;
  public bool IsHoverThumbstick;
}

public class VrHeadset
{
  public Matrix4x4 Transform = Matrix4x4.Identity;
  public VrSide Left;
  public VrSide Right;

  public VrController LeftController { get; private set; }
  public VrController RightController { get; private set; }
  public VrController[] Controller => [LeftController, RightController];

  public Vector3 PositionOffset;
  public Quaternion RotationOffset = Quaternion.Identity;

  public Matrix4x4 OffsetMatrix = Matrix4x4.Identity;

  public VrHeadset()
  {
    Left = new VrSide(this);
    Right = new VrSide(this);

    LeftController = new VrController();
    RightController = new VrController();
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