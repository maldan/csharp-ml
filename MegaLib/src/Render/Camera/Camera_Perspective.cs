using System;
using MegaLib.Ext;
using MegaLib.Mathematics;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Camera;

public class Camera_Perspective : Camera_Base
{
  private float _fov = 45.0f;
  private float _aspectRatio = 1f;

  public Camera_Perspective(float fov, float aspectRatio, float near, float far)
  {
    _fov = fov;
    _aspectRatio = aspectRatio;
    Near = near;
    Far = far;

    CalculateProjection();
  }

  public Camera_Perspective()
  {
    Near = 0.01f;
    Far = 32f;
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

  public Vector4 XrFOV
  {
    set
    {
      var tanLeft = MathF.Tan(value.X);
      var tanUp = MathF.Tan(value.Y);
      var tanRight = MathF.Tan(value.Z);
      var tanDown = MathF.Tan(value.W);

      var tanWidth = tanRight - tanLeft;
      var tanHeight = tanUp - tanDown;

      var a = 2.0f / tanWidth;
      var b = 2.0f / tanHeight;
      var c = (tanRight + tanLeft) / tanWidth;
      var d = (tanUp + tanDown) / tanHeight;
      var q = -(Far + Near) / (Far - Near);
      var qn = -2.0f * (Far * Near) / (Far - Near);

      _projectionMatrix = new Matrix4x4
      {
        M00 = a,
        M01 = 0.0f,
        M02 = 0.0f,
        M03 = 0.0f,

        M10 = 0.0f,
        M11 = b,
        M12 = 0.0f,
        M13 = 0.0f,

        M20 = c,
        M21 = d,
        M22 = q,
        M23 = -1.0f,

        M30 = 0.0f,
        M31 = 0.0f,
        M32 = qn,
        M33 = 0.0f
      };
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


  /*public override void CalculateProjection()
  {
    var tanHalfFovy = MathF.Tan(FOV.DegToRad() / 2.0f);
    var near = Near;
    var far = Far;

    var matrix = new Matrix4x4();

    matrix[0] = 1.0f / (AspectRatio * tanHalfFovy);
    matrix[1] = 0.0f;
    matrix[2] = 0.0f;
    matrix[3] = 0.0f;

    matrix[4] = 0.0f;
    matrix[5] = 1.0f / tanHalfFovy;
    matrix[6] = 0.0f;
    matrix[7] = 0.0f;

    matrix[8] = 0.0f;
    matrix[9] = 0.0f;
    matrix[10] = -(far + near) / (far - near);
    matrix[11] = -1.0f;

    matrix[12] = 0.0f;
    matrix[13] = 0.0f;
    matrix[14] = -(2.0f * far * near) / (far - near);
    matrix[15] = 0.0f;

    _projectionMatrix = matrix;
  }*/

  public override void CalculateProjection()
  {
    var yScale = 1f / (float)Math.Tan(FOV.DegToRad() / 2);
    var xScale = yScale / AspectRatio;

    var nearPlane = Near;
    var farPlane = Far;

    _projectionMatrix = new Matrix4x4
    {
      M00 = xScale,
      M11 = yScale,
      M22 = farPlane / (farPlane - nearPlane),
      M23 = 1,
      M32 = -(farPlane * nearPlane) / (farPlane - nearPlane),
      M33 = 0
    };
  }
}