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
  private OpenGL_Framebuffer _framebufferFirst;

  private OpenGL_Framebuffer _framebufferFinal;

  private float[] _ssaoKernel;
  private Texture_2D<RGB32F> _randomNoise;

  private OpenGL_Shader _shaderSSAO;
  private OpenGL_Shader _shaderFinal;

  private Random _random = new();

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

    _framebufferFirst = context.CreateFrameBuffer();
    _framebufferFirst.Init();
    _framebufferFirst.CaptureTexture("color", TextureFormat.RGB8, 0);
    _framebufferFirst.CaptureTexture("viewNormal", TextureFormat.RGB8, 1);
    _framebufferFirst.CaptureTexture("viewPosition", TextureFormat.RGB16F, 2);
    // Roughness metallic emission
    _framebufferFirst.CaptureTexture("rme", TextureFormat.RGB8, 3);
    _framebufferFirst.CaptureDepth("depth");
    _framebufferFirst.FinishAttachment();

    _framebufferFinal = context.CreateFrameBuffer();
    _framebufferFinal.Init();
    _framebufferFinal.CaptureTexture("occlusion", TextureFormat.RGB8, 0);
    _framebufferFinal.CaptureTexture("indirectLight", TextureFormat.RGB8, 1);
    _framebufferFinal.FinishAttachment();

    // Generate 64 random sample points in a hemisphere
    var ssaoKernel = new List<float>();


    for (var i = 0; i < 64; i++)
    {
      var sample = new Vector3(
        (float)_random.NextDouble() * 2.0f - 1.0f,
        (float)_random.NextDouble() * 2.0f - 1.0f,
        (float)_random.NextDouble()
      );
      sample = Vector3.Normalize(sample) * (float)_random.NextDouble();
      var scale = (float)i / 64.0f;
      sample *= MathEx.Lerp(0.1f, 1.0f, scale * scale); // Distribute samples
      ssaoKernel.Add(sample.X);
      ssaoKernel.Add(sample.Y);
      ssaoKernel.Add(sample.Z);
    }

    _randomNoise = new Texture_2D<RGB32F>(8, 8);
    _randomNoise.Options.FiltrationMode = TextureFiltrationMode.Linear;
    _randomNoise.Options.WrapMode = TextureWrapMode.Repeat;
    for (var i = 0; i < 8 * 8; i++)
      _randomNoise.RAW[i] = new RGB32F(
        (float)_random.NextDouble() * 2.0f - 1.0f,
        (float)_random.NextDouble() * 2.0f - 1.0f,
        (float)_random.NextDouble()
      );
    context.MapTexture(_randomNoise);

    _ssaoKernel = ssaoKernel.ToArray();
  }

  public override void Init()
  {
    var ss = ShaderProgram.Compile("PostProcessingFirst");
    _shaderSSAO = new OpenGL_Shader
    {
      Name = "PostProcessingFirst",
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
      Name = "PostProcessing",
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
    _framebufferFirst.Bind();
    _framebufferFirst.Clear();
  }

  public override void AfterRender()
  {
    _framebufferFirst.Unbind();
  }

  public void ResizeFramebuffer(ushort width, ushort height)
  {
    _framebufferFirst.Resize(width, height);
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
    /*for (var i = 0; i < _ssaoKernel.Length; i += 3)
    {
      _ssaoKernel[i] = (float)(_random.NextDouble() * 2.0f - 1.0f);
      _ssaoKernel[i + 1] = (float)(_random.NextDouble() * 2.0f - 1.0f);
      _ssaoKernel[i + 2] = (float)_random.NextDouble();
    }*/
    shader.SetUniform("uSSAOKernel", 3, _ssaoKernel);
    shader.SetUniform("_uSSAOSettings", layer.SSAO_Settings);
    shader.IsStrictMode = true;

    shader.PassDefaultUniform(Scene.Camera);

    shader.ActivateTexture(_framebufferFirst.GetTexture<RGB8>("color"), "_uScreenTexture", 0);
    shader.ActivateTexture(_framebufferFirst.GetTexture<RGB8>("viewNormal"), "uViewNormalTexture", 1);
    shader.ActivateTexture(_framebufferFirst.GetTexture<RGB16F>("viewPosition"), "uViewPositionTexture", 2);
    shader.ActivateTexture(_framebufferFirst.GetTexture<float>("depth"), "uDepthTexture", 3);
    shader.ActivateTexture(_randomNoise, "uRandomNoiseTexture", 4);
    shader.ActivateTexture(_framebufferFirst.GetTexture<RGB8>("rme"), "_uRMETexture", 5);

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

    shader.ActivateTexture(_framebufferFirst.GetTexture<RGB8>("color"), "_uScreenTexture", 0);
    shader.ActivateTexture(_framebufferFinal.GetTexture<RGB8>("occlusion"), "uOcclusionTexture", 1);
    shader.ActivateTexture(_framebufferFinal.GetTexture<RGB8>("indirectLight"), "_uILTexture", 2);
    shader.ActivateTexture(_framebufferFirst.GetTexture<RGB8>("rme"), "_uRMETexture", 3);

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