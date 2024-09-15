using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Shader;

public class MyVertexShader
{
  public Vector3 aVertex;
  public Vector2 aUV;

  public Vector4 aColor;

  public Matrix4x4 uProjectionMatrix;

  public Matrix4x4 uViewMatrix;

  public Vector4 vo_Color;

  public float DistributionGGX(Vector3 N, Vector3 H, float roughness)
  {
    var a = roughness * roughness;
    var a2 = a * a;
    var NdotH = MathF.Max(Vector3.Dot(N, H), 0.0f);
    var NdotH2 = NdotH * NdotH;

    var num = a2;
    var denom = NdotH2 * (a2 - 1.0f) + 1.0f;
    denom = MathF.PI * denom * denom;

    return num / denom;
  }

  public Vector3 FresnelSchlick(float cosTheta, Vector3 F0)
  {
    return F0 + (1.0f - F0) * MathF.Pow(1.0f - cosTheta, 5.0f);
  }

  public Vector4 Main()
  {
    var gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(aVertex.X, aVertex.Y, aVertex.Z, 1.0f);
    vo_Color = aColor;
    return gl_Position;
  }
}