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

      var r = 0;
      glShaderSource(shader, 1, dataPtr, ref r);
    }

    //[DllImport("opengl32.dll", CallingConvention = CallingConvention.StdCall)]
    //public static extern void glDebugMessageCallback(DebugProc callback, IntPtr userParam);

    // Описание функции glEnable
    /*[DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glPolygonMode(GLenum face, GLenum mode);

    // Описание функции glEnable
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glCullFace(GLenum mode);

    // Описание функции glEnable
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glEnable(uint cap);

    // Описание функции glDisable
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glDisable(uint cap);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glLineWidth(GLfloat width);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glBlendFunc(GLenum sfactor, GLenum dfactor);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glDepthFunc(GLenum func);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr glGetString(uint name);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr glClear(uint mask);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr glClearColor(float red, float green, float blue, float alpha);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern IntPtr wglCreateContext(IntPtr hdc);

    // Определение сигнатуры функции wglCreateContextAttribsARB
    //[DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    //public static extern IntPtr wglCreateContextAttribs(IntPtr hDC, IntPtr hShareContext, int[] attribList);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool wglDeleteContext(IntPtr hglrc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern IntPtr wglSwapBuffers(IntPtr hdc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern IntPtr wglGetCurrentDC();

    

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern void glDrawArrays(uint mode, int first, int count);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern void glDrawElements(GLenum mode, GLsizei count, GLenum type, IntPtr indices);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern void glBindTexture(GLenum target, GLuint texture);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glTexImage2D(uint target, int level, int internalFormat, int width, int height,
      int border, uint format, uint type, IntPtr data);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glTexParameteri(uint target, uint pname, int param);

    // Импорт функции glReadPixels из библиотеки OpenGL
    [DllImport("opengl32.dll")]
    public static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);

    // Импорт функции glViewport из библиотеки OpenGL
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Winapi)]
    public static extern void glViewport(int x, int y, int width, int height);*/

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

    /*public static void glPolygonMode(GLenum face, GLenum mode)
    {
      var ptr = wglGetProcAddress("glPolygonMode");
      Marshal.GetDelegateForFunctionPointer<glTwoIntInt>(ptr)((int)face, (int)mode);
    }*/

    /*public static IntPtr wglCreateContextAttribsARB(IntPtr hDC, IntPtr hShareContext, int[] attribList)
    {
      var ptr = wglGetProcAddress("wglCreateContextAttribsARB");
      if (ptr == IntPtr.Zero) throw new Exception("Method not found");
      var dataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(attribList, 0);
      return Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARBDelegate>(ptr)(hDC, hShareContext,
        dataPointer);
    }

    public static bool wglChoosePixelFormatARB(IntPtr hDC, int[] piAttribIList, IntPtr pfAttribFList,
      uint nMaxFormats, IntPtr piFormats, IntPtr nNumFormats)
    {
      var ptr = wglGetProcAddress("wglChoosePixelFormatARB");
      if (ptr == IntPtr.Zero) throw new Exception("Method not found");
      var attrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(piAttribIList, 0);

      int pixelCount = 0;
      int format = 0;

      return Marshal.GetDelegateForFunctionPointer<wglChoosePixelFormatARBDelegate>(ptr)(hDC, attrPtr, IntPtr.Zero, 1,
        ref format, ref pixelCount);
    }

    public static void glGenBuffers(int n, ref uint buffers)
    {
      var ptr = wglGetProcAddress("glGenBuffers");
      Marshal.GetDelegateForFunctionPointer<glGenBuffersDelegate>(ptr)(n, ref buffers);
    }

    public static void glPatchParameteri(GLenum pname, GLint value)
    {
      var ptr = wglGetProcAddress("glPatchParameteri");
      Marshal.GetDelegateForFunctionPointer<glTwoIntInt>(ptr)((int)pname, value);
    }

    public static void glGenVertexArrays(int n, ref uint buffers)
    {
      var ptr = wglGetProcAddress("glGenVertexArrays");
      var glGenBuffersFunc = Marshal.GetDelegateForFunctionPointer<glGenBuffersDelegate>(ptr);
      glGenBuffersFunc(n, ref buffers);
    }

    public static void glBindVertexArray(GLuint array)
    {
      var ptr = wglGetProcAddress("glBindVertexArray");
      var glGenBuffersFunc = Marshal.GetDelegateForFunctionPointer<glOneUint>(ptr);
      glGenBuffersFunc(array);
    }

    public static void glCreateTextures(GLenum target, GLsizei n, ref GLuint textures)
    {
      var ptr = wglGetProcAddress("glCreateTextures");
      Marshal.GetDelegateForFunctionPointer<glCreateTexturesDelegate>(ptr)(target, n, ref textures);
    }

    public static void glDeleteTextures(GLsizei n, GLuint[] textures)
    {
      var ptr = wglGetProcAddress("glDeleteTextures");
      Marshal.GetDelegateForFunctionPointer<glDeleteTexturesDelegate>(ptr)(n, textures);
    }

    public static void glBindBuffer(uint type, uint bufferId)
    {
      var ptr = wglGetProcAddress("glBindBuffer");
      Marshal.GetDelegateForFunctionPointer<glBindBufferDelegate>(ptr)(type, bufferId);
    }

    public static void glBufferData(uint target, int size, IntPtr data, uint usage)
    {
      var ptr = wglGetProcAddress("glBufferData");
      Marshal.GetDelegateForFunctionPointer<glBufferDataDelegate>(ptr)(target, size, data, usage);
    }

    public static void glBufferData(uint target, float[] data, uint usage)
    {
      var sizeInBytes = data.Length * sizeof(float);
      var dataPointer = Marshal.AllocHGlobal(sizeInBytes);
      Marshal.Copy(data, 0, dataPointer, data.Length);
      glBufferData(target, sizeInBytes, dataPointer, usage);
      Marshal.FreeHGlobal(dataPointer);
    }

    public static void glTexSubImage2D(GLenum target, int level, int xOffset, int yOffset, int width, int height,
      GLenum format, GLenum type, IntPtr data)
    {
      var ptr = wglGetProcAddress("glTexSubImage2D");
      Marshal.GetDelegateForFunctionPointer<glTexSubImage2DDelegate>(ptr)(target, level, xOffset, yOffset, width,
        height, format, type, data);
    }

    public static void glBufferData(uint target, uint[] data, uint usage)
    {
      //var sizeInBytes = data.Length * sizeof(uint);
      //var dataPointer = Marshal.AllocHGlobal(sizeInBytes);
      //Marshal.Copy(data, 0, dataPointer, data.Length);
      //glBufferData(target, sizeInBytes, dataPointer, usage);
      //Marshal.FreeHGlobal(dataPointer);

      var sizeInBytes = data.Length * sizeof(uint);
      var dataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
      glBufferData(target, sizeInBytes, dataPointer, usage);
    }*/

    /*public static void glUniformMatrix4fv(GLint location, GLsizei count, bool transpose, float[] data)
    {
      GLboolean t = (byte)GL_FALSE;
      if (transpose) t = (byte)GL_TRUE;
      // var pointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);

      // Allocate
      var sizeInBytes = data.Length * sizeof(float);
      var dataPointer = Marshal.AllocHGlobal(sizeInBytes);
      Marshal.Copy(data, 0, dataPointer, data.Length);

      glUniformMatrix4fv(location, count, t, dataPointer);

      // Free
      Marshal.FreeHGlobal(dataPointer);
    }

    public static void glUniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, IntPtr data)
    {
      var ptr = wglGetProcAddress("glUniformMatrix4fv");
      Marshal.GetDelegateForFunctionPointer<glUniformMatrix4fvDelegate>(ptr)(location, count, transpose, data);
    }

    public static void glUniform1i(GLint location, GLint v0)
    {
      var ptr = wglGetProcAddress("glUniform1i");
      Marshal.GetDelegateForFunctionPointer<glTwoIntInt>(ptr)(location, v0);
    }

    public static void glUniform3f(GLint location, float x, float y, float z)
    {
      var ptr = wglGetProcAddress("glUniform3f");
      Marshal.GetDelegateForFunctionPointer<glUniform3fDelegate>(ptr)(location, x, y, z);
    }

    public static void glEnableVertexAttribArray(uint index)
    {
      var ptr = wglGetProcAddress("glEnableVertexAttribArray");
      Marshal.GetDelegateForFunctionPointer<glCreateShaderDelegate>(ptr)(index);
    }

    public static void glActiveTexture(GLenum index)
    {
      var ptr = wglGetProcAddress("glActiveTexture");
      Marshal.GetDelegateForFunctionPointer<glOneUint>(ptr)(index);
    }

    public static uint glCreateShader(GLenum shaderType)
    {
      var ptr = wglGetProcAddress("glCreateShader");
      return Marshal.GetDelegateForFunctionPointer<glCreateShaderDelegate>(ptr)(shaderType);
    }

    public static uint glCreateProgram()
    {
      var ptr = wglGetProcAddress("glCreateProgram");
      return Marshal.GetDelegateForFunctionPointer<glCreateProgramDelegate>(ptr)();
    }

    public static void glAttachShader(uint program, uint shader)
    {
      var ptr = wglGetProcAddress("glAttachShader");
      Marshal.GetDelegateForFunctionPointer<glAttachShaderDelegate>(ptr)(program, shader);
    }

    public static void glLinkProgram(uint program)
    {
      var ptr = wglGetProcAddress("glLinkProgram");
      Marshal.GetDelegateForFunctionPointer<glLinkProgramDelegate>(ptr)(program);
    }

    public static void glUseProgram(uint program)
    {
      var ptr = wglGetProcAddress("glUseProgram");
      Marshal.GetDelegateForFunctionPointer<glLinkProgramDelegate>(ptr)(program);
    }

    public static void glDeleteShader(uint shader)
    {
      var ptr = wglGetProcAddress("glDeleteShader");
      Marshal.GetDelegateForFunctionPointer<glLinkProgramDelegate>(ptr)(shader);
    }

    public static void glShaderSource(uint shader, int count, IntPtr stringPtr, IntPtr length)
    {
      var ptr = wglGetProcAddress("glShaderSource");
      Marshal.GetDelegateForFunctionPointer<glShaderSourceDelegate>(ptr)(shader, count, stringPtr, length);
    }

    public static void glShaderSource(uint shader, string shaderCode)
    {
      byte[][] byteArray = { Encoding.ASCII.GetBytes(shaderCode) };
      var ptrArray = new IntPtr[byteArray.Length];
      for (var i = 0; i < byteArray.Length; i++)
        ptrArray[i] = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray[i], 0);
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(ptrArray, 0);

      glShaderSource(shader, 1, dataPtr, IntPtr.Zero);
    }

    public static void glCompileShader(uint shader)
    {
      var ptr = wglGetProcAddress("glCompileShader");
      Marshal.GetDelegateForFunctionPointer<glCompileShaderDelegate>(ptr)(shader);
    }

    public static void glGetShaderiv(uint shader, uint pname, out int parameters)
    {
      var ptr = wglGetProcAddress("glGetShaderiv");
      Marshal.GetDelegateForFunctionPointer<glGetShaderivDelegate>(ptr)(shader, pname, out parameters);
    }

    public static void glGetProgramiv(uint program, uint pname, out int param)
    {
      var ptr = wglGetProcAddress("glGetProgramiv");
      Marshal.GetDelegateForFunctionPointer<glGetProgramivDelegate>(ptr)(program, pname, out param);
    }

    public static string glGetShaderInfoLog(uint shader, int maxLength)
    {
      var ptr = wglGetProcAddress("glGetShaderInfoLog");

      var byteArray = new byte[maxLength];
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

      Marshal.GetDelegateForFunctionPointer<glGetShaderInfoLogDelegate>(ptr)(shader, maxLength, out var length,
        dataPtr);
      var segment = new ArraySegment<byte>(byteArray, 0, length);

      return Encoding.UTF8.GetString(segment);
    }

    public static string glGetProgramInfoLog(uint shader, int maxLength)
    {
      var ptr = wglGetProcAddress("glGetProgramInfoLog");

      var byteArray = new byte[maxLength];
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

      Marshal.GetDelegateForFunctionPointer<glGetShaderInfoLogDelegate>(ptr)(shader, maxLength, out var length,
        dataPtr);
      var segment = new ArraySegment<byte>(byteArray, 0, length);
      return Encoding.UTF8.GetString(segment);
    }

    public static void glVertexAttribPointer(uint index, int size, uint type, bool normalized, int stride,
      IntPtr pointer)
    {
      var ptr = wglGetProcAddress("glVertexAttribPointer");
      Marshal.GetDelegateForFunctionPointer<glVertexAttribPointerDelegate>(ptr)(index, size, type, normalized, stride,
        pointer);
    }

    public static void glVertexAttribIPointer(uint index, int size, uint type, int stride, IntPtr pointer)
    {
      var ptr = wglGetProcAddress("glVertexAttribIPointer");
      Marshal.GetDelegateForFunctionPointer<glVertexAttribIPointerDelegate>(ptr)(index, size, type, stride, pointer);
    }*/

    /*public static void glDrawArrays(uint mode, int first, int count)
    {
      var ptr = wglGetProcAddress("glDrawArrays");
      Marshal.GetDelegateForFunctionPointer<glDrawArraysDelegate>(ptr)(mode, first, count);
    }*/

    /*public static int glGetAttribLocation(uint program, string name)
    {
      var ptr = wglGetProcAddress("glGetAttribLocation");
      //var bytes = Encoding.ASCII.GetBytes(name);
      //var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);

      var namePtr = Marshal.StringToHGlobalAnsi(name);
      var attribLocation = Marshal.GetDelegateForFunctionPointer<glGetAttribLocationDelegate>(ptr)(program, namePtr);
      Marshal.FreeHGlobal(namePtr);
      return attribLocation;
    }

    public static int glGetUniformLocation(uint program, string name)
    {
      var ptr = wglGetProcAddress("glGetUniformLocation");
      //var bytes = Encoding.ASCII.GetBytes(name);
      //var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);

      var namePtr = Marshal.StringToHGlobalAnsi(name);
      var uniformLocation = Marshal.GetDelegateForFunctionPointer<glGetAttribLocationDelegate>(ptr)(program, namePtr);
      Marshal.FreeHGlobal(namePtr);
      return uniformLocation;
    }

    public static IntPtr glMapBuffer(GLenum target, GLenum access)
    {
      var ptr = wglGetProcAddress("glMapBuffer");
      return Marshal.GetDelegateForFunctionPointer<glMapBufferDelegate>(ptr)(target, access);
    }

    public static GLboolean glUnmapBuffer(GLenum target)
    {
      var ptr = wglGetProcAddress("glUnmapBuffer");
      return Marshal.GetDelegateForFunctionPointer<glUnmapBufferDelegate>(ptr)(target);
    }

    public static void glGenerateMipmap(GLuint target)
    {
      var ptr = wglGetProcAddress("glGenerateMipmap");
      Marshal.GetDelegateForFunctionPointer<glOneUint>(ptr)(target);
    }*/

    /*public static void glDrawElements(GLenum mode, GLsizei count, GLenum type, IntPtr indices)
    {
      var ptr = wglGetProcAddress("glDrawElements");
      Marshal.GetDelegateForFunctionPointer<glDrawElementsDelegate>(ptr)(mode, count, type, indices);
    }*/

    /*private delegate void glOneUint(uint p);

    private delegate void glTwoIntInt(int p1, int p2);

    private delegate void glGenBuffersDelegate(int n, ref uint buffers);

    private delegate void glCreateTexturesDelegate(GLenum target, GLsizei n, ref GLuint buffers);

    private delegate void glDeleteTexturesDelegate(GLsizei n, GLuint[] buffers);

    private delegate void glBindBufferDelegate(uint type, uint bufferId);

    private delegate void glBufferDataDelegate(uint target, int size, IntPtr data, uint usage);

    private delegate void glTexSubImage2DDelegate(GLenum target, int level, int xOffset, int yOffset, int width,
      int height,
      GLenum format, GLenum type, IntPtr data);

    private delegate uint glCreateShaderDelegate(uint shaderType);

    private delegate void glShaderSourceDelegate(uint shader, int count, IntPtr stringPtr, IntPtr length);

    private delegate void glCompileShaderDelegate(uint shader);

    private delegate void glGetShaderivDelegate(uint shader, uint pname, out int parameters);

    private delegate void glGetShaderInfoLogDelegate(uint shader, int maxLength, out int length, IntPtr infoLog);

    private delegate uint glCreateProgramDelegate();

    private delegate void glAttachShaderDelegate(uint program, uint shader);

    private delegate void glLinkProgramDelegate(uint program);

    private delegate void glGetProgramivDelegate(uint program, uint pname, out int param);

    private delegate void glDrawArraysDelegate(uint mode, int first, int count);

    private delegate int glGetAttribLocationDelegate(uint program, IntPtr name);

    private delegate IntPtr glMapBufferDelegate(GLenum target, GLenum access);

    private delegate GLboolean glUnmapBufferDelegate(GLenum target);

    private delegate void glUniform3fDelegate(int location, float x, float y, float z);

    private delegate void glUniformMatrix4fvDelegate(int location, int count, byte transpose, IntPtr data);

    private delegate void glDrawElementsDelegate(GLenum mode, GLsizei count, GLenum type, IntPtr indices);

    private delegate void glVertexAttribPointerDelegate(uint index, int size, uint type, bool normalized, int stride,
      IntPtr pointer);

    private delegate void glVertexAttribIPointerDelegate(uint index, int size, uint type, int stride,
      IntPtr pointer);

    private delegate IntPtr wglCreateContextAttribsARBDelegate(IntPtr a, IntPtr b, IntPtr c);

    private delegate bool wglChoosePixelFormatARBDelegate(IntPtr hDC, IntPtr piAttribIList, IntPtr pfAttribFList,
      uint nMaxFormats, ref int piFormats, ref int nNumFormats);*/
  }
}