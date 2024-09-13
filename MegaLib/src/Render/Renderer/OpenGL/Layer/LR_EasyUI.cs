using System;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.UI.EasyUI;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_EasyUI : LR_Base
{
  private ListGPU<Vector3> _vertices;
  private ListGPU<uint> _indices;
  private ListGPU<Vector4> _colors;
  private ListGPU<Vector2> _uv;
  private uint _vaoId;
  private Matrix4x4 _mx = Matrix4x4.Identity;

  public LR_EasyUI(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
  }

  public override void Init()
  {
    # region vertex

    // language=glsl
    var vertex = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aUV;
        layout (location = 2) in vec4 aColor;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform mat4 uModelMatrix;
        
        out vec3 vo_Position;
        out vec4 vo_Color;
        out vec2 vo_UV;
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            
            vo_Position = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            vo_Color = aColor;
            vo_UV = aUV;
        }";

    #endregion

    #region fragment

    // language=glsl
    var fragment = @"
        #version 330 core
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vo_Position;
        in vec4 vo_Color;
        in vec2 vo_UV;
        
        out vec4 color;
        
        uniform sampler2D uFontTexture;
        uniform vec3 uMode;
        
        const float PI = 3.141592653589793;
        
        float remap(float value, float fromMin, float fromMax, float toMin, float toMax) {
            float normalizedValue = (value - fromMin) / (fromMax - fromMin);
            return mix(toMin, toMax, normalizedValue);
        }
        
        vec3 linearToSRGB(vec3 colorLinear) {
            vec3 colorSRGB;
            for (int i = 0; i < 3; ++i) {
                if (colorLinear[i] <= 0.0031308) {
                    colorSRGB[i] = 12.92 * colorLinear[i];
                } else {
                    colorSRGB[i] = 1.055 * pow(colorLinear[i], 1.0 / 2.4) - 0.055;
                }
            }
            return colorSRGB;
        }
            
        vec3 sRGBToLinear(vec3 colorSRGB) {
            vec3 colorLinear;
            for (int i = 0; i < 3; ++i) {
                if (colorSRGB[i] <= 0.04045) {
                    colorLinear[i] = colorSRGB[i] / 12.92;
                } else {
                    colorLinear[i] = pow((colorSRGB[i] + 0.055) / 1.055, 2.4);
                }
            }
            return colorLinear;
        }
        
        void main()
        {
            if (uMode == vec3(1, 0, 0)) {
                color = vo_Color;
            } else {
                if (vo_UV.x < 0.0) {
                    color = vo_Color;
                } else {
                    vec4 texelColor = texture(uFontTexture, vo_UV) * vo_Color;
                    if (texelColor.a <= 0.01) discard;
                    color = texelColor;
                }
            }
        }";

    #endregion

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();

    _vertices = [];
    _colors = [];
    _indices = [];
    _uv = [];
    Context.MapBuffer(_vertices);
    Context.MapBuffer(_uv);
    Context.MapBuffer(_colors);
    Context.MapBuffer(_indices, true);

    // Create vao
    OpenGL32.glGenVertexArrays(1, ref _vaoId);
  }

  private void RenderLines(EasyUI_RenderData rd)
  {
    var layer = (Layer_EasyUI)Layer;

    _vertices.Clear();
    _colors.Clear();

    foreach (var line in rd.Lines)
    {
      _vertices.Add(line.From);
      _vertices.Add(line.To);
      _colors.Add(new Vector4(line.FromColor.R, line.FromColor.G, line.FromColor.B,
        line.FromColor.A));
      _colors.Add(new Vector4(line.ToColor.R, line.ToColor.G, line.ToColor.B,
        line.ToColor.A));
    }

    // Загружаем на гпу
    _vertices.Sync();
    _colors.Sync();

    // Биндим vao
    OpenGL32.glBindVertexArray(_vaoId);

    // Активируем атрибуты
    Shader.EnableAttribute(_vertices, "aPosition");
    Shader.EnableAttribute(_colors, "aColor");

    Shader.SetUniform("uMode", new Vector3(1, 0, 0));

    // Рисуем
    OpenGL32.glDrawArrays(OpenGL32.GL_LINES, 0, rd.Lines.Count * 2);

    // Разбиндим vao
    OpenGL32.glBindVertexArray(0);
  }

  private void RenderMain(EasyUI_RenderData rd)
  {
    var layer = (Layer_EasyUI)Layer;

    _vertices.Clear();
    _uv.Clear();
    _colors.Clear();
    _indices.Clear();

    _vertices.AddRange(rd.Vertices.ToArray());
    _uv.AddRange(rd.UV.ToArray());
    _colors.AddRange(rd.Colors.ToArray());
    _indices.AddRange(rd.Indices.ToArray());

    // Маппим текстуру шрифтов
    Context.MapTexture(layer.FontTexture);

    // Загружаем на гпу
    _vertices.Sync();
    _uv.Sync();
    _colors.Sync();
    _indices.Sync();

    // Биндим vao
    OpenGL32.glBindVertexArray(_vaoId);

    // Активируем атрибуты
    Shader.EnableAttribute(_vertices, "aPosition");
    Shader.EnableAttribute(_uv, "aUV");
    Shader.EnableAttribute(_colors, "aColor");

    // Texture
    Shader.ActivateTexture(layer.FontTexture, "uFontTexture", 0);

    Shader.SetUniform("uMode", new Vector3(0, 0, 0));

    // Биндим индексы
    OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(_indices));

    // Рисуем
    OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _indices.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

    // Разбиндим vao
    OpenGL32.glBindVertexArray(0);
  }

  public override void Render()
  {
    var layer = (Layer_EasyUI)Layer;
    var renderData = layer.Build(Scene.DeltaTime);

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    //var cp = Scene.Camera.Position;
    // cp.Z *= -1;
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

    Shader.SetUniform("uModelMatrix", _mx);

    // Основной рендер
    renderData.ForEach(rd =>
    {
      if (rd.IsLine)
      {
        RenderLines(rd);
      }
      else
      {
        RenderMain(rd);
      }
    });
  }
}