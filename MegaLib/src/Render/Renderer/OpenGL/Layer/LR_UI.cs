using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_UI : LR_Base
{
  public LR_UI(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
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
        layout (location = 2) in vec2 aUV;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform mat4 uModelMatrix;
        
        out vec3 vo_Position;
        out vec2 vo_UV;
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            
            vo_Position = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
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
        in vec2 vo_UV;
        
        out vec4 color;
        
        uniform sampler2D uTexture;
        uniform vec4 uTint;
        
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
            vec4 texelColor = texture(uTexture, vo_UV) * uTint;
            if (texelColor.a <= 0.01) discard;
            color = texelColor;
        }";

    #endregion

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();
  }

  public override void Render()
  {
    /*var layer = (Layer_UI)Layer;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    var cp = Scene.Camera.Position;
    cp.Z *= -1;

    Shader.SetUniform("uProjectionMatrix", layer.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", layer.Camera.ViewMatrix);

    // Draw each mesh
    layer.ForEach<RO_Sprite>(sprite =>
    {
      Context.MapObject(sprite);

      // Bind vao
      OpenGL32.glBindVertexArray(Context.GetVaoId(sprite));

      // Buffer
      Shader.EnableAttribute(sprite.VertexList, "aPosition");
      Shader.EnableAttribute(sprite.UV0List, "aUV");

      // Texture
      Shader.ActivateTexture(sprite.Texture, "uTexture", 0);

      Shader.SetUniform("uModelMatrix", sprite.Transform.Matrix);

      Shader.SetUniform("uTint", new Vector4(sprite.Tint.R, sprite.Tint.G, sprite.Tint.B, sprite.Tint.A));

      // Bind indices
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(sprite.IndexList));

      // Draw
      OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, sprite.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

      // Unbind vao
      OpenGL32.glBindVertexArray(0);
    });*/
  }
}