/*using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class GL_Layer_BitmapText : LR_Base
{
  public GL_Layer_BitmapText(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
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
        out vec2 vo_UV;
        out vec4 vo_Color;

        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);

            vo_Position = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            vo_UV = aUV;
            vo_Color = aColor;
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
        in vec4 vo_Color;

        out vec4 color;

        uniform sampler2D uTexture;

        void main()
        {
            vec4 texelColor = texture(uTexture, vo_UV) * vo_Color;
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
    var layer = (Layer_BitmapText)Layer;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    var cp = Scene.Camera.Position;
    cp.Z *= -1;
    // cp.Y *= -1;
    // Shader.SetUniform("uCameraPosition", cp);

    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);

    // Draw each mesh
    layer.ForEach<RO_BitmapText>(text =>
    {
      if (text.Text == "" || text.VertexList.Count == 0) return;

      // Явно синхронизируем буферы
      text.VertexList.Sync();
      text.UV0List.Sync();
      text.IndexList.Sync();
      text.ColorList.Sync();

      Context.MapObject(text);

      // Bind vao
      OpenGL32.glBindVertexArray(Context.GetVaoId(text));

      // Buffer
      Shader.EnableAttribute(text.VertexList, "aPosition");
      Shader.EnableAttribute(text.UV0List, "aUV");
      Shader.EnableAttribute(text.ColorList, "aColor");

      // Texture
      Shader.ActivateTexture(text.Font.Texture, "uTexture", 0);
      Shader.SetUniform("uModelMatrix", text.Transform.Matrix);

      // Shader.SetUniform("uTint", new Vector4(sprite.Tint.R, sprite.Tint.G, sprite.Tint.B, sprite.Tint.A));

      // Bind indices
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(text.IndexList));

      // Draw
      OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, text.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

      // Unbind vao
      OpenGL32.glBindVertexArray(0);
    });
  }
}*/

