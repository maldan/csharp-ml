using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Shader;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

internal class GG
{
  public List<Vector3> Lines = [];
  public List<Vector4> Colors = [];
}

public class LR_Line : LR_Base
{
  private ListGPU<Vector3> _lines;
  private ListGPU<Vector4> _colors;
  private uint _vaoId;

  public LR_Line(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
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
        layout (location = 1) in vec4 aColor;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        
        out vec4 vo_Color;

        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * vec4(aVertex.xyz, 1.0);
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

    // Create shader
    Shader.ShaderCode = ShaderProgram.Compile("Line");
    Shader.Compile();

    _lines = [];
    _colors = [];
    Context.MapBuffer(_lines);
    Context.MapBuffer(_colors);

    // Create vao
    OpenGL32.glGenVertexArrays(1, ref _vaoId);
  }

  public override void Render()
  {
    var layer = (Layer_Line)Layer;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    if (layer.Camera != null)
    {
      Shader.SetUniform("uProjectionMatrix", layer.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", layer.Camera.ViewMatrix);
    }
    else
    {
      Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
    }

    if (layer.IsSmooth) OpenGL32.glEnable(OpenGL32.GL_LINE_SMOOTH);
    else OpenGL32.glDisable(OpenGL32.GL_LINE_SMOOTH);

    // OpenGL32.glLineWidth(layer.LineWidth);


    var dict = new Dictionary<float, GG>();

    layer.ForEach<RO_Line>((line) =>
    {
      if (!dict.ContainsKey(line.Width)) dict[line.Width] = new GG();

      var from = line.From;
      var to = line.To;

      /*if (line.Transform != null)
      {
        from = line.From * line.Transform.Matrix;
        to = line.To * line.Transform.Matrix;
      }*/

      /*_lines.Add(from);
      _lines.Add(to);
      _colors.Add(new Vector4(line.FromColor.R, line.FromColor.G, line.FromColor.B, line.FromColor.A));
      _colors.Add(new Vector4(line.ToColor.R, line.ToColor.G, line.ToColor.B, line.ToColor.A));*/

      dict[line.Width].Lines.Add(from);
      dict[line.Width].Lines.Add(to);
      dict[line.Width].Colors.Add(line.FromColor.Vector4);
      dict[line.Width].Colors.Add(line.ToColor.Vector4);
    });

    foreach (var (width, value) in dict)
    {
      // Устанавливаем ширину
      OpenGL32.glLineWidth(width);

      // Очищаем прошлые линии
      _lines.Clear();
      _colors.Clear();

      // Загружаем в список
      _lines.AddRange(value.Lines.ToArray());
      _colors.AddRange(value.Colors.ToArray());

      // Загружаем на гпу
      _lines.Sync();
      _colors.Sync();

      // gl enable attributes
      OpenGL32.glBindVertexArray(_vaoId);
      Shader.EnableAttribute(_lines, "aPosition");
      Shader.EnableAttribute(_colors, "aColor");

      // Рисуем линии
      OpenGL32.glDrawArrays(OpenGL32.GL_LINES, 0, _lines.Count);
    }

    // Очищаем список
    layer.Clear();

    OpenGL32.glBindVertexArray(0);
  }
}