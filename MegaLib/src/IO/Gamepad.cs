using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MegaLib.IO;

public class Gamepad
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