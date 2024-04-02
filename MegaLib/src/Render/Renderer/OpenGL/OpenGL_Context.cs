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
    private readonly Dictionary<string, uint> _textureList = new();
    private readonly Dictionary<string, uint> _cubeTextureList = new();
    private readonly Dictionary<string, TextureOptions> _textureOptionsList = new();

    public void MapBuffer<T>(ListGPU<T> buffer)
    {
      // Already mapped
      if (_bufferList.ContainsKey(buffer.Id)) return;

      // Create opengl buffer
      uint bufferId = 0;
      OpenGL32.glGenBuffers(1, ref bufferId);
      if (bufferId == 0) throw new Exception("Can't create buffer");

      // Sync with GPU
      buffer.OnSync = data =>
      {
        var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
        var size = Marshal.SizeOf(data[0]) * data.Length;
        OpenGL32.glBufferData(bufferId, (IntPtr)size, dataPtr, OpenGL32.GL_STATIC_DRAW);
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
  }
}