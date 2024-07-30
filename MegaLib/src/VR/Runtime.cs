using System;
using System.Runtime.InteropServices;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using Console = System.Console;
using XrAction = System.IntPtr;
using XrPath = ulong;
using XrActionSet = System.IntPtr;

namespace MegaLib.VR;

public class VrRuntime
{
  public IntPtr Instance;
  public IntPtr Session;
  public IntPtr LocalSpace;
  public IntPtr ViewSpace;
  public IntPtr StageSpace;

  public IntPtr LeftControllerSpace;
  public IntPtr RightControllerSpace;

  public OpenXR.SwapchainInfo[] ColorSwapchainList;
  public OpenXR.SwapchainInfo[] DepthSwapchainList;
  public bool IsSessionRunning;
  public XrSessionState SessionState;
  public Action<long, XrPosef, XrFovf> OnRender;
  public uint ViewWidth;
  public uint ViewHeight;

  public XrAction PoseAction;
  public XrPath LeftControllerPath;
  public XrActionSet ActionSet;

  private bool Gas;

  public void InitSession(IntPtr dc, IntPtr glrc)
  {
    Console.WriteLine("--- VR ---");

    // Create instance
    var xrInstance = OpenXR.CreateInstance();
    OpenXR.PrintInstanceVersion(xrInstance);

    // System info
    var systemId = OpenXR.GetSystemId(xrInstance);
    var systemProperties = OpenXR.GetSystemInfo(xrInstance, systemId);
    Console.WriteLine(systemProperties.SystemName);

    // Requirements call
    var req = OpenXR.GraphicsRequirements(xrInstance, systemId);
    req.PrintVersion();

    // Create session
    var sessionId = OpenXR.CreateOpenGlSession(xrInstance, systemId, dc, glrc);
    //Console.WriteLine($"CurrentDC {window.CurrentDC}");
    //Console.WriteLine($"CurrentGLRC {window.CurrentGLRC}");
    Console.WriteLine($"Session id {sessionId}");

    // Get views
    Console.WriteLine("Enumerate views");
    var viewList = OpenXR.GetViewConfigurationList(
      xrInstance,
      systemId,
      XrViewConfigurationType.XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO
    );
    for (var i = 0; i < viewList.Length; i++)
    {
      Console.Write($"View {i + 1}: ");
      viewList[i].PrintInfo();

      // Задаем текущую ширину и высоту вью
      ViewWidth = viewList[i].RecommendedImageRectWidth;
      ViewHeight = viewList[i].RecommendedImageRectHeight;
    }

    // Swapchains
    Console.WriteLine("Enumerate swapchain formats");
    var swapchainFormats = OpenXR.EnumerateSwapchainFormats(sessionId);
    for (var i = 0; i < swapchainFormats.Length; i++) Console.WriteLine($"Swapchain format {swapchainFormats[i]}");

    XrSwapchainFormatGL depthFormat = 0;
    for (var i = 0; i < swapchainFormats.Length; i++)
    {
      if ((swapchainFormats[i] & XrSwapchainFormatGL.GL_DEPTH_COMPONENT32F) ==
          XrSwapchainFormatGL.GL_DEPTH_COMPONENT32F)
      {
        depthFormat = swapchainFormats[i];
        break;
      }

      if ((swapchainFormats[i] & XrSwapchainFormatGL.GL_DEPTH_COMPONENT24) ==
          XrSwapchainFormatGL.GL_DEPTH_COMPONENT24)
        depthFormat = swapchainFormats[i];

      if ((swapchainFormats[i] & XrSwapchainFormatGL.GL_DEPTH_COMPONENT16) ==
          XrSwapchainFormatGL.GL_DEPTH_COMPONENT16)
        depthFormat = swapchainFormats[i];
    }

    // Create swapchain
    var colorSwapchainList = new OpenXR.SwapchainInfo[viewList.Length];
    for (var i = 0; i < colorSwapchainList.Length; i++)
      colorSwapchainList[i] = OpenXR.CreateSwapchain(
        sessionId, viewList[i], swapchainFormats[3],
        viewList[i].RecommendedSwapchainSampleCount, OpenXR.SwapchainType.Color);

    var depthSwapchainList = new OpenXR.SwapchainInfo[viewList.Length];
    for (var i = 0; i < depthSwapchainList.Length; i++)
      depthSwapchainList[i] = OpenXR.CreateSwapchain(
        sessionId, viewList[i], depthFormat,
        viewList[i].RecommendedSwapchainSampleCount, OpenXR.SwapchainType.Depth);

    // Get swapchain images
    Console.WriteLine("Enumerate color swapchain images");
    for (var i = 0; i < colorSwapchainList.Length; i++)
    {
      Console.WriteLine($"Swapchain {i + 1}");
      colorSwapchainList[i].OpenGL_Images = OpenXR.OpenGL_EnumerateSwapchainImages(colorSwapchainList[i].Swapchain);
      // Generate frame buffers
      //colorSwapchainList[i].GenerateFrameBuffers();
      colorSwapchainList[i].PrintInfo();
    }

    Console.WriteLine("Enumerate depth swapchain images");
    for (var i = 0; i < depthSwapchainList.Length; i++)
    {
      Console.WriteLine($"Swapchain {i + 1}");
      depthSwapchainList[i].OpenGL_Images = OpenXR.OpenGL_EnumerateSwapchainImages(depthSwapchainList[i].Swapchain);
      // Generate frame buffers
      //depthSwapchainList[i].GenerateFrameBuffers();
      depthSwapchainList[i].PrintInfo();
    }

    Console.WriteLine("Enumerate environment blend modes");
    var blendModes = OpenXR.EnumerateEnvironmentBlendModes(xrInstance, systemId,
      XrViewConfigurationType.XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO);
    for (var i = 0; i < blendModes.Length; i++) Console.WriteLine(blendModes[i]);

    Console.WriteLine("--- END VR ---");

    // throw new Exception("X");

    Instance = xrInstance;
    LocalSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL);
    ViewSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_VIEW);
    StageSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_STAGE);

    //LeftControllerSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL);
    //RightControllerSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL);

    Session = sessionId;
    ColorSwapchainList = colorSwapchainList;
    DepthSwapchainList = depthSwapchainList;

    // Теперь настраиваем управление
    ActionSet = OpenXR.CreateActionSet(xrInstance, "vr-action-set");
    PoseAction = OpenXR.CreateAction(xrInstance, ActionSet, XrActionType.XR_ACTION_TYPE_POSE_INPUT,
      "hand-position",
      ["/user/hand/left", "/user/hand/right"]);
    LeftControllerPath = OpenXR.CreateXrPath(xrInstance, "/user/hand/left");

    OpenXR.SuggestBindings(Instance, "/interaction_profiles/khr/simple_controller", [
      new XrActionSuggestedBinding
      {
        Action = PoseAction,
        Binding = OpenXR.CreateXrPath(Instance, "/user/hand/left/input/grip/pose")
      }
    ]);
    OpenXR.AttachActionSet(Session, ActionSet);

    LeftControllerSpace = OpenXR.CreateActionSpace(sessionId, PoseAction, LeftControllerPath);
  }

  public XrPosef GetHeadsetPose(long predictedDisplayTime)
  {
    var viewInStage = new XrSpaceLocation()
    {
      Type = XrStructureType.XR_TYPE_SPACE_LOCATION,
      LocationFlags = 0,
      Pose = new XrPosef()
      {
        Orientation = new XrQuaternionf() { X = 0, Y = 0, Z = 0, W = 1.0f },
        Position = new XrVector3f() { X = 0, Y = 0, Z = 0 }
      }
    };
    OpenXR.xrLocateSpace(ViewSpace, StageSpace, predictedDisplayTime, ref viewInStage);

    if ((viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0 &&
        (viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0)
      return viewInStage.Pose;

    // Возвращение стандартной позы при ошибке
    var invalidPose = new XrPosef() { };
    invalidPose.Orientation.W = 1.0f;
    return invalidPose;
  }

  public XrPosef GetControllerPose(long predictedDisplayTime)
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

    OpenXR.Check(OpenXR.xrSyncActions(Session, ref actionsSyncInfo), "Failed to sync Actions.");

    // Теперь получаем инфу
    var actionStateGetInfo = new XrActionStateGetInfo
    {
      Type = XrStructureType.XR_TYPE_ACTION_STATE_GET_INFO,
      Action = PoseAction,
      SubactionPath = LeftControllerPath
    };

    var handPoseState = new XrActionStatePose { Type = XrStructureType.XR_TYPE_ACTION_STATE_POSE };
    OpenXR.Check(OpenXR.xrGetActionStatePose(Session, ref actionStateGetInfo, ref handPoseState),
      "Failed to get Pose State.");

    // Console.WriteLine(handPoseState.IsActive);

    var viewInStage = new XrSpaceLocation()
    {
      Type = XrStructureType.XR_TYPE_SPACE_LOCATION,
      LocationFlags = 0,
      Pose = new XrPosef()
      {
        Orientation = new XrQuaternionf() { X = 0, Y = 0, Z = 0, W = 1.0f },
        Position = new XrVector3f() { X = 0, Y = 0, Z = 0 }
      }
    };
    // Console.WriteLine($"{LeftControllerSpace} {LocalSpace} {predictedDisplayTime}");
    OpenXR.xrLocateSpace(LeftControllerSpace, LocalSpace, predictedDisplayTime, ref viewInStage);

    if ((viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0 &&
        (viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0)
    {
      handle.Free();
      return viewInStage.Pose;
    }

    // Возвращение стандартной позы при ошибке
    handle.Free();
    var invalidPose = new XrPosef() { };
    invalidPose.Orientation.W = 1.0f;
    return invalidPose;
  }

  public void Tick()
  {
    // Read events
    while (true)
    {
      var (status, eventData) = OpenXR.xrPollEvents(Instance);
      if (status != XrResult.XR_SUCCESS) break;
      /*if (!(status == XrResult.XR_SUCCESS || status == XrResult.XR_EVENT_UNAVAILABLE))
      {
        Console.WriteLine($"xrPollEvent {status}");
      }*/

      // Console.WriteLine(status);
      switch (eventData.Type)
      {
        case XrStructureType.XR_TYPE_EVENT_DATA_EVENTS_LOST:
          // Console.WriteLine("XR Lost");
          break;
        case XrStructureType.XR_TYPE_EVENT_DATA_INSTANCE_LOSS_PENDING:
          // Console.WriteLine("XR instance loss");
          IsSessionRunning = false;
          break;
        case XrStructureType.XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED:
          // Console.WriteLine("XR Changed");

          var eventDataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<XrEventDataBuffer>());
          Marshal.StructureToPtr(eventData, eventDataPtr, false);
          var sessionStateChanged = Marshal.PtrToStructure<XrEventDataSessionStateChanged>(eventDataPtr);
          Marshal.FreeHGlobal(eventDataPtr);

          // Console.WriteLine("Time " + sessionStateChanged.Time);
          // Console.WriteLine("State " + sessionStateChanged.State);

          switch (sessionStateChanged.State)
          {
            case XrSessionState.XR_SESSION_STATE_READY:
              var sessionBeginInfo = new XrSessionBeginInfo
              {
                Type = XrStructureType.XR_TYPE_SESSION_BEGIN_INFO,
                PrimaryViewConfigurationType = XrViewConfigurationType.XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO
              };
              Console.WriteLine(OpenXR.xrBeginSession(Session, ref sessionBeginInfo));

              IsSessionRunning = true;
              break;
            case XrSessionState.XR_SESSION_STATE_STOPPING:
              OpenXR.xrEndSession(Session);
              IsSessionRunning = false;
              break;
            case XrSessionState.XR_SESSION_STATE_EXITING:
              IsSessionRunning = false;
              break;
            case XrSessionState.XR_SESSION_STATE_LOSS_PENDING:
              IsSessionRunning = false;
              break;
            case XrSessionState.XR_SESSION_STATE_SYNCHRONIZED:
              // Console.WriteLine("SYNCED");
              break;
            case XrSessionState.XR_SESSION_STATE_VISIBLE:
              // Console.WriteLine("VISIBLEEE");
              break;
            case XrSessionState.XR_SESSION_STATE_FOCUSED:
              // Console.WriteLine("FOCUSEED");
              break;
            default:
              // Console.WriteLine($"State {sessionStateChanged.State}");
              break;
          }

          SessionState = sessionStateChanged.State;

          break;
        default:
          break;
      }
    }

    if (!IsSessionRunning) return;

    // Render
    var sessionActive = SessionState == XrSessionState.XR_SESSION_STATE_SYNCHRONIZED ||
                        SessionState == XrSessionState.XR_SESSION_STATE_VISIBLE ||
                        SessionState == XrSessionState.XR_SESSION_STATE_FOCUSED;

    // if (!sessionActive) return;

    var frameState = new XrFrameState { Type = XrStructureType.XR_TYPE_FRAME_STATE };
    var rli = new OpenXR.RenderLayerInfo();
    rli.Init();
    rli.ColorSwapchainList = ColorSwapchainList;
    rli.DepthSwapchainList = DepthSwapchainList;

    // OpenXR.xrSyncActions(Session, );

    // Wait frame
    var frameWaitInfo = new XrFrameWaitInfo
    {
      Type = XrStructureType.XR_TYPE_FRAME_WAIT_INFO
    };
    OpenXR.xrWaitFrame(Session, ref frameWaitInfo, ref frameState);

    //var posex = GetHeadsetPose(frameState.PredictedDisplayTime);
    //Console.WriteLine(posex);

    if (SessionState == XrSessionState.XR_SESSION_STATE_FOCUSED && Gas)
    {
      var posex2 = GetControllerPose(frameState.PredictedDisplayTime);
      // Console.WriteLine(posex2);
      var mx = Matrix4x4.Identity;
      mx = mx.Translate(posex2.Position.X, posex2.Position.Y, -posex2.Position.Z);
      mx = mx.Rotate(new Quaternion(posex2.Orientation.X, posex2.Orientation.Y, posex2.Orientation.Z,
        posex2.Orientation.W));
      VrInput.Headset.Left.Controller.Transform = mx;
    }

    // Begin
    var frameBeginInfo = new XrFrameBeginInfo { Type = XrStructureType.XR_TYPE_FRAME_BEGIN_INFO };
    // Console.WriteLine("BEGIN");
    OpenXR.xrBeginFrame(Session, ref frameBeginInfo);
    rli.PredictedDisplayTime = frameState.PredictedDisplayTime;

    // Render
    if (sessionActive)
      rli.Render(Session,
        XrViewConfigurationType.XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO,
        LocalSpace,
        (pose, fov) => { OnRender?.Invoke(frameState.PredictedDisplayTime, pose, fov); });

    // Finish frame
    var frameEndInfo = new XrFrameEndInfo
    {
      Type = XrStructureType.XR_TYPE_FRAME_END_INFO,
      DisplayTime = frameState.PredictedDisplayTime,
      EnvironmentBlendMode = XrEnvironmentBlendMode.XR_ENVIRONMENT_BLEND_MODE_OPAQUE,
      LayerCount = (uint)rli.Layers.Length
    };
    if (rli.Layers.Length > 0) frameEndInfo.Layers = Marshal.UnsafeAddrOfPinnedArrayElement(rli.LayersPointers, 0);

    // Console.WriteLine($"End frame. Layers len {rli.Layers.Length}");
    OpenXR.xrEndFrame(Session, ref frameEndInfo);

    Gas = true;
  }
}