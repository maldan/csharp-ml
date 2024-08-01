using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using Console = System.Console;

namespace MegaLib.VR;

public class VrAction
{
  private readonly IntPtr _instance;
  private readonly IntPtr _session;
  private readonly IntPtr _space;
  public IntPtr ActionSet;

  private Dictionary<string, IntPtr> _action = new();

  private ulong[] _controllerPath = [0, 0];
  private ulong[] _controllerSpacePath = [0, 0, 0, 0];
  private IntPtr[] _controllerSpace = [IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero];

  public VrAction(IntPtr instance, IntPtr session, IntPtr space)
  {
    _instance = instance;
    _session = session;
    _space = space;
  }

  private void CreateAction(string name, XrActionType actionType)
  {
    _action[name] =
      OpenXR.CreateAction(_instance, ActionSet, actionType, name.Replace("/", "-"),
        ["/user/hand/left", "/user/hand/right"]);
  }

  public void Init()
  {
    // Создаем сет
    ActionSet = OpenXR.CreateActionSet(_instance, "vr-action-set");

    // Создаем все экшены
    CreateAction("grip/pose", XrActionType.XR_ACTION_TYPE_POSE_INPUT);
    CreateAction("aim/pose", XrActionType.XR_ACTION_TYPE_POSE_INPUT);
    CreateAction("thumbstick", XrActionType.XR_ACTION_TYPE_VECTOR2F_INPUT);
    CreateAction("trigger/value", XrActionType.XR_ACTION_TYPE_FLOAT_INPUT);
    CreateAction("squeeze/value", XrActionType.XR_ACTION_TYPE_FLOAT_INPUT);
    CreateAction("a/click", XrActionType.XR_ACTION_TYPE_BOOLEAN_INPUT);
    CreateAction("b/click", XrActionType.XR_ACTION_TYPE_BOOLEAN_INPUT);
    CreateAction("x/click", XrActionType.XR_ACTION_TYPE_BOOLEAN_INPUT);
    CreateAction("y/click", XrActionType.XR_ACTION_TYPE_BOOLEAN_INPUT);

    // Биндим всю дичь
    var bindings = new List<XrActionSuggestedBinding>();
    foreach (var (key, value) in _action)
      switch (key[..2])
      {
        case "x/":
        case "y/":
          bindings.Add(new XrActionSuggestedBinding
          {
            Action = value, Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/" + key)
          });
          break;
        case "a/":
        case "b/":
          bindings.Add(new XrActionSuggestedBinding
          {
            Action = value, Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/" + key)
          });
          break;
        default:
          bindings.Add(new XrActionSuggestedBinding
          {
            Action = value, Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/" + key)
          });
          bindings.Add(new XrActionSuggestedBinding
          {
            Action = value, Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/" + key)
          });
          break;
      }

    OpenXR.SuggestBindings(_instance, "/interaction_profiles/oculus/touch_controller", bindings.ToArray());

    // Аттачим сет к сессии
    OpenXR.AttachActionSet(_session, ActionSet);

    _controllerPath[0] = OpenXR.CreateXrPath(_instance, "/user/hand/left");
    _controllerPath[1] = OpenXR.CreateXrPath(_instance, "/user/hand/right");

    _controllerSpacePath[0] = OpenXR.CreateXrPath(_instance, "/user/hand/left");
    _controllerSpacePath[1] = OpenXR.CreateXrPath(_instance, "/user/hand/right");
    _controllerSpacePath[2] = OpenXR.CreateXrPath(_instance, "/user/hand/left");
    _controllerSpacePath[3] = OpenXR.CreateXrPath(_instance, "/user/hand/right");

    // Создаем спейс для контроллера
    _controllerSpace[0] = OpenXR.CreateActionSpace(_session, _action["grip/pose"], _controllerSpacePath[0]);
    _controllerSpace[1] = OpenXR.CreateActionSpace(_session, _action["grip/pose"], _controllerSpacePath[1]);
    _controllerSpace[2] = OpenXR.CreateActionSpace(_session, _action["aim/pose"], _controllerSpacePath[2]);
    _controllerSpace[3] = OpenXR.CreateActionSpace(_session, _action["aim/pose"], _controllerSpacePath[3]);
  }

  public void Sync(long predictedDisplayTime)
  {
    // Сначала синкаем экшены все
    var activeActionSet = new XrActiveActionSet
    {
      ActionSet = ActionSet,
      SubactionPath = 0
    };
    var handle = GCHandle.Alloc(activeActionSet, GCHandleType.Pinned);

    var actionsSyncInfo = new XrActionsSyncInfo
    {
      Type = XrStructureType.XR_TYPE_ACTIONS_SYNC_INFO,
      CountActiveActionSets = 1,
      ActiveActionSets = handle.AddrOfPinnedObject()
    };

    OpenXR.Check(OpenXR.xrSyncActions(_session, ref actionsSyncInfo), "Failed to sync Actions.");


    // Читаем триггер, грип, A, B кнопки
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      OpenXR.GetActionStateFloat(_session, _action["trigger/value"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].TriggerValue);
      OpenXR.GetActionStateFloat(_session, _action["squeeze/value"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].GripValue);
      OpenXR.GetActionStateBoolean(_session, _action["x/click"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].IsPressA);
      OpenXR.GetActionStateBoolean(_session, _action["y/click"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].IsPressB);
      OpenXR.GetActionStateBoolean(_session, _action["a/click"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].IsPressA);
      OpenXR.GetActionStateBoolean(_session, _action["b/click"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].IsPressB);
      OpenXR.GetActionStateVector2(_session, _action["thumbstick"], _controllerPath[i],
        ref VrInput.Headset.Controller[i].ThumbstickDirection);
    }

    UpdateControllerPosition(predictedDisplayTime);

    handle.Free();
  }

  private void UpdateControllerPosition(long predictedDisplayTime)
  {
    var nn = new[] { "grip/pose", "aim/pose" };
    var spaceId = -1;
    foreach (var key in nn)
      for (var i = 0; i < _controllerPath.Length; i++)
      {
        spaceId += 1;

        // Теперь получаем инфу
        var actionStateInfo = new XrActionStateGetInfo
        {
          Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
          Action = _action[key],
          SubactionPath = _controllerSpacePath[spaceId]
        };

        var poseState = new XrActionStatePose { Type = XrStructureType.XR_TYPE_ACTION_STATE_POSE };
        OpenXR.Check(OpenXR.xrGetActionStatePose(_session, ref actionStateInfo, ref poseState),
          "Failed to get Pose State.");

        // Типа получаем положение контроллеров
        var viewInStage = new XrSpaceLocation
        {
          Type = XrStructureType.XR_TYPE_SPACE_LOCATION,
          Pose = new XrPosef
          {
            Orientation = new XrQuaternionf { W = 1.0f },
            Position = new XrVector3f()
          }
        };

        OpenXR.xrLocateSpace(_controllerSpace[spaceId], _space, predictedDisplayTime, ref viewInStage);

        if ((viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0 &&
            (viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0)
        {
          var pose = viewInStage.Pose;

          // Обновляем здесь положение контроллеров
          var mx = Matrix4x4.Identity;
          mx = mx.Translate(pose.Position.X, pose.Position.Y, -pose.Position.Z);
          mx = mx.Rotate(new Quaternion(pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z,
            pose.Orientation.W));

          switch (key)
          {
            case "grip/pose":
              VrInput.Headset.Controller[i].GripTransform = mx;
              break;
            case "aim/pose":
              VrInput.Headset.Controller[i].AimTransform = mx;
              break;
          }
        }
      }
  }
}