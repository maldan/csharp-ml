using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class LR_Line : LR_Base
  {
    private ListGPU<Vector3> _lines;
    private uint _vaoId;

    public LR_Line(OpenGL_Context context, RL_Base layer, Render_Scene scene) : base(context, layer, scene)
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

        layout (location = 0) in vec3 aVertex;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;

        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * vec4(aVertex.xyz, 1.0);
        }".Replace("\r", "");

      // language=glsl
      var fragment = @"#version 330 core
          precision highp float;
          precision highp int;
          precision highp sampler2D;

          out vec4 color;

          void main()
          {
              color = vec4(1.0, 1.0, 1.0, 1.0);
          }".Replace("\r", "");

      // Create shader line
      //Context.CreateShader(Layer.Name, lineShaderVertex, lineShaderFragment);

      // Create buffer vertex
      //Context.CreateBuffer($"{Layer.Name}.vertex");
      //Context.CreateVAO($"{Layer.Name}");

      // Create buffer color
      Shader.ShaderCode["vertex"] = vertex;
      Shader.ShaderCode["fragment"] = fragment;
      Shader.Compile();

      _lines = new ListGPU<Vector3>();
      Context.MapBuffer(_lines);

      // Create vao
      OpenGL32.glGenVertexArrays(1, ref _vaoId);
    }

    public override void Render()
    {
      var layer = (RL_StaticLine)Layer;

      Shader.Use();
      Shader.Enable(OpenGL32.GL_BLEND);
      Shader.Enable(OpenGL32.GL_DEPTH_TEST);

      Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);

      if (layer.IsSmooth) OpenGL32.glEnable(OpenGL32.GL_LINE_SMOOTH);
      else OpenGL32.glDisable(OpenGL32.GL_LINE_SMOOTH);
      OpenGL32.glLineWidth(layer.LineWidth);

      // Build actual line arrays data
      _lines.Clear();
      layer.ForEach<RO_Line>((line) =>
      {
        var from = line.From;
        var to = line.To;
        if (line.Transform != null)
        {
          from = line.From * line.Transform.Matrix;
          to = line.To * line.Transform.Matrix;
        }

        _lines.Add(from);
        _lines.Add(to);
      });

      // Upload on gpu
      _lines.Sync();

      // gl enable attributes
      OpenGL32.glBindVertexArray(_vaoId);
      Shader.EnableAttribute(_lines, "aVertex");

      // gl draw arrays
      OpenGL32.glDrawArrays(OpenGL32.GL_LINES, 0, layer.Count * 2);

      // Clear list
      layer.Clear();

      OpenGL32.glBindVertexArray(0);
    }
  }
}