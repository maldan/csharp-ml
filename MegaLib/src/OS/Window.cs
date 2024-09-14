using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MegaLib.Crypto;
using MegaLib.IO;
using MegaLib.OS.Api;
using Console = System.Console;

namespace MegaLib.OS;

public class Window
{
  private string _className;
  private IntPtr _handleWindow;
  private IntPtr _handleDeviceContext;
  private IntPtr _hglrc;
  private IntPtr _handleInstance;
  private WNDCLASS _wndclass;
  private Stopwatch _timer = new();
  private float _delta = 0.016f;
  private string _title = "Untitled";

  // События

  public Action OnShow;
  public Action OnClose;
  public Action OnCreated;
  public Action<float> OnPaint;
  public Action<int, int> OnResize;

  // Пропсы

  public IntPtr CurrentDeviceContext => _handleDeviceContext;
  public IntPtr CurrentGLRC => _hglrc;

  public bool IsFocused
  {
    get
    {
      var foregroundWindow = User32.GetForegroundWindow();
      return foregroundWindow == _handleWindow;
    }
  }

  public string Title
  {
    get => _title;
    set
    {
      _title = value;
      User32.SetWindowText(_handleWindow, value);
    }
  }

  public int Width
  {
    get
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      return rect.Right - rect.Left;
    }
    set
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      User32.MoveWindow(_handleWindow, rect.Left, rect.Top, value, rect.Bottom - rect.Top, true);
    }
  }

  public int Height
  {
    get
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      return rect.Bottom - rect.Top;
    }
    set
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      User32.MoveWindow(_handleWindow, rect.Left, rect.Top, rect.Right - rect.Left, value, true);
    }
  }

  public int X
  {
    get
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      return rect.Left;
    }
    set
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      User32.MoveWindow(_handleWindow, value, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
    }
  }

  public int Y
  {
    get
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      return rect.Top;
    }
    set
    {
      User32.GetWindowRect(_handleWindow, out var rect);
      User32.MoveWindow(_handleWindow, rect.Left, value, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
    }
  }

  // Методы

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

  public void Center()
  {
    X = User32.GetScreenWidth() / 2 - Width / 2;
    Y = User32.GetScreenHeight() / 2 - Height / 2;
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
        // User32.SetCursor(User32.LoadCursor(IntPtr.Zero, User32.IDC_ARROW));
        Mouse.Cursor = MouseCursor.Arrow;
        _timer.Start();
        OnPaint?.Invoke(_delta);
        var deltaTime = _timer.Elapsed;
        _timer.Restart();
        _delta = (float)deltaTime.TotalMilliseconds / 1000f;
        if (_delta > 0.05) _delta = 0.05f;
        Mouse.WheelDirection = 0;
        return IntPtr.Zero;
      case WinApi.WM_CLOSE:
        OnClose?.Invoke();
        OpenGL32.wglDeleteContext(_hglrc);
        User32.ReleaseDC(hWnd, _handleDeviceContext);
        User32.DestroyWindow(hWnd);
        User32.UnregisterClass(_className, _handleInstance);
        User32.PostQuitMessage(0);
        return IntPtr.Zero;
      case WinApi.WM_SIZE:
        var width = (int)(lParam.ToInt64() & 0xFFFF);
        var height = (int)((lParam.ToInt64() >> 16) & 0xFFFF);
        OnResize?.Invoke(width, height);
        return IntPtr.Zero;
      case WinApi.WM_SETCURSOR:
        var pointer = 32512;
        if (Mouse.Cursor == MouseCursor.Pointer) pointer = 32649;
        if (Mouse.Cursor == MouseCursor.Move) pointer = 32646;
        var hCursor = User32.LoadCursor(IntPtr.Zero, pointer);
        User32.SetCursor(hCursor);

        return IntPtr.Zero;
      /*case WinApi.WM_SETCURSOR:
        const int IDC_ARROW = 32512;
        var hCursor = User32.LoadCursor(IntPtr.Zero, IDC_ARROW);
        User32.SetCursor(hCursor);
        return IntPtr.Zero;*/
      case WinApi.WM_MOUSEWHEEL:
        int delta = (short)((wParam.ToInt64() >> 16) & 0xFFFF);
        var direction = delta > 0 ? 1 : -1;
        // OnMouseWheel?.Invoke(direction);
        Mouse.WheelDirection = direction;
        return IntPtr.Zero;
      default:
        return User32.DefWindowProc(hWnd, msg, wParam, lParam);
    }
  }

  public byte[] GetSex()
  {
    return GDI32.GetImageBytesFromHDC(_handleDeviceContext);
  }

  public void InitOpenGL()
  {
    _handleDeviceContext = User32.GetDC(_handleWindow);

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

    var pixelFormat = GDI32.ChoosePixelFormat(_handleDeviceContext, ref pfd);
    if (pixelFormat == 0) throw new Exception("Failed to choose pixel format.");
    if (!GDI32.SetPixelFormat(_handleDeviceContext, pixelFormat, ref pfd))
      throw new Exception("Failed to set pixel format.");

    // Create olf contenxt
    var oldContextPtr = OpenGL32.wglCreateContext(_handleDeviceContext);
    OpenGL32.PrintGlError("wglCreateContext");

    if (oldContextPtr == IntPtr.Zero) throw new Exception("Failed to create OpenGL context.");
    if (!OpenGL32.wglMakeCurrent(_handleDeviceContext, oldContextPtr))
      throw new Exception("Failed to make OpenGL context current.");
    OpenGL32.PrintGlError("wglMakeCurrent oldContextPtr");

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
    _hglrc = OpenGL32.wglCreateContextAttribsARB(_handleDeviceContext, IntPtr.Zero, attribs);
    if (_hglrc == IntPtr.Zero) throw new Exception("Failed to create OpenGL context.");
    OpenGL32.PrintGlError("wglCreateContextAttribsARB");

    OpenGL32.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
    OpenGL32.PrintGlError("wglMakeCurrent");

    OpenGL32.wglDeleteContext(oldContextPtr);
    OpenGL32.PrintGlError("wglDeleteContext");

    if (!OpenGL32.wglMakeCurrent(_handleDeviceContext, _hglrc))
      throw new Exception("Failed to make OpenGL context current.");

    var versionPtr = OpenGL32.glGetString(OpenGL32.GL_VERSION);
    if (versionPtr == IntPtr.Zero) throw new Exception("Failed to get OpenGL version.");

    var version = Marshal.PtrToStringAnsi(versionPtr);
    Console.WriteLine($"OpenGL Version: {version}");

    var extensionsPtr = OpenGL32.glGetString(OpenGL32.GL_EXTENSIONS);
    var extensions = Marshal.PtrToStringAnsi(extensionsPtr);
    Console.WriteLine($"OpenGL Extensions: {extensions}");

    // Инициализация дебага по умолчанию
    OpenGL32.glEnable(OpenGL32.GL_DEBUG_OUTPUT);
    OpenGL32.InitDebugCallback();
  }

  public Window()
  {
    var hInstance = User32.GetModuleHandle(null);
    _className = UID.Generate();

    // Создаем класс окна
    _wndclass = new WNDCLASS
    {
      style = WinApi.CS_HREDRAW | WinApi.CS_VREDRAW | WinApi.CS_OWNDC,
      lpfnWndProc = WndProc,
      hInstance = hInstance,
      lpszClassName = _className
    };

    // Регаем его
    if (User32.RegisterClass(ref _wndclass) == 0) throw new Exception("Failed to register window class.");

    // Создаем само окно
    var hWnd = User32.CreateWindowEx(
      0,
      _className,
      _title,
      WinApi.WS_OVERLAPPEDWINDOW | WinApi.WS_CLIPSIBLINGS | WinApi.WS_CLIPCHILDREN,
      0, 0, 800, 600,
      IntPtr.Zero,
      IntPtr.Zero,
      hInstance,
      IntPtr.Zero
    );

    // Чекаем создалось ли
    if (hWnd == IntPtr.Zero) throw new Exception("Failed to create window.");

    // Устанавливаем параметры
    _handleWindow = hWnd;
    _handleInstance = hInstance;
  }

  /*public static Window Create(string className, string title)
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
 */
}