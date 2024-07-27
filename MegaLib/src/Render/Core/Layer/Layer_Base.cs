using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Core.Layer;

public interface ILayerRenderer
{
  public void Init();
  public void Render();
}

public class Layer_Sprite : Layer_Base
{
  public bool IsYInverted = false;
}

public class Layer_UI : Layer_Base
{
  public Camera_Orthographic Camera;
}

public class Layer_BitmapText : Layer_Base
{
}

public class Layer_StaticMesh : Layer_Base
{
}

public class Layer_SkinnedMesh : Layer_Base
{
}

public class Layer_Skybox : Layer_Base
{
}

public class Layer_Point : Layer_Base
{
}

public class Layer_Line : Layer_Base
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

public class Layer_Base
{
  private readonly List<RO_Base> _objectList = new();
  public string Name;
  public int Count => _objectList.Count;
  public ILayerRenderer LayerRenderer;

  public void Add(RO_Base obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Base obj)
  {
    _objectList.Remove(obj);
  }

  public void ForEach<T>(Action<T> fn) where T : RO_Base
  {
    foreach (var obj in _objectList.Where(obj => obj.IsVisible)) fn((T)obj);
  }

  public void Clear()
  {
    _objectList.Clear();
  }

  public void Init()
  {
    LayerRenderer?.Init();
  }

  public void Update(float delta)
  {
    foreach (var obj in _objectList) obj.Update(delta);
  }

  public void Render()
  {
    LayerRenderer?.Render();
  }
}