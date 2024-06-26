using System;
using System.Runtime.InteropServices;
using MegaLib.OS;
using MegaLib.OS.Api;

namespace MegaLib.VR;

public class VrRuntime
{
  public IntPtr Instance;
  public IntPtr Session;
  public IntPtr LocalSpace;
  public OpenXR.SwapchainInfo[] ColorSwapchainList;
  public OpenXR.SwapchainInfo[] DepthSwapchainList;
  public bool IsSessionRunning;
  public XrSessionState SessionState;
  public Action<XrPosef, XrFovf> OnRender;

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
    }

    // Swapchains
    Console.WriteLine("Enumerate swapchain formats");
    var swapchainFormats = OpenXR.EnumerateSwapchainFormats(sessionId);
    for (var i = 0; i < swapchainFormats.Length; i++) Console.WriteLine($"Swapchain format {swapchainFormats[i]}");

    XrSwapchainFormatGL depthFormat = 0;
    for (var i = 0; i < swapchainFormats.Length; i++)
      /*if ((swapchainFormats[i] & XrSwapchainFormatGL.GL_DEPTH_COMPONENT24) ==
          XrSwapchainFormatGL.GL_DEPTH_COMPONENT24)
      {
        depthFormat = swapchainFormats[i];
        break;
      }*/
      if ((swapchainFormats[i] & XrSwapchainFormatGL.GL_DEPTH_COMPONENT16) ==
          XrSwapchainFormatGL.GL_DEPTH_COMPONENT16)
      {
        depthFormat = swapchainFormats[i];
        break;
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

    // Create space
    var referenceSpace = OpenXR.CreateReferenceSpace(sessionId);

    Console.WriteLine("--- END VR ---");

    // throw new Exception("X");

    Instance = xrInstance;
    LocalSpace = referenceSpace;
    Session = sessionId;
    ColorSwapchainList = colorSwapchainList;
    DepthSwapchainList = depthSwapchainList;
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
          Console.WriteLine("XR Lost");
          break;
        case XrStructureType.XR_TYPE_EVENT_DATA_INSTANCE_LOSS_PENDING:
          Console.WriteLine("XR instance loss");
          IsSessionRunning = false;
          break;
        case XrStructureType.XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED:
          Console.WriteLine("XR Changed");

          var eventDataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<XrEventDataBuffer>());
          Marshal.StructureToPtr(eventData, eventDataPtr, false);
          var sessionStateChanged = Marshal.PtrToStructure<XrEventDataSessionStateChanged>(eventDataPtr);
          Marshal.FreeHGlobal(eventDataPtr);

          Console.WriteLine("Time " + sessionStateChanged.Time);
          Console.WriteLine("State " + sessionStateChanged.State);

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
              Console.WriteLine("SYNCED");
              break;
            case XrSessionState.XR_SESSION_STATE_VISIBLE:
              Console.WriteLine("VISIBLEEE");
              break;
            case XrSessionState.XR_SESSION_STATE_FOCUSED:
              Console.WriteLine("FOCUSEED");
              break;
            default:
              Console.WriteLine($"State {sessionStateChanged.State}");
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

    // Wait frame
    var frameWaitInfo = new XrFrameWaitInfo
    {
      Type = XrStructureType.XR_TYPE_FRAME_WAIT_INFO
    };
    OpenXR.xrWaitFrame(Session, ref frameWaitInfo, ref frameState);

    // Begin
    var frameBeginInfo = new XrFrameBeginInfo { Type = XrStructureType.XR_TYPE_FRAME_BEGIN_INFO };
    Console.WriteLine("BEGIN");
    Console.WriteLine(OpenXR.xrBeginFrame(Session, ref frameBeginInfo));
    rli.PredictedDisplayTime = frameState.PredictedDisplayTime;

    // Render
    rli.Render(Session,
      XrViewConfigurationType.XR_VIEW_CONFIGURATION_TYPE_PRIMARY_STEREO,
      LocalSpace,
      (pose, fov) => { OnRender?.Invoke(pose, fov); });

    // Finish frame
    var frameEndInfo = new XrFrameEndInfo
    {
      Type = XrStructureType.XR_TYPE_FRAME_END_INFO,
      DisplayTime = frameState.PredictedDisplayTime,
      EnvironmentBlendMode = XrEnvironmentBlendMode.XR_ENVIRONMENT_BLEND_MODE_OPAQUE,
      LayerCount = (uint)rli.Layers.Length
    };
    if (rli.Layers.Length > 0) frameEndInfo.Layers = Marshal.UnsafeAddrOfPinnedArrayElement(rli.LayersPointers, 0);

    Console.WriteLine($"End frame. Layers len {rli.Layers.Length}");
    OpenXR.xrEndFrame(Session, ref frameEndInfo);
  }
}