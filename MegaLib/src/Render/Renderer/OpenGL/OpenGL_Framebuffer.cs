using System;
using System.Collections.Generic;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Renderer.OpenGL;

public class OpenGL_Framebuffer
{
  public uint Id => _id;
  private uint _id;
  private OpenGL_Context _context;
  private uint _rbo;
  private uint _previousId;

  private Texture_2D<RGB<byte>> _texture;

  // private readonly Dictionary<ulong, uint> _textureList = new();
  public Texture_2D<RGB<byte>> Texture => _texture;

  public OpenGL_Framebuffer(OpenGL_Context context)
  {
    _context = context;
    Init();
  }

  private void Init()
  {
    // Генерируем и биндим фреймбуффер
    OpenGL32.glGenFramebuffers(1, ref _id);
    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, _id);

    // Создает текстуру и маппим ее
    _texture = new Texture_2D<RGB<byte>>(1280, 720);
    _context.MapRenderTexture(_texture);

    // Прикрепляем текстуру к фреймбуферу
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      OpenGL32.GL_COLOR_ATTACHMENT0,
      OpenGL32.GL_TEXTURE_2D,
      _context.GetTextureId(_texture),
      0);

    // Создаем рендербуфер для глубины и трафарета
    OpenGL32.glGenRenderbuffers(1, ref _rbo);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, _rbo);
    OpenGL32.glRenderbufferStorage(OpenGL32.GL_RENDERBUFFER, OpenGL32.GL_DEPTH24_STENCIL8, 1280, 720);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, 0);

    // Прикрепляем рендербуфер к фреймбуферу
    OpenGL32.glFramebufferRenderbuffer(OpenGL32.GL_FRAMEBUFFER, OpenGL32.GL_DEPTH_STENCIL_ATTACHMENT,
      OpenGL32.GL_RENDERBUFFER, _rbo);

    // Проверяем фреймбуфер на корректность
    var status = OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER);
    if (OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER) != OpenGL32.GL_FRAMEBUFFER_COMPLETE)
      throw new Exception($"ERROR::FRAMEBUFFER:: Framebuffer is not complete! {status}");

    // Открепляем
    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);
  }

  public void Resize(ushort width, ushort height)
  {
    // Bind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, _context.GetTextureId(_texture));

    // Fill texture
    OpenGL32.glTexImage2D(
      OpenGL32.GL_TEXTURE_2D,
      0,
      (int)OpenGL32.GL_RGB,
      width,
      height,
      0,
      OpenGL32.GL_RGB, OpenGL32.GL_UNSIGNED_BYTE,
      0
    );

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // Расайз буфера
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, _rbo);
    OpenGL32.glRenderbufferStorage(OpenGL32.GL_RENDERBUFFER, OpenGL32.GL_DEPTH24_STENCIL8, width, height);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, 0);
  }

  /*private void MapTexture(Texture_2D<RGB<byte>> texture)
  {
    if (texture?.RAW == null) return;
    if (_textureList.ContainsKey(texture.RAW.Id)) return;

    // Create gl texture
    uint textureId = 0;
    OpenGL32.glGenTextures(1, ref textureId);
    if (textureId == 0) throw new Exception("Can't create texture");

    // Bind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

    // Fill texture
    OpenGL32.glTexImage2D(
      OpenGL32.GL_TEXTURE_2D,
      0,
      (int)OpenGL32.GL_RGB,
      texture.RAW.Width,
      texture.RAW.Height,
      0,
      OpenGL32.GL_RGB, OpenGL32.GL_UNSIGNED_BYTE,
      0
    );

    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_CLAMP_TO_EDGE);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_CLAMP_TO_EDGE);

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // Save to buffer
    _textureList[texture.RAW.Id] = textureId;
  }*/

  public void Bind()
  {
    var currentFBO = 0;
    OpenGL32.glGetIntegerv(OpenGL32.GL_FRAMEBUFFER_BINDING, ref currentFBO);
    _previousId = (uint)currentFBO;

    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, _id);
    // OpenGL32.glViewport(0, 0, 1280, 720);
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