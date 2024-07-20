using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Core.Layer;

public class RL_Line : RL_Base
{
  public float LineWidth = 1.0f;
  public bool IsSmooth = true;
  public bool IsYInverted = false;

  public void DrawRectangle(Vector3 lt, Vector3 rb, RGBA<float> color)
  {
    Add(new RO_Line(lt.X, rb.Y, lt.Z, lt.X, lt.Y, lt.Z, color, color));
    Add(new RO_Line(lt.X, lt.Y, lt.Z, rb.X, lt.Y, lt.Z, color, color));
    Add(new RO_Line(rb.X, lt.Y, lt.Z, rb.X, rb.Y, lt.Z, color, color));
    Add(new RO_Line(rb.X, rb.Y, lt.Z, lt.X, rb.Y, lt.Z, color, color));
  }

  public void DrawAABB(Vector3 center, Vector3 size, RGBA<float> color)
  {
    //var xOffset = new float[] { -size.X / 2, size.X / 2 };
    var yOffset = new float[] { -size.Y / 2, size.Y / 2 };

    foreach (var y in yOffset)
    {
      Add(new RO_Line(
        center.X - size.X / 2, center.Y + y, center.Z - size.Z / 2,
        center.X - size.X / 2, center.Y + y, center.Z + size.Z / 2,
        color, color));
      Add(new RO_Line(
        center.X + size.X / 2, center.Y + y, center.Z - size.Z / 2,
        center.X + size.X / 2, center.Y + y, center.Z + size.Z / 2,
        color, color));
      Add(new RO_Line(
        center.X - size.X / 2, center.Y + y, center.Z - size.Z / 2,
        center.X + size.X / 2, center.Y + y, center.Z - size.Z / 2,
        color, color));
      Add(new RO_Line(
        center.X - size.X / 2, center.Y + y, center.Z + size.Z / 2,
        center.X + size.X / 2, center.Y + y, center.Z + size.Z / 2,
        color, color));
    }

    Add(new RO_Line(
      center.X - size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2,
      center.X - size.X / 2, center.Y + size.Y / 2, center.Z - size.Z / 2,
      color, color));
    Add(new RO_Line(
      center.X + size.X / 2, center.Y - size.Y / 2, center.Z - size.Z / 2,
      center.X + size.X / 2, center.Y + size.Y / 2, center.Z - size.Z / 2,
      color, color));
    Add(new RO_Line(
      center.X - size.X / 2, center.Y - size.Y / 2, center.Z + size.Z / 2,
      center.X - size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2,
      color, color));
    Add(new RO_Line(
      center.X + size.X / 2, center.Y - size.Y / 2, center.Z + size.Z / 2,
      center.X + size.X / 2, center.Y + size.Y / 2, center.Z + size.Z / 2,
      color, color));
  }

  public void DrawAABB(AABB aabb, RGBA<float> color)
  {
    DrawAABB(aabb.Center, aabb.Size, color);
  }
}