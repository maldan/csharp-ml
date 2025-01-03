using System;
using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Shader;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_PostProcess : LR_Base
{
  private RO_Mesh _mesh;
  public OpenGL_Framebuffer Framebuffer;
  private float[] _ssaoKernel;

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

    // Generate 64 random sample points in a hemisphere
    var ssaoKernel = new List<float>();
    var random = new Random();

    for (var i = 0; i < 64; i++)
    {
      var sample = new Vector3(
        (float)random.NextDouble() * 2.0f - 1.0f,
        (float)random.NextDouble() * 2.0f - 1.0f,
        (float)random.NextDouble()
      );
      sample = Vector3.Normalize(sample) * (float)random.NextDouble();
      var scale = (float)i / 64.0f;
      sample *= MathEx.Lerp(0.1f, 1.0f, scale * scale); // Distribute samples
      ssaoKernel.Add(sample.X);
      ssaoKernel.Add(sample.Y);
      ssaoKernel.Add(sample.Z);
    }

    _ssaoKernel = ssaoKernel.ToArray();
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
        
        uniform vec3 uSSAOKernel[64];
        uniform mat4 uProjectionMatrix;
        uniform sampler2D uScreenTexture;
        uniform sampler2D uNormalTexture;
        uniform sampler2D uDepthTexture;
        
        vec3 ReconstructViewPosition(vec2 uv, float depth) {
            vec4 clipSpace = vec4(uv * 2.0 - 1.0, depth, 1.0);
            vec4 viewSpace = inverse(uProjectionMatrix) * clipSpace;
            return viewSpace.xyz / viewSpace.w;
        }

        float remap(float value, float sourceMin, float sourceMax, float targetMin, float targetMax) {
            return targetMin + ((value - sourceMin) / (sourceMax - sourceMin)) * (targetMax - targetMin);
        }

float LinearizeDepth(float depth, float near, float far) {
    return (2.0 * near * far) / (far + near - depth * (far - near));
}

        void main()
        {
            vec3 normal = texture(uNormalTexture, vo_UV).rgb;
            float depth = texture(uDepthTexture, vo_UV).r;
            
            //depth = 1.0f - remap(depth, 1.0, 0.9997, 1.0, 0.0);

            
            color = vec4(texture(uScreenTexture, vo_UV).rgb, 1.0) * 0.5f;
            
            //color = vec4(texture(uScreenTexture, vo_UV).rgb, 1.0) * 0.001 + vec4(vo_UV.xy, 0.0, 0.0) * 0.1 + normal * 0.03;
            //color.rgb += depth;
            
             float occlusion = 0.0;
              for (int i = 0; i < 64; ++i) {
                  vec3 sample = vec3(vo_UV, depth) + uSSAOKernel[i] * 0.1; // Small radius
                  occlusion += dot(normal, normalize(uSSAOKernel[i]));
              }

              float ao = clamp(1.0 - (occlusion / 64.0), 0.0, 1.0);
              color.rgb += ao * 0.001f; // Output AO directly
              color.rgb += LinearizeDepth(depth, 32f, 0.1f);
        }";

    /*Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();*/

    var ss = ShaderProgram.Compile("PostProcessing");

    Shader.ShaderCode["vertex"] = ss["vertex"];
    Shader.ShaderCode["fragment"] = ss["fragment"];
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
    Shader.Disable(OpenGL32.GL_CULL_FACE);

    Shader.SetUniform("uSSAOKernel", 3, _ssaoKernel);
    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);

    Shader.ActivateTexture(Framebuffer.Texture, "uScreenTexture", 0);
    Shader.ActivateTexture(Framebuffer.NormalTexture, "uNormalTexture", 1);
    Shader.ActivateTexture(Framebuffer.DepthTexture, "uDepthTexture", 2);

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