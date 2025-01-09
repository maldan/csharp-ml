using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class PostProcessingFirstVertexShader : Shader_Base
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

public class PostProcessingFirstFragmentShader : Shader_Base
{
  [ShaderFieldIn] public Vector2 vo_UV;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uViewNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uViewPositionTexture;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uRandomNoiseTexture;
  [ShaderFieldUniformArray(64)] public Vector3[] uSSAOKernel;

  //[ShaderFieldUniform] public Texture_2D<RGBA32F> uDepthTexture;

  [ShaderFieldOut] public Vector4 fragAO;
  [ShaderFieldOut] public Vector4 fragIndirectLight;

  private float SSAO(
    Matrix3x3 TBN,
    Texture_2D<RGBA32F> viewPositionTexture,
    Vector2 uv
  )
  {
    var currentViewPosition = texture(viewPositionTexture, uv).XYZ;
    if (currentViewPosition.Z <= 0) return 1f;
    
    // Loop over kernel samples
    var occlusion = 0.0f;
    var dist = (currentViewPosition.Z + 1f) * 0.05f;
    var uRadius = _uSSAOSettings.X * dist;
    var uBias = _uSSAOSettings.Y * dist;

    for (var i = 0; i < 64; i++)
    {
      // Sample position in view space
      var samplePos = currentViewPosition + TBN * uSSAOKernel[i] * uRadius;

      // Project sample position back to screen space
      var ss = new Vector4(samplePos, 1.0f);
      var offset = _uCameraProjectionMatrix * ss;
      var oxy = offset.XY;
      var ow = offset.W;
      //var oo = oxy / ow;
      var sampleUV = oxy / ow * 0.5f + 0.5f;

      // Depth comparison
      var sampleDepth = texture(viewPositionTexture, sampleUV).Z;
      var rangeCheck = smoothstep(0.0f, 1.0f, uRadius / abs(currentViewPosition.Z - sampleDepth));

      if (sampleDepth < samplePos.Z - uBias)
      {
        occlusion += rangeCheck;
      }
    }

    // Normalize and invert occlusion
    occlusion = 1.0f - occlusion / 64.0f;

    // if (occlusion < 0.2f) return 0f;
    return occlusion;
  }

  private Matrix3x3 GetTBNForSSAO(
    Texture_2D<RGBA32F> viewNormalTexture,
    Texture_2D<RGBA32F> randomNoiseTexture,
    Vector2 uv
  )
  {
    // Unpack normals
    var normal = normalize(texture(viewNormalTexture, uv).XYZ); // Sample normal
    normal.X = Remap(normal.X, 0.0f, 1.0f, -1f, 1f);
    normal.Y = Remap(normal.Y, 0.0f, 1.0f, -1f, 1f);
    normal.Z = Remap(normal.Z, 0.0f, 1.0f, -1f, 1f);

    // Construct TBN matrix to align kernel with surface
    var uNoiseScale = _uScreenSize / 8f;
    var noise = texture(randomNoiseTexture, uv * uNoiseScale).XYZ;
    var tangent = normalize(noise - normal * dot(noise, normal));
    var bitangent = cross(normal, tangent);
    var TBN = new Matrix3x3(tangent, bitangent, normal);
    return TBN;
  }

  private Vector3 LL(
    Matrix3x3 TBN,
    Texture_2D<RGBA32F> colorTexture,
    Texture_2D<RGBA32F> viewPositionTexture,
    Vector2 uv
  )
  {
    var currentViewPosition = texture(viewPositionTexture, uv).XYZ;
    if (currentViewPosition.Z <= 0)
    {
      var a = new Vector3();
      return a;
    }

    // Loop over kernel samples
    var inderectLight = new Vector3();
    var dist = (currentViewPosition.Z + 1f) * 0.2f;
    var uRadius = 1.2f * dist;
    var uBias = 0.05f * dist;

    for (var i = 0; i < 64; i++)
    {
      // Sample position in view space
      var samplePos = currentViewPosition + TBN * uSSAOKernel[i] * uRadius;

      // Project sample position back to screen space
      var ss = new Vector4(samplePos, 1.0f);
      var offset = _uCameraProjectionMatrix * ss;
      var oxy = offset.XY;
      var ow = offset.W;
      //var oo = oxy / ow;
      var sampleUV = oxy / ow * 0.5f + 0.5f;

      // Depth comparison
      var sampleDepth = texture(viewPositionTexture, sampleUV).Z;
      /*
      var rangeCheck = smoothstep(0.0f, 1.0f, uRadius / abs(currentViewPosition.Z - sampleDepth));

      if (sampleDepth < samplePos.Z - uBias)
      {
        occlusion += rangeCheck;
      }*/

      var color = texture(colorTexture, sampleUV).XYZ;
      inderectLight += color;
    }

    // Normalize and invert occlusion
    //occlusion = 1.0f - occlusion / 64.0f;
    return inderectLight / 64f;
  }

  private Vector4 Uber_SSAO(
    Matrix3x3 TBN,
    Texture_2D<RGBA32F> viewPositionTexture,
    Vector2 uv
  )
  {
    var ret = new Vector4(0, 0, 0, 1);
    var currentViewPosition = texture(viewPositionTexture, uv).XYZ;
    if (currentViewPosition.Z <= 0) return ret;

    // Loop over kernel samples
    var occlusion = 0.0f;
    var indirectLight = new Vector3();

    var distAO = (currentViewPosition.Z + 1f) * 0.05f;
    var distIR = (currentViewPosition.Z + 1f) * 0.1f;
    var uRadiusAO = 1.2f * distAO;
    var uRadiusIR = 2.4f * distIR;
    var uBias = 0.05f * distAO;

    for (var i = 0; i < 64; i++)
    {
      var tt = TBN * uSSAOKernel[i];

      // Sample position in view space
      var samplePos = currentViewPosition + tt * uRadiusAO;

      // Project sample position back to screen space
      var ss = new Vector4(samplePos, 1.0f);
      var offset = _uCameraProjectionMatrix * ss;
      var oxy = offset.XY;
      var sampleUV = oxy / offset.W * 0.5f + 0.5f;

      // Depth comparison
      var sampleDepth = texture(viewPositionTexture, sampleUV).Z;
      var rangeCheck = smoothstep(0.0f, 1.0f, uRadiusAO / abs(currentViewPosition.Z - sampleDepth));
      if (sampleDepth < samplePos.Z - uBias) occlusion += rangeCheck;

      // Sample position in view space
      samplePos = currentViewPosition + tt * uRadiusIR;

      // Project sample position back to screen space
      ss = new Vector4(samplePos, 1.0f);
      offset = _uCameraProjectionMatrix * ss;
      oxy = offset.XY;
      sampleUV = oxy / offset.W * 0.5f + 0.5f;

      var color = texture(_uScreenTexture, sampleUV).XYZ;
      // var emission = 0.2f + GetEmission(sampleUV) * 5f;
      indirectLight += color;
    }

    // Normalize and invert occlusion
    ret = new Vector4(indirectLight / 64f, 1.0f - occlusion / 64.0f);
    // ret.W = 1.0f - occlusion / 64.0f;
    return ret;
  }

  private float SSS(
    Texture_2D<RGBA32F> viewPositionTexture,
    Vector2 uv
    )
  {
    var startPosition = texture(viewPositionTexture, uv).XYZ;
    var startDepth = startPosition.Z;
    var lightDir = normalize(new Vector3(1, 1, 1));
    var myDir = -normalize(startPosition - lightDir);

    if (startPosition.Z <= 0) return 1f;
    
    for (int i = 0; i < 30; i++)
    {
      startPosition += myDir;
      
      // Project sample position back to screen space
      var ss = new Vector4(startPosition, 1.0f);
      var offset = _uCameraProjectionMatrix * ss * 0.05f;
      var oxy = offset.XY;
      var ow = offset.W;
      var sampleUV = oxy / ow * 0.5f + 0.5f;

      if (sampleUV.X < 0 || sampleUV.X > 1) return 1f;
      if (sampleUV.Y < 0 || sampleUV.Y > 1) return 1f;
      
      var newDepth = texture(viewPositionTexture, sampleUV).Z;
      if (newDepth <= 0) return 1f;
      if (newDepth < startDepth)
      {
        return 0f;
      }
    }
    
    return 1f;
  }
  
  public void Main()
  {
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

    var tbn = GetTBNForSSAO(uViewNormalTexture, uRandomNoiseTexture, vo_UV);

    //var uber = Uber_SSAO(tbn, uViewPositionTexture, vo_UV);
    //fragAO = new Vector4(uber.W, uber.W, uber.W, 1.0f);
    //fragInderectLight = new Vector4(uber.XYZ, 1.0f);

    var occlusion = SSAO(tbn, uViewPositionTexture, vo_UV);
    var sss = 1f; //SSS(uViewPositionTexture, vo_UV);
    fragAO = new Vector4(occlusion * sss, occlusion * sss, occlusion * sss, 1.0f);

    // var il = LL(tbn, _uScreenTexture, uViewPositionTexture, vo_UV);
    // fragInderectLight = new Vector4(il, 1.0f);
    
    var pp = texture(uViewPositionTexture, vo_UV).XYZ;
    fragIndirectLight = new Vector4(pp, 1.0f);
  }
}