using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;
using GLint = int;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_PostProcess : LR_Base
{
  private RO_Mesh _mesh;
  public OpenGL_Framebuffer Framebuffer;

  public LR_PostProcess(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
    _mesh = new RO_Mesh
    {
      VertexList =
      [
        new Vector3(-1f, -1f, 0),
        new Vector3(-1f, 1f, 0),
        new Vector3(1f, 1f, 0),
        new Vector3(1f, -1f, 0)
      ],
      UV0List =
      [
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0)
      ],
      IndexList = [0, 1, 2, 0, 2, 3]
    };

    context.MapObject(_mesh);

    Framebuffer = context.CreateFrameBuffer();
  }

  public override void Init()
  {
    // language=glsl
    var vertex = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aUV;
        
        out vec2 vo_UV;
        
        void main() {
            gl_Position = vec4(aPosition.x, aPosition.y, 0.0, 1.0); 
            vo_UV = aUV;
        }";

    // language=glsl
    var fragment = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec2 vo_UV;
        
        out vec4 color;
        
        uniform sampler2D uScreenTexture;
        
        void main()
        {
            color = vec4(texture(uScreenTexture, vo_UV).rgb, 1.0) + vec4(vo_UV.xy, 0.0, 0.0) * 0.1;
        }";

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();
  }

  public override void BeforeRender()
  {
    Framebuffer.Bind();
    Framebuffer.Clear();
  }

  public override void AfterRender()
  {
    Framebuffer.Unbind();
  }

  public override void Render()
  {
    var layer = (Layer_PostProcess)Layer;
    //var ppl = Scene.GetLayer<Layer_Capture>();
    //var ppl2 = (LR_Capture)ppl.LayerRenderer;
    // Console.WriteLine(ppl2.Framebuffer.Id);

    Shader.Use();
    Shader.Disable(OpenGL32.GL_BLEND);
    Shader.Disable(OpenGL32.GL_DEPTH_TEST);

    Shader.ActivateTexture(Framebuffer.Texture, "uScreenTexture", 0);

    // Bind vao
    OpenGL32.glBindVertexArray(Context.GetVaoId(_mesh));

    // Buffer
    Shader.EnableAttribute(_mesh.VertexList, "aPosition");
    Shader.EnableAttribute(_mesh.UV0List, "aUV");

    // Bind indices
    OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(_mesh.IndexList));

    // Draw
    OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

    // Unbind vao
    OpenGL32.glBindVertexArray(0);
  }
}