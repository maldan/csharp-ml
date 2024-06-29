using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GLint = int;
using GLsizei = int;

namespace MegaLib.OS.Api;

using XrFlags64 = ulong;
using XrInstanceCreateFlags = ulong;
using XrVersion = ulong;
using XrInstance = IntPtr;
using XrSession = IntPtr;
using XrSpace = IntPtr;
using XrSwapchain = IntPtr;
using XrSystemId = ulong;
using XrBool32 = uint;
using XrTime = long;

public static partial class OpenXR
{
  public static XrInstance CreateInstance()
  {
    // App info
    var applicationInfo = new XrApplicationInfo
    {
      ApplicationName = "VR Simulator",
      ApplicationVersion = 1,
      EngineName = "MegaEngine",
      EngineVersion = 1,
      ApiVersion = XR_CURRENT_API_VERSION
    };

    // Create info
    var createInfo = new XrInstanceCreateInfo
    {
      Type = XrStructureType.XR_TYPE_INSTANCE_CREATE_INFO,
      CreateFlags = 0,
      ApplicationInfo = applicationInfo
    };

    // Add extensions
    createInfo.EnableExtensions(new[] { "XR_EXT_debug_utils", "XR_KHR_opengl_enable" });

    // Try to create
    var xrInstance = XrInstance.Zero;
    Check(xrCreateInstance(ref createInfo, ref xrInstance), "Can't create instance");

    return xrInstance;
  }

  public static void EnumerateApiLayerProperties()
  {
    // Get counts
    uint amount = 0;
    Check(
      xrEnumerateApiLayerProperties(0, ref amount, null),
      "Can't get amount of api layers"
    );

    // Get layers
    var layers = new XrApiLayerProperties[amount];
    for (var i = 0; i < amount; i++) layers[i].Type = XrStructureType.XR_TYPE_API_LAYER_PROPERTIES;

    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(layers, 0);
    Check(
      xrEnumerateApiLayerProperties(amount, ref amount, ptr),
      "Can't get api layers"
    );

    for (var i = 0; i < layers.Length; i++) Console.WriteLine(layers[i].LayerName);

    /*var instanceProperties = new XrInstanceProperties()
    {
      Type = XrStructureType.XR_TYPE_INSTANCE_PROPERTIES,
    };
    Console.WriteLine(OpenXR.xrGetInstanceProperties(xrInstance, ref instanceProperties));
    instanceProperties.PrintVersion();*/
  }

  public static void PrintInstanceVersion(XrInstance xrInstance)
  {
    var instanceProperties = new XrInstanceProperties
    {
      Type = XrStructureType.XR_TYPE_INSTANCE_PROPERTIES
    };
    Check(xrGetInstanceProperties(xrInstance, ref instanceProperties));
    instanceProperties.PrintVersion();
  }

  /*public static void EnumerateInstanceExtensionProperties()
  {
    // Get all the Instance Extensions from the OpenXR instance.
    uint count = 0;
    Check(
      xrEnumerateInstanceExtensionProperties(IntPtr.Zero, 0, ref count, IntPtr.Zero),
      "Failed to enumerate InstanceExtensionProperties."
    );
    Console.WriteLine(count);

    var extensionProperties = new XrExtensionProperties[count];
    for (var i = 0; i < count; i++)
    {
      extensionProperties[i].Type = XrStructureType.XR_TYPE_EXTENSION_PROPERTIES;
    }

    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(extensionProperties, 0);
    Check(
      xrEnumerateInstanceExtensionProperties(IntPtr.Zero, count, ref count, ptr),
      "Failed to enumerate InstanceExtensionProperties."
    );

    for (var i = 0; i < count; i++)
    {
      //Console.WriteLine(extensionProperties[i].ExtensionName + " " + extensionProperties[i].ExtensionVersion);
    }
  }*/

  public static XrSystemId GetSystemId(XrInstance xrInstance)
  {
    var systemGetInfo = new XrSystemGetInfo
    {
      Type = XrStructureType.XR_TYPE_SYSTEM_GET_INFO,
      FormFactor = XrFormFactor.XR_FORM_FACTOR_HEAD_MOUNTED_DISPLAY
    };
    ulong systemId = 0;
    Check(xrGetSystem(xrInstance, ref systemGetInfo, ref systemId));
    return systemId;
  }

  public static XrSystemProperties GetSystemInfo(XrInstance xrInstance, XrSystemId systemId)
  {
    var systemProperties = new XrSystemProperties
      { Type = XrStructureType.XR_TYPE_SYSTEM_PROPERTIES };
    Check(xrGetSystemProperties(xrInstance, systemId, ref systemProperties));
    return systemProperties;
  }

  public static XrGraphicsRequirementsOpenGLKHR GraphicsRequirements(XrInstance xrInstance, XrSystemId systemId)
  {
    var req = new XrGraphicsRequirementsOpenGLKHR
    {
      Type = XrStructureType.XR_TYPE_GRAPHICS_REQUIREMENTS_OPENGL_KHR
    };
    Check(xrGetOpenGLGraphicsRequirementsKHR(xrInstance, systemId, ref req));
    return req;
  }

  public static XrSession CreateSession(Window window, XrInstance xrInstance, XrSystemId systemId)
  {
    // Create opengl binding
    var openGlBinding = new XrGraphicsBindingOpenGLWin32KHR
    {
      Type = XrStructureType.XR_TYPE_GRAPHICS_BINDING_OPENGL_WIN32_KHR,
      Next = IntPtr.Zero,
      hDC = window.CurrentDeviceContext,
      hGLRC = window.CurrentGLRC
    };
    var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<XrGraphicsBindingOpenGLWin32KHR>());
    Marshal.StructureToPtr(openGlBinding, ptr, false);

    // Create Session
    var sessionCreateInfo = new XrSessionCreateInfo
    {
      Type = XrStructureType.XR_TYPE_SESSION_CREATE_INFO,
      Next = ptr,
      SystemId = systemId
    };
    var sessionId = IntPtr.Zero;
    Check(xrCreateSession(xrInstance, ref sessionCreateInfo, ref sessionId));
    return sessionId;
  }

  public static XrSession CreateOpenGlSession(XrInstance xrInstance, XrSystemId systemId, IntPtr dc, IntPtr glrc)
  {
    // Create opengl binding
    var openGlBinding = new XrGraphicsBindingOpenGLWin32KHR
    {
      Type = XrStructureType.XR_TYPE_GRAPHICS_BINDING_OPENGL_WIN32_KHR,
      Next = IntPtr.Zero,
      hDC = dc,
      hGLRC = glrc
    };
    var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<XrGraphicsBindingOpenGLWin32KHR>());
    Marshal.StructureToPtr(openGlBinding, ptr, false);

    // Create Session
    var sessionCreateInfo = new XrSessionCreateInfo
    {
      Type = XrStructureType.XR_TYPE_SESSION_CREATE_INFO,
      Next = ptr,
      SystemId = systemId
    };
    var sessionId = IntPtr.Zero;
    Check(xrCreateSession(xrInstance, ref sessionCreateInfo, ref sessionId));
    return sessionId;
  }

  /*public static void ViewTypes(XrInstance xrInstance, XrSystemId systemId)
  {
    Console.WriteLine("View types");
    uint count = 0;
    Check(xrEnumerateViewConfigurations(
      xrInstance,
      systemId,
      0,
      ref count,
      IntPtr.Zero
    ));

    var viewConfigurations = new XrViewConfigurationType[count];
    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(viewConfigurations, 0);
    Check(xrEnumerateViewConfigurations(
      xrInstance,
      systemId,
      count,
      ref count,
      ptr
    ));

    foreach (var view in viewConfigurations)
    {
      Console.WriteLine(view);
    }
  }*/

  public static XrViewConfigurationView[] GetViewConfigurationList(
    XrInstance xrInstance,
    XrSystemId systemId,
    XrViewConfigurationType type)
  {
    // Get amount of views
    uint count = 0;
    Check(
      xrEnumerateViewConfigurationViews(
        xrInstance,
        systemId,
        type,
        0,
        ref count,
        IntPtr.Zero)
    );

    // Get views
    var viewConfigurationViews = new XrViewConfigurationView[count];
    for (var i = 0; i < viewConfigurationViews.Length; i++)
      viewConfigurationViews[i].Type = XrStructureType.XR_TYPE_VIEW_CONFIGURATION_VIEW;

    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(viewConfigurationViews, 0);
    Check(
      xrEnumerateViewConfigurationViews(
        xrInstance,
        systemId,
        type,
        count,
        ref count,
        ptr)
    );

    /*for (var i = 0; i < viewConfigurationViews.Length; i++)
    {
      Console.WriteLine("Type: " + viewConfigurationViews[i].Type);
      Console.WriteLine(viewConfigurationViews[i].RecommendedImageRectWidth);
      Console.WriteLine(viewConfigurationViews[i].RecommendedImageRectHeight);
      Console.WriteLine(viewConfigurationViews[i].MaxImageRectHeight);
      Console.WriteLine(viewConfigurationViews[i].MaxImageRectWidth);
      Console.WriteLine(viewConfigurationViews[i].MaxSwapchainSampleCount);
    }*/

    return viewConfigurationViews;
  }

  public static XrSwapchainFormatGL[] EnumerateSwapchainFormats(XrSession sessionId)
  {
    uint count = 0;
    Check(xrEnumerateSwapchainFormats(sessionId, 0, ref count, null));
    var formats = new long[count];
    Check(xrEnumerateSwapchainFormats(sessionId, count, ref count, formats));

    var formatsX = new XrSwapchainFormatGL[count];
    for (var i = 0; i < count; i++) formatsX[i] = (XrSwapchainFormatGL)formats[i];
    return formatsX;
  }

  public struct RenderLayerInfo
  {
    public XrTime PredictedDisplayTime;
    public XrCompositionLayerProjection[] Layers;
    public IntPtr[] LayersPointers;
    public XrCompositionLayerProjection LayerProjection;
    public XrCompositionLayerProjectionView[] LayerProjectionViews;
    public SwapchainInfo[] ColorSwapchainList;
    public SwapchainInfo[] DepthSwapchainList;

    public void Init()
    {
      Layers = Array.Empty<XrCompositionLayerProjection>();
      LayersPointers = Array.Empty<IntPtr>();
      LayerProjectionViews = Array.Empty<XrCompositionLayerProjectionView>();
      ColorSwapchainList = Array.Empty<SwapchainInfo>();
      DepthSwapchainList = Array.Empty<SwapchainInfo>();
    }

    public void Render(
      XrSession sessionId,
      XrViewConfigurationType m_viewConfiguration,
      XrSpace space,
      Action<XrPosef, XrFovf> OnRender
    )
    {
      // Prepare views
      var views = new XrView[ColorSwapchainList.Length];
      for (var i = 0; i < views.Length; i++) views[i].Type = XrStructureType.XR_TYPE_VIEW;

      var viewsPtr = Marshal.UnsafeAddrOfPinnedArrayElement(views, 0);
      var viewState = new XrViewState
      {
        Type = XrStructureType.XR_TYPE_VIEW_STATE
      };
      var viewLocateInfo = new XrViewLocateInfo
      {
        Type = XrStructureType.XR_TYPE_VIEW_LOCATE_INFO,
        ViewConfigurationType = m_viewConfiguration,
        DisplayTime = PredictedDisplayTime,
        Space = space
      };
      uint viewCount = 0;
      Check(
        xrLocateViews(
          sessionId,
          ref viewLocateInfo,
          ref viewState,
          (uint)views.Length,
          ref viewCount,
          viewsPtr
        )
      );
      LayerProjectionViews = new XrCompositionLayerProjectionView[ColorSwapchainList.Length];

      // Per swapchain
      for (var i = 0; i < ColorSwapchainList.Length; i++)
      {
        Console.WriteLine($"VIEW: {views[i].Pose} {views[i].Fov}");

        var colorSwapchainImageId = ColorSwapchainList[i].AcquireSwapchainImage();
        var depthSwapchainImageId = DepthSwapchainList[i].AcquireSwapchainImage();

        // Set views
        LayerProjectionViews[i].Type = XrStructureType.XR_TYPE_COMPOSITION_LAYER_PROJECTION_VIEW;
        LayerProjectionViews[i].Pose = views[i].Pose;
        LayerProjectionViews[i].Fov = views[i].Fov;
        LayerProjectionViews[i].SubImage.Swapchain = ColorSwapchainList[i].Swapchain;
        LayerProjectionViews[i].SubImage.ImageRect.Offset.X = 0;
        LayerProjectionViews[i].SubImage.ImageRect.Offset.Y = 0;
        LayerProjectionViews[i].SubImage.ImageRect.Extent.Width =
          (int)ColorSwapchainList[i].View.RecommendedImageRectWidth;
        LayerProjectionViews[i].SubImage.ImageRect.Extent.Height =
          (int)ColorSwapchainList[i].View.RecommendedImageRectHeight;
        LayerProjectionViews[i].SubImage.ImageArrayIndex = 0;

        // Set viewport
        var width = ColorSwapchainList[i].View.RecommendedImageRectWidth;
        var height = ColorSwapchainList[i].View.RecommendedImageRectHeight;
        OpenGL32.glViewport(
          0, 0,
          (int)ColorSwapchainList[i].View.RecommendedImageRectWidth,
          (int)ColorSwapchainList[i].View.RecommendedImageRectHeight);

        // Clear color
        /*OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, ColorSwapchainList[i].FrameBuffer[colorSwapchainImageId]);
        OpenGL32.glClearColor(0, 0, 0, 1.0f);
        OpenGL32.glClear(OpenGL32.GL_DEPTH_BUFFER_BIT);
        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);

        // Clear depth
        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, DepthSwapchainList[i].FrameBuffer[depthSwapchainImageId]);
        OpenGL32.glClearDepth(1.0f);
        OpenGL32.glClear(OpenGL32.GL_DEPTH_BUFFER_BIT);
        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);*/

        // Console.WriteLine($"GL BIND {OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER)}");

        // Bind image
        uint setFramebuffer = 0;
        OpenGL32.glGenFramebuffers(1, ref setFramebuffer);
        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, setFramebuffer);

        OpenGL32.glFramebufferTexture2D(
          OpenGL32.GL_FRAMEBUFFER,
          OpenGL32.GL_COLOR_ATTACHMENT0,
          OpenGL32.GL_TEXTURE_2D,
          ColorSwapchainList[i].OpenGL_Images[colorSwapchainImageId].Image,
          0
        );

        // Генерация и настройка глубинного буфера
        /*uint depthTexture = 0;
        OpenGL32.glGenTextures(1, ref depthTexture);
        OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, depthTexture);
        OpenGL32.glTexImage2D(OpenGL32.GL_TEXTURE_2D, 0, (GLint)OpenGL32.GL_DEPTH_COMPONENT, (GLsizei)width, (GLsizei)height, 0, OpenGL32.GL_DEPTH_COMPONENT, OpenGL32.GL_FLOAT, IntPtr.Zero);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (GLint)OpenGL32.GL_NEAREST);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (GLint)OpenGL32.GL_NEAREST);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);

        // Привязка глубинного буфера
        OpenGL32.glFramebufferTexture2D(
          OpenGL32.GL_FRAMEBUFFER,
          OpenGL32.GL_DEPTH_ATTACHMENT,
          OpenGL32.GL_TEXTURE_2D,
          depthTexture,
          0
        );*/

        OpenGL32.glFramebufferTexture2D(
          OpenGL32.GL_FRAMEBUFFER,
          OpenGL32.GL_DEPTH_ATTACHMENT,
          OpenGL32.GL_TEXTURE_2D,
          DepthSwapchainList[i].OpenGL_Images[depthSwapchainImageId].Image,
          0
        );

        //OpenGL32.glClearDepth(1.0f);
        //OpenGL32.glClear(OpenGL32.GL_DEPTH_BUFFER_BIT);

        //OpenGL32.glClear(OpenGL32.GL_DEPTH_BUFFER_BIT);
        //OpenGL32.glClearColor(0, 0, 0, 1.0f);

        // Check
        var result = OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_DRAW_FRAMEBUFFER);
        if (result != OpenGL32.GL_FRAMEBUFFER_COMPLETE)
          throw new Exception($"ERROR: OPENGL: Framebuffer is not complete. {result}");

        OnRender?.Invoke(views[i].Pose, views[i].Fov);

        // Unbind

        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);
        OpenGL32.glDeleteFramebuffers(1, new[] { setFramebuffer });

        ColorSwapchainList[i].ReleaseSwapchainImage();
        DepthSwapchainList[i].ReleaseSwapchainImage();
      }

      // Fuck
      LayerProjection.Type = XrStructureType.XR_TYPE_COMPOSITION_LAYER_PROJECTION;
      LayerProjection.LayerFlags = XrCompositionLayerFlags.XR_COMPOSITION_LAYER_BLEND_TEXTURE_SOURCE_ALPHA_BIT |
                                   XrCompositionLayerFlags.XR_COMPOSITION_LAYER_CORRECT_CHROMATIC_ABERRATION_BIT;
      LayerProjection.Space = space;
      LayerProjection.ViewCount = (uint)LayerProjectionViews.Length;
      LayerProjection.Views = Marshal.UnsafeAddrOfPinnedArrayElement(LayerProjectionViews, 0);

      // Layers
      Layers = new[] { LayerProjection };
      LayersPointers = new IntPtr[Layers.Length];
      for (var i = 0; i < Layers.Length; i++) LayersPointers[i] = Marshal.UnsafeAddrOfPinnedArrayElement(Layers, i);
      // LayersPointer = new[] { Marshal.UnsafeAddrOfPinnedArrayElement(Layers, 0) };
    }
  }

  public struct SwapchainInfo
  {
    public XrSwapchain Swapchain;

    public long Format;
    public XrViewConfigurationView View;

    // public IntPtr ImageViews;
    public XrSwapchainImageOpenGLKHR[] OpenGL_Images;
    public uint[] FrameBuffer;

    public SwapchainType Type;

    public void GenerateFrameBuffers()
    {
      FrameBuffer = new uint[OpenGL_Images.Length];

      for (var i = 0; i < OpenGL_Images.Length; i++)
      {
        // Generate
        uint framebuffer = 0;
        OpenGL32.glGenFramebuffers(1, ref framebuffer);

        // Bind
        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, framebuffer);

        // Create
        if (Type == SwapchainType.Color)
          OpenGL32.glFramebufferTexture2D(
            OpenGL32.GL_DRAW_FRAMEBUFFER,
            OpenGL32.GL_COLOR_ATTACHMENT0,
            OpenGL32.GL_TEXTURE_2D,
            OpenGL_Images[i].Image,
            0
          );
        else
          OpenGL32.glFramebufferTexture2D(
            OpenGL32.GL_DRAW_FRAMEBUFFER,
            OpenGL32.GL_DEPTH_ATTACHMENT,
            OpenGL32.GL_TEXTURE_2D,
            OpenGL_Images[i].Image,
            0
          );

        // Check
        var result = OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_DRAW_FRAMEBUFFER);
        if (result != OpenGL32.GL_FRAMEBUFFER_COMPLETE)
          throw new Exception("ERROR: OPENGL: Framebuffer is not complete.");

        // Unbind
        OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);

        // Set
        FrameBuffer[i] = framebuffer;
      }
    }

    public uint AcquireSwapchainImage()
    {
      Console.WriteLine($"ACUIR SWAPCHAIN {Swapchain}");

      // Acquire
      uint imageIndex = 0;
      var acquireInfo = new XrSwapchainImageAcquireInfo
      {
        Type = XrStructureType.XR_TYPE_SWAPCHAIN_IMAGE_ACQUIRE_INFO
      };
      Check(
        xrAcquireSwapchainImage(Swapchain, ref acquireInfo, ref imageIndex),
        "Failed to acquire Image from the Swapchian"
      );
      Console.WriteLine($"Image index {imageIndex}");

      // Wait
      Console.WriteLine($"Wait image {Swapchain}");
      var waitInfo = new XrSwapchainImageWaitInfo
      {
        Type = XrStructureType.XR_TYPE_SWAPCHAIN_IMAGE_WAIT_INFO,
        Timeout = (long)XR_INFINITE_DURATION
      };
      Check(xrWaitSwapchainImage(Swapchain, ref waitInfo),
        "Failed to wait for Image from the Swapchain");

      return imageIndex;
    }

    public void ReleaseSwapchainImage()
    {
      Console.WriteLine($"Release image {Swapchain}");
      var releaseInfo = new XrSwapchainImageReleaseInfo
        { Type = XrStructureType.XR_TYPE_SWAPCHAIN_IMAGE_RELEASE_INFO };
      Check(xrReleaseSwapchainImage(Swapchain, ref releaseInfo),
        "Failed to release Image back to the Color Swapchain");
    }

    public void PrintInfo()
    {
      Console.WriteLine("Swapchain {");
      Console.WriteLine($"  ID {Swapchain}");
      Console.WriteLine($"  Format {Format}");
      View.PrintInfo();

      if (OpenGL_Images != null)
      {
        Console.WriteLine("OpenGL Image {");
        for (var i = 0; i < OpenGL_Images.Length; i++)
          Console.WriteLine($"  Image: {OpenGL_Images[i].Image}. Type: {OpenGL_Images[i].Type}");
        Console.WriteLine("}");
      }
      else
      {
        Console.WriteLine("OpenGL Images null");
      }

      if (FrameBuffer != null)
      {
        Console.WriteLine("Frame Buffers {");
        foreach (var framebuffer in FrameBuffer) Console.WriteLine($"  {framebuffer}");
        Console.WriteLine("}");
      }
      else
      {
        Console.WriteLine("Framebuffer null");
      }

      Console.WriteLine("}");
    }
  }

  public enum SwapchainType
  {
    Color,
    Depth
  }

  public static SwapchainInfo CreateSwapchain(
    XrSession sessionId,
    XrViewConfigurationView view,
    XrSwapchainFormatGL format,
    uint sampleCount,
    SwapchainType type
  )
  {
    var swapchainInfo = new SwapchainInfo
    {
      Format = (long)format,
      View = view,
      Type = type
    };

    var usageFlags = XrSwapchainUsageFlags.XR_SWAPCHAIN_USAGE_SAMPLED_BIT |
                     XrSwapchainUsageFlags.XR_SWAPCHAIN_USAGE_COLOR_ATTACHMENT_BIT;
    if (type == SwapchainType.Depth)
      usageFlags = XrSwapchainUsageFlags.XR_SWAPCHAIN_USAGE_SAMPLED_BIT |
                   XrSwapchainUsageFlags.XR_SWAPCHAIN_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;

    var swapchainCreateInfo = new XrSwapchainCreateInfo
    {
      Type = XrStructureType.XR_TYPE_SWAPCHAIN_CREATE_INFO,
      CreateFlags = 0,
      UsageFlags = usageFlags,
      Format = (long)format,
      SampleCount = sampleCount,
      Width = view.RecommendedImageRectWidth,
      Height = view.RecommendedImageRectHeight,
      FaceCount = 1,
      ArraySize = 1,
      MipCount = 1
    };

    Check(
      xrCreateSwapchain(
        sessionId,
        ref swapchainCreateInfo,
        ref swapchainInfo.Swapchain
      )
    );

    return swapchainInfo;
  }

  public static XrSwapchainImageOpenGLKHR[] OpenGL_EnumerateSwapchainImages(XrSwapchain swapchain)
  {
    uint count = 0;
    Check(xrEnumerateSwapchainImages(swapchain, 0, ref count, IntPtr.Zero));
    var images = new XrSwapchainImageOpenGLKHR[count];
    for (var i = 0; i < images.Length; i++)
      images[i].Type = XrStructureType.XR_TYPE_SWAPCHAIN_IMAGE_OPENGL_KHR;
    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(images, 0);
    Check(xrEnumerateSwapchainImages(swapchain, count, ref count, ptr));

    return images;
  }

  public static XrEnvironmentBlendMode[] EnumerateEnvironmentBlendModes(
    XrInstance instance,
    XrSystemId systemId,
    XrViewConfigurationType viewConfiguration
  )
  {
    uint count = 0;
    Check(
      xrEnumerateEnvironmentBlendModes(
        instance,
        systemId,
        viewConfiguration,
        0,
        ref count,
        IntPtr.Zero
      )
    );
    var list = new XrEnvironmentBlendMode[count];
    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(list, 0);
    Check(
      xrEnumerateEnvironmentBlendModes(
        instance,
        systemId,
        viewConfiguration,
        count,
        ref count,
        ptr
      )
    );

    return list;
  }

  public static XrSpace CreateReferenceSpace(XrSession sessionId)
  {
    var createInfo = new XrReferenceSpaceCreateInfo
    {
      Type = XrStructureType.XR_TYPE_REFERENCE_SPACE_CREATE_INFO,
      ReferenceSpaceType = XrReferenceSpaceType.XR_REFERENCE_SPACE_TYPE_LOCAL,
      PoseInReferenceSpace = new XrPosef
      {
        Orientation = new XrQuaternionf { X = 0, Y = 0, Z = 0, W = 1.0f },
        Position = new XrVector3f { X = 0, Y = 0, Z = 0 }
      }
    };
    var space = IntPtr.Zero;
    Check(xrCreateReferenceSpace(sessionId, ref createInfo, ref space));
    return space;
  }

  private static void Check(XrResult result, string errorMessage)
  {
    if (result != XrResult.XR_SUCCESS)
      throw new Exception(result + " " + errorMessage);
  }

  private static void Check(XrResult result)
  {
    if (result != XrResult.XR_SUCCESS)
      throw new Exception(result.ToString());
  }
}