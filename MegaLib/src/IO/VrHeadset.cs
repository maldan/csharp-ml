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
  public Matrix4x4 GripLocalTransform = Matrix4x4.Identity;
  public Matrix4x4 AimLocalTransform = Matrix4x4.Identity;

  public Matrix4x4 GripWorldTransform
  {
    get
    {
      // To
      var fr = Matrix4x4.Identity;
      var position = (GripLocalTransform
                        .Position
                        .AddW(0.0f)
                      * _headset.RotationOffset.Inverted.Matrix4x4).DropW() + _headset.PositionOffset;

      fr = fr.Translate(position);
      fr = fr.Rotate(_headset.RotationOffset.Inverted * GripLocalTransform.Rotation);

      return fr;
    }
  }

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

  private VrHeadset _headset;

  public VrController(VrHeadset headset)
  {
    _headset = headset;
  }
}

public class VrHeadset
{
  public Matrix4x4 LocalTransform = Matrix4x4.Identity;
  public VrSide Left;
  public VrSide Right;

  public VrController LeftController { get; private set; }
  public VrController RightController { get; private set; }
  public VrController[] Controller => [LeftController, RightController];

  public Vector3 PositionOffset;
  public Quaternion RotationOffset = Quaternion.Identity;

  public Matrix4x4 WorldTransform =>
    Matrix4x4.Identity
      .Rotate(RotationOffset)
      .Translate(PositionOffset);

  public VrHeadset()
  {
    Left = new VrSide(this);
    Right = new VrSide(this);

    LeftController = new VrController(this);
    RightController = new VrController(this);
  }

  public void OffsetPosition(Vector3 dir)
  {
    var hh = RotationOffset.Inverted * LocalTransform.Rotation.Inverted;
    var head = Matrix4x4.Identity.Rotate(hh);

    var dirNew = dir.AddW(1.0f) * head;
    PositionOffset += dirNew.DropW();
  }

  public void OffsetRotation(Vector3 dir)
  {
    RotationOffset *= Quaternion.FromEuler(dir, "rad");
  }

  public void BaseMovement(float delta)
  {
    OffsetPosition(new Vector3(delta * -LeftController.ThumbstickDirection.X, 0.0f,
      delta * LeftController.ThumbstickDirection.Y));
    OffsetRotation(new Vector3(delta * -RightController.ThumbstickDirection.Y,
      delta * RightController.ThumbstickDirection.X, 0.0f));
  }

  /*public void CalculateOffset()
  {
    var offsetTransform = Matrix4x4.Identity
      .Rotate(RotationOffset)
      .Translate(PositionOffset.Inverted);
    OffsetMatrix = offsetTransform;

    Left.View *= offsetTransform;
    Right.View *= offsetTransform;
  }*/
}

public static class VrInput
{
  public static VrHeadset Headset = new();
}