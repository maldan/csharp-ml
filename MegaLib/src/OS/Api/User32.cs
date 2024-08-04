using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MegaLib.OS.Api;

public class User32
{
  // Константы для стандартных типов курсоров
  public const int IDC_ARROW = 32512;
  public const int IDC_HAND = 32649;
  public const int IDC_SIZEWE = 32644;
  public const int IDC_SIZENS = 32645;

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

  // Импорт функции GetCursorPos из user32.dll
  [DllImport("user32.dll")]
  public static extern bool GetCursorPos(out POINT lpPoint);

  // Импорт функции ScreenToClient из user32.dll
  [DllImport("user32.dll")]
  public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

  // Импорт функции GetClientRect из user32.dll
  [DllImport("user32.dll")]
  public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

  // Импорт функции ToUnicode из user32.dll
  [DllImport("user32.dll", CharSet = CharSet.Unicode)]
  public static extern int ToUnicode(
    uint wVirtKey, // Виртуальный код клавиши
    uint wScanCode, // Скан-код клавиши
    byte[] lpKeyState, // Массив состояния клавиш
    [Out] [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
    StringBuilder pwszBuff, // Буфер для символов
    int cchBuff, // Размер буфера
    uint wFlags); // Флаги

  // Импорт функции GetKeyboardState из user32.dll
  [DllImport("user32.dll")]
  public static extern bool GetKeyboardState(byte[] lpKeyState);

  // Импорт функции MapVirtualKey из user32.dll
  [DllImport("user32.dll")]
  public static extern uint MapVirtualKey(uint uCode, uint uMapType);


  public static int GetScreenWidth()
  {
    return GetSystemMetrics(0);
  }

  public static int GetScreenHeight()
  {
    return GetSystemMetrics(1);
  }
}