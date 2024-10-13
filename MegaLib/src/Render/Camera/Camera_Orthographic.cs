using System;
using MegaLib.Mathematics;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Renderer.OpenGL;

namespace MegaLib.Render.Camera;

public class Camera_Orthographic : Camera_Base
{
  private float _left = 0;
  private float _top = 0;
  private float _right = 100;
  private float _bottom = 100;

  public float LeftBorder
  {
    get => _left;
    set
    {
      _left = value;
      CalculateProjection();
    }
  }

  public float RightBorder
  {
    get => _right;
    set
    {
      _right = value;
      CalculateProjection();
    }
  }

  public float TopBorder
  {
    get => _top;
    set
    {
      _top = value;
      CalculateProjection();
    }
  }

  public float BottomBorder
  {
    get => _bottom;
    set
    {
      _bottom = value;
      CalculateProjection();
    }
  }

  public Camera_Orthographic(float left, float top, float right, float bottom, float near, float far)
  {
    _left = left;
    _top = top;
    _right = right;
    _bottom = bottom;
    Near = near;
    Far = far;

    CalculateProjection();
  }

  public Camera_Orthographic()
  {
    CalculateProjection();
  }

  public override void CalculateProjection()
  {
    var farPlane = Far;
    var nearPlane = Near;
    var width = _right - _left;
    var height = _top - _bottom;
    var depth = farPlane - nearPlane;

    _projectionMatrix = new Matrix4x4
    {
      M00 = 2 / (_right - _left),
      M11 = 2 / (_top - _bottom),
      M22 = 1 / (farPlane - nearPlane),
      M33 = 1,
      M30 = -(_right + _left) / (_right - _left),
      M31 = -(_top + _bottom) / (_top - _bottom),
      M32 = -nearPlane / (farPlane - nearPlane)
    };
  }
}