using System;
using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Shader;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_PostProcess : LR_Base
{
  private RO_Mesh _mesh;
  private OpenGL_Framebuffer _framebufferSSAO;

  private OpenGL_Framebuffer _framebufferFinal;

  private float[] _ssaoKernel;
  private Texture_2D<float> _randomNoise;

  private OpenGL_Shader _shaderSSAO;
  private OpenGL_Shader _shaderFinal;

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

    _framebufferSSAO = context.CreateFrameBuffer();
    _framebufferSSAO.Init();
    _framebufferSSAO.CaptureTexture("color", TextureFormat.RGB8, 0);
    _framebufferSSAO.CaptureTexture("viewNormal", TextureFormat.RGB8, 1);
    _framebufferSSAO.CaptureTexture("viewPosition", TextureFormat.RGB16F, 2);
    _framebufferSSAO.CaptureDepth("depth");
    _framebufferSSAO.FinishAttachment();

    _framebufferFinal = context.CreateFrameBuffer();
    _framebufferFinal.Init();
    _framebufferFinal.CaptureTexture("occlusion", TextureFormat.RGB8, 0);
    _framebufferFinal.FinishAttachment();

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

    _randomNoise = new Texture_2D<float>(4, 4);
    _randomNoise.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    _randomNoise.Options.WrapMode = TextureWrapMode.Repeat;
    for (var i = 0; i < 4 * 4; i++) _randomNoise.RAW[i] = (float)random.NextDouble();
    context.MapTexture(_randomNoise);

    _ssaoKernel = ssaoKernel.ToArray();
  }

  public override void Init()
  {
    var ss = ShaderProgram.Compile("PostProcessingSSAO");
    _shaderSSAO = new OpenGL_Shader
    {
      Context = Context,
      ShaderCode =
      {
        ["vertex"] = ss["vertex"],
        ["fragment"] = ss["fragment"]
      }
    };
    _shaderSSAO.Compile();

    ss = ShaderProgram.Compile("PostProcessing");
    _shaderFinal = new OpenGL_Shader
    {
      Context = Context,
      ShaderCode =
      {
        ["vertex"] = ss["vertex"],
        ["fragment"] = ss["fragment"]
      }
    };
    _shaderFinal.Compile();
  }

  public override void BeforeRender()
  {
    _framebufferSSAO.Bind();
    _framebufferSSAO.Clear();
  }

  public override void AfterRender()
  {
    _framebufferSSAO.Unbind();
  }

  public void ResizeFramebuffer(ushort width, ushort height)
  {
    _framebufferSSAO.Resize(width, height);
    _framebufferFinal.Resize(width, height);
  }

  private void SSAO_Pass()
  {
    var layer = (Layer_PostProcess)Layer;
    var shader = _shaderSSAO;

    shader.Use();
    shader.Disable(OpenGL32.GL_BLEND);
    shader.Disable(OpenGL32.GL_DEPTH_TEST);
    shader.Disable(OpenGL32.GL_CULL_FACE);

    shader.IsStrictMode = false;
    shader.SetUniform("uSSAOKernel", 3, _ssaoKernel);
    shader.IsStrictMode = true;

    shader.PassDefaultUniform(Scene.Camera);

    shader.ActivateTexture(_framebufferSSAO.GetTexture<RGB8>("color"), "uScreenTexture", 0);
    shader.ActivateTexture(_framebufferSSAO.GetTexture<RGB8>("viewNormal"), "uViewNormalTexture", 1);
    shader.ActivateTexture(_framebufferSSAO.GetTexture<RGB16F>("viewPosition"), "uViewPositionTexture", 2);
    shader.ActivateTexture(_framebufferSSAO.GetTexture<float>("depth"), "uDepthTexture", 3);
    shader.ActivateTexture(_randomNoise, "uRandomNoiseTexture", 4);

    // Bind vao
    OpenGL32.glBindVertexArray(Context.GetVaoId(_mesh));

    // Buffer
    shader.EnableAttribute(_mesh.VertexList, "aPosition");
    shader.EnableAttribute(_mesh.UV0List, "aUV");

    // Bind indices
    OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(_mesh.IndexList));

    // Draw
    OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

    // Unbind vao
    OpenGL32.glBindVertexArray(0);
  }

  private void Final_Pass()
  {
    var layer = (Layer_PostProcess)Layer;
    var shader = _shaderFinal;

    shader.Use();
    shader.Disable(OpenGL32.GL_BLEND);
    shader.Disable(OpenGL32.GL_DEPTH_TEST);
    shader.Disable(OpenGL32.GL_CULL_FACE);

    shader.PassDefaultUniform(Scene.Camera);

    shader.ActivateTexture(_framebufferSSAO.GetTexture<RGB8>("color"), "uScreenTexture", 0);
    shader.ActivateTexture(_framebufferFinal.GetTexture<RGB8>("occlusion"), "uOcclusionTexture", 1);

    // Bind vao
    OpenGL32.glBindVertexArray(Context.GetVaoId(_mesh));

    // Buffer
    shader.EnableAttribute(_mesh.VertexList, "aPosition");
    shader.EnableAttribute(_mesh.UV0List, "aUV");

    // Bind indices
    OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(_mesh.IndexList));

    // Draw
    OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

    // Unbind vao
    OpenGL32.glBindVertexArray(0);
  }

  public override void Render()
  {
    _framebufferFinal.Bind();
    SSAO_Pass();
    _framebufferFinal.Unbind();
    Final_Pass();
  }
}