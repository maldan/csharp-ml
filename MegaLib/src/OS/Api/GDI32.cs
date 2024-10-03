using System;
using System.Runtime.InteropServices;

namespace MegaLib.OS.Api;

public static class GDI32
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

  [StructLayout(LayoutKind.Sequential)]
  public struct BITMAPINFO2
  {
    public BITMAPINFOHEADER bmiHeader;
    public uint bmiColors;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BITMAPINFOHEADER
  {
    public uint biSize;
    public int biWidth;
    public int biHeight;
    public ushort biPlanes;
    public ushort biBitCount;
    public uint biCompression;
    public uint biSizeImage;
    public int biXPelsPerMeter;
    public int biYPelsPerMeter;
    public uint biClrUsed;
    public uint biClrImportant;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct SIZE
  {
    public int cx;
    public int cy;
  }

  public const int TRANSPARENT = 1;
  public const int OPAQUE = 2;
  public const uint DIB_RGB_COLORS = 0;
  public const uint NONANTIALIASED_QUALITY = 3;
  public const uint ANTIALIASED_QUALITY = 4;
  public const uint CLEARTYPE_QUALITY = 5;

  [DllImport("gdi32.dll", SetLastError = true)]
  public static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PIXELFORMATDESCRIPTOR ppfd);

  [DllImport("gdi32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, [In] ref PIXELFORMATDESCRIPTOR ppfd);

  [DllImport("gdi32.dll")]
  public static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines, IntPtr lpvBits,
    ref BITMAPINFO lpbmi, uint usage);

  //[DllImport("gdi32.dll")]
  //public static extern IntPtr GetDC(IntPtr hwnd);

  [DllImport("gdi32.dll")]
  public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

  [DllImport("gdi32.dll")]
  public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

  [DllImport("gdi32.dll")]
  public static extern bool DeleteObject(IntPtr hObject);

  [DllImport("gdi32.dll")]
  public static extern bool SwapBuffers(IntPtr hdc);

  [DllImport("gdi32.dll")]
  public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

  [DllImport("gdi32.dll")]
  public static extern bool DeleteDC(IntPtr hdc);

  [DllImport("gdi32.dll")]
  public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO2 pbmi, uint iUsage, out IntPtr ppvBits,
    IntPtr hSection, uint dwOffset);

  [DllImport("gdi32.dll")]
  public static extern int SetBkMode(IntPtr hdc, int mode);

  [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
  public static extern bool TextOut(IntPtr hdc, int x, int y, string lpString, int c);

  [DllImport("gdi32.dll")]
  public static extern uint SetTextColor(IntPtr hdc, uint color);

  [DllImport("gdi32.dll")]
  public static extern uint SetBkColor(IntPtr hdc, uint color);

  [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
  public static extern IntPtr CreateFont(
    int nHeight, int nWidth, int nEscapement, int nOrientation,
    int fnWeight, uint fdwItalic, uint fdwUnderline, uint fdwStrikeOut,
    uint fdwCharSet, uint fdwOutputPrecision, uint fdwClipPrecision,
    uint fdwQuality, uint fdwPitchAndFamily, string lpszFace);

  [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
  public static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int c, out SIZE lpSize);

  public static byte[] GetImageBytesFromHDC(IntPtr hDC)
  {
    var bmi = new BITMAPINFO
    {
      biSize = Marshal.SizeOf(typeof(BITMAPINFO)),
      biBitCount = 24,
      biWidth = 100,
      biHeight = 100
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