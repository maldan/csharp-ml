using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MegaLib.OS.Api
{
  using GLenum = UInt32;
  using GLboolean = Byte;
  using GLint = Int32;
  using GLsizei = Int32;
  using GLuint = UInt32;
  using GLfloat = Single;

  public static partial class OpenGL32
  {
    private static Dictionary<string, IntPtr> _cache = new();

    private static T GetProcedure<T>(string name)
    {
      if (_cache.ContainsKey(name))
        return Marshal.GetDelegateForFunctionPointer<T>(_cache[name]);

      var ptr = wglGetProcAddress(name);
      if (ptr == IntPtr.Zero) throw new Exception($"Procedure {name} not found");
      _cache[name] = ptr;
      return Marshal.GetDelegateForFunctionPointer<T>(ptr);
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
      var namePtr = Marshal.StringToHGlobalAnsi(name);
      var uniformLocation = glGetUniformLocation(program, namePtr);
      Marshal.FreeHGlobal(namePtr);
      return uniformLocation;
    }

    public static int glGetAttribLocation(uint program, string name)
    {
      var namePtr = Marshal.StringToHGlobalAnsi(name);
      var attribLocation = glGetAttribLocation(program, namePtr);
      Marshal.FreeHGlobal(namePtr);
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
    
    /*public static void glDeleteFramebuffers(GLsizei n, uint[] framebuffers) {
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(framebuffers, 0); 
      glDeleteFramebuffers(n, dataPtr);
    }*/

    public static void PrintGlError()
    {
      var error = glGetError();
      Console.WriteLine(error);

      // Проверка наличия ошибки
      if (error != GL_NO_ERROR)
        // Обработка ошибки
        switch (error)
        {
          case GL_INVALID_ENUM:
            Console.WriteLine("OpenGL error: GL_INVALID_ENUM");
            break;
          case GL_INVALID_VALUE:
            Console.WriteLine("OpenGL error: GL_INVALID_VALUE");
            break;
          case GL_INVALID_OPERATION:
            Console.WriteLine("OpenGL error: GL_INVALID_OPERATION");
            break;
          case GL_OUT_OF_MEMORY:
            Console.WriteLine("OpenGL error: GL_OUT_OF_MEMORY");
            break;
          default:
            Console.WriteLine("OpenGL error: " + error);
            break;
        }
    }

    public static void InitDebugCallback(DebugMessageDelegate debugCallback)
    {
      var ptr = wglGetProcAddress("glDebugMessageCallback");
      if (ptr == IntPtr.Zero) throw new Exception("Method not found");
      Marshal.GetDelegateForFunctionPointer<Gavno>(ptr)(debugCallback);
    }
  }
}