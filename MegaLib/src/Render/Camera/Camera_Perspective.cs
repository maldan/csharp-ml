using System;
using MegaLib.Ext;
using MegaLib.Geometry;
using MegaLib.Mathematics;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Camera;

public class Camera_Perspective : Camera_Base
{
  private float _fov = 45.0f;
  private float _aspectRatio = 1f;

  public override Frustum Frustum
  {
    get
    {
      var camera = this;
      // Положение камеры
      var cameraPosition = camera.Position;

      // Направление камеры 
      var forward = camera.Forward.Normalized;
      var up = camera.Up.Normalized;
      var right = camera.Right.Normalized;

      // Параметры камеры
      var nearClip = camera.Near; // Меняем местами near и far для Reversed-Z
      var farClip = camera.Far; // Меняем местами near и far для Reversed-Z
      var fov = camera.FOV;
      var aspectRatio = camera.AspectRatio;

      // Половина угла обзора по вертикали и горизонтали
      var tanFovY = (float)Math.Tan(fov * 0.5f);
      var tanFovX = tanFovY * aspectRatio;

      // Определяем размеры near и far плоскостей
      var nearHeight = 2.0f * nearClip * tanFovY;
      var nearWidth = nearHeight * aspectRatio;

      var farHeight = 2.0f * farClip * tanFovY;
      var farWidth = farHeight * aspectRatio;

      // Определяем центры near и far плоскостей
      var nearCenter = cameraPosition + forward * nearClip;
      var farCenter = cameraPosition + forward * farClip;

      // Вычисляем угловые точки near-плоскости
      var nearTopLeft = nearCenter + up * (nearHeight / 2.0f) - right * (nearWidth / 2.0f);
      var nearTopRight = nearCenter + up * (nearHeight / 2.0f) + right * (nearWidth / 2.0f);
      var nearBottomLeft = nearCenter - up * (nearHeight / 2.0f) - right * (nearWidth / 2.0f);
      var nearBottomRight = nearCenter - up * (nearHeight / 2.0f) + right * (nearWidth / 2.0f);

      // Вычисляем угловые точки far-плоскости
      var farTopLeft = farCenter + up * (farHeight / 2.0f) - right * (farWidth / 2.0f);
      var farTopRight = farCenter + up * (farHeight / 2.0f) + right * (farWidth / 2.0f);
      var farBottomLeft = farCenter - up * (farHeight / 2.0f) - right * (farWidth / 2.0f);
      var farBottomRight = farCenter - up * (farHeight / 2.0f) + right * (farWidth / 2.0f);

      // Построение плоскостей Frustum
      var topPlane = new Plane(nearTopLeft, farTopLeft, farTopRight); // Верхняя плоскость
      var bottomPlane = new Plane(nearBottomRight, farBottomRight, farBottomLeft); // Нижняя плоскость
      var leftPlane = new Plane(nearBottomLeft, farBottomLeft, farTopLeft); // Левая плоскость
      var rightPlane = new Plane(nearTopRight, farTopRight, farBottomRight); // Правая плоскость
      var nearPlane = new Plane(nearTopLeft, nearTopRight, nearBottomRight); // Near-плоскость
      var farPlane = new Plane(farTopRight, farTopLeft, farBottomLeft); // Far-плоскость

      return new Frustum(topPlane, bottomPlane, leftPlane, rightPlane, nearPlane, farPlane);
    }
  }

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

        M20 = -c,
        M21 = -d,
        M22 = Far / (Far - Near),
        M23 = 1.0f,

        M30 = 0.0f,
        M31 = 0.0f,
        M32 = -(Far * Near) / (Far - Near),
        M33 = 0.0f
      };
      
      /*var tanLeft = MathF.Tan(value.X);
      var tanUp = MathF.Tan(value.Y);
      var tanRight = MathF.Tan(value.Z);
      var tanDown = MathF.Tan(value.W);

      var tanWidth = tanRight - tanLeft;
      var tanHeight = tanUp - tanDown;

      var a = 2.0f / tanWidth;  // Horizontal scale
      var b = 2.0f / tanHeight; // Vertical scale
      var c = (tanRight + tanLeft) / tanWidth;  // Horizontal offset
      var d = (tanUp + tanDown) / tanHeight;    // Vertical offset

      // Adjust Z scaling and offset for inverted Z
      var q = (Near + Far) / (Far - Near);        // Z scaling for reversed depth
      var qn = (2.0f * Near * Far) / (Far - Near); // Z offset for reversed depth

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
        M22 = q,       // Z scaling for inverted depth
        M23 = 1.0f,    // Maintain perspective division

        M30 = 0.0f,
        M31 = 0.0f,
        M32 = qn,      // Z offset for inverted depth
        M33 = 0.0f
      };*/
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