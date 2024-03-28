using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api
{
  [StructLayout(LayoutKind.Sequential)]
  public struct BITMAPINFO
  {
    public int biSize;
    public int biWidth;
    public int biHeight;
    public short biPlanes;
    public short biBitCount;
    public int biCompression;
    public int biSizeImage;
    public int biXPelsPerMeter;
    public int biYPelsPerMeter;
    public int biClrUsed;
    public int biClrImportant;
    public byte bmiColors_rgbBlue;
    public byte bmiColors_rgbGreen;
    public byte bmiColors_rgbRed;
    public byte bmiColors_rgbReserved;
  }

  public class GDI32
  {
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, [In] ref PIXELFORMATDESCRIPTOR ppfd);

    [DllImport("gdi32.dll")]
    public static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines, IntPtr lpvBits,
      ref BITMAPINFO lpbmi, uint usage);

    [DllImport("gdi32.dll")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern bool SwapBuffers(IntPtr hdc);

    public static byte[] GetImageBytesFromHDC(IntPtr hDC)
    {
      var bmi = new BITMAPINFO
      {
        biSize = Marshal.SizeOf(typeof(BITMAPINFO)),
        biBitCount = 24,
        biWidth = 100,
        biHeight = 100,
      };

      var hBitmap = CreateCompatibleBitmap(hDC, 100, 100);
      var hOldBitmap = SelectObject(hDC, hBitmap);

      var bufferSize = bmi.biWidth * bmi.biHeight * 3;
      var buffer = new byte[bufferSize];

      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

      GetDIBits(hDC, hBitmap, 0, (uint)bmi.biHeight, ptr, ref bmi, 0);

      SelectObject(hDC, hOldBitmap);
      DeleteObject(hBitmap);

      return buffer;
    }
  }
}