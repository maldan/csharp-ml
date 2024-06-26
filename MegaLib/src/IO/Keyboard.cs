using MegaLib.OS.Api;

namespace MegaLib.IO;

public enum KeyboardKey
{
  None = 0,
  Backspace = 8,
  Tab = 9,
  Enter = 13,
  Shift = 16,
  Control = 17,
  Alt = 18,
  CapsLock = 20,
  Escape = 27,
  Space = 32,
  PageUp = 33,
  PageDown = 34,
  End = 35,
  Home = 36,
  ArrowLeft = 37,
  ArrowUp = 38,
  ArrowRight = 39,
  ArrowDown = 40,
  Insert = 45,
  Delete = 46,
  D0 = 48,
  D1 = 49,
  D2 = 50,
  D3 = 51,
  D4 = 52,
  D5 = 53,
  D6 = 54,
  D7 = 55,
  D8 = 56,
  D9 = 57,
  A = 65,
  B = 66,
  C = 67,
  D = 68,
  E = 69,
  F = 70,
  G = 71,
  H = 72,
  I = 73,
  J = 74,
  K = 75,
  L = 76,
  M = 77,
  N = 78,
  O = 79,
  P = 80,
  Q = 81,
  R = 82,
  S = 83,
  T = 84,
  U = 85,
  V = 86,
  W = 87,
  X = 88,
  Y = 89,
  Z = 90,
  LeftWindows = 91,
  RightWindows = 92,
  Menu = 93,
  Sleep = 95,
  NumPad0 = 96,
  NumPad1 = 97,
  NumPad2 = 98,
  NumPad3 = 99,
  NumPad4 = 100,
  NumPad5 = 101,
  NumPad6 = 102,
  NumPad7 = 103,
  NumPad8 = 104,
  NumPad9 = 105,
  Multiply = 106,
  Add = 107,
  Separator = 108,
  Subtract = 109,
  Decimal = 110,
  Divide = 111,
  F1 = 112,
  F2 = 113,
  F3 = 114,
  F4 = 115,
  F5 = 116,
  F6 = 117,
  F7 = 118,
  F8 = 119,
  F9 = 120,
  F10 = 121,
  F11 = 122,
  F12 = 123,
  NumLock = 144,
  ScrollLock = 145,
  BrowserBack = 166,
  BrowserForward = 167,
  VolumeMute = 173,
  VolumeDown = 174,
  VolumeUp = 175,
  MediaNextTrack = 176,
  MediaPreviousTrack = 177,
  MediaStop = 178,
  MediaPlayPause = 179,
  SemiColon = 186,
  Equal = 187,
  Comma = 188,
  Minus = 189,
  Period = 190,
  Slash = 191,
  Grave = 192,
  OpenBracket = 219,
  BackSlash = 220,
  CloseBracket = 221,
  Quote = 222,
  OEM102 = 226,
  ProcessKey = 229,
  Packet = 231,
  Attn = 246,
  CrSel = 247,
  ExSel = 248,
  EraseEof = 249,
  Play = 250,
  Zoom = 251,
  PA1 = 253,
  Clear = 254
}

public static class Keyboard
{
  private static byte[] _array =
  [
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
  ];

  public static bool IsKeyDown(KeyboardKey key)
  {
    return _array[(int)key] == 1;
  }

  public static bool IsKeyUp(KeyboardKey key)
  {
    return _array[(int)key] == 0;
  }

  public static void Update()
  {
    for (var i = 0; i < _array.Length; i++)
      if ((User32.GetAsyncKeyState(i) & 0x8000) != 0) _array[i] = 1;
      else _array[i] = 0;
  }
}