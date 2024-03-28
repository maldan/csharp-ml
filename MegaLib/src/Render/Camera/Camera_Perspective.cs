using System;
using MegaLib.Mathematics;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Camera
{
  public class Camera_Perspective : Camera_Base
  {
    private float _fov = 45.0f;
    private float _aspectRatio = 1f;
    private float _near = 0.01f;
    private float _far = 100.0f;

    public Camera_Perspective(float fov, float aspectRatio, float near, float far)
    {
      _fov = fov;
      _aspectRatio = aspectRatio;
      _near = near;
      _far = far;

      CalculateProjection();
    }

    public Camera_Perspective()
    {
      CalculateProjection();
    }

    public float FOV
    {
      get => _fov;
      set
      {
        _fov = value;
        CalculateProjection();
      }
    }

    public float AspectRatio
    {
      get => _aspectRatio;
      set
      {
        _aspectRatio = value;
        CalculateProjection();
      }
    }

    public float Near
    {
      get => _near;
      set
      {
        _near = value;
        CalculateProjection();
      }
    }

    public float Far
    {
      get => _far;
      set
      {
        _far = value;
        CalculateProjection();
      }
    }

    private void CalculateProjection()
    {
      //var tanHalfFov = (float)Math.Tan(_fov.DegToRad() * 0.5f);
      //var range = _near - _far;
      var f = (float)(1.0 / Math.Tan(_fov.DegToRad() / 2.0));

      _projectionMatrix = new Matrix4x4
      {
        M00 = f / _aspectRatio,
        M01 = 0.0f,
        M02 = 0.0f,
        M03 = 0.0f,

        M10 = 0.0f,
        M11 = f,
        M12 = 0.0f,
        M13 = 0.0f,

        M20 = 0.0f,
        M21 = 0.0f,
        M22 = (_far + _near) / (_near - _far),
        M23 = -1.0f,

        M30 = 0.0f,
        M31 = 0.0f,
        M32 = (2.0f * _far * _near) / (_near - _far),
        M33 = 0.0f
      };
    }
  }
}