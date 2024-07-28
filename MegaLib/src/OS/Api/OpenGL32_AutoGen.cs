
using System;
using System.Runtime.InteropServices;
using GLenum = System.UInt32;
using GLboolean = System.Byte;
using GLint = System.Int32;
using GLsizei = System.Int32;
using GLuint = System.UInt32;
using GLbitfield = System.UInt32;
using GLfloat = System.Single;
using GLsizeiptr = System.IntPtr;
namespace MegaLib.OS.Api
  {
  public static partial class OpenGL32 {
// enable server-side GL capabilities
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glEnable(GLenum cap);

// disable server-side GL capabilities
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glDisable(GLenum cap);

// specify whether front- or back-facing facets can be culled
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glCullFace(GLenum mode);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glLineWidth(GLfloat width);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glBlendFunc(GLenum sfactor, GLenum dfactor);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glDepthFunc(GLenum func);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glPolygonMode(GLenum face, GLenum mode);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glDrawArrays(GLenum mode, GLint first, GLsizei count);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glBindTexture(GLenum target, GLuint texture);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, IntPtr data);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, IntPtr pixels);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureSubImage2D(GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, IntPtr pixels);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexParameterf(GLenum target, GLenum pname, GLfloat param);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexParameteri(GLenum target, GLenum pname, GLint param);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureParameterf(GLuint texture, GLenum pname, GLfloat param);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureParameteri(GLuint texture, GLenum pname, GLint param);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexParameterfv(GLenum target, GLenum pname, ref GLfloat paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexParameteriv(GLenum target, GLenum pname, ref GLint paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexParameterIiv(GLenum target, GLenum pname, ref GLint paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTexParameterIuiv(GLenum target, GLenum pname, ref GLuint paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureParameterfv(GLuint texture, GLenum pname, ref GLfloat paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureParameteriv(GLuint texture, GLenum pname, ref GLint paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureParameterIiv(GLuint texture, GLenum pname, ref GLint paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glTextureParameterIuiv(GLuint texture, GLenum pname, ref GLuint paramsList);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern bool wglDeleteContext(IntPtr hglrc);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern IntPtr wglCreateContext(IntPtr hdc);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern IntPtr glGetString(uint name);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glClear(GLbitfield mask);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glClearDepth(GLfloat depth);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glClearColor(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, IntPtr data);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern IntPtr wglGetCurrentDC();

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern IntPtr wglSwapBuffers(IntPtr hdc);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glViewport(GLint x, GLint y, GLsizei width, GLsizei height);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glPixelStoref(GLenum pname, GLfloat param);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glPixelStorei(GLenum pname, GLint param);

// 
[DllImport("opengl32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
public static extern void glDeleteTextures(GLsizei n, ref GLuint []textures);

// 
public static void glGenBuffers(GLsizei n, ref GLuint buffers) {
GetProcedure<glGenBuffersDelegate>("glGenBuffers")(n, ref buffers);
}
private delegate void glGenBuffersDelegate(GLsizei n, ref GLuint buffers);

// 
public static void glBindBuffer(GLenum target, GLuint buffer) {
GetProcedure<glBindBufferDelegate>("glBindBuffer")(target, buffer);
}
private delegate void glBindBufferDelegate(GLenum target, GLuint buffer);

// 
public static void glPatchParameteri(GLenum pname, GLint value) {
GetProcedure<glPatchParameteriDelegate>("glPatchParameteri")(pname, value);
}
private delegate void glPatchParameteriDelegate(GLenum pname, GLint value);

// specifies the parameters for patch primitives
public static void glPatchParameterfv(GLenum pname, GLfloat[] values) {
GetProcedure<glPatchParameterfvDelegate>("glPatchParameterfv")(pname, values);
}
private delegate void glPatchParameterfvDelegate(GLenum pname, GLfloat[] values);

// 
public static GLuint glCreateProgram() {
return GetProcedure<glCreateProgramDelegate>("glCreateProgram")();
}
private delegate GLuint glCreateProgramDelegate();

// 
public static void glGenVertexArrays(GLsizei n, ref GLuint arrays) {
GetProcedure<glGenVertexArraysDelegate>("glGenVertexArrays")(n, ref arrays);
}
private delegate void glGenVertexArraysDelegate(GLsizei n, ref GLuint arrays);

// 
public static void glDrawElements(GLenum mode, GLsizei count, GLenum type, IntPtr indices) {
GetProcedure<glDrawElementsDelegate>("glDrawElements")(mode, count, type, indices);
}
private delegate void glDrawElementsDelegate(GLenum mode, GLsizei count, GLenum type, IntPtr indices);

// 
public static void glBindVertexArray(GLuint array) {
GetProcedure<glBindVertexArrayDelegate>("glBindVertexArray")(array);
}
private delegate void glBindVertexArrayDelegate(GLuint array);

// 
public static GLuint glCreateShader(GLenum shaderType) {
return GetProcedure<glCreateShaderDelegate>("glCreateShader")(shaderType);
}
private delegate GLuint glCreateShaderDelegate(GLenum shaderType);

// 
public static void glShaderSource(GLuint shader, GLsizei count, IntPtr str, GLint[] length) {
GetProcedure<glShaderSourceDelegate>("glShaderSource")(shader, count, str, length);
}
private delegate void glShaderSourceDelegate(GLuint shader, GLsizei count, IntPtr str, GLint[] length);

// 
public static void glActiveTexture(GLenum texture) {
GetProcedure<glActiveTextureDelegate>("glActiveTexture")(texture);
}
private delegate void glActiveTextureDelegate(GLenum texture);

// 
public static void glUniform1f(GLint location, GLfloat v0) {
GetProcedure<glUniform1fDelegate>("glUniform1f")(location, v0);
}
private delegate void glUniform1fDelegate(GLint location, GLfloat v0);

// 
public static void glUniform2f(GLint location, GLfloat v0, GLfloat v1) {
GetProcedure<glUniform2fDelegate>("glUniform2f")(location, v0, v1);
}
private delegate void glUniform2fDelegate(GLint location, GLfloat v0, GLfloat v1);

// 
public static void glUniform3f(GLint location, GLfloat v0, GLfloat v1, GLfloat v2) {
GetProcedure<glUniform3fDelegate>("glUniform3f")(location, v0, v1, v2);
}
private delegate void glUniform3fDelegate(GLint location, GLfloat v0, GLfloat v1, GLfloat v2);

// 
public static void glUniform4f(GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3) {
GetProcedure<glUniform4fDelegate>("glUniform4f")(location, v0, v1, v2, v3);
}
private delegate void glUniform4fDelegate(GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3);

// 
public static void glUniform1i(GLint location, GLint v0) {
GetProcedure<glUniform1iDelegate>("glUniform1i")(location, v0);
}
private delegate void glUniform1iDelegate(GLint location, GLint v0);

// 
public static void glUniform2i(GLint location, GLint v0, GLint v1) {
GetProcedure<glUniform2iDelegate>("glUniform2i")(location, v0, v1);
}
private delegate void glUniform2iDelegate(GLint location, GLint v0, GLint v1);

// 
public static void glUniform3i(GLint location, GLint v0, GLint v1, GLint v2) {
GetProcedure<glUniform3iDelegate>("glUniform3i")(location, v0, v1, v2);
}
private delegate void glUniform3iDelegate(GLint location, GLint v0, GLint v1, GLint v2);

// 
public static void glUniform4i(GLint location, GLint v0, GLint v1, GLint v2, GLint v3) {
GetProcedure<glUniform4iDelegate>("glUniform4i")(location, v0, v1, v2, v3);
}
private delegate void glUniform4iDelegate(GLint location, GLint v0, GLint v1, GLint v2, GLint v3);

// 
public static void glUniform1ui(GLint location, GLuint v0) {
GetProcedure<glUniform1uiDelegate>("glUniform1ui")(location, v0);
}
private delegate void glUniform1uiDelegate(GLint location, GLuint v0);

// 
public static void glUniform2ui(GLint location, GLuint v0, GLuint v1) {
GetProcedure<glUniform2uiDelegate>("glUniform2ui")(location, v0, v1);
}
private delegate void glUniform2uiDelegate(GLint location, GLuint v0, GLuint v1);

// 
public static void glUniform3ui(GLint location, GLuint v0, GLuint v1, GLuint v2) {
GetProcedure<glUniform3uiDelegate>("glUniform3ui")(location, v0, v1, v2);
}
private delegate void glUniform3uiDelegate(GLint location, GLuint v0, GLuint v1, GLuint v2);

// 
public static void glUniform4ui(GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3) {
GetProcedure<glUniform4uiDelegate>("glUniform4ui")(location, v0, v1, v2, v3);
}
private delegate void glUniform4uiDelegate(GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3);

// 
public static void glUniform1fv(GLint location, GLsizei count, GLfloat[] value) {
GetProcedure<glUniform1fvDelegate>("glUniform1fv")(location, count, value);
}
private delegate void glUniform1fvDelegate(GLint location, GLsizei count, GLfloat[] value);

// 
public static void glUniform2fv(GLint location, GLsizei count, GLfloat[] value) {
GetProcedure<glUniform2fvDelegate>("glUniform2fv")(location, count, value);
}
private delegate void glUniform2fvDelegate(GLint location, GLsizei count, GLfloat[] value);

// 
public static void glUniform3fv(GLint location, GLsizei count, GLfloat[] value) {
GetProcedure<glUniform3fvDelegate>("glUniform3fv")(location, count, value);
}
private delegate void glUniform3fvDelegate(GLint location, GLsizei count, GLfloat[] value);

// 
public static void glUniform4fv(GLint location, GLsizei count, GLfloat[] value) {
GetProcedure<glUniform4fvDelegate>("glUniform4fv")(location, count, value);
}
private delegate void glUniform4fvDelegate(GLint location, GLsizei count, GLfloat[] value);

// 
public static void glUniform1iv(GLint location, GLsizei count, GLint[] value) {
GetProcedure<glUniform1ivDelegate>("glUniform1iv")(location, count, value);
}
private delegate void glUniform1ivDelegate(GLint location, GLsizei count, GLint[] value);

// 
public static void glUniform2iv(GLint location, GLsizei count, GLint[] value) {
GetProcedure<glUniform2ivDelegate>("glUniform2iv")(location, count, value);
}
private delegate void glUniform2ivDelegate(GLint location, GLsizei count, GLint[] value);

// 
public static void glUniform3iv(GLint location, GLsizei count, GLint[] value) {
GetProcedure<glUniform3ivDelegate>("glUniform3iv")(location, count, value);
}
private delegate void glUniform3ivDelegate(GLint location, GLsizei count, GLint[] value);

// 
public static void glUniform4iv(GLint location, GLsizei count, GLint[] value) {
GetProcedure<glUniform4ivDelegate>("glUniform4iv")(location, count, value);
}
private delegate void glUniform4ivDelegate(GLint location, GLsizei count, GLint[] value);

// 
public static void glUniform1uiv(GLint location, GLsizei count, GLuint[] value) {
GetProcedure<glUniform1uivDelegate>("glUniform1uiv")(location, count, value);
}
private delegate void glUniform1uivDelegate(GLint location, GLsizei count, GLuint[] value);

// 
public static void glUniform2uiv(GLint location, GLsizei count, GLuint[] value) {
GetProcedure<glUniform2uivDelegate>("glUniform2uiv")(location, count, value);
}
private delegate void glUniform2uivDelegate(GLint location, GLsizei count, GLuint[] value);

// 
public static void glUniform3uiv(GLint location, GLsizei count, GLuint[] value) {
GetProcedure<glUniform3uivDelegate>("glUniform3uiv")(location, count, value);
}
private delegate void glUniform3uivDelegate(GLint location, GLsizei count, GLuint[] value);

// 
public static void glUniform4uiv(GLint location, GLsizei count, GLuint[] value) {
GetProcedure<glUniform4uivDelegate>("glUniform4uiv")(location, count, value);
}
private delegate void glUniform4uivDelegate(GLint location, GLsizei count, GLuint[] value);

// 
public static void glUniformMatrix2fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix2fvDelegate>("glUniformMatrix2fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix2fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix3fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix3fvDelegate>("glUniformMatrix3fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix3fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix4fvDelegate>("glUniformMatrix4fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix4fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix2x3fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix2x3fvDelegate>("glUniformMatrix2x3fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix2x3fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix3x2fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix3x2fvDelegate>("glUniformMatrix3x2fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix3x2fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix2x4fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix2x4fvDelegate>("glUniformMatrix2x4fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix2x4fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix4x2fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix4x2fvDelegate>("glUniformMatrix4x2fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix4x2fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix3x4fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix3x4fvDelegate>("glUniformMatrix3x4fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix3x4fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static void glUniformMatrix4x3fv(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value) {
GetProcedure<glUniformMatrix4x3fvDelegate>("glUniformMatrix4x3fv")(location, count, transpose, value);
}
private delegate void glUniformMatrix4x3fvDelegate(GLint location, GLsizei count, GLboolean transpose, GLfloat[] value);

// 
public static GLint glGetUniformLocation(GLuint program, IntPtr name) {
return GetProcedure<glGetUniformLocationDelegate>("glGetUniformLocation")(program, name);
}
private delegate GLint glGetUniformLocationDelegate(GLuint program, IntPtr name);

// 
public static void glVertexAttribPointer(GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, IntPtr pointer) {
GetProcedure<glVertexAttribPointerDelegate>("glVertexAttribPointer")(index, size, type, normalized, stride, pointer);
}
private delegate void glVertexAttribPointerDelegate(GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, IntPtr pointer);

// 
public static void glVertexAttribIPointer(GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer) {
GetProcedure<glVertexAttribIPointerDelegate>("glVertexAttribIPointer")(index, size, type, stride, pointer);
}
private delegate void glVertexAttribIPointerDelegate(GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer);

// 
public static void glVertexAttribLPointer(GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer) {
GetProcedure<glVertexAttribLPointerDelegate>("glVertexAttribLPointer")(index, size, type, stride, pointer);
}
private delegate void glVertexAttribLPointerDelegate(GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer);

// 
public static GLint glGetAttribLocation(GLuint program, IntPtr name) {
return GetProcedure<glGetAttribLocationDelegate>("glGetAttribLocation")(program, name);
}
private delegate GLint glGetAttribLocationDelegate(GLuint program, IntPtr name);

// 
public static void glCompileShader(GLuint shader) {
GetProcedure<glCompileShaderDelegate>("glCompileShader")(shader);
}
private delegate void glCompileShaderDelegate(GLuint shader);

// 
public static void glAttachShader(GLuint program, GLuint shader) {
GetProcedure<glAttachShaderDelegate>("glAttachShader")(program, shader);
}
private delegate void glAttachShaderDelegate(GLuint program, GLuint shader);

// 
public static void glLinkProgram(GLuint program) {
GetProcedure<glLinkProgramDelegate>("glLinkProgram")(program);
}
private delegate void glLinkProgramDelegate(GLuint program);

// 
public static void glDeleteShader(GLuint shader) {
GetProcedure<glDeleteShaderDelegate>("glDeleteShader")(shader);
}
private delegate void glDeleteShaderDelegate(GLuint shader);

// 
public static void glCreateTextures(GLenum target, GLsizei n, ref GLuint textures) {
GetProcedure<glCreateTexturesDelegate>("glCreateTextures")(target, n, ref textures);
}
private delegate void glCreateTexturesDelegate(GLenum target, GLsizei n, ref GLuint textures);

// 
public static void glGenerateMipmap(GLuint target) {
GetProcedure<glGenerateMipmapDelegate>("glGenerateMipmap")(target);
}
private delegate void glGenerateMipmapDelegate(GLuint target);

// 
public static void glUseProgram(GLuint program) {
GetProcedure<glUseProgramDelegate>("glUseProgram")(program);
}
private delegate void glUseProgramDelegate(GLuint program);

// 
public static void glGetProgramiv(GLuint program, GLenum pname, ref GLint paramsList) {
GetProcedure<glGetProgramivDelegate>("glGetProgramiv")(program, pname, ref paramsList);
}
private delegate void glGetProgramivDelegate(GLuint program, GLenum pname, ref GLint paramsList);

// 
public static void glBufferData(GLenum target, GLsizeiptr size, IntPtr data, GLenum usage) {
GetProcedure<glBufferDataDelegate>("glBufferData")(target, size, data, usage);
}
private delegate void glBufferDataDelegate(GLenum target, GLsizeiptr size, IntPtr data, GLenum usage);

// 
public static void glEnableVertexAttribArray(GLuint index) {
GetProcedure<glEnableVertexAttribArrayDelegate>("glEnableVertexAttribArray")(index);
}
private delegate void glEnableVertexAttribArrayDelegate(GLuint index);

// 
public static void glDisableVertexAttribArray(GLuint index) {
GetProcedure<glDisableVertexAttribArrayDelegate>("glDisableVertexAttribArray")(index);
}
private delegate void glDisableVertexAttribArrayDelegate(GLuint index);

// 
public static void glEnableVertexArrayAttrib(GLuint vaobj, GLuint index) {
GetProcedure<glEnableVertexArrayAttribDelegate>("glEnableVertexArrayAttrib")(vaobj, index);
}
private delegate void glEnableVertexArrayAttribDelegate(GLuint vaobj, GLuint index);

// 
public static void glDisableVertexArrayAttrib(GLuint vaobj, GLuint index) {
GetProcedure<glDisableVertexArrayAttribDelegate>("glDisableVertexArrayAttrib")(vaobj, index);
}
private delegate void glDisableVertexArrayAttribDelegate(GLuint vaobj, GLuint index);

// 
public static void glGetShaderInfoLog(GLuint shader, GLsizei maxLength, ref GLsizei length, IntPtr infoLog) {
GetProcedure<glGetShaderInfoLogDelegate>("glGetShaderInfoLog")(shader, maxLength, ref length, infoLog);
}
private delegate void glGetShaderInfoLogDelegate(GLuint shader, GLsizei maxLength, ref GLsizei length, IntPtr infoLog);

// 
public static void glGetProgramInfoLog(GLuint program, GLsizei maxLength, ref GLsizei length, IntPtr infoLog) {
GetProcedure<glGetProgramInfoLogDelegate>("glGetProgramInfoLog")(program, maxLength, ref length, infoLog);
}
private delegate void glGetProgramInfoLogDelegate(GLuint program, GLsizei maxLength, ref GLsizei length, IntPtr infoLog);

// 
public static IntPtr wglCreateContextAttribsARB(IntPtr hDC, IntPtr hShareContext, GLint[] attribList) {
return GetProcedure<wglCreateContextAttribsARBDelegate>("wglCreateContextAttribsARB")(hDC, hShareContext, attribList);
}
private delegate IntPtr wglCreateContextAttribsARBDelegate(IntPtr hDC, IntPtr hShareContext, GLint[] attribList);

// 
public static void glDeleteBuffers(GLsizei n, GLuint[] buffers) {
GetProcedure<glDeleteBuffersDelegate>("glDeleteBuffers")(n, buffers);
}
private delegate void glDeleteBuffersDelegate(GLsizei n, GLuint[] buffers);

// 
public static void glGenFramebuffers(GLsizei n, ref GLuint ids) {
GetProcedure<glGenFramebuffersDelegate>("glGenFramebuffers")(n, ref ids);
}
private delegate void glGenFramebuffersDelegate(GLsizei n, ref GLuint ids);

// 
public static void glDeleteFramebuffers(GLsizei n, GLuint[] framebuffers) {
GetProcedure<glDeleteFramebuffersDelegate>("glDeleteFramebuffers")(n, framebuffers);
}
private delegate void glDeleteFramebuffersDelegate(GLsizei n, GLuint[] framebuffers);

// 
public static void glGenTextures(GLsizei n, ref GLuint textures) {
GetProcedure<glGenTexturesDelegate>("glGenTextures")(n, ref textures);
}
private delegate void glGenTexturesDelegate(GLsizei n, ref GLuint textures);

// 
public static void glFramebufferTexture2D(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level) {
GetProcedure<glFramebufferTexture2DDelegate>("glFramebufferTexture2D")(target, attachment, textarget, texture, level);
}
private delegate void glFramebufferTexture2DDelegate(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level);

// 
public static GLenum glCheckFramebufferStatus(GLenum target) {
return GetProcedure<glCheckFramebufferStatusDelegate>("glCheckFramebufferStatus")(target);
}
private delegate GLenum glCheckFramebufferStatusDelegate(GLenum target);

// 
public static void glBindFramebuffer(GLenum target, GLuint framebuffer) {
GetProcedure<glBindFramebufferDelegate>("glBindFramebuffer")(target, framebuffer);
}
private delegate void glBindFramebufferDelegate(GLenum target, GLuint framebuffer);

// 
public static void glGenRenderbuffers(GLsizei n, ref GLuint renderbuffers) {
GetProcedure<glGenRenderbuffersDelegate>("glGenRenderbuffers")(n, ref renderbuffers);
}
private delegate void glGenRenderbuffersDelegate(GLsizei n, ref GLuint renderbuffers);

// 
public static void glBindRenderbuffer(GLenum target, GLuint renderbuffer) {
GetProcedure<glBindRenderbufferDelegate>("glBindRenderbuffer")(target, renderbuffer);
}
private delegate void glBindRenderbufferDelegate(GLenum target, GLuint renderbuffer);

// 
public static void glRenderbufferStorage(GLenum target, GLenum internalformat, GLsizei width, GLsizei height) {
GetProcedure<glRenderbufferStorageDelegate>("glRenderbufferStorage")(target, internalformat, width, height);
}
private delegate void glRenderbufferStorageDelegate(GLenum target, GLenum internalformat, GLsizei width, GLsizei height);

// 
public static void glFramebufferRenderbuffer(GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer) {
GetProcedure<glFramebufferRenderbufferDelegate>("glFramebufferRenderbuffer")(target, attachment, renderbuffertarget, renderbuffer);
}
private delegate void glFramebufferRenderbufferDelegate(GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer);

}
}
