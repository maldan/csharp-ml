using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class PostProcessingSSAOVertexShader : Shader_Base
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

public class PostProcessingSSAOFragmentShader : Shader_Base
{
  [ShaderFieldIn] public Vector2 vo_UV;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uViewNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uViewPositionTexture;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uRandomNoiseTexture;
  [ShaderFieldUniformArray(64)] public Vector3[] uSSAOKernel;

  [ShaderFieldUniform] public Vector2 _uScreenSize;
  [ShaderFieldUniform] public Matrix4x4 _uCameraProjectionMatrix;

  [ShaderFieldOut] public Vector4 fragColor;

  public void Main()
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

    var fragPos = texture(uViewPositionTexture, vo_UV).XYZ;

    // Loop over kernel samples
    var occlusion = 0.0f;
    var uRadius = 0.8f;
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

      if (sampleDepth < samplePos.Z - 0.05f)
      {
        occlusion += rangeCheck;
      }
    }

    // Normalize and invert occlusion
    occlusion = 1.0f - occlusion / 64.0f;

    // Normalize and invert occlusion
    //occlusion = 1.0f - occlusion / 64.0f;

    // Outline
    /*var outline = 1f;
    var pp = texture(uViewPositionTexture, vo_UV).Z;

    for (var i = 0; i < 64; i++)
    {
      var oo = uSSAOKernel[i].XY;
      var offset = vo_UV + oo * 0.001f;
      var ld2 = texture(uViewPositionTexture, offset).Z;
      if (abs(pp - ld2) > 0.002f)
      {
        outline -= 1 / 64f;
      }
    }*/

    fragColor = new Vector4(occlusion, occlusion, occlusion, 1.0f);
  }
}