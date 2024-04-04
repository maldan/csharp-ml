using System;
using System.Runtime.InteropServices;
using XrFlags64 = System.UInt64;
using XrInstanceCreateFlags = System.UInt64;
using XrVersion = System.UInt64;
using XrInstance = System.IntPtr;

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
    public static extern XrResult xrDestroyInstance(XrInstance instance);

    [DllImport("D:/csharp_lib/MegaLib/MegaLib/lib/openxr_loader.dll", CharSet = CharSet.Ansi,
      CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern XrResult xrGetInstanceProperties(XrInstance instance,
      ref XrInstanceProperties instanceProperties);
  }
}