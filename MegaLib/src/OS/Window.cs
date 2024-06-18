using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MegaLib.OS.Api;

namespace MegaLib.OS
{
  public class Window
  {
    private string _className;
    private IntPtr _handleWindow;
    private IntPtr _hdc;
    private IntPtr _hglrc;
    private IntPtr _hInstance;
    private WNDCLASS _wndclass;

    public Action OnShow;
    public Action OnClose;
    public Action OnCreated;
    public Action<float> OnPaint;
    public Action<int, int> OnResize;

    public IntPtr CurrentDC => _hdc;
    public IntPtr CurrentGLRC => _hglrc;

    private Stopwatch _timer = new();
    private float _delta = 0.016f;

    public bool IsFocused
    {
      get
      {
        var foregroundWindow = User32.GetForegroundWindow();
        return foregroundWindow == _handleWindow;
      }
    }

    public void Run()
    {
      MSG msg;
      while (User32.GetMessage(out msg, IntPtr.Zero, 0, 0))
      {
        User32.TranslateMessage(ref msg);
        User32.DispatchMessage(ref msg);
        if (msg.message == WinApi.WM_QUIT)
          break;
      }
    }

    public void Show()
    {
      User32.ShowWindow(_handleWindow, 1);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
      switch (msg)
      {
        case WinApi.WM_CREATE:
          OnCreated?.Invoke();
          return IntPtr.Zero;
        case WinApi.WM_SHOWWINDOW:
          OnShow?.Invoke();
          return IntPtr.Zero;
        case WinApi.WM_PAINT:
          _timer.Start();
          OnPaint?.Invoke(_delta);
          var deltaTime = _timer.Elapsed;
          _timer.Restart();
          _delta = (float)deltaTime.TotalMilliseconds / 1000f;
          if (_delta > 0.05) _delta = 0.05f;
          return IntPtr.Zero;
        case WinApi.WM_CLOSE:
          OnClose?.Invoke();
          OpenGL32.wglDeleteContext(_hglrc);
          User32.ReleaseDC(hWnd, _hdc);
          User32.DestroyWindow(hWnd);
          User32.UnregisterClass(_className, _hInstance);
          User32.PostQuitMessage(0);
          return IntPtr.Zero;
        case WinApi.WM_SIZE:
          var width = (int)(lParam.ToInt64() & 0xFFFF);
          var height = (int)((lParam.ToInt64() >> 16) & 0xFFFF);
          OnResize?.Invoke(width, height);
          return IntPtr.Zero;
        default:
          return User32.DefWindowProc(hWnd, msg, wParam, lParam);
      }
    }

    public byte[] GetSex()
    {
      return GDI32.GetImageBytesFromHDC(_hdc);
    }

    public void InitOpenGL()
    {
      _hdc = User32.GetDC(_handleWindow);

      var pfd = new PIXELFORMATDESCRIPTOR
      {
        nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR)),
        nVersion = 1,
        dwFlags = WinApi.PFD_DRAW_TO_WINDOW | WinApi.PFD_SUPPORT_OPENGL | WinApi.PFD_DOUBLEBUFFER,
        iPixelType = 0,
        cColorBits = 32,
        cDepthBits = 24,
        cStencilBits = 8,
        iLayerType = 0
      };

      var pixelFormat = GDI32.ChoosePixelFormat(_hdc, ref pfd);
      if (pixelFormat == 0) throw new Exception("Failed to choose pixel format.");
      if (!GDI32.SetPixelFormat(_hdc, pixelFormat, ref pfd)) throw new Exception("Failed to set pixel format.");

      //_hglrc = OpenGL32.wglCreateContext(_hdc);

      // Create olf contenxt
      var oldContextPtr = OpenGL32.wglCreateContext(_hdc);
      if (oldContextPtr == IntPtr.Zero) throw new Exception("Failed to create OpenGL context.");
      if (!OpenGL32.wglMakeCurrent(_hdc, oldContextPtr)) throw new Exception("Failed to make OpenGL context current.");

      // Create new context
      var attribs = new[]
      {
        (int)OpenGL32.WGL_CONTEXT_MAJOR_VERSION_ARB, 4,
        (int)OpenGL32.WGL_CONTEXT_MINOR_VERSION_ARB, 5,
        (int)OpenGL32.WGL_CONTEXT_FLAGS_ARB, 0,
        (int)OpenGL32.WGL_CONTEXT_PROFILE_MASK_ARB,
        (int)OpenGL32.WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
        0
      };
      _hglrc = OpenGL32.wglCreateContextAttribsARB(_hdc, IntPtr.Zero, attribs);
      if (_hglrc == IntPtr.Zero) throw new Exception("Failed to create OpenGL context.");

      OpenGL32.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
      OpenGL32.wglDeleteContext(oldContextPtr);

      if (!OpenGL32.wglMakeCurrent(_hdc, _hglrc)) throw new Exception("Failed to make OpenGL context current.");

      /*var pixAttribs = new[]
      {
        (int)OpenGL32.WGL_DRAW_TO_WINDOW_ARB, (int)OpenGL32.GL_TRUE,
        (int)OpenGL32.WGL_SUPPORT_OPENGL_ARB, (int)OpenGL32.GL_TRUE,
        (int)OpenGL32.WGL_DOUBLE_BUFFER_ARB, (int)OpenGL32.GL_TRUE,
        (int)OpenGL32.WGL_PIXEL_TYPE_ARB, (int)OpenGL32.WGL_TYPE_RGBA_ARB,
        (int)OpenGL32.WGL_COLOR_BITS_ARB, 32,
        (int)OpenGL32.WGL_DEPTH_BITS_ARB, 24,
        (int)OpenGL32.WGL_STENCIL_BITS_ARB, 8,
        0, // End
      };
      var ss = OpenGL32.wglChoosePixelFormatARB(_hdc, pixAttribs, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero);
      if (ss == false) throw new Exception("SS");*/

      var versionPtr = OpenGL32.glGetString(OpenGL32.GL_VERSION);
      if (versionPtr == IntPtr.Zero) throw new Exception("Failed to get OpenGL version.");

      var version = Marshal.PtrToStringAnsi(versionPtr);
      Console.WriteLine($"OpenGL Version: {version}");

      /*var extensionsPtr = OpenGL32.glGetString(OpenGL32.GL_EXTENSIONS);
      var extensions = Marshal.PtrToStringAnsi(extensionsPtr);
      Console.WriteLine($"OpenGL Extensions: {extensions}");*/
    }

    public static Window Create(string className, string title)
    {
      var window = new Window { _className = className };

      var hInstance = User32.GetModuleHandle(null);

      window._wndclass = new WNDCLASS
      {
        style = WinApi.CS_HREDRAW | WinApi.CS_VREDRAW | WinApi.CS_OWNDC,
        lpfnWndProc = window.WndProc,
        hInstance = hInstance,
        lpszClassName = className
      };

      if (User32.RegisterClass(ref window._wndclass) == 0) throw new Exception("Failed to register window class.");

      var hWnd = User32.CreateWindowEx(
        0,
        className,
        title,
        WinApi.WS_OVERLAPPEDWINDOW | WinApi.WS_CLIPSIBLINGS | WinApi.WS_CLIPCHILDREN,
        0, 0, 800, 600,
        IntPtr.Zero,
        IntPtr.Zero,
        hInstance,
        IntPtr.Zero
      );

      if (hWnd == IntPtr.Zero) throw new Exception("Failed to create window.");

      // Set window params
      window._handleWindow = hWnd;
      window._hInstance = hInstance;

      return window;
    }
  }
}