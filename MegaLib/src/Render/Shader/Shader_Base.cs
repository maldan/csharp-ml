using System;
using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public struct GlIn
{
  public Vector4 gl_Position; // Clip-space position of the vertex.
  public float gl_PointSize; // Point size, if applicable (optional, used for point primitives).
  public float[] gl_ClipDistance; // Array of user-defined clipping distances.
}

public class Shader_Base
{
  protected List<GlIn> gl_in;
  protected Vector4 gl_Position;
  protected Vector4 gl_FragCoord;

  [ShaderBuiltinMethod]
  protected Vector3 normalize(Vector3 v)
  {
    return v.Normalized;
  }

  [ShaderBuiltinMethod]
  protected Matrix3x3 inverse(Matrix3x3 m)
  {
    return Matrix3x3.Inverse(m);
  }

  [ShaderBuiltinMethod]
  protected Matrix4x4 inverse(Matrix4x4 m)
  {
    return Matrix4x4.Inverse(m);
  }

  [ShaderBuiltinMethod]
  protected Matrix3x3 transpose(Matrix3x3 m)
  {
    return Matrix3x3.Transpose(m);
  }

  [ShaderBuiltinMethod]
  protected float pow(float a, float b)
  {
    return MathF.Pow(a, b);
  }

  [ShaderBuiltinMethod]
  protected float abs(float a)
  {
    return MathF.Abs(a);
  }

  [ShaderBuiltinMethod]
  protected Vector4 texture(Texture_2D<RGBA32F> texture2D, Vector2 uv)
  {
    return new Vector4();
  }

  [ShaderBuiltinMethod]
  protected void discard()
  {
  }

  [ShaderBuiltinMethod]
  protected float length(Vector3 v)
  {
    return v.Length;
  }

  [ShaderBuiltinMethod]
  protected float dot(Vector3 v1, Vector3 v2)
  {
    return Vector3.Dot(v1, v2);
  }

  [ShaderBuiltinMethod]
  protected float max(float v1, float v2)
  {
    return MathF.Max(v1, v2);
  }

  [ShaderBuiltinMethod]
  protected float mix(float v1, float v2, float t)
  {
    throw new Exception("FUCK");
    return 0;
  }

  [ShaderBuiltinMethod]
  protected Vector4 mix(Vector4 v1, Vector4 v2, float t)
  {
    throw new Exception("FUCK");
  }

  [ShaderBuiltinMethod]
  protected Vector3 reflect(Vector3 a, Vector3 b)
  {
    return Vector3.Reflect(a, b);
  }

  [ShaderBuiltinMethod]
  protected float clamp(float v, float min, float max)
  {
    return v.Clamp(min, max);
  }

  [ShaderBuiltinMethod]
  protected Vector4 texelFetch(object a, IVector2 uv, int hz)
  {
    return new Vector4();
  }

  [ShaderBuiltinMethod]
  protected uint toUInt(int x)
  {
    return (uint)x;
  }

  [ShaderBuiltinMethod]
  protected int toInt(uint x)
  {
    return (int)x;
  }

  [ShaderBuiltinMethod]
  protected int toInt(float x)
  {
    return (int)x;
  }

  [ShaderBuiltinMethod]
  protected Vector4 texture(Texture_Cube a, Vector3 v)
  {
    return new Vector4();
  }

  [ShaderBuiltinMethod]
  protected Vector3 pow(Vector3 a, Vector3 b)
  {
    return Vector3.Pow(a, b);
  }

  [ShaderBuiltinMethod]
  protected void EmitVertex()
  {
  }

  [ShaderBuiltinMethod]
  protected void EndPrimitive()
  {
  }

  public static float remap(float value, float from1, float to1, float from2, float to2)
  {
    return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
  }
}