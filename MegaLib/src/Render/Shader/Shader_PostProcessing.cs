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
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uDepthTexture;
  [ShaderFieldUniformArray(64)] public Vector3[] uSSAOKernel;

  [ShaderFieldUniform] public Vector2 uScreenSize;
  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;

  // Functions
  private Vector3 ReconstructPosition(Vector2 uv, float depth)
  {
    var clipSpace = new Vector4(
      uv.X * 2.0f - 1.0f,
      uv.Y * 2.0f - 1.0f,
      depth * 2.0f - 1.0f,
      1.0f); // NDC
    var viewSpace = inverse(uProjectionMatrix) * clipSpace; // Back to view space
    return viewSpace.XYZ / viewSpace.W;
  }

  public void Main()
  {
    var depth = texture(uDepthTexture, vo_UV).R; // Sample depth

    var linearDepth = remap(depth, 0.99973f, 1, 0, 1);

    var normal = normalize(texture(uNormalTexture, vo_UV).XYZ); // Sample normal
    normal.X = remap(normal.X, 0.0f, 1.0f, -1f, 1f);
    normal.Y = remap(normal.Y, 0.0f, 1.0f, -1f, 1f);
    normal.Z = remap(normal.Z, 0.0f, 1.0f, -1f, 1f);

    var fragPos = ReconstructPosition(vo_UV, linearDepth); // Reconstruct position in view space

    var occlusion = 0.0f;
    for (var i = 0; i < 64; ++i)
    {
      // Sample kernel point
      var sample = fragPos + normal * uSSAOKernel[i] * 0.005f; // Scale kernel radius

      // Project sample back to screen space
      var aa = new Vector4(sample, 1.0f);
      var offset = uProjectionMatrix * aa;
      offset.X /= offset.W; // Perspective divide
      offset.Y /= offset.W; // Perspective divide
      offset.X = offset.X * 0.5f + 0.5f; // NDC to texture coordinates
      offset.Y = offset.Y * 0.5f + 0.5f; // NDC to texture coordinates

      // Sample depth at the offset
      var sampleDepth = texture(uDepthTexture, offset.XY).R;
      sampleDepth = remap(sampleDepth, 0.99973f, 1, 0, 1);

      // Compare depth values
      occlusion += sampleDepth < sample.Z ? 1.0f : 0.0f;
    }

    // Normalize occlusion
    var ao = occlusion / 64.0f;

    var textureColor = texture(uScreenTexture, vo_UV).XYZ;
    var dd = texture(uScreenTexture, vo_UV).A;
    fragColor = new Vector4(textureColor * ao, 1.0f);

    /*fragColor.R += dd;
    fragColor.G += dd;
    fragColor.B += dd;*/
  }
}