using System;
using System.Collections.Generic;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class LR_Line : LR_Base
  {
    public LR_Line(OpenGL_Context context, RL_Base layer, Render_Scene scene) : base(context, layer, scene)
    {
    }

    public new void Init()
    {
      // language=glsl
      var lineShaderVertex = @"#version 330 core
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
      var lineShaderFragment = @"#version 330 core
          precision highp float;
          precision highp int;
          precision highp sampler2D;

          out vec4 color;

          void main()
          {
              color = vec4(1.0, 1.0, 1.0, 1.0);
          }".Replace("\r", "");

      // Create shader line
      Context.CreateShader(Layer.Name, lineShaderVertex, lineShaderFragment);

      // Create buffer vertex
      Context.CreateBuffer($"{Layer.Name}.vertex");
      Context.CreateVAO($"{Layer.Name}");

      // Create buffer color
    }

    public new void Render()
    {
      var layer = (RL_StaticLine)Layer;

      // User shader line
      Context.UseProgram(layer.Name);

      // gl blend fn
      OpenGL32.glEnable(OpenGL32.GL_BLEND);
      OpenGL32.glBlendFunc(OpenGL32.GL_SRC_ALPHA, OpenGL32.GL_ONE_MINUS_SRC_ALPHA);

      // bind matrix
      Context.BindMatrix(layer.Name, "uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Context.BindMatrix(layer.Name, "uViewMatrix", Scene.Camera.ViewMatrix);

      // gl enable depth test
      OpenGL32.glEnable(OpenGL32.GL_DEPTH_TEST);
      OpenGL32.glDepthFunc(OpenGL32.GL_LEQUAL);

      if (layer.IsSmooth) OpenGL32.glEnable(OpenGL32.GL_LINE_SMOOTH);
      else OpenGL32.glDisable(OpenGL32.GL_LINE_SMOOTH);
      OpenGL32.glLineWidth(layer.LineWidth);

      // Build actual line arrays data
      var vertexList = new List<float>();
      layer.ForEach<RO_Line>((line) =>
      {
        var from = line.From;
        var to = line.To;
        if (line.Transform != null)
        {
          from = line.From * line.Transform.Matrix;
          to = line.To * line.Transform.Matrix;
        }

        vertexList.Add(from.X);
        vertexList.Add(from.Y);
        vertexList.Add(from.Z);
        vertexList.Add(to.X);
        vertexList.Add(to.Y);
        vertexList.Add(to.Z);
      });

      // gl upload buffers
      Context.UploadBuffer($"{layer.Name}.vertex", vertexList.ToArray());

      // gl enable attributes
      Context.BindVAO(layer.Name);
      Context.EnableAttribute(layer.Name, $"{layer.Name}.vertex", "aVertex:vec3");

      // gl draw arrays
      OpenGL32.glDrawArrays(OpenGL32.GL_LINES, 0, layer.Count * 2);

      // Clear list
      layer.Clear();

      OpenGL32.glBindVertexArray(0);
    }
  }
}