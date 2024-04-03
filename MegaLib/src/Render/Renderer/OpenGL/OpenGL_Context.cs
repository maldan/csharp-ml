using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class OpenGL_Context
  {
    private readonly Dictionary<ulong, uint> _bufferList = new();
    private readonly Dictionary<string, uint> _vaoList = new();
    private readonly Dictionary<string, uint> _shaderList = new();
    private readonly Dictionary<ulong, uint> _textureList = new();
    private readonly Dictionary<string, uint> _cubeTextureList = new();
    private readonly Dictionary<string, TextureOptions> _textureOptionsList = new();

    public uint GetBufferId<T>(ListGPU<T> buffer)
    {
      return _bufferList[buffer.Id];
    }

    public void MapObject(RO_Mesh mesh)
    {
      MapBuffer(mesh.VertexList);
      MapBuffer(mesh.NormalList);
      MapBuffer(mesh.UV0List);
      MapBuffer(mesh.TangentList);
      MapBuffer(mesh.BiTangentList);
      MapBuffer(mesh.IndexList, true);

      MapTexture(mesh.AlbedoTexture);
      MapTexture(mesh.NormalTexture);
      MapTexture(mesh.RoughnessTexture);
      MapTexture(mesh.MetallicTexture);
    }

    public void MapTexture<T>(Texture_2D<T> texture)
    {
      if (texture == null) return;

      // Create gl texture
      uint textureId = 0;
      OpenGL32.glCreateTextures(OpenGL32.GL_TEXTURE_2D, 1, ref textureId);
      if (textureId == 0) throw new Exception("Can't create texture");

      // At least 1 pixel size
      if (texture.Options.Width == 0) texture.Options.Width = 1;
      if (texture.Options.Height == 0) texture.Options.Height = 1;

      // Sync with GPU
      texture.RAW.OnSync = pixels =>
      {
        // Get type
        var (internalFormat, srcFormat, srcType) = GetTextureType(texture.Options);

        // Bind
        OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

        // Fill texture
        var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
        OpenGL32.glTexImage2D(
          OpenGL32.GL_TEXTURE_2D,
          0,
          internalFormat,
          texture.Options.Width,
          texture.Options.Height,
          0,
          srcFormat,
          srcType,
          pixelPtr
        );

        // Unbind
        OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
      };

      // On destroy
      texture.RAW.OnDestroy += (sender, id) =>
      {
        OpenGL32.glDeleteTextures(1, new[] { textureId });
        _textureList.Remove(id);
      };

      // Do shit
      SetTextureFiltrationMode(textureId, texture.Options.FiltrationMode, texture.Options.UseMipMaps);
      SetTextureWrapMode(textureId, texture.Options.WrapMode);

      // Save to buffer
      _textureList[texture.RAW.Id] = textureId;
    }

    public void MapBuffer<T>(ListGPU<T> buffer, bool isIndex = false)
    {
      if (buffer == null) return;

      // Already mapped
      if (_bufferList.ContainsKey(buffer.Id)) return;

      // Create opengl buffer
      uint bufferId = 0;
      OpenGL32.glGenBuffers(1, ref bufferId);
      if (bufferId == 0) throw new Exception("Can't create buffer");

      // Sync with GPU
      buffer.OnSync = data =>
      {
        // Bind
        OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);

        // Upload
        var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
        var size = Marshal.SizeOf(data[0]) * data.Length;
        var target = isIndex ? OpenGL32.GL_ELEMENT_ARRAY_BUFFER : OpenGL32.GL_ARRAY_BUFFER;
        OpenGL32.glBufferData(target, (IntPtr)size, dataPtr, OpenGL32.GL_STATIC_DRAW);

        // Unbind
        OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
      };

      // On destroy
      buffer.OnDestroy += (sender, id) =>
      {
        OpenGL32.glDeleteBuffers(1, new[] { bufferId });
        _bufferList.Remove(id);
      };

      // Sync buffer
      buffer.Sync();

      // Store buffer
      _bufferList[buffer.Id] = bufferId;
    }

    public (int internalFormat, uint srcFormat, uint srcType) GetTextureType(TextureOptions options)
    {
      int internalFormat;
      uint srcFormat;
      uint srcType;

      switch (options.Format)
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
        default:
          throw new Exception("Unsupported texture format");
      }

      return (internalFormat, srcFormat, srcType);
    }

    public void SetTextureFiltrationMode(uint textureId, TextureFiltrationMode mode, bool useMipMaps)
    {
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      switch (mode)
      {
        case TextureFiltrationMode.Nearest:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_NEAREST);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_NEAREST);
          break;
        case TextureFiltrationMode.Linear when useMipMaps:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER,
            (int)OpenGL32.GL_LINEAR_MIPMAP_LINEAR);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_LINEAR);
          OpenGL32.glGenerateMipmap(OpenGL32.GL_TEXTURE_2D);
          break;
        case TextureFiltrationMode.Linear:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_LINEAR);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_LINEAR);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
      }

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    public void SetTextureWrapMode(uint textureId, TextureWrapMode mode)
    {
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      switch (mode)
      {
        case TextureWrapMode.Clamp:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_CLAMP_TO_EDGE);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_CLAMP_TO_EDGE);
          break;
        case TextureWrapMode.Repeat:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_REPEAT);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_REPEAT);
          break;
        case TextureWrapMode.Mirror:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S,
            (int)OpenGL32.GL_MIRRORED_REPEAT);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T,
            (int)OpenGL32.GL_MIRRORED_REPEAT);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
      }

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }
  }
}