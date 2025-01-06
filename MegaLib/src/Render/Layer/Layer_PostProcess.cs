using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Layer;

public class Layer_PostProcess : Layer_Base
{
  public Vector4 SSAO_Settings = new(1.5f, 0.1f, 1.0f, 1.0f);
}