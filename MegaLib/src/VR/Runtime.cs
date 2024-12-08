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

  private VrAction VrAction;

  public void InitSession(IntPtr dc, IntPtr glrc)
  {
    Console.WriteLine("--- VR ---");

    // Create instance
    var xrInstance = OpenXR.CreateInstance();
    OpenXR.PrintInstanceVersion(xrInstance);
    OpenXR.InitDebugMessenger(xrInstance);

    //EnumerateApiLayers();
    //EnumerateExtensions();

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

    // Устанавливаем все переменные
    Instance = xrInstance;
    LocalSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL);
    ViewSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_VIEW);
    StageSpace = OpenXR.CreateReferenceSpace(sessionId, XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_STAGE);
    Session = sessionId;
    ColorSwapchainList = colorSwapchainList;
    DepthSwapchainList = depthSwapchainList;

    VrAction = new VrAction(Instance, Session, LocalSpace);
    VrAction.Init();

    InitHandTracking();
  }

  public void InitHandTracking()
  {
    IntPtr xrCreateHandTrackerExtPtr = 0;
    IntPtr xrLocateHandJointsExtPtr = 0;

    var xrCreateHandTrackerExt = Marshal.StringToHGlobalAnsi("xrCreateHandTrackerEXT");
    var xrLocateHandJointsExt = Marshal.StringToHGlobalAnsi("xrLocateHandJointsEXT");

    OpenXR.Check(OpenXR.xrGetInstanceProcAddr(
      Instance,
      xrCreateHandTrackerExt,
      ref xrCreateHandTrackerExtPtr
    ), "Failed xrCreateHandTrackerEXT");

    OpenXR.Check(OpenXR.xrGetInstanceProcAddr(
      Instance,
      xrLocateHandJointsExt,
      ref xrLocateHandJointsExtPtr
    ), "Failed xrLocateHandJointsEXT");

    var xrCreateHandTrackerEXT =
      Marshal.GetDelegateForFunctionPointer<xrCreateHandTrackerEXTDelegate>(xrCreateHandTrackerExtPtr);
    var xrLocateHandJointsEXT =
      Marshal.GetDelegateForFunctionPointer<xrLocateHandJointsEXTDelegate>(xrLocateHandJointsExtPtr);

    var createLeftInfo = new XrHandTrackerCreateInfoEXT
    {
      Type = XrStructureType.XR_TYPE_HAND_TRACKER_CREATE_INFO_EXT,
      Next = IntPtr.Zero,
      Hand = XrHandEXT.XR_HAND_LEFT_EXT, // or XR_HAND_RIGHT_EXT
      HandJointSet = XrHandJointSetEXT.XR_HAND_JOINT_SET_DEFAULT_EXT
    };
    var createRightInfo = new XrHandTrackerCreateInfoEXT
    {
      Type = XrStructureType.XR_TYPE_HAND_TRACKER_CREATE_INFO_EXT,
      Next = IntPtr.Zero,
      Hand = XrHandEXT.XR_HAND_RIGHT_EXT, // or XR_HAND_RIGHT_EXT
      HandJointSet = XrHandJointSetEXT.XR_HAND_JOINT_SET_DEFAULT_EXT
    };

    IntPtr leftHandTracker = 0;
    IntPtr rightHandTracker = 0;
    OpenXR.Check(xrCreateHandTrackerEXT(Session, ref createLeftInfo, ref leftHandTracker),
      "Failed xrCreateHandTrackerEXT");
    OpenXR.Check(xrCreateHandTrackerEXT(Session, ref createRightInfo, ref rightHandTracker),
      "Failed xrCreateHandTrackerEXT");
    VrAction.LeftHandTracker = leftHandTracker;
    VrAction.RightHandTracker = rightHandTracker;
    VrAction.LocateHandJoints = xrLocateHandJointsEXT;
  }

  public void EnumerateApiLayers()
  {
    Console.WriteLine("Enumerate api layers");

    uint layerCount = 0;
    // First call to get the number of API layers
    OpenXR.Check(OpenXR.xrEnumerateApiLayerProperties(0, ref layerCount, IntPtr.Zero),
      "Failed to get API layer count.");
    if (layerCount == 0)
    {
      Console.WriteLine("No API layers found.");
      return;
    }

    // Allocate memory for the API layer properties
    var layers = new XrApiLayerProperties[layerCount];
    for (uint i = 0; i < layerCount; i++)
    {
      layers[i].Type = XrStructureType.XR_TYPE_API_LAYER_PROPERTIES;
      layers[i].Next = IntPtr.Zero;
    }

    // Pin the array in memory
    var handle = GCHandle.Alloc(layers, GCHandleType.Pinned);
    try
    {
      var layersPtr = handle.AddrOfPinnedObject();

      // Second call to get the API layer properties
      OpenXR.Check(
        OpenXR.xrEnumerateApiLayerProperties(layerCount, ref layerCount, layersPtr),
        "Failed to enumerate API layers.");

      // Display the API layers
      for (uint i = 0; i < layerCount; i++)
      {
        Console.WriteLine($"Layer Name: {layers[i].LayerName}");
        Console.WriteLine($"Description: {layers[i].Description}");
        Console.WriteLine($"Spec Version: {layers[i].SpecVersion}");
        Console.WriteLine($"Layer Version: {layers[i].LayerVersion}");
        Console.WriteLine();
      }
    }
    finally
    {
      // Free the pinned handle
      handle.Free();
    }
  }

  public void EnumerateExtensions()
  {
    // Enumerate extensions
    Console.WriteLine("Enumerate extensions");
    uint extensionCount = 0;
    OpenXR.Check(OpenXR.xrEnumerateInstanceExtensionProperties(
      IntPtr.Zero, 0, ref extensionCount, IntPtr.Zero
    ), "Failed to enumerate extensions");
    Console.WriteLine($"Count {extensionCount}");
    var extensions = new XrExtensionProperties[extensionCount];
    for (var i = 0; i < extensions.Length; i++)
    {
      extensions[i].Type = XrStructureType.XR_TYPE_EXTENSION_PROPERTIES;
    }

    var extPtr = Marshal.UnsafeAddrOfPinnedArrayElement(extensions, 0);

    // Second call to get the extension properties
    OpenXR.Check(OpenXR.xrEnumerateInstanceExtensionProperties(
      IntPtr.Zero,
      extensionCount,
      ref extensionCount, extPtr), "Failed to enumerate extensions");

    for (var i = 0; i < extensionCount; i++)
    {
      Console.WriteLine($"Ext: {extensions[i].GetExtensionName()} ({extensions[i].ExtensionVersion})");
    }
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
    OpenXR.xrLocateSpace(ViewSpace, LocalSpace, predictedDisplayTime, ref viewInStage);

    if ((viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0 &&
        (viewInStage.LocationFlags & OpenXR.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0)
      return viewInStage.Pose;

    // Возвращение стандартной позы при ошибке
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

    var frameState = new XrFrameState { Type = XrStructureType.XR_TYPE_FRAME_STATE };
    var rli = new OpenXR.RenderLayerInfo();
    rli.Init();
    rli.ColorSwapchainList = ColorSwapchainList;
    rli.DepthSwapchainList = DepthSwapchainList;

    // Ждем фрейм
    var frameWaitInfo = new XrFrameWaitInfo
    {
      Type = XrStructureType.XR_TYPE_FRAME_WAIT_INFO
    };
    OpenXR.xrWaitFrame(Session, ref frameWaitInfo, ref frameState);

    // Приложение в состоянии фокуса. Значит может считывать экшены
    if (SessionState == XrSessionState.XR_SESSION_STATE_FOCUSED)
    {
      // Обновляем экшены
      VrAction.Sync(frameState.PredictedDisplayTime);

      // Считываем позицию шлема и ориентацию
      var headPose = GetHeadsetPose(frameState.PredictedDisplayTime);
      var mx = Matrix4x4.Identity;
      mx = mx.Translate(headPose.Position.X, headPose.Position.Y, headPose.Position.Z);
      mx = mx.Rotate(new Quaternion(headPose.Orientation.X, headPose.Orientation.Y, headPose.Orientation.Z,
        headPose.Orientation.W));
      VrInput.Headset.LocalTransform = mx;
    }
    else
    {
      VrAction.UpdateHandsTracking(frameState.PredictedDisplayTime);
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
  }
}