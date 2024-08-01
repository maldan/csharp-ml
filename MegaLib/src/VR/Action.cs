using System;
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

  private IntPtr _poseAction;
  private IntPtr _moveAction;
  private IntPtr _triggerAction;
  private IntPtr _grabAction;
  private IntPtr _aAction;
  private IntPtr _bAction;

  private ulong[] _controllerPath = [0, 0];
  private IntPtr[] _controllerSpace = [IntPtr.Zero, IntPtr.Zero];

  public VrAction(IntPtr instance, IntPtr session, IntPtr space)
  {
    _instance = instance;
    _session = session;
    _space = space;
  }

  public void Init()
  {
    // Создаем сет
    ActionSet = OpenXR.CreateActionSet(_instance, "vr-action-set");

    // Экшен для позиции
    _poseAction = OpenXR.CreateAction(_instance, ActionSet, XrActionType.XR_ACTION_TYPE_POSE_INPUT,
      "hand-position",
      ["/user/hand/left", "/user/hand/right"]);

    // Экшен для перемещения
    _moveAction = OpenXR.CreateAction(_instance, ActionSet, XrActionType.XR_ACTION_TYPE_VECTOR2F_INPUT,
      "joystick-move",
      ["/user/hand/left", "/user/hand/right"]);

    // Экшен для триггера
    _triggerAction = OpenXR.CreateAction(_instance, ActionSet, XrActionType.XR_ACTION_TYPE_FLOAT_INPUT,
      "trigger-press",
      ["/user/hand/left", "/user/hand/right"]);

    // Экшен для захвата
    _grabAction = OpenXR.CreateAction(_instance, ActionSet, XrActionType.XR_ACTION_TYPE_FLOAT_INPUT,
      "hand-grab",
      ["/user/hand/left", "/user/hand/right"]);

    // Создаем экшен для кнопки A (цифровой вход)
    _aAction = OpenXR.CreateAction(_instance, ActionSet, XrActionType.XR_ACTION_TYPE_BOOLEAN_INPUT,
      "a-button-press",
      ["/user/hand/left", "/user/hand/right"]);

    // Создаем экшен для кнопки B (цифровой вход)
    _bAction = OpenXR.CreateAction(_instance, ActionSet, XrActionType.XR_ACTION_TYPE_BOOLEAN_INPUT,
      "b-button-press",
      ["/user/hand/left", "/user/hand/right"]);

    // Биндим всю дичь
    OpenXR.SuggestBindings(_instance, "/interaction_profiles/oculus/touch_controller", [
      new XrActionSuggestedBinding
      {
        Action = _poseAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/grip/pose")
      },
      new XrActionSuggestedBinding
      {
        Action = _poseAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/grip/pose")
      },
      new XrActionSuggestedBinding
      {
        Action = _moveAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/thumbstick")
      },
      new XrActionSuggestedBinding
      {
        Action = _moveAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/thumbstick")
      },
      new XrActionSuggestedBinding
      {
        Action = _triggerAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/trigger/value")
      },
      new XrActionSuggestedBinding
      {
        Action = _triggerAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/trigger/value")
      },
      new XrActionSuggestedBinding
      {
        Action = _grabAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/squeeze/value")
      },
      new XrActionSuggestedBinding
      {
        Action = _grabAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/squeeze/value")
      },
      new XrActionSuggestedBinding
      {
        Action = _aAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/x/click")
      },
      new XrActionSuggestedBinding
      {
        Action = _aAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/a/click")
      },
      new XrActionSuggestedBinding
      {
        Action = _bAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/left/input/y/click")
      },
      new XrActionSuggestedBinding
      {
        Action = _bAction,
        Binding = OpenXR.CreateXrPath(_instance, "/user/hand/right/input/b/click")
      }
    ]);

    // Аттачим сет к сессии
    OpenXR.AttachActionSet(_session, ActionSet);

    _controllerPath[0] = OpenXR.CreateXrPath(_instance, "/user/hand/left");
    _controllerPath[1] = OpenXR.CreateXrPath(_instance, "/user/hand/right");

    // Создаем спейс для контроллера
    _controllerSpace[0] = OpenXR.CreateActionSpace(_session, _poseAction, _controllerPath[0]);
    _controllerSpace[1] = OpenXR.CreateActionSpace(_session, _poseAction, _controllerPath[1]);
  }

  public void Sync()
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
    handle.Free();
  }

  public void UpdateControllerButtons()
  {
    // Считываем триггер
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      var getInfo = new XrActionStateGetInfo
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
        Action = _triggerAction,
        SubactionPath = _controllerPath[i]
      };

      var state = new XrActionStateFloat
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_FLOAT,
        Next = IntPtr.Zero
      };

      OpenXR.Check(OpenXR.xrGetActionStateFloat(_session, ref getInfo, ref state), "Failed to get joystick state.");

      if (state.IsActive == 1)
        if (state.IsActive == 1)
          switch (i)
          {
            case 0:
              VrInput.Headset.Left.Controller.Trigger = state.CurrentState;
              break;
            case 1:
              VrInput.Headset.Right.Controller.Trigger = state.CurrentState;
              break;
          }
    }

    // Считываем состояние grab
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      var getInfo = new XrActionStateGetInfo
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
        Action = _grabAction,
        SubactionPath = _controllerPath[i]
      };

      var state = new XrActionStateFloat
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_FLOAT,
        Next = IntPtr.Zero
      };

      OpenXR.Check(OpenXR.xrGetActionStateFloat(_session, ref getInfo, ref state), "Failed to get joystick state.");

      if (state.IsActive == 1)
        switch (i)
        {
          case 0:
            VrInput.Headset.Left.Controller.Grab = state.CurrentState;
            break;
          case 1:
            VrInput.Headset.Right.Controller.Grab = state.CurrentState;
            break;
        }
    }

    // Кнопки А
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      // Считываем состояние джойстиков
      var getInfo = new XrActionStateGetInfo
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
        Action = _aAction,
        SubactionPath = _controllerPath[i]
      };

      var state = new XrActionStateBoolean
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_BOOLEAN,
        Next = IntPtr.Zero
      };

      OpenXR.Check(OpenXR.xrGetActionStateBoolean(_session, ref getInfo, ref state), "Failed to get joystick state.");

      if (state.IsActive == 1)
        switch (i)
        {
          case 0:
            VrInput.Headset.Left.Controller.IsA = state.CurrentState == 1;
            break;
          case 1:
            VrInput.Headset.Right.Controller.IsA = state.CurrentState == 1;
            break;
        }
    }

    // Кнопки B
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      // Считываем состояние кнопок
      var getInfo = new XrActionStateGetInfo
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
        Action = _aAction,
        SubactionPath = _controllerPath[i]
      };
      var state = new XrActionStateBoolean
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_BOOLEAN,
        Next = IntPtr.Zero
      };

      OpenXR.Check(OpenXR.xrGetActionStateBoolean(_session, ref getInfo, ref state), "Failed to get joystick state.");

      if (state.IsActive == 1)
        switch (i)
        {
          case 0:
            VrInput.Headset.Left.Controller.IsB = state.CurrentState == 1;
            break;
          case 1:
            VrInput.Headset.Right.Controller.IsB = state.CurrentState == 1;
            break;
        }
    }
  }

  public void UpdateControllerJoystick()
  {
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      // Считываем состояние джойстиков
      var getInfo = new XrActionStateGetInfo
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
        Action = _moveAction,
        SubactionPath = _controllerPath[i]
      };

      var state = new XrActionStateVector2f
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_VECTOR2F,
        Next = IntPtr.Zero
      };

      OpenXR.Check(OpenXR.xrGetActionStateVector2f(_session, ref getInfo, ref state), "Failed to get joystick state.");

      if (state.IsActive == 1)
      {
        var x = state.CurrentState.X;
        var y = state.CurrentState.Y;

        // Обрабатывайте значения X и Y по вашему усмотрению
        // Console.WriteLine($"Joystick position {i}: X={x}, Y={y}");
        VrInput.Headset.Left.Controller.JoystickDirection = new Vector2(x, y);
      }
    }
  }

  public void UpdateControllerPosition(long predictedDisplayTime)
  {
    for (var i = 0; i < _controllerPath.Length; i++)
    {
      // Теперь получаем инфу
      var actionStateGetInfo = new XrActionStateGetInfo
      {
        Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
        Action = _poseAction,
        SubactionPath = _controllerPath[i]
      };

      var handPoseState = new XrActionStatePose { Type = XrStructureType.XR_TYPE_ACTION_STATE_POSE };
      OpenXR.Check(OpenXR.xrGetActionStatePose(_session, ref actionStateGetInfo, ref handPoseState),
        "Failed to get Pose State.");

      // Типа получаем положение контроллеров
      var viewInStage = new XrSpaceLocation
      {
        Type = XrStructureType.XR_TYPE_SPACE_LOCATION,
        LocationFlags = 0,
        Pose = new XrPosef
        {
          Orientation = new XrQuaternionf { X = 0, Y = 0, Z = 0, W = 1.0f },
          Position = new XrVector3f { X = 0, Y = 0, Z = 0 }
        }
      };
      OpenXR.xrLocateSpace(_controllerSpace[i], _space, predictedDisplayTime, ref viewInStage);

      if ((viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0 &&
          (viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0)
      {
        var pose = viewInStage.Pose;

        // Обновляем здесь положение контроллеров
        var mx = Matrix4x4.Identity;
        mx = mx.Translate(pose.Position.X, pose.Position.Y, -pose.Position.Z);
        mx = mx.Rotate(new Quaternion(pose.Orientation.X, pose.Orientation.Y, pose.Orientation.Z, pose.Orientation.W));

        switch (i)
        {
          case 0:
            VrInput.Headset.Left.Controller.Transform = mx;
            break;
          case 1:
            VrInput.Headset.Right.Controller.Transform = mx;
            break;
        }
      }
    }
  }
}