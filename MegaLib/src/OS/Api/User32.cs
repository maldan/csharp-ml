using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api;

public class User32
{
  [DllImport("user32.dll", SetLastError = true)]
  public static extern IntPtr CreateWindowEx(
    uint dwExStyle,
    [MarshalAs(UnmanagedType.LPStr)] string lpClassName,
    [MarshalAs(UnmanagedType.LPStr)] string lpWindowName,
    uint dwStyle,
    int x,
    int y,
    int nWidth,
    int nHeight,
    IntPtr hWndParent,
    IntPtr hMenu,
    IntPtr hInstance,
    IntPtr lpParam
  );

  [DllImport("kernel32.dll", SetLastError = true)]
  public static extern IntPtr GetModuleHandle(string lpModuleName);

  [DllImport("user32.dll")]
  public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

  [DllImport("user32.dll", SetLastError = true)]
  public static extern ushort RegisterClass([In] ref WNDCLASS lpWndClass);

  [DllImport("user32.dll", SetLastError = true)]
  public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

  [DllImport("user32.dll")]
  public static extern void PostQuitMessage(int nExitCode);

  [DllImport("user32.dll")]
  public static extern IntPtr GetDC(IntPtr hWnd);

  [DllImport("user32.dll", SetLastError = true)]
  public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

  [DllImport("user32.dll", SetLastError = true)]
  public static extern bool DestroyWindow(IntPtr hWnd);

  [DllImport("user32.dll")]
  public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

  [DllImport("user32.dll")]
  public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

  [DllImport("user32.dll")]
  public static extern bool TranslateMessage([In] ref MSG lpMsg);

  [DllImport("user32.dll")]
  public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);

  // Подключаем библиотеку user32.dll для использования функции GetAsyncKeyState
  [DllImport("user32.dll")]
  public static extern short GetAsyncKeyState(int vKey);

  // Импортируем функцию GetForegroundWindow из user32.dll
  [DllImport("user32.dll")]
  public static extern IntPtr GetForegroundWindow();

  [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
  public static extern bool SetWindowText(IntPtr hWnd, string lpString);

  [DllImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

  [DllImport("user32.dll", SetLastError = true)]
  public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

  [DllImport("user32.dll")]
  public static extern int GetSystemMetrics(int nIndex);

  // Импортирование функций Win32 API
  [DllImport("user32.dll")]
  public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

  [DllImport("user32.dll")]
  public static extern IntPtr SetCursor(IntPtr hCursor);

  public static int GetScreenWidth()
  {
    return GetSystemMetrics(0);
  }

  public static int GetScreenHeight()
  {
    return GetSystemMetrics(1);
  }
}