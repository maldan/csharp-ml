using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Shader;

public class PointVertexShader
{
  [ShaderFieldAttribute] public Vector4 aPosition;
  [ShaderFieldAttribute] public Vector4 aColor;

  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;

  [ShaderFieldOut] public Vector4 vo_Color;

  public Vector4 Main()
  {
    var gl_PointSize = aPosition.W;
    vo_Color = aColor;
    var newPosition = new Vector4(aPosition.X, aPosition.Y, aPosition.Z, 1.0f);
    return uProjectionMatrix * uViewMatrix * newPosition;
  }
}

public class PointFragmentShader
{
  [ShaderFieldIn] public Vector4 vo_Color;
  [ShaderFieldOut] public Vector4 color;

  public void Main()
  {
    color = vo_Color;
  }
}