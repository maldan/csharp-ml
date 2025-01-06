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

  // Base uniforms
  [ShaderFieldUniform] public Vector2 _uScreenSize;
  [ShaderFieldUniform] public Vector2 _uCameraFarNear;
  [ShaderFieldUniform] public Matrix4x4 _uCameraProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 _uCameraProjectionInversedMatrix;

  // PP
  [ShaderFieldUniform] public Vector4 _uSSAOSettings;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> _uScreenTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> _uILTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> _uRMETexture;

  [ShaderBuiltinMethod]
  protected Vector3 normalize(Vector3 v)
  {
    return v.Normalized;
  }

  [ShaderBuiltinMethod]
  protected Vector3 cross(Vector3 v, Vector3 v2)
  {
    return Vector3.Cross(v2, v.Normalized);
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
  protected Vector4 textureLod(Texture_2D<RGBA32F> texture2D, Vector2 uv, float lod)
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
  protected float ceil(float v1)
  {
    return MathF.Ceiling(v1);
  }

  [ShaderBuiltinMethod]
  protected float exp(float v1)
  {
    return MathF.Exp(v1);
  }

  [ShaderBuiltinMethod]
  protected float mix(float v1, float v2, float t)
  {
    throw new Exception("FUCK");
    return 0;
  }

  [ShaderBuiltinMethod]
  protected float smoothstep(float v1, float v2, float t)
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
  protected float toFloat(int x)
  {
    return (float)x;
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

  public static float Remap(float value, float from1, float to1, float from2, float to2)
  {
    return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
  }

  public float DepthToLinear(float near, float far, float depth)
  {
    var z = depth;
    var zBufferParams = new Vector4(
      1.0f - far / near, // x
      far / near, // y
      (1.0f - far / near) / far, // z
      far / near / far // w
    );
    var linearDepth = 1.0f / (zBufferParams.Z * z + zBufferParams.W) / (far - near);
    return linearDepth;
  }

  public float DepthToLinear(Vector2 nf, float depth)
  {
    return DepthToLinear(nf.X, nf.Y, depth);
  }

  public Vector3 Blur13(Texture_2D<RGBA32F> image, Vector2 uv, Vector2 resolution, Vector2 direction)
  {
    var color = new Vector4();
    var off1 = new Vector2(1.411764705882353f, 1.411764705882353f) * direction;
    var off2 = new Vector2(3.2941176470588234f, 3.2941176470588234f) * direction;
    var off3 = new Vector2(5.176470588235294f, 5.176470588235294f) * direction;
    color += texture(image, uv) * 0.1964825501511404f;
    color += texture(image, uv + off1 / resolution) * 0.2969069646728344f;
    color += texture(image, uv - off1 / resolution) * 0.2969069646728344f;
    color += texture(image, uv + off2 / resolution) * 0.09447039785044732f;
    color += texture(image, uv - off2 / resolution) * 0.09447039785044732f;
    color += texture(image, uv + off3 / resolution) * 0.010381362401148057f;
    color += texture(image, uv - off3 / resolution) * 0.010381362401148057f;
    return color.XYZ;
  }

  public float GetEmission(Vector2 uv)
  {
    return texture(_uRMETexture, uv).Z;
  }
}