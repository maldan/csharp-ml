using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class PostProcessingVertexShader : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;
  [ShaderFieldAttribute] public Vector2 aUV;

  [ShaderFieldOut] public Vector2 vo_UV;

  public Vector4 Main()
  {
    vo_UV = aUV;
    return new Vector4(aPosition.X, aPosition.Y, 0.0f, 1.0f);
  }
}

public class PostProcessingFragmentShader : Shader_Base
{
  [ShaderFieldIn] public Vector2 vo_UV;

  [ShaderFieldOut] public Vector4 fragColor;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uScreenTexture;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uViewNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uViewPositionTexture;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uOcclusionTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uDepthTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uRandomNoiseTexture;
  [ShaderFieldUniformArray(64)] public Vector3[] uSSAOKernel;

  [ShaderFieldUniform] public Vector2 _uScreenSize;
  [ShaderFieldUniform] public Vector2 _uCameraFarNear;
  [ShaderFieldUniform] public Matrix4x4 _uCameraProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 _uCameraProjectionInversedMatrix;


  private Vector3 ReconstructViewPos(float depth, Vector2 uv, Matrix4x4 projInv)
  {
    var clipPos = new Vector4(uv.X * 2.0f - 1.0f, uv.Y * 2.0f - 1.0f, depth, 1.0f);
    var viewPos = projInv * clipPos;
    return viewPos.XYZ / viewPos.W;
  }

  private float computeSSAO(
    Vector3 fragPos,
    Texture_2D<RGBA32F> depthTex,
    Matrix4x4 proj
  )
  {
    var occlusion = 0.0f;
    for (var i = 0; i < 64; ++i)
    {
      var samplePos = fragPos + uSSAOKernel[i] * 0.1f;
      var gg = new Vector4(samplePos, 1.0f);
      var offset = proj * gg;
      var ow = offset.W;
      var oxy = offset.XY;
      var oo = oxy / ow;
      var sampleUV = oo * 0.5f + 0.5f;
      var sampleDepth = texture(depthTex, sampleUV).R;
      //var linearDepth = DepthToLinear(_uCameraFarNear, sampleDepth);

      var rangeCheck = smoothstep(0.0f, 1.0f, 1.0f - abs(fragPos.Z - sampleDepth));
      occlusion += (sampleDepth >= samplePos.Z ? 1.0f : 0.0f) * rangeCheck;
    }

    return 1.0f - occlusion / 64f;
  }

  private Vector4 horizontalBlur(Texture_2D<RGBA32F> t, Vector2 uv, Vector2 texelSize)
  {
    var result = new Vector4();
    var kernel = new[] { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };

    for (var i = -2; i <= 2; ++i)
    {
      result += texture(t, uv + new Vector2(i, 0) * texelSize) * kernel[toInt(abs(i))];
    }

    return result;
  }

  private Vector3 BoxBlur(Texture_2D<RGBA32F> t, Vector2 uv, Vector2 texelSize, float power)
  {
    if (power <= 0) return texture(t, uv).XYZ;

    var result = new Vector3();
    var radius = toInt(ceil(power)); // Radius based on power
    float kernelSize = (2 * radius + 1) * (2 * radius + 1); // Total samples in the kernel

    for (var i = -radius; i <= radius; ++i)
    {
      for (var j = -radius; j <= radius; ++j)
      {
        result += texture(t, uv + new Vector2(i, j) * texelSize).XYZ / kernelSize;
      }
    }

    return result;
  }

  private Vector3 GaussianBlur(Texture_2D<RGBA32F> t, Vector2 uv, float blurRadius)
  {
    var result = new Vector3();
    var texelSize = new Vector2(0.1f, 0.1f);
    var r = toInt(clamp(blurRadius, 1, 64f));

    for (var i = 0; i < r; ++i)
    {
      var pp = uv;
      pp += uSSAOKernel[i].XY * texelSize;
      result += texture(t, pp).XYZ / r;
    }

    return result;
  }

  private Vector3 GaussianBlur2(Texture_2D<RGBA32F> t, Vector2 uv, int radius)
  {
    var kernel = new[] { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };
    var vx = texture(t, uv).XYZ;
    var result = vx * kernel[0];

    for (var i = 1; i <= radius; i++)
    {
      var v = new Vector2(i, 0) * 0.001f;
      var v2 = new Vector2(0, i) * 0.001f;

      result += texture(t, uv + v).XYZ * kernel[i] * 0.5f;
      result += texture(t, uv - v).XYZ * kernel[i] * 0.5f;
      result += texture(t, uv + v2).XYZ * kernel[i] * 0.5f;
      result += texture(t, uv - v2).XYZ * kernel[i] * 0.5f;
    }

    return result;
  }

  private Vector3 GaussianBlur3(Texture_2D<RGBA32F> t, Vector2 uv)
  {
    var r = new Vector3();
    var c = 0f;
    for (var i = -4; i <= 4; i++)
    {
      for (var j = -4; j <= 4; j++)
      {
        r += texture(t, uv + new Vector2(i * 0.0005f, j * 0.0005f)).XYZ;
        c += 1;
      }
    }

    return r / c;
  }

  public void Main()
  {
    var ff = texture(uScreenTexture, vo_UV).XYZ;
    //var ao = texture(uOcclusionTexture, vo_UV).XYZ;

    var ao2 = GaussianBlur3(uOcclusionTexture, vo_UV);

    fragColor = new Vector4(ff, 1.0f);
    if (vo_UV.X > 0.5f) fragColor = new Vector4(ff * ao2, 1.0f);
  }

  public void _Main()
  {
    // Unpack normals
    var normal = normalize(texture(uViewNormalTexture, vo_UV).XYZ); // Sample normal
    normal.X = remap(normal.X, 0.0f, 1.0f, -1f, 1f);
    normal.Y = remap(normal.Y, 0.0f, 1.0f, -1f, 1f);
    normal.Z = remap(normal.Z, 0.0f, 1.0f, -1f, 1f);

    // Construct TBN matrix to align kernel with surface
    var uNoiseScale = _uScreenSize / 4f;
    var noise = texture(uRandomNoiseTexture, vo_UV * uNoiseScale).XYZ;
    var tangent = normalize(noise - normal * dot(noise, normal));
    var bitangent = cross(normal, tangent);
    var TBN = new Matrix3x3(tangent, bitangent, normal);


    var depth = texture(uDepthTexture, vo_UV).R; // Sample depth
    var linearDepth = DepthToLinear(_uCameraFarNear, depth);
    var fragPos = texture(uViewPositionTexture, vo_UV).XYZ;

    // Loop over kernel samples
    var occlusion = 0.0f;
    var uRadius = 0.2f;
    for (var i = 0; i < 64; i++)
    {
      // Sample position in view space
      var samplePos = fragPos + TBN * uSSAOKernel[i] * uRadius;

      // Project sample position back to screen space
      var ss = new Vector4(samplePos, 1.0f);
      var offset = _uCameraProjectionMatrix * ss;
      var oxy = offset.XY;
      var ow = offset.W;
      var oo = oxy / ow;
      var sampleUV = oo * 0.5f + 0.5f;

      // Depth comparison
      var sampleDepth = texture(uViewPositionTexture, sampleUV).Z;
      var rangeCheck = smoothstep(0.0f, 1.0f, uRadius / abs(fragPos.Z - sampleDepth));

      if (sampleDepth < samplePos.Z - 0.005f)
      {
        occlusion += rangeCheck;
      }
    }

    // Normalize and invert occlusion
    occlusion = 1.0f - occlusion / 64.0f;

    // Outline
    var outline = 1f;
    for (var i = 0; i < 64; i++)
    {
      var oo = uSSAOKernel[i].XY;
      var offset = vo_UV + oo * 0.005f;
      var ld2 = DepthToLinear(_uCameraFarNear, texture(uDepthTexture, offset).R);
      if (abs(linearDepth - ld2) > 0.006f)
      {
        outline -= 1 / 64f;
      }
    }

    // Sasao
    var sasao = 1f;
    for (var i = 0; i < 64; i++)
    {
      var oo = uSSAOKernel[i].XY;
      //oo.X += uRandomNoise[toInt(gl_FragCoord.X) % 64].X * 0.05f;
      //oo.Y += uRandomNoise[toInt(gl_FragCoord.Y) % 64].Y * 0.05f;

      var offset = vo_UV + oo * 0.05f;
      var ld2 = DepthToLinear(_uCameraFarNear, texture(uDepthTexture, offset).R);
      if (linearDepth - ld2 > 0.01f) continue;
      if (linearDepth > ld2) sasao -= 1 / 64f;
    }

    /*
    var textureColor = texture(uScreenTexture, vo_UV).XYZ;
    if (linearDepth > 0.5f)
    {
      textureColor = BoxBlur(
        uScreenTexture, vo_UV, new Vector2(0.005f, 0.005f), (linearDepth - 0.5f) * 10f);
    }
    */

    var ff = texture(uScreenTexture, vo_UV).XYZ;
    /*if (linearDepth > 0.3f)
    {
      ff = GaussianBlur(
        uScreenTexture, vo_UV, (linearDepth - 0.3f) * 128f);
    }*/

    //var viewPos = ReconstructViewPos(depth, vo_UV, _uCameraProjectionInversedMatrix);
    //var ao = computeSSAO(viewPos, uDepthTexture, _uCameraProjectionMatrix);

    fragColor = new Vector4(ff, 1.0f);
    if (vo_UV.X > 0.2f) fragColor = new Vector4(ff * occlusion, 1.0f);
    if (vo_UV.X > 0.4f) fragColor = new Vector4(texture(uViewNormalTexture, vo_UV).XYZ, 1.0f);
    if (vo_UV.X > 0.6f) fragColor = new Vector4(occlusion, occlusion, occlusion, 1.0f);
    if (vo_UV.X > 0.8f) fragColor = new Vector4(texture(uViewPositionTexture, vo_UV).XYZ, 1.0f);

    /*var w = ReconstructViewPos(linearDepth, vo_UV, inverse(_uCameraProjectionMatrix));
    fragColor.X += w.X;
    fragColor.Y += w.Y;
    fragColor.Z += w.Z;*/
  }
}