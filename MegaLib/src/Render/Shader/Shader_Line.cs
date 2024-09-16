using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Shader;

public class LineVertexShader
{
  [ShaderField("attribute")] public Vector3 aPosition;
  [ShaderField("attribute")] public Vector4 aColor;

  [ShaderField("uniform")] public Matrix4x4 uProjectionMatrix;
  [ShaderField("uniform")] public Matrix4x4 uViewMatrix;

  [ShaderField("out")] public Vector4 vo_Color;

  public Vector4 Main()
  {
    vo_Color = aColor;
    return uProjectionMatrix * uViewMatrix * new Vector4(aPosition, 1.0f);
  }
}

public class LineFragmentShader
{
  [ShaderField("in")] public Vector4 vo_Color;
  [ShaderField("out")] public Vector4 color;

  public void Main()
  {
    color = vo_Color;
  }
}