using System.Collections.Generic;
using System.Dynamic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;

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

public class VrHand
{
  public List<Vector3> Joints = [];
  private VrHeadset _headset;

  public VrHand(VrHeadset headset)
  {
    _headset = headset;

    for (var i = 0; i < 26; i++)
    {
      Joints.Add(new Vector3());
    }
  }

  public void Draw(Layer_Line layerLine)
  {
    // Define the connections between joints
    var jointConnections = new List<(XrHandJointEXT, XrHandJointEXT)>
    {
      // Palm and Wrist
      (XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT, XrHandJointEXT.XR_HAND_JOINT_PALM_EXT),

      // Thumb
      (XrHandJointEXT.XR_HAND_JOINT_PALM_EXT, XrHandJointEXT.XR_HAND_JOINT_THUMB_METACARPAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_THUMB_METACARPAL_EXT, XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_THUMB_PROXIMAL_EXT, XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_THUMB_DISTAL_EXT, XrHandJointEXT.XR_HAND_JOINT_THUMB_TIP_EXT),

      // Index Finger
      (XrHandJointEXT.XR_HAND_JOINT_PALM_EXT, XrHandJointEXT.XR_HAND_JOINT_INDEX_METACARPAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_INDEX_METACARPAL_EXT, XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_INDEX_PROXIMAL_EXT, XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_INDEX_INTERMEDIATE_EXT, XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_INDEX_DISTAL_EXT, XrHandJointEXT.XR_HAND_JOINT_INDEX_TIP_EXT),

      // Middle Finger
      (XrHandJointEXT.XR_HAND_JOINT_PALM_EXT, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_METACARPAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_MIDDLE_METACARPAL_EXT, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_MIDDLE_PROXIMAL_EXT, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_MIDDLE_DISTAL_EXT, XrHandJointEXT.XR_HAND_JOINT_MIDDLE_TIP_EXT),

      // Ring Finger
      (XrHandJointEXT.XR_HAND_JOINT_PALM_EXT, XrHandJointEXT.XR_HAND_JOINT_RING_METACARPAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_RING_METACARPAL_EXT, XrHandJointEXT.XR_HAND_JOINT_RING_PROXIMAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_RING_PROXIMAL_EXT, XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_RING_INTERMEDIATE_EXT, XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_RING_DISTAL_EXT, XrHandJointEXT.XR_HAND_JOINT_RING_TIP_EXT),

      // Little Finger
      (XrHandJointEXT.XR_HAND_JOINT_PALM_EXT, XrHandJointEXT.XR_HAND_JOINT_LITTLE_METACARPAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_LITTLE_METACARPAL_EXT, XrHandJointEXT.XR_HAND_JOINT_LITTLE_PROXIMAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_LITTLE_PROXIMAL_EXT, XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_LITTLE_INTERMEDIATE_EXT, XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT),
      (XrHandJointEXT.XR_HAND_JOINT_LITTLE_DISTAL_EXT, XrHandJointEXT.XR_HAND_JOINT_LITTLE_TIP_EXT)
    };

    foreach (var (joint1, joint2) in jointConnections)
    {
      var p1 = GetJointWorldPosition((int)joint1);
      var p2 = GetJointWorldPosition((int)joint2);
      layerLine.DrawLine(p1, p2, new RGBA32F(0, 1, 0, 1), 6f); // Drawing in green
    }
  }

  public Vector3 GetJointWorldPosition(int index)
  {
    // To
    /*var fr = Matrix4x4.Identity;
    var position = (Joints[index]
                      .AddW(0.0f)
                    * _headset.RotationOffset.Inverted.Matrix4x4).DropW() + _headset.PositionOffset;

    fr = fr.Translate(position);
    fr = fr.Rotate(_headset.RotationOffset.Inverted);*/
    var mm = Matrix4x4.Identity;

    mm = mm.Translate(_headset.PositionOffset.X, -_headset.PositionOffset.Y, -_headset.PositionOffset.Z);
    mm = mm.Rotate(_headset.RotationOffset.Inverted);

    return Joints[index] * mm;
  }
}

public class VrHeadset
{
  public Matrix4x4 LocalTransform = Matrix4x4.Identity;
  public VrSide Left;
  public VrSide Right;

  public VrHand LeftHand { get; private set; }
  public VrHand RightHand { get; private set; }

  public VrController LeftController { get; private set; }
  public VrController RightController { get; private set; }
  public VrController[] Controller => [LeftController, RightController];

  public Vector3 PositionOffset;
  public Quaternion RotationOffset = Quaternion.Identity;

  public Matrix4x4 WorldTransform =>
    Matrix4x4.Identity
      .Rotate(RotationOffset.Inverted)
      .Translate(PositionOffset);

  public VrHeadset()
  {
    Left = new VrSide(this);
    Right = new VrSide(this);

    LeftController = new VrController(this);
    RightController = new VrController(this);

    LeftHand = new VrHand(this);
    RightHand = new VrHand(this);
  }

  public void OffsetPosition(Vector3 dir)
  {
    var hh = RotationOffset * LocalTransform.Rotation;
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
    OffsetRotation(new Vector3(delta * RightController.ThumbstickDirection.Y,
      delta * -RightController.ThumbstickDirection.X, 0.0f));
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