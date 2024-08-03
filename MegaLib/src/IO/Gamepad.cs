using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MegaLib.IO;

public class GamepadOld
{
  [StructLayout(LayoutKind.Sequential)]
  public struct XInputState
  {
    public int PacketNumber;
    public XInputGamepad Gamepad;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct XInputGamepad
  {
    public ushort Buttons;
    public byte LeftTrigger;
    public byte RightTrigger;
    public short ThumbLX;
    public short ThumbLY;
    public short ThumbRX;
    public short ThumbRY;
  }

  [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")]
  public static extern int XInputGetState(int playerIndex, ref XInputState state);

  [DllImport("xinput1_3.dll")]
  public static extern void XInputEnable([MarshalAs(UnmanagedType.Bool)] bool enable);

  public const int ERROR_SUCCESS = 0;
  public const int ERROR_DEVICE_NOT_CONNECTED = 1167;

  public class GamepadInfo
  {
    public bool IsConnected;
    public XInputGamepad State;
  }

  public static List<GamepadInfo> Gamepads = new()
  {
    new GamepadInfo(),
    new GamepadInfo(),
    new GamepadInfo(),
    new GamepadInfo()
  };

  [StructLayout(LayoutKind.Sequential)]
  public struct DIJOYSTATE2
  {
    public int lX;
    public int lY;
    public int lZ;
    public int lRx;
    public int lRy;
    public int lRz;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglSlider;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public int[] rgdwPOV;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public byte[] rgbButtons;

    public int lVX;
    public int lVY;
    public int lVZ;
    public int lVRx;
    public int lVRy;
    public int lVRz;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglVSlider;

    public int lAX;
    public int lAY;
    public int lAZ;
    public int lARx;
    public int lARy;
    public int lARz;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglASlider;

    public int lFX;
    public int lFY;
    public int lFZ;
    public int lFRx;
    public int lFRy;
    public int lFRz;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglFSlider;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct DIDEVICEINSTANCE
  {
    public int dwSize;
    public Guid guidInstance;
    public Guid guidProduct;
    public int dwDevType;
    public short tszInstanceName;
    public short tszProductName;
    public Guid guidFFDriver;
    public short wUsagePage;
    public short wUsage;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct DIDEVCAPS
  {
    public int dwSize;
    public int dwFlags;
    public int dwDevType;
    public int dwAxes;
    public int dwButtons;
    public int dwPOVs;
    public int dwFFSamplePeriod;
    public int dwFFMinTimeResolution;
    public int dwFirmwareRevision;
    public int dwHardwareRevision;
    public int dwFFDriverVersion;
  }

  [DllImport("dinput8.dll", EntryPoint = "DirectInput8Create")]
  public static extern int DirectInput8Create(IntPtr hInstance, int dwVersion, ref Guid riidltf, out IntPtr ppvOut,
    IntPtr punkOuter);

  [DllImport("dinput8.dll", EntryPoint = "IDirectInput8_EnumDevices")]
  public static extern int IDirectInput8_EnumDevices(IntPtr self, int dwDevType, IntPtr lpCallback, IntPtr pvRef,
    int dwFlags);

  [DllImport("dinput8.dll", EntryPoint = "IDirectInput8_CreateDevice")]
  public static extern int IDirectInput8_CreateDevice(IntPtr self, ref Guid rguid, out IntPtr lplpDirectInputDevice,
    IntPtr pUnkOuter);

  [DllImport("dinput8.dll", EntryPoint = "IDirectInputDevice8_SetDataFormat")]
  public static extern int IDirectInputDevice8_SetDataFormat(IntPtr self, IntPtr lpdf);

  [DllImport("dinput8.dll", EntryPoint = "IDirectInputDevice8_SetCooperativeLevel")]
  public static extern int IDirectInputDevice8_SetCooperativeLevel(IntPtr self, IntPtr hwnd, int dwFlags);

  [DllImport("dinput8.dll", EntryPoint = "IDirectInputDevice8_Acquire")]
  public static extern int IDirectInputDevice8_Acquire(IntPtr self);

  [DllImport("dinput8.dll", EntryPoint = "IDirectInputDevice8_GetDeviceState")]
  public static extern int IDirectInputDevice8_GetDeviceState(IntPtr self, int cbData, ref DIJOYSTATE2 lpvData);

  public static void Update()
  {
    var directInputGuid = new Guid("BF798031-483A-4DA2-AA99-5D64ED369700"); // IID_IDirectInput8
    IntPtr directInput;
    var hResult = DirectInput8Create(IntPtr.Zero, 0x0800, ref directInputGuid, out directInput, IntPtr.Zero);

    if (hResult != 0)
    {
      Console.WriteLine("Ошибка инициализации DirectInput.");
      return;
    }

    Console.WriteLine("DirectInput инициализирован.");

    // Здесь должна быть логика для перечисления и выбора устройства, пропускаю ради краткости.

    var joystick = IntPtr.Zero;
    // Пропущена инициализация конкретного устройства для краткости.
    // Обычно здесь требуется вызвать EnumDevices и CreateDevice.

    var joystickState = new DIJOYSTATE2();
    while (true)
    {
      hResult = IDirectInputDevice8_GetDeviceState(joystick, Marshal.SizeOf(joystickState), ref joystickState);
      if (hResult != 0)
      {
        Console.WriteLine("Ошибка получения состояния геймпада.");
        break;
      }

      Console.WriteLine($"X: {joystickState.lX}, Y: {joystickState.lY}, Z: {joystickState.lZ}");
      Console.WriteLine($"Rx: {joystickState.lRx}, Ry: {joystickState.lRy}, Rz: {joystickState.lRz}");
      // Вывод остальных данных состояния, при необходимости

      System.Threading.Thread.Sleep(100); // Пауза для уменьшения нагрузки на процессор
    }
  }
}

public class Gamepad
{
  private const int FILE_SHARE_READ = 0x00000001;
  private const int FILE_SHARE_WRITE = 0x00000002;
  private const int OPEN_EXISTING = 3;

  private const uint GENERIC_READ = 0x80000000;
  private const uint GENERIC_WRITE = 0x40000000;

  private const string DevicePath =
    @"\\?\hid#vid_054c&pid_0ce6#8&12345678&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";

  [StructLayout(LayoutKind.Sequential)]
  public struct HIDP_CAPS
  {
    public ushort Usage;
    public ushort UsagePage;
    public ushort InputReportByteLength;
    public ushort OutputReportByteLength;
    public ushort FeatureReportByteLength;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
    public ushort[] Reserved;

    public ushort NumberLinkCollectionNodes;
    public ushort NumberInputButtonCaps;
    public ushort NumberInputValueCaps;
    public ushort NumberInputDataIndices;
    public ushort NumberOutputButtonCaps;
    public ushort NumberOutputValueCaps;
    public ushort NumberOutputDataIndices;
    public ushort NumberFeatureButtonCaps;
    public ushort NumberFeatureValueCaps;
    public ushort NumberFeatureDataIndices;
  }

  [DllImport("hid.dll", SetLastError = true)]
  private static extern bool HidD_GetPreparsedData(SafeFileHandle hObject, out IntPtr PreparsedData);

  [DllImport("hid.dll", SetLastError = true)]
  private static extern int HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

  [DllImport("hid.dll", SetLastError = true)]
  private static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern SafeFileHandle CreateFile(
    string lpFileName,
    uint dwDesiredAccess,
    uint dwShareMode,
    IntPtr lpSecurityAttributes,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    IntPtr hTemplateFile);

  public void Gas()
  {
    // Открытие устройства
    var deviceHandle = CreateFile(DevicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
      IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
    if (deviceHandle.IsInvalid)
    {
      Console.WriteLine("Не удалось открыть устройство.");
      return;
    }

    // Получение данных устройства
    if (HidD_GetPreparsedData(deviceHandle, out var preparsedData))
    {
      if (HidP_GetCaps(preparsedData, out var capabilities) == 0)
      {
        Console.WriteLine("Не удалось получить возможности устройства.");
        HidD_FreePreparsedData(preparsedData);
        deviceHandle.Close();
        return;
      }

      var inputReport = new byte[capabilities.InputReportByteLength];
      var fs = new FileStream(deviceHandle, FileAccess.ReadWrite, inputReport.Length, false);
      fs.Read(inputReport, 0, inputReport.Length);

      // Чтение данных контроллера
      var buttonData = inputReport[5]; // Пример, уточните по документации
      var leftStickX = BitConverter.ToInt16(inputReport, 7);
      var leftStickY = BitConverter.ToInt16(inputReport, 9);
      var rightStickX = BitConverter.ToInt16(inputReport, 11);
      var rightStickY = BitConverter.ToInt16(inputReport, 13);

      Console.WriteLine($"Buttons: {buttonData}");
      Console.WriteLine($"Left Stick X: {leftStickX}");
      Console.WriteLine($"Left Stick Y: {leftStickY}");
      Console.WriteLine($"Right Stick X: {rightStickX}");
      Console.WriteLine($"Right Stick Y: {rightStickY}");

      HidD_FreePreparsedData(preparsedData);
    }
    else
    {
      Console.WriteLine("Не удалось получить данные устройства.");
    }

    deviceHandle.Close();
  }
}