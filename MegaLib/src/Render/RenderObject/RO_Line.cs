using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;

namespace MegaLib.Render.RenderObject;

public struct RO_Line
{
  public Vector3 From;
  public Vector3 To;
  public RGBA32F FromColor = new(1, 1, 1, 1);
  public RGBA32F ToColor = new(1, 1, 1, 1);
  public float Width = 1;

  public RO_Line()
  {
  }

  public RO_Line(Vector3 from, Vector3 to)
  {
    From = from;
    To = to;
  }

  public RO_Line(Vector3 from, Vector3 to, RGBA32F color)
  {
    From = from;
    To = to;
    FromColor = color;
    ToColor = color;
  }

  public RO_Line(Vector3 from, Vector3 to, RGBA32F color, float width)
  {
    From = from;
    To = to;
    FromColor = color;
    ToColor = color;
    Width = width;
  }

  public RO_Line(Vector3 from, Vector3 to, RGBA32F fromColor, RGBA32F toColor)
  {
    From = from;
    To = to;
    FromColor = fromColor;
    ToColor = toColor;
  }

  public RO_Line(float fx, float fy, float fz, float tx, float ty, float tz, RGBA32F fc, RGBA32F tc)
  {
    From = new Vector3(fx, fy, fz);
    To = new Vector3(tx, ty, tz);
    FromColor = fc;
    ToColor = tc;
  }

  public override string ToString()
  {
    return $"Line({From},{To})";
  }
}