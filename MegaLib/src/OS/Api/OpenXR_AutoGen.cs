
using System;
using System.Runtime.InteropServices;
using XrFlags64 = System.UInt64;
using XrInstanceCreateFlags = System.UInt64;
using XrVersion = System.UInt64;
using XrInstance = System.IntPtr;
using XrSession = System.IntPtr;
using XrSpace = System.IntPtr;
using XrSwapchain = System.IntPtr;
using XrActionSet = System.IntPtr;
using XrSystemId = System.UInt64;
using XrTime = System.Int64;
using XrBool32 = System.UInt32;
using XrPath = ulong;
using XrAction = System.IntPtr;
namespace MegaLib.OS.Api
  {
  public static partial class OpenXR {
[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateInstance(ref XrInstanceCreateInfo createInfo, ref XrInstance instance);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateApiLayerProperties(uint propertyCapacityInput, ref uint propertyCountOutput, XrApiLayerProperties [] properties);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateApiLayerProperties(uint propertyCapacityInput, ref uint propertyCountOutput, IntPtr properties);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrDestroyInstance(XrInstance instance);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetInstanceProperties(XrInstance instance, ref XrInstanceProperties instanceProperties);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetSystem(XrInstance instance, ref XrSystemGetInfo getInfo, ref XrSystemId systemId);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetSystemProperties(XrInstance instance, XrSystemId systemId, ref XrSystemProperties properties);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateSession(XrInstance instance, ref XrSessionCreateInfo createInfo, ref XrSession session);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetInstanceProcAddr(XrInstance instance, IntPtr name, ref IntPtr fn);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrPollEvent(XrInstance instance, ref XrEventDataBuffer eventData);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrBeginSession(XrSession session, ref XrSessionBeginInfo beginInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEndSession(XrSession session);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateSwapchain(XrSession session, ref XrSwapchainCreateInfo createInfo, ref XrSwapchain swapchain);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateSwapchainFormats(XrSession session, uint formatCapacityInput, ref uint formatCountOutput, long [] formats);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateViewConfigurations(XrInstance instance, XrSystemId systemId, uint viewConfigurationTypeCapacityInput, ref uint viewConfigurationTypeCountOutput, IntPtr viewConfigurationTypes);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateViewConfigurations(XrInstance instance, XrSystemId systemId, uint viewConfigurationTypeCapacityInput, ref uint viewConfigurationTypeCountOutput, XrViewConfigurationType [] viewConfigurationTypes);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateViewConfigurationViews(XrInstance instance, XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint viewCapacityInput, ref uint viewCountOutput, ref XrViewConfigurationView [] views);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateViewConfigurationViews(XrInstance instance, XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint viewCapacityInput, ref uint viewCountOutput, IntPtr views);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateSwapchainImages(XrSwapchain swapchain, uint imageCapacityInput, ref uint imageCountOutput, ref XrSwapchainImageBaseHeader images);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateSwapchainImages(XrSwapchain swapchain, uint imageCapacityInput, ref uint imageCountOutput, IntPtr images);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrWaitFrame(XrSession session, ref XrFrameWaitInfo frameWaitInfo, ref XrFrameState frameState);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrBeginFrame(XrSession session, ref XrFrameBeginInfo frameBeginInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEndFrame(XrSession session, ref XrFrameEndInfo frameEndInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateReferenceSpace(XrSession session, ref XrReferenceSpaceCreateInfo createInfo, ref XrSpace space);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateInstanceExtensionProperties(IntPtr layerName, uint propertyCapacityInput, ref uint propertyCountOutput, ref XrExtensionProperties properties);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateInstanceExtensionProperties(IntPtr layerName, uint propertyCapacityInput, ref uint propertyCountOutput, IntPtr properties);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrAcquireSwapchainImage(XrSwapchain swapchain, ref XrSwapchainImageAcquireInfo acquireInfo, ref uint index);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrWaitSwapchainImage(XrSwapchain swapchain, ref XrSwapchainImageWaitInfo waitInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrReleaseSwapchainImage(XrSwapchain swapchain, ref XrSwapchainImageReleaseInfo releaseInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrLocateViews(XrSession session, ref XrViewLocateInfo viewLocateInfo, ref XrViewState viewState, uint viewCapacityInput, ref uint viewCountOutput, ref XrView views);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrLocateViews(XrSession session, ref XrViewLocateInfo viewLocateInfo, ref XrViewState viewState, uint viewCapacityInput, ref uint viewCountOutput, IntPtr views);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrLocateSpace(XrSpace space, XrSpace baseSpace, XrTime time, ref XrSpaceLocation location);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrEnumerateEnvironmentBlendModes(XrInstance instance, XrSystemId systemId, XrViewConfigurationType viewConfigurationType, uint environmentBlendModeCapacityInput, ref uint environmentBlendModeCountOutput, IntPtr environmentBlendModes);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateAction(XrActionSet actionSet, ref XrActionCreateInfo createInfo, ref XrAction action);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateActionSet(XrInstance instance, ref XrActionSetCreateInfo createInfo, ref XrActionSet actionSet);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrCreateActionSpace(XrSession session, ref XrActionSpaceCreateInfo createInfo, ref XrSpace space);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrSyncActions(XrSession session, ref XrActionsSyncInfo syncInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrStringToPath(XrInstance instance, IntPtr pathString, ref XrPath path);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetActionStatePose(XrSession session, ref XrActionStateGetInfo getInfo, ref XrActionStatePose state);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrAttachSessionActionSets(XrSession session, ref XrSessionActionSetsAttachInfo attachInfo);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrSuggestInteractionProfileBindings(XrInstance instance, ref XrInteractionProfileSuggestedBinding suggestedBindings);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetActionStateVector2f(XrSession session, ref XrActionStateGetInfo getInfo, ref XrActionStateVector2f state);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetActionStateFloat(XrSession session, ref XrActionStateGetInfo getInfo, ref XrActionStateFloat state);

[DllImport("openxr_loader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern XrResult xrGetActionStateBoolean(XrSession session, ref XrActionStateGetInfo getInfo, ref XrActionStateBoolean state);

}
}
