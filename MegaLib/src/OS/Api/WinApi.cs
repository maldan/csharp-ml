using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api;

public static class WinApi
{
  public const uint CS_HREDRAW = 0x0001;
  public const uint CS_VREDRAW = 0x0002;
  public const uint CS_OWNDC = 0x0020;

  public const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
  public const uint WS_CLIPSIBLINGS = 0x04000000;
  public const uint WS_CLIPCHILDREN = 0x02000000;

  public const uint PFD_DRAW_TO_WINDOW = 0x00000004;
  public const uint PFD_SUPPORT_OPENGL = 0x00000020;
  public const uint PFD_DOUBLEBUFFER = 0x00000001;
  public const uint PFD_MAIN_PLANE = 0x00000000;
  public const uint PFD_TYPE_RGBA = 0;

  public const int WM_CREATE = 0x0001;
  public const int WM_PAINT = 0x000F;
  public const int WM_CLOSE = 0x0010;
  public const int WM_QUIT = 0x0012;
  public const int WM_SIZE = 0x0005;
  public const int WM_SHOWWINDOW = 0x0018;
  public const int WM_SETCURSOR = 0x0020;
  public const int WM_MOUSEWHEEL = 0x020A;
}

public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct WNDCLASS
{
  public uint style;
  public WndProcDelegate lpfnWndProc;
  public int cbClsExtra;
  public int cbWndExtra;
  public IntPtr hInstance;
  public IntPtr hIcon;
  public IntPtr hCursor;
  public IntPtr hbrBackground;
  [MarshalAs(UnmanagedType.LPStr)] public string lpszMenuName;
  [MarshalAs(UnmanagedType.LPStr)] public string lpszClassName;
}

[StructLayout(LayoutKind.Sequential)]
public struct PIXELFORMATDESCRIPTOR
{
  public ushort nSize;
  public ushort nVersion;
  public uint dwFlags;
  public byte iPixelType;
  public byte cColorBits;
  public byte cRedBits;
  public byte cRedShift;
  public byte cGreenBits;
  public byte cGreenShift;
  public byte cBlueBits;
  public byte cBlueShift;
  public byte cAlphaBits;
  public byte cAlphaShift;
  public byte cAccumBits;
  public byte cAccumRedBits;
  public byte cAccumGreenBits;
  public byte cAccumBlueBits;
  public byte cAccumAlphaBits;
  public byte cDepthBits;
  public byte cStencilBits;
  public byte cAuxBuffers;
  public byte iLayerType;
  public byte bReserved;
  public uint dwLayerMask;
  public uint dwVisibleMask;
  public uint dwDamageMask;
}

[StructLayout(LayoutKind.Sequential)]
public struct MSG
{
  public IntPtr hwnd;
  public uint message;
  public IntPtr wParam;
  public IntPtr lParam;
  public uint time;
  public POINT pt;
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
  public int X;
  public int Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
  public int Left;
  public int Top;
  public int Right;
  public int Bottom;
}