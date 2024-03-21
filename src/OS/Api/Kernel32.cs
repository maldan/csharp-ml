using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api
{
  public class Kernel32
  {
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
  }
}