using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.RenderObject;

public class RO_Line : RO_Base
{
  public Vector3 From;
  public Vector3 To;
  public RGBA<float> FromColor = new(1, 1, 1, 1);
  public RGBA<float> ToColor = new(1, 1, 1, 1);

  public RO_Line()
  {
  }

  public RO_Line(Vector3 from, Vector3 to)
  {
    From = from;
    To = to;
  }

  public RO_Line(Vector3 from, Vector3 to, RGBA<float> color)
  {
    From = from;
    To = to;
    FromColor = color;
    ToColor = color;
  }

  public RO_Line(Vector3 from, Vector3 to, RGBA<float> fromColor, RGBA<float> toColor)
  {
    From = from;
    To = to;
    FromColor = fromColor;
    ToColor = toColor;
  }

  public RO_Line(float fx, float fy, float fz, float tx, float ty, float tz, RGBA<float> fc, RGBA<float> tc)
  {
    From = new Vector3(fx, fy, fz);
    To = new Vector3(tx, ty, tz);
    FromColor = fc;
    ToColor = tc;
  }
}