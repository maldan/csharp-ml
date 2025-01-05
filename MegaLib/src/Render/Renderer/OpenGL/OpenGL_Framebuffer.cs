using System;
using System.Collections.Generic;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;
using GLenum = uint;

namespace MegaLib.Render.Renderer.OpenGL;

public class OpenGL_TextureCapture
{
  public string Name;
  public TextureFormat Format;
  public int GlInternalFormat;
  public GLenum GlFormat;
  public GLenum GlType;
  public uint TextureId;
  public object Texture;
}

public class OpenGL_Framebuffer
{
  public uint Id => _id;
  private uint _id;
  private OpenGL_Context _context;
  private uint _rbo;
  private uint _previousId;

  private Dictionary<string, OpenGL_TextureCapture> _textures = new();

  public OpenGL_Framebuffer(OpenGL_Context context)
  {
    _context = context;
  }

  public void CaptureDepth(string name)
  {
    var attachId = OpenGL32.GL_DEPTH_ATTACHMENT;

    _textures[name] = new OpenGL_TextureCapture()
    {
      Name = name,
      Texture = new Texture_2D<float>(1280, 720),
      Format = TextureFormat.R32F,
      GlInternalFormat = (int)OpenGL32.GL_DEPTH_COMPONENT24,
      GlFormat = OpenGL32.GL_DEPTH_COMPONENT,
      GlType = OpenGL32.GL_FLOAT
    };
    _context.MapDepthTexture(_textures[name].Texture as Texture_2D<float>);
    _textures[name].TextureId = _context.GetTextureId(_textures[name].Texture as Texture_2D<float>);

    // Attach
    Bind();

    // Attach texture
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      (GLenum)attachId, OpenGL32.GL_TEXTURE_2D, _textures[name].TextureId, 0);

    // Detach
    Unbind();
  }

  public Texture_2D<T> GetTexture<T>(string name)
  {
    return (Texture_2D<T>)_textures[name].Texture;
  }

  public void CaptureTexture(string name, TextureFormat kind, int order)
  {
    var attachId = OpenGL32.GL_COLOR_ATTACHMENT0 + order;
    // Get type
    var (internalFormat, srcFormat, srcType) = _context.GetTextureType(kind);

    if (kind == TextureFormat.RGB8)
    {
      _textures[name] = new OpenGL_TextureCapture()
      {
        Texture = new Texture_2D<RGB8>(1280, 720)
      };
      _context.MapRenderTexture(_textures[name].Texture as Texture_2D<RGB8>);
      _textures[name].TextureId = _context.GetTextureId(_textures[name].Texture as Texture_2D<RGB8>);
    }
    else if (kind == TextureFormat.RGB16F)
    {
      _textures[name] = new OpenGL_TextureCapture()
      {
        Texture = new Texture_2D<RGB16F>(1280, 720)
      };
      _context.MapRenderTexture(_textures[name].Texture as Texture_2D<RGB16F>);
      _textures[name].TextureId = _context.GetTextureId(_textures[name].Texture as Texture_2D<RGB16F>);
    }
    else
    {
      throw new Exception("Fuck this texture format");
    }

    _textures[name].Name = name;
    _textures[name].Format = kind;
    _textures[name].GlInternalFormat = internalFormat;
    _textures[name].GlFormat = srcFormat;
    _textures[name].GlType = srcType;

    // Attach
    Bind();

    // Attach texture
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      (GLenum)attachId, OpenGL32.GL_TEXTURE_2D, _textures[name].TextureId, 0);

    // Detach
    Unbind();
  }

  public void Init()
  {
    OpenGL32.glGenFramebuffers(1, ref _id);
  }

  /*private void Init()
  {
    // Генерируем и биндим фреймбуффер
    OpenGL32.glGenFramebuffers(1, ref _id);
    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, _id);

    // Создает текстуру и маппим ее
    _texture = new Texture_2D<RGB8>(1280, 720);
    _context.MapRenderTexture(_texture);

    // Прикрепляем текстуру к фреймбуферу
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      OpenGL32.GL_COLOR_ATTACHMENT0,
      OpenGL32.GL_TEXTURE_2D,
      _context.GetTextureId(_texture),
      0);

    // Create and map normal texture
    _normalTexture = new Texture_2D<RGB8>(1280, 720); // Use high precision for normals
    _context.MapRenderTexture(_normalTexture);
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      OpenGL32.GL_COLOR_ATTACHMENT1, // Attachment point 1
      OpenGL32.GL_TEXTURE_2D,
      _context.GetTextureId(_normalTexture),
      0);

    // Создаем рендербуфер для глубины и трафарета
    /*OpenGL32.glGenRenderbuffers(1, ref _rbo);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, _rbo);
    OpenGL32.glRenderbufferStorage(OpenGL32.GL_RENDERBUFFER, OpenGL32.GL_DEPTH24_STENCIL8, 1280, 720);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, 0);

    // Прикрепляем рендербуфер к фреймбуферу
    OpenGL32.glFramebufferRenderbuffer(OpenGL32.GL_FRAMEBUFFER, OpenGL32.GL_DEPTH_STENCIL_ATTACHMENT,
      OpenGL32.GL_RENDERBUFFER, _rbo);#1#

    // Create and attach depth texture
    _depthTexture = new Texture_2D<float>(1280, 720);
    _context.MapDepthTexture(_depthTexture);
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      OpenGL32.GL_DEPTH_ATTACHMENT,
      OpenGL32.GL_TEXTURE_2D,
      _context.GetTextureId(_depthTexture),
      0);

    // Specify the attachments to draw into
    var drawBuffers = new uint[]
    {
      OpenGL32.GL_COLOR_ATTACHMENT0, // Color
      OpenGL32.GL_COLOR_ATTACHMENT1 // Normals
      //OpenGL32.GL_DEPTH_ATTACHMENT  // Normals
    };
    OpenGL32.glDrawBuffers(2, drawBuffers);

    // Проверяем фреймбуфер на корректность
    var status = OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER);
    if (OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER) != OpenGL32.GL_FRAMEBUFFER_COMPLETE)
      throw new Exception($"ERROR::FRAMEBUFFER:: Framebuffer is not complete! {status}");

    // Открепляем
    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);
  }*/

  public void FinishAttachment()
  {
    Bind();

    var buffers = new List<uint>();
    foreach (var (name, texture) in _textures)
    {
      if (texture.GlFormat == OpenGL32.GL_DEPTH_COMPONENT) continue;
      buffers.Add((GLenum)(OpenGL32.GL_COLOR_ATTACHMENT0 + buffers.Count));
    }

    // Specify the attachments to draw into
    var drawBuffers = buffers.ToArray();
    OpenGL32.glDrawBuffers(buffers.Count, drawBuffers);

    // Check
    var status = OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER);
    if (OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER) != OpenGL32.GL_FRAMEBUFFER_COMPLETE)
      throw new Exception($"ERROR::FRAMEBUFFER:: Framebuffer is not complete! {status}");

    Unbind();
  }

  public void Resize(ushort width, ushort height)
  {
    foreach (var (name, data) in _textures)
    {
      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, data.TextureId);

      // Fill texture
      OpenGL32.glTexImage2D(
        OpenGL32.GL_TEXTURE_2D,
        0,
        data.GlInternalFormat,
        width,
        height,
        0,
        data.GlFormat,
        data.GlType,
        0
      );

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }
  }

  /*public void Resize(ushort width, ushort height)
  {
    // Bind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, _context.GetTextureId(_texture));

    // Fill texture
    OpenGL32.glTexImage2D(
      OpenGL32.GL_TEXTURE_2D,
      0,
      (int)OpenGL32.GL_RGB8,
      width,
      height,
      0,
      OpenGL32.GL_RGB, OpenGL32.GL_UNSIGNED_BYTE,
      0
    );

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // Resize normal texture
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, _context.GetTextureId(_normalTexture));
    OpenGL32.glTexImage2D(
      OpenGL32.GL_TEXTURE_2D,
      0,
      (int)OpenGL32.GL_RGB8,
      width,
      height,
      0,
      OpenGL32.GL_RGB, OpenGL32.GL_UNSIGNED_BYTE,
      0
    );

    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // Resize depth texture
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, _context.GetTextureId(_depthTexture));
    OpenGL32.glTexImage2D(
      OpenGL32.GL_TEXTURE_2D,
      0,
      (int)OpenGL32.GL_DEPTH_COMPONENT32F,
      width,
      height,
      0,
      OpenGL32.GL_DEPTH_COMPONENT,
      OpenGL32.GL_FLOAT,
      IntPtr.Zero);
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // Расайз буфера
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, _rbo);
    OpenGL32.glRenderbufferStorage(OpenGL32.GL_RENDERBUFFER, OpenGL32.GL_DEPTH24_STENCIL8, width, height);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, 0);
  }*/

  public void Bind()
  {
    var currentFBO = 0;
    OpenGL32.glGetIntegerv(OpenGL32.GL_FRAMEBUFFER_BINDING, ref currentFBO);
    _previousId = (uint)currentFBO;

    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, _id);
  }

  public void Clear()
  {
    OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT);
    OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
  }

  public void Unbind()
  {
    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, _previousId);
  }

  public void Destroy()
  {
  }
}