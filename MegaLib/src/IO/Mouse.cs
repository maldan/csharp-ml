using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;

namespace MegaLib.IO;

public enum MouseKey
{
  Left = 1,
  Right = 2,
  Center = 4
}

public enum MouseCursor
{
  Arrow,
  Pointer,
  TextInput,
  Move,
  ResizeHorizontal,
  ResizeVertical
}

public static class Mouse
{
  public static Vector2 Screen { get; private set; }
  public static Vector2 Client { get; private set; }
  public static Vector2 ClientDelta { get; private set; }
  public static Vector2 ClientClamped { get; private set; }

  public static Vector2 Normalized { get; private set; }

  public static float WheelDirection;

  public static MouseCursor Cursor = MouseCursor.Arrow;

  private static byte[] _state =
  [
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0
  ];
  private static byte[] _previousState = new byte[_state.Length];

  public static void Init()
  {
  }

  public static void Update()
  {
    // Получение текущего положения курсора мыши
    if (User32.GetCursorPos(out var p))
    {
      Screen = new Vector2(p.X, p.Y);

      // Получение дескриптора активного окна
      var hWnd = User32.GetForegroundWindow();
      if (hWnd != IntPtr.Zero)
      {
        // Преобразование координат в клиентские относительно активного окна
        var clientCursorPos = p;
        if (User32.ScreenToClient(hWnd, ref clientCursorPos))
        {
          var oldClient = Client;
          Client = new Vector2(clientCursorPos.X, clientCursorPos.Y);
          ClientDelta = Client - oldClient;

          // Получение размеров клиентской области активного окна
          if (User32.GetClientRect(hWnd, out var clientRect))
          {
            // Ограничение координат курсора размерами окна
            clientCursorPos.X = Math.Max(clientRect.Left, Math.Min(clientCursorPos.X, clientRect.Right - 1));
            clientCursorPos.Y = Math.Max(clientRect.Top, Math.Min(clientCursorPos.Y, clientRect.Bottom - 1));

            ClientClamped = new Vector2(clientCursorPos.X, clientCursorPos.Y);
            Normalized = new Vector2(clientCursorPos.X / (float)(clientRect.Right - 1),
              clientCursorPos.Y / (float)(clientRect.Bottom - 1));
          }
        }
      }
    }

    // Предыдущее состояние клавиш
    for (var i = 0; i < _state.Length; i++)
    {
      _previousState[i] = _state[i];
    }
    
    _state[(int)MouseKey.Left] = (byte)(User32.GetAsyncKeyState((int)MouseKey.Left) < 0 ? 1 : 0);
    _state[(int)MouseKey.Right] = (byte)(User32.GetAsyncKeyState((int)MouseKey.Right) < 0 ? 1 : 0);
    _state[(int)MouseKey.Center] = (byte)(User32.GetAsyncKeyState((int)MouseKey.Center) < 0 ? 1 : 0);
  }

  public static bool IsKeyDown(MouseKey key)
  {
    return _state[(int)key] == 1;
  }

  public static bool IsKeyUp(MouseKey key)
  {
    return _state[(int)key] == 0;
  }
  
  public static bool IsKeyPressed(MouseKey key)
  {
    return IsKeyDown(key) && _previousState[(int)key] == 0;
  }
}