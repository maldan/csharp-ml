using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api;

public class Kernel32
{
  [DllImport("kernel32.dll", SetLastError = true)]
  public static extern IntPtr GetModuleHandle(string lpModuleName);

  [DllImport("kernel32.dll", SetLastError = true)]
  public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect,
    out uint lpflOldProtect);
}