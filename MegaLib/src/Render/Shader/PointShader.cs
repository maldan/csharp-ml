using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Shader;

public class PointVertexShader
{
  [ShaderField("attribute")] public Vector4 aPosition;
  [ShaderField("attribute")] public Vector4 aColor;

  [ShaderField("uniform")] public Matrix4x4 uProjectionMatrix;
  [ShaderField("uniform")] public Matrix4x4 uViewMatrix;

  [ShaderField("out")] public Vector4 vo_Color;

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
  [ShaderField("in")] public Vector4 vo_Color;
  [ShaderField("out")] public Vector4 color;

  public void Main()
  {
    color = vo_Color;
  }
}