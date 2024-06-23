using System;
using System.Runtime.InteropServices;
using XrFlags64 = System.UInt64;
using XrInstanceCreateFlags = System.UInt64;
using XrVersion = System.UInt64;
using XrInstance = System.IntPtr;
using XrSession = System.IntPtr;
using XrSpace = System.IntPtr;
using XrSwapchain = System.IntPtr;
using XrSystemId = System.UInt64;
using XrTime = System.Int64;
using XrBool32 = System.UInt32;

namespace MegaLib.OS.Api
{
  public static partial class OpenXR
  {
    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrCreateInstance(ref XrInstanceCreateInfo createInfo, ref XrInstance instance);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateApiLayerProperties(uint propertyCapacityInput,
      ref uint propertyCountOutput, XrApiLayerProperties[] properties);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateApiLayerProperties(uint propertyCapacityInput,
      ref uint propertyCountOutput, IntPtr properties);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrDestroyInstance(XrInstance instance);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrGetInstanceProperties(XrInstance instance,
      ref XrInstanceProperties instanceProperties);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrGetSystem(XrInstance instance, ref XrSystemGetInfo getInfo,
      ref XrSystemId systemId);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrGetSystemProperties(XrInstance instance, XrSystemId systemId,
      ref XrSystemProperties properties);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrCreateSession(XrInstance instance, ref XrSessionCreateInfo createInfo,
      ref XrSession session);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrGetInstanceProcAddr(XrInstance instance, IntPtr name, ref IntPtr fn);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrPollEvent(XrInstance instance, ref XrEventDataBuffer eventData);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrBeginSession(XrSession session, ref XrSessionBeginInfo beginInfo);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEndSession(XrSession session);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrCreateSwapchain(XrSession session, ref XrSwapchainCreateInfo createInfo,
      ref XrSwapchain swapchain);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateSwapchainFormats(XrSession session, uint formatCapacityInput,
      ref uint formatCountOutput, long[] formats);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateViewConfigurations(XrInstance instance, XrSystemId systemId,
      uint viewConfigurationTypeCapacityInput, ref uint viewConfigurationTypeCountOutput,
      IntPtr viewConfigurationTypes);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateViewConfigurations(XrInstance instance, XrSystemId systemId,
      uint viewConfigurationTypeCapacityInput, ref uint viewConfigurationTypeCountOutput,
      XrViewConfigurationType[] viewConfigurationTypes);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateViewConfigurationViews(XrInstance instance, XrSystemId systemId,
      XrViewConfigurationType viewConfigurationType, uint viewCapacityInput, ref uint viewCountOutput,
      ref XrViewConfigurationView[] views);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateViewConfigurationViews(XrInstance instance, XrSystemId systemId,
      XrViewConfigurationType viewConfigurationType, uint viewCapacityInput, ref uint viewCountOutput, IntPtr views);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateSwapchainImages(XrSwapchain swapchain, uint imageCapacityInput,
      ref uint imageCountOutput, ref XrSwapchainImageBaseHeader images);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateSwapchainImages(XrSwapchain swapchain, uint imageCapacityInput,
      ref uint imageCountOutput, IntPtr images);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrWaitFrame(XrSession session, ref XrFrameWaitInfo frameWaitInfo,
      ref XrFrameState frameState);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrBeginFrame(XrSession session, ref XrFrameBeginInfo frameBeginInfo);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEndFrame(XrSession session, ref XrFrameEndInfo frameEndInfo);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrCreateReferenceSpace(XrSession session, ref XrReferenceSpaceCreateInfo createInfo,
      ref XrSpace space);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateInstanceExtensionProperties(IntPtr layerName, uint propertyCapacityInput,
      ref uint propertyCountOutput, ref XrExtensionProperties properties);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateInstanceExtensionProperties(IntPtr layerName, uint propertyCapacityInput,
      ref uint propertyCountOutput, IntPtr properties);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrAcquireSwapchainImage(XrSwapchain swapchain,
      ref XrSwapchainImageAcquireInfo acquireInfo, ref uint index);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrWaitSwapchainImage(XrSwapchain swapchain, ref XrSwapchainImageWaitInfo waitInfo);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrReleaseSwapchainImage(XrSwapchain swapchain,
      ref XrSwapchainImageReleaseInfo releaseInfo);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrLocateViews(XrSession session, ref XrViewLocateInfo viewLocateInfo,
      ref XrViewState viewState, uint viewCapacityInput, ref uint viewCountOutput, ref XrView views);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrLocateViews(XrSession session, ref XrViewLocateInfo viewLocateInfo,
      ref XrViewState viewState, uint viewCapacityInput, ref uint viewCountOutput, IntPtr views);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrLocateSpace(XrSpace space, XrSpace baseSpace, XrTime time,
      ref XrSpaceLocation location);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrEnumerateEnvironmentBlendModes(XrInstance instance, XrSystemId systemId,
      XrViewConfigurationType viewConfigurationType, uint environmentBlendModeCapacityInput,
      ref uint environmentBlendModeCountOutput, IntPtr environmentBlendModes);
  }
}