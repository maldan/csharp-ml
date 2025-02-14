using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;
using GLint = int;

namespace MegaLib.Render.Renderer.OpenGL;

public class OpenGL_Context
{
  private readonly Dictionary<ulong, uint> _bufferList = new();

  private readonly Dictionary<ulong, uint> _vaoList = new();

  // private readonly Dictionary<string, uint> _shaderList = new();
  private readonly Dictionary<ulong, uint> _textureList = new();
  private readonly Dictionary<ulong, uint> _cubeTextureList = new();
  private readonly Dictionary<string, TextureOptions> _textureOptionsList = new();

  private readonly List<uint> _removeBufferQueue = new();
  private readonly List<uint> _removeTextureQueue = new();
  private Mutex _mutex = new();

  private IRenderer _renderer;

  public OpenGL_Context(IRenderer renderer)
  {
    _renderer = renderer;
  }

  public uint GetBufferId<T>(ListGPU<T> buffer)
  {
    return _bufferList[buffer.Id];
  }

  public uint GetTextureId<T>(Texture_2D<T> texture)
  {
    return _textureList[texture.RAW.Id];
  }

  public uint GetTextureId(Texture_Cube texture)
  {
    return _cubeTextureList[texture.Id];
  }

  public uint GetVaoId(RO_Base obj)
  {
    return _vaoList[obj.Id];
  }

  public void MapObject(RO_Mesh mesh)
  {
    // Already mapped
    if (_vaoList.ContainsKey(mesh.Id)) return;

    // Create vao
    uint vaoId = 0;
    OpenGL32.glGenVertexArrays(1, ref vaoId);
    _vaoList[mesh.Id] = vaoId;

    // Map all buffers
    MapBuffer(mesh.VertexList);
    MapBuffer(mesh.NormalList);
    MapBuffer(mesh.UV0List);
    MapBuffer(mesh.TangentList);
    MapBuffer(mesh.BiTangentList);
    MapBuffer(mesh.BoneWeightList);
    MapBuffer(mesh.BoneIndexList);
    MapBuffer(mesh.IndexList, true);

    // Map all textures
    if (mesh.Material != null)
    {
      MapTexture(mesh.Material.AlbedoTexture);
      MapTexture(mesh.Material.NormalTexture);
      MapTexture(mesh.Material.RoughnessTexture);
      MapTexture(mesh.Material.MetallicTexture);
    }
  }

  public void MapObject(RO_VoxelMesh mesh)
  {
    // Already mapped
    if (_vaoList.ContainsKey(mesh.Id)) return;

    // Create vao
    uint vaoId = 0;
    OpenGL32.glGenVertexArrays(1, ref vaoId);
    _vaoList[mesh.Id] = vaoId;

    // Map all buffers
    MapBuffer(mesh.VertexList);
    MapBuffer(mesh.ColorList);
    MapBuffer(mesh.VoxelInfoList);
    MapBuffer(mesh.ShadowInfoList);
  }

  public void MapObject(RO_Sprite sprite)
  {
    // Already mapped
    if (_vaoList.ContainsKey(sprite.Id)) return;

    // Create vao
    uint vaoId = 0;
    OpenGL32.glGenVertexArrays(1, ref vaoId);
    _vaoList[sprite.Id] = vaoId;

    // Map all buffers
    MapBuffer(sprite.VertexList);
    MapBuffer(sprite.UV0List);
    MapBuffer(sprite.IndexList, true);

    // Map all textures
    MapTexture(sprite.Texture);
  }

  public void MapObject(RO_BitmapText text)
  {
    // Already mapped
    if (_vaoList.ContainsKey(text.Id)) return;

    // Create vao
    uint vaoId = 0;
    OpenGL32.glGenVertexArrays(1, ref vaoId);
    _vaoList[text.Id] = vaoId;

    // Map all buffers
    MapBuffer(text.VertexList);
    MapBuffer(text.UV0List);
    MapBuffer(text.ColorList);
    MapBuffer(text.IndexList, true);

    // Map all textures
    MapTexture(text.Font.Texture);
  }

  public void MapTexture(Texture_Cube texture)
  {
    if (texture == null) return;
    if (_cubeTextureList.ContainsKey(texture.Id)) return;

    // Create gl texture
    uint textureId = 0;
    OpenGL32.glCreateTextures(OpenGL32.GL_TEXTURE_CUBE_MAP, 1, ref textureId);
    if (textureId == 0) throw new Exception("Can't create texture");

    // Sync with GPU
    var l = new[] { texture.RIGHT, texture.LEFT, texture.TOP, texture.BOTTOM, texture.BACK, texture.FRONT };
    var sideId = 0;
    foreach (var side in l)
    {
      var id = sideId++;
      side.OnSync = pixels =>
      {
        // Get type
        var (internalFormat, srcFormat, srcType) = GetTextureType(texture.Options.Format);

        // Bind
        OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_CUBE_MAP, textureId);

        var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
        OpenGL32.glPixelStorei(OpenGL32.GL_UNPACK_ALIGNMENT, 1);
        OpenGL32.glTexImage2D(
          OpenGL32.GL_TEXTURE_CUBE_MAP_POSITIVE_X + (uint)id,
          0,
          internalFormat,
          side.Width,
          side.Height,
          0,
          srcFormat,
          srcType,
          pixelPtr
        );

        // Unbind
        OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_CUBE_MAP, 0);
      };

      // Sync texture
      side.Sync();
    }

    // On destroy
    texture.OnDestroy += (sender, id) =>
    {
      //OpenGL32.glDeleteTextures([textureId]);
      //_cubeTextureList.Remove(id);
      _mutex.WaitOne();
      _removeTextureQueue.Add(_cubeTextureList[id]);
      _mutex.ReleaseMutex();
    };

    // Do shit
    SetTextureFiltrationMode(OpenGL32.GL_TEXTURE_CUBE_MAP, textureId, texture.Options.FiltrationMode,
      texture.Options.UseMipMaps);
    SetTextureWrapMode(OpenGL32.GL_TEXTURE_CUBE_MAP, textureId, texture.Options.WrapMode);

    // Save to buffer
    _cubeTextureList[texture.Id] = textureId;
  }

  /*public void MapRenderTexture(Texture_2D<RGBA8> texture)
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
      (GLint)OpenGL32.GL_RGBA,
      texture.RAW.Width,
      texture.RAW.Height,
      0,
      OpenGL32.GL_RGBA, OpenGL32.GL_UNSIGNED_BYTE,
      0
    );

    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // On destroy
    texture.RAW.OnDestroy += (sender, id) =>
    {
      _mutex.WaitOne();
      _removeTextureQueue.Add(_textureList[id]);
      _mutex.ReleaseMutex();
    };

    // Save to buffer
    _textureList[texture.RAW.Id] = textureId;
  }*/

  /*public void MapRenderTexture(Texture_2D<RGB8> texture)
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
      (GLint)OpenGL32.GL_RGB,
      texture.RAW.Width,
      texture.RAW.Height,
      0,
      OpenGL32.GL_RGB, OpenGL32.GL_UNSIGNED_BYTE,
      0
    );

    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // On destroy
    texture.RAW.OnDestroy += (sender, id) =>
    {
      _mutex.WaitOne();
      _removeTextureQueue.Add(_textureList[id]);
      _mutex.ReleaseMutex();
    };

    // Save to buffer
    _textureList[texture.RAW.Id] = textureId;
  }*/

  public void MapRenderTexture<T>(Texture_2D<T> texture)
  {
    if (texture?.RAW == null) return;
    if (_textureList.ContainsKey(texture.RAW.Id)) return;

    // Create gl texture
    uint textureId = 0;
    OpenGL32.glGenTextures(1, ref textureId);
    if (textureId == 0) throw new Exception("Can't create texture");

    // Get type
    var (internalFormat, srcFormat, srcType) = GetTextureType(texture.Options.Format);

    // Bind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

    // Fill texture
    OpenGL32.glTexImage2D(
      OpenGL32.GL_TEXTURE_2D,
      0,
      internalFormat,
      texture.RAW.Width,
      texture.RAW.Height,
      0,
      srcFormat,
      srcType,
      0
    );

    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // On destroy
    texture.RAW.OnDestroy += (sender, id) =>
    {
      _mutex.WaitOne();
      _removeTextureQueue.Add(_textureList[id]);
      _mutex.ReleaseMutex();
    };

    // Save to buffer
    _textureList[texture.RAW.Id] = textureId;
  }

  public void MapDepthTexture(Texture_2D<float> texture)
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
      (GLint)OpenGL32.GL_DEPTH_COMPONENT32F,
      texture.RAW.Width,
      texture.RAW.Height,
      0,
      OpenGL32.GL_DEPTH_COMPONENT, OpenGL32.GL_FLOAT,
      0
    );

    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (GLint)OpenGL32.GL_CLAMP_TO_EDGE);

    // Unbind
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    // On destroy
    texture.RAW.OnDestroy += (sender, id) =>
    {
      _mutex.WaitOne();
      _removeTextureQueue.Add(_textureList[id]);
      _mutex.ReleaseMutex();
    };

    // Save to buffer
    _textureList[texture.RAW.Id] = textureId;
  }

  public void MapTexture<T>(Texture_2D<T> texture)
  {
    if (texture?.RAW == null) return;
    if (_textureList.ContainsKey(texture.RAW.Id)) return;

    // Create gl texture
    uint textureId = 0;
    OpenGL32.glCreateTextures(OpenGL32.GL_TEXTURE_2D, 1, ref textureId);
    if (textureId == 0) throw new Exception("Can't create texture");

    // Sync with GPU
    texture.RAW.OnSync = pixels =>
    {
      // Get type
      var (internalFormat, srcFormat, srcType) = GetTextureType(texture.Options.Format);

      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Fill texture
      var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      OpenGL32.glPixelStorei(OpenGL32.GL_UNPACK_ALIGNMENT, 1);
      OpenGL32.glTexImage2D(
        OpenGL32.GL_TEXTURE_2D,
        0,
        internalFormat,
        texture.RAW.Width,
        texture.RAW.Height,
        0,
        srcFormat,
        srcType,
        pixelPtr
      );

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

      // Console.WriteLine(texture.GetType().Name);
    };

    // On destroy
    texture.RAW.OnDestroy += (sender, id) =>
    {
      //Console.WriteLine("Try to destroy texture");
      //OpenGL32.glDeleteTextures([textureId]);
      //_textureList.Remove(id);
      _mutex.WaitOne();
      _removeTextureQueue.Add(_textureList[id]);
      // _textureList.Remove(id);
      _mutex.ReleaseMutex();
    };

    // Sync texture
    texture.RAW.Sync();

    // Do shit
    SetTextureFiltrationMode(OpenGL32.GL_TEXTURE_2D, textureId, texture.Options.FiltrationMode,
      texture.Options.UseMipMaps);

    if (texture.Options.WrapMode != TextureWrapMode.None)
      SetTextureWrapMode(OpenGL32.GL_TEXTURE_2D, textureId, texture.Options.WrapMode);

    // Save to buffer
    _textureList[texture.RAW.Id] = textureId;
  }

  public void MapBuffer<T>(ListGPU<T> buffer, bool isIndex = false)
  {
    if (buffer == null) return;
    if (_bufferList.ContainsKey(buffer.Id)) return;

    // Create opengl buffer
    uint bufferId = 0;
    OpenGL32.glGenBuffers(1, ref bufferId);
    if (bufferId == 0) throw new Exception("Can't create buffer");

    // Sync with GPU
    buffer.OnSync = data =>
    {
      // if (data.Length == 0) return;
      var target = isIndex ? OpenGL32.GL_ELEMENT_ARRAY_BUFFER : OpenGL32.GL_ARRAY_BUFFER;

      // Bind
      OpenGL32.glBindBuffer(target, bufferId);

      // Upload
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
      var size = Marshal.SizeOf(typeof(T)) * data.Length;

      OpenGL32.glBufferData(target, (IntPtr)size, dataPtr, OpenGL32.GL_STATIC_DRAW);

      // Unbind
      OpenGL32.glBindBuffer(target, 0);
    };

    // On destroy
    buffer.OnDestroy += (sender, id) =>
    {
      //Console.WriteLine("Try to destroy buffer");
      //OpenGL32.glDeleteBuffers(1, new[] { bufferId });
      //_bufferList.Remove(id);

      _mutex.WaitOne();
      _removeBufferQueue.Add(_bufferList[id]);
      // _bufferList.Remove(id);
      _mutex.ReleaseMutex();
    };

    // Sync buffer
    buffer.Sync();

    // Store buffer
    _bufferList[buffer.Id] = bufferId;
  }

  public (int internalFormat, uint srcFormat, uint srcType) GetTextureType(TextureFormat format)
  {
    int internalFormat;
    uint srcFormat;
    uint srcType;

    switch (format)
    {
      case TextureFormat.BGR8:
        internalFormat = (int)OpenGL32.GL_RGBA;
        srcFormat = OpenGL32.GL_BGR;
        srcType = OpenGL32.GL_UNSIGNED_BYTE;
        break;
      case TextureFormat.RGB8:
        internalFormat = (int)OpenGL32.GL_RGBA;
        srcFormat = OpenGL32.GL_RGB;
        srcType = OpenGL32.GL_UNSIGNED_BYTE;
        break;
      case TextureFormat.RGBA8:
        internalFormat = (int)OpenGL32.GL_RGBA;
        srcFormat = OpenGL32.GL_RGBA;
        srcType = OpenGL32.GL_UNSIGNED_BYTE;
        break;
      case TextureFormat.BGRA8:
        internalFormat = (int)OpenGL32.GL_RGBA;
        srcFormat = OpenGL32.GL_BGRA;
        srcType = OpenGL32.GL_UNSIGNED_BYTE;
        break;
      case TextureFormat.R32F:
        internalFormat = (int)OpenGL32.GL_R32F;
        srcFormat = OpenGL32.GL_RED;
        srcType = OpenGL32.GL_FLOAT;
        break;
      case TextureFormat.R8:
        internalFormat = (int)OpenGL32.GL_R8;
        srcFormat = OpenGL32.GL_RED;
        srcType = OpenGL32.GL_UNSIGNED_BYTE;
        break;
      case TextureFormat.RGB16F:
        internalFormat = (int)OpenGL32.GL_RGB16F;
        srcFormat = OpenGL32.GL_RGB;
        srcType = OpenGL32.GL_HALF_FLOAT;
        break;
      default:
        throw new Exception("Unsupported texture format");
    }

    return (internalFormat, srcFormat, srcType);
  }

  public void SetTextureFiltrationMode(uint target, uint textureId, TextureFiltrationMode mode, bool useMipMaps)
  {
    OpenGL32.glBindTexture(target, textureId);

    switch (mode)
    {
      case TextureFiltrationMode.Nearest:
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_NEAREST);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_NEAREST);
        break;
      case TextureFiltrationMode.Linear when useMipMaps:
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_LINEAR_MIPMAP_LINEAR);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_LINEAR);
        OpenGL32.glGenerateMipmap(target);
        break;
      case TextureFiltrationMode.Linear:
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_LINEAR);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_LINEAR);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
    }

    // Unbind
    OpenGL32.glBindTexture(target, 0);
  }

  public void SetTextureWrapMode(uint target, uint textureId, TextureWrapMode mode)
  {
    OpenGL32.glBindTexture(target, textureId);

    switch (mode)
    {
      case TextureWrapMode.Clamp:
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_CLAMP_TO_EDGE);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_CLAMP_TO_EDGE);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_R, (int)OpenGL32.GL_CLAMP_TO_EDGE);
        break;
      case TextureWrapMode.Repeat:
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_REPEAT);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_REPEAT);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_R, (int)OpenGL32.GL_REPEAT);
        break;
      case TextureWrapMode.Mirror:
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_MIRRORED_REPEAT);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_MIRRORED_REPEAT);
        OpenGL32.glTexParameteri(target, OpenGL32.GL_TEXTURE_WRAP_R, (int)OpenGL32.GL_MIRRORED_REPEAT);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
    }

    // Unbind
    OpenGL32.glBindTexture(target, 0);
  }

  public OpenGL_Framebuffer CreateFrameBuffer()
  {
    var fb = new OpenGL_Framebuffer(_renderer, this);
    return fb;
  }

  public void Clean()
  {
    _mutex.WaitOne();

    if (_removeBufferQueue.Count > 0)
    {
      for (var i = 0; i < _removeBufferQueue.Count; i++) OpenGL32.glDeleteBuffer(_removeBufferQueue[i]);
      OpenGL32.PrintGlError("glDeleteBuffers");
      Console.WriteLine($"Removed buffers {_removeBufferQueue.Count}");
      _removeBufferQueue.Clear();
    }

    if (_removeTextureQueue.Count > 0)
    {
      for (var i = 0; i < _removeTextureQueue.Count; i++) OpenGL32.glDeleteTexture(_removeTextureQueue[i]);
      OpenGL32.PrintGlError("glDeleteTextures");
      Console.WriteLine($"Removed textures {_removeTextureQueue.Count}");
      _removeTextureQueue.Clear();
    }

    _mutex.ReleaseMutex();
  }
}