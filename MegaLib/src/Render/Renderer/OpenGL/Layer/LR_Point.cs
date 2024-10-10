using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Shader;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Point : LR_Base
{
  private ListGPU<Vector4> _points;
  private ListGPU<Vector4> _colors;
  private uint _vaoId;

  public LR_Point(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
  }

  public override void Init()
  {
    // language=glsl
    var vertex = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec4 aVertex;
        layout (location = 1) in vec4 aColor;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        
        out vec4 vo_Color;

        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * vec4(aVertex.xyz, 1.0);
            gl_PointSize = aVertex.w;
            vo_Color = aColor;
        }".Replace("\r", "");

    // language=glsl
    var fragment = @"#version 330 core
          precision highp float;
          precision highp int;
          precision highp sampler2D;

          in vec4 vo_Color;
          out vec4 color;

          void main()
          {
              color = vo_Color;
          }".Replace("\r", "");

    // Create buffer color
    Shader.ShaderCode = ShaderProgram.Compile("Point");
    /*Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;*/
    Shader.Compile();

    _points = [];
    _colors = [];
    Context.MapBuffer(_points);
    Context.MapBuffer(_colors);

    // Create vao
    OpenGL32.glGenVertexArrays(1, ref _vaoId);
  }

  public override void Render()
  {
    var layer = (Layer_Point)Layer;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    OpenGL32.glEnable(OpenGL32.GL_PROGRAM_POINT_SIZE);

    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);

    // Build actual line arrays data
    _points.Clear();
    _colors.Clear();
    layer.ForEach((point) =>
    {
      _points.Add(new Vector4(point.Position.X, point.Position.Y, -point.Position.Z, point.Size));
      _colors.Add(new Vector4(point.Color.R, point.Color.G, point.Color.B, point.Color.A));
    });

    // Upload on gpu
    _points.Sync();
    _colors.Sync();

    // gl enable attributes
    OpenGL32.glBindVertexArray(_vaoId);
    Shader.EnableAttribute(_points, "aPosition");
    Shader.EnableAttribute(_colors, "aColor");

    // gl draw arrays
    OpenGL32.glDrawArrays(OpenGL32.GL_POINTS, 0, layer.Count);

    // Clear list
    layer.Clear();

    OpenGL32.glBindVertexArray(0);
  }
}