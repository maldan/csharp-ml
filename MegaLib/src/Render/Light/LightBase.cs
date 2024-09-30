using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.Light;

public class LightBase
{
  public Vector3 Position { get; set; }
  public RGBA<float> Color { get; set; } = new(1, 1, 1, 1);
  public float Intensity { get; set; } = 1f;
  public bool CastShadows { get; set; }
}