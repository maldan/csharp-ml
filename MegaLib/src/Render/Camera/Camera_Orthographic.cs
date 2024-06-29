using System;
using MegaLib.Mathematics;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Camera;

public class Camera_Orthographic : Camera_Base
{
  private float _left = 0;
  private float _top = 0;
  private float _right = 100;
  private float _bottom = 100;

  public float Left
  {
    get => _left;
    set
    {
      _left = value;
      CalculateProjection();
    }
  }

  public float Right
  {
    get => _right;
    set
    {
      _right = value;
      CalculateProjection();
    }
  }

  public float Top
  {
    get => _top;
    set
    {
      _top = value;
      CalculateProjection();
    }
  }

  public float Bottom
  {
    get => _bottom;
    set
    {
      _bottom = value;
      CalculateProjection();
    }
  }

  public Camera_Orthographic(float left, float top, float right, float bottom)
  {
    _left = left;
    _top = top;
    _right = right;
    _bottom = bottom;

    CalculateProjection();
  }

  public Camera_Orthographic()
  {
    CalculateProjection();
  }

  private void CalculateProjection()
  {
    _projectionMatrix = new Matrix4x4
    {
      M00 = 2.0f / (_right - _left),
      M01 = 0.0f,
      M02 = 0.0f,
      M03 = 0.0f,

      M10 = 0.0f,
      M11 = 2.0f / (_top - _bottom),
      M12 = 0.0f,
      M13 = 0.0f,

      M20 = 0.0f,
      M21 = 0.0f,
      M22 = -1.0f,
      M23 = 0.0f,

      M30 = -(_right + _left) / (_right - _left),
      M31 = -(_top + _bottom) / (_top - _bottom),
      M32 = 0.0f,
      M33 = 1.0f
    };
  }
}