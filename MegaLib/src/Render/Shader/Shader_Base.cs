using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class Shader_Base
{
  [ShaderMethod("ignore")]
  protected Vector3 normalize(Vector3 v)
  {
    return v.Normalized;
  }

  protected Matrix3x3 inverse(Matrix3x3 m)
  {
    return Matrix3x3.Inverse(m);
  }

  protected Matrix4x4 inverse(Matrix4x4 m)
  {
    return Matrix4x4.Inverse(m);
  }

  protected Matrix3x3 transpose(Matrix3x3 m)
  {
    return Matrix3x3.Transpose(m);
  }

  protected float pow(float a, float b)
  {
    return MathF.Pow(a, b);
  }

  protected Vector4 texture(Texture_2D<RGBA<float>> texture2D, Vector2 uv)
  {
    return new Vector4();
  }

  protected void discard()
  {
  }

  protected float length(Vector3 v)
  {
    return v.Length;
  }

  protected float dot(Vector3 v1, Vector3 v2)
  {
    return Vector3.Dot(v1, v2);
  }

  protected float max(float v1, float v2)
  {
    return MathF.Max(v1, v2);
  }

  protected Vector3 reflect(Vector3 a, Vector3 b)
  {
    return Vector3.Reflect(a, b);
  }

  protected Vector4 texelFetch(object a, IVector2 uv, int hz)
  {
    return new Vector4();
  }

  protected uint toUInt(int x)
  {
    return (uint)x;
  }

  protected int toInt(uint x)
  {
    return (int)x;
  }

  protected int toInt(float x)
  {
    return (int)x;
  }

  protected Vector4 texture(Texture_Cube a, Vector3 v)
  {
    return new Vector4();
  }

  protected Vector3 pow(Vector3 a, Vector3 b)
  {
    return Vector3.Pow(a, b);
  }
}