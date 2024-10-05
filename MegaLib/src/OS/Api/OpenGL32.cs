using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MegaLib.OS.Api;

using GLenum = uint;
using GLboolean = byte;
using GLint = int;
using GLsizei = int;
using GLuint = uint;
using GLfloat = float;

public static partial class OpenGL32
{
  private static Dictionary<string, IntPtr> _cache = new();
  private static Dictionary<string, object> _cache2 = new();
  private static Dictionary<string, IntPtr> _stringNamesCache = new();

  private static T GetProcedure<T>(string name)
  {
    if (_cache.ContainsKey(name))
      return (T)_cache2[name];
    //return Marshal.GetDelegateForFunctionPointer<T>(_cache[name]);

    var ptr = wglGetProcAddress(name);
    if (ptr == IntPtr.Zero) throw new Exception($"Procedure {name} not found");
    _cache[name] = ptr;
    var fn = Marshal.GetDelegateForFunctionPointer<T>(ptr);
    _cache2[name] = fn;
    return fn;
  }

  private static IntPtr GetStringNamePtr(string name)
  {
    if (_stringNamesCache.ContainsKey(name)) return _stringNamesCache[name];
    var namePtr = Marshal.StringToHGlobalAnsi(name);
    _stringNamesCache[name] = namePtr;
    return namePtr;
  }

  public delegate void DebugMessageDelegate(uint source, uint type, uint id, uint severity, int length,
    IntPtr message, IntPtr userParam);

  public delegate void Gavno(DebugMessageDelegate gav);

  [DllImport("opengl32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
  public static extern IntPtr wglGetProcAddress(string functionName);

  [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
  public static extern uint glGetError();

  public static void glShaderSource(uint shader, string shaderCode)
  {
    byte[][] byteArray = { Encoding.ASCII.GetBytes(shaderCode) };
    var ptrArray = new IntPtr[byteArray.Length];
    for (var i = 0; i < byteArray.Length; i++)
      ptrArray[i] = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray[i], 0);
    var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(ptrArray, 0);

    glShaderSource(shader, 1, dataPtr, null);
  }

  public static string glGetShaderInfoLog(uint shader, int maxLength)
  {
    var byteArray = new byte[maxLength];
    var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

    var length = 0;
    glGetShaderInfoLog(shader, maxLength, ref length, dataPtr);
    var segment = new ArraySegment<byte>(byteArray, 0, length);
    return Encoding.UTF8.GetString(segment);
  }

  public static string glGetProgramInfoLog(uint shader, int maxLength)
  {
    var byteArray = new byte[maxLength];
    var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

    var length = 0;
    glGetProgramInfoLog(shader, maxLength, ref length, dataPtr);
    var segment = new ArraySegment<byte>(byteArray, 0, length);
    return Encoding.UTF8.GetString(segment);
  }

  public static int glGetUniformLocation(uint program, string name)
  {
    var namePtr = GetStringNamePtr(name);
    var uniformLocation = glGetUniformLocation(program, namePtr);
    return uniformLocation;
  }

  public static int glGetAttribLocation(uint program, string name)
  {
    var namePtr = GetStringNamePtr(name);
    var attribLocation = glGetAttribLocation(program, namePtr);
    return attribLocation;
  }

  public static void glBufferData(uint target, float[] data, uint usage)
  {
    var sizeInBytes = data.Length * sizeof(float);
    var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
    glBufferData(target, (IntPtr)sizeInBytes, dataPtr, usage);
  }

  public static void glBufferData(uint target, uint[] data, uint usage)
  {
    var sizeInBytes = data.Length * sizeof(uint);
    var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
    glBufferData(target, (IntPtr)sizeInBytes, dataPtr, usage);
  }

  public static void glDeleteTextures(GLuint[] list)
  {
    // var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(list, 0);
    glDeleteTextures(list.Length, ref list);
  }

  public static void glDeleteTexture(GLuint id)
  {
    // var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(list, 0);
    glDeleteTextures([id]);
  }

  public static void glDeleteBuffer(GLuint id)
  {
    // var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(list, 0);
    glDeleteBuffers(1, [id]);
  }

  public static void glDeleteRenderbuffer(GLuint id)
  {
    // var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(list, 0);
    glDeleteRenderbuffers(1, [id]);
  }

  /*public static void glDeleteFramebuffers(GLsizei n, uint[] framebuffers) {
    var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(framebuffers, 0);
    glDeleteFramebuffers(n, dataPtr);
  }*/

  public static void PrintGlError(string fn = "")
  {
    var error = glGetError();
    // Console.WriteLine(error);

    // Проверка наличия ошибки
    if (error != GL_NO_ERROR)
      // Обработка ошибки
      switch (error)
      {
        case GL_INVALID_ENUM:
          Console.WriteLine($"{fn} OpenGL error: GL_INVALID_ENUM");
          break;
        case GL_INVALID_VALUE:
          Console.WriteLine($"{fn} OpenGL error: GL_INVALID_VALUE");
          break;
        case GL_INVALID_OPERATION:
          Console.WriteLine($"{fn} OpenGL error: GL_INVALID_OPERATION");
          break;
        case GL_OUT_OF_MEMORY:
          Console.WriteLine($"{fn} OpenGL error: GL_OUT_OF_MEMORY");
          break;
        default:
          Console.WriteLine($"{fn} OpenGL error: " + error);
          break;
      }
  }

  private static void DebugCallback(uint source, uint type, uint id, uint severity, int length, IntPtr message,
    IntPtr userParam)
  {
    if (type == GL_DEBUG_TYPE_ERROR)
    {
      var msg = Marshal.PtrToStringAnsi(message, length);
      Console.WriteLine($"OpenGL Error: [type={type}, severity={severity}] {msg}");
    }
  }

  public static void InitDebugCallback()
  {
    var ptr = wglGetProcAddress("glDebugMessageCallback");
    if (ptr == IntPtr.Zero) throw new Exception("Method not found");
    Marshal.GetDelegateForFunctionPointer<Gavno>(ptr)(DebugCallback);
  }

  public static void InitDebugCallback(DebugMessageDelegate debugCallback)
  {
    var ptr = wglGetProcAddress("glDebugMessageCallback");
    if (ptr == IntPtr.Zero) throw new Exception("Method not found");
    Marshal.GetDelegateForFunctionPointer<Gavno>(ptr)(debugCallback);
  }
}