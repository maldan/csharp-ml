using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace MegaLib.OS.Api;

public class HID
{
  [StructLayout(LayoutKind.Sequential)]
  private struct SP_DEVICE_INTERFACE_DATA
  {
    public int cbSize;
    public Guid InterfaceClassGuid;
    public int Flags;
    public IntPtr Reserved;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  private struct SP_DEVICE_INTERFACE_DETAIL_DATA
  {
    public int cbSize;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string DevicePath;
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct SP_DEVINFO_DATA
  {
    public int cbSize;
    public Guid ClassGuid;
    public int DevInst;
    public IntPtr Reserved;
  }

  [DllImport("setupapi.dll", SetLastError = true)]
  private static extern IntPtr
    SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

  [DllImport("setupapi.dll", SetLastError = true)]
  private static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData,
    ref Guid InterfaceClassGuid, uint MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

  [DllImport("setupapi.dll", SetLastError = true)]
  private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
    ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData,
    int DeviceInterfaceDetailDataSize, out int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

  [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
  private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
    ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
    int DeviceInterfaceDetailDataSize, out int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

  [DllImport("setupapi.dll", SetLastError = true)]
  private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

  [DllImport("hid.dll", SetLastError = true)]
  private static extern void HidD_GetHidGuid(out Guid HidGuid);

  [DllImport("hid.dll", SetLastError = true)]
  private static extern bool HidD_GetProductString(SafeFileHandle HidDeviceObject, byte[] Buffer, uint BufferLength);

  private const int DIGCF_PRESENT = 0x02;
  private const int DIGCF_DEVICEINTERFACE = 0x10;

  public static void Main()
  {
    // Получение GUID для HID устройств
    HidD_GetHidGuid(out var hidGuid);

    // Получение списка всех HID устройств
    var deviceInfoSet =
      SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
    if (deviceInfoSet == IntPtr.Zero)
    {
      Console.WriteLine("Не удалось получить список устройств.");
      return;
    }

    try
    {
      var deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
      deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

      uint index = 0;
      while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref hidGuid, index, ref deviceInterfaceData))
      {
        // Получение требуемого размера буфера
        var requiredSize = 0;
        var devInfoData = new SP_DEVINFO_DATA();
        devInfoData.cbSize = Marshal.SizeOf(devInfoData);

        SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, out requiredSize,
          ref devInfoData);

        // Создание буфера нужного размера и получение деталей устройства
        var deviceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA();
        deviceDetailData.cbSize = IntPtr.Size == 8 ? 8 : 5;

        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ref deviceDetailData, requiredSize,
              out _, ref devInfoData))
          Console.WriteLine($"Device Path: {deviceDetailData.DevicePath}");
        else
          Console.WriteLine(
            $"Не удалось получить детали устройства для {index}. Ошибка: {Marshal.GetLastWin32Error()}");

        index++;
      }

      if (index == 0) Console.WriteLine("Не найдено HID устройств.");
    }
    finally
    {
      SetupDiDestroyDeviceInfoList(deviceInfoSet);
    }
  }
}