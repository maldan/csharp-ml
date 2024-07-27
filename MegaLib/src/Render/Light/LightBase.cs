using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.Light;

public class LightBase
{
  public Vector3 Position { get; set; }
  public RGBA<float> Color { get; set; }
  public float Intensity { get; set; }
  public bool CastShadows { get; set; }
}