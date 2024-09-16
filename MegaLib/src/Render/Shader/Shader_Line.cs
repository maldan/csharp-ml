using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Shader;

public class LineVertexShader
{
  [ShaderFieldAttribute] public Vector3 aPosition;
  [ShaderFieldAttribute] public Vector4 aColor;

  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;

  [ShaderFieldOut] public Vector4 vo_Color;

  public Vector4 Main()
  {
    vo_Color = aColor;
    return uProjectionMatrix * uViewMatrix * new Vector4(aPosition, 1.0f);
  }
}

public class LineFragmentShader
{
  [ShaderFieldIn] public Vector4 vo_Color;
  [ShaderFieldOut] public Vector4 color;

  public void Main()
  {
    color = vo_Color;
  }
}