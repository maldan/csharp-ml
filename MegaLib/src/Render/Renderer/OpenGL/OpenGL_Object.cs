using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class OpenGL_Object
  {
    public readonly List<string> BufferList = new();
    public readonly List<string> TextureList = new();
    public readonly Dictionary<string, Func<dynamic>> UniformList = new();

    private RO_Base _object;
    private OpenGL_Shader _shader;
    private Dictionary<string, uint> _glBuffer = new();
    private Dictionary<string, uint> _glTexture = new();
    private Dictionary<string, string> _glTextureMapping = new();
    private Dictionary<string, string> _glBufferMapping = new();
    private Dictionary<string, TextureOptions> _glTextureOptions = new();
    private uint _vaoId;
    private int _indexAmount;

    public OpenGL_Object(RO_Base obj, OpenGL_Shader shader)
    {
      _object = obj;
      _shader = shader;
    }

    public void Init()
    {
      // Create vao
      _vaoId = 0;
      OpenGL32.glGenVertexArrays(1, ref _vaoId);
      if (_vaoId == 0) throw new Exception("Can't create VAO");

      // Create buffers
      foreach (var tuple in BufferList.Select(t => t.Split(" -> ")))
      {
        var name = tuple[0];
        var vaoAssignName = "";
        var vaoAssignType = "";
        if (tuple.Length > 1)
        {
          vaoAssignName = tuple[1].Split(":")[0];
          vaoAssignType = tuple[1].Split(":")[1];
        }

        // Index list
        if (name == "index")
        {
          var list = (List<uint>)_object.GetDataByName(RO_DataType.Buffer, name);
          var buff = list.ToArray();
          glCreateBuffer(name);
          glUploadElementBuffer(name, buff);
          _indexAmount = list.Count;
          continue;
        }

        if (vaoAssignType == "vec2")
        {
          var list = (List<Vector2>)_object.GetDataByName(RO_DataType.Buffer, name);
          var buff = Vec2ToGPU(list);
          glCreateBuffer(name);
          glUploadBuffer(name, buff);
          _glBufferMapping.Add(name, vaoAssignName + ":" + vaoAssignType);
        }

        if (vaoAssignType == "vec3")
        {
          var list = (List<Vector3>)_object.GetDataByName(RO_DataType.Buffer, name);
          var buff = Vec3ToGPU(list);
          glCreateBuffer(name);
          glUploadBuffer(name, buff);
          _glBufferMapping.Add(name, vaoAssignName + ":" + vaoAssignType);
        }
      }

      // Create textures
      foreach (var tuple in TextureList.Select(t => t.Split(" -> ")))
      {
        var name = tuple[0];
        var uniformName = "";
        if (tuple.Length > 1) uniformName = tuple[1];

        // Create texture
        var texture = (Texture_Base)_object.GetDataByName(RO_DataType.Texture, name);
        if (texture == null) continue;
        glCreateTexture(name, texture.RAW_BYTE, texture.Options);
        _glTextureMapping[name] = uniformName;
      }
    }

    public void Render()
    {
      // Bind vao
      OpenGL32.glBindVertexArray(_vaoId);

      // Activate textures
      uint slotId = 0;
      foreach (var (textureName, uniformName) in _glTextureMapping)
      {
        // Update texture if changed
        var texture = (Texture_Base)_object.GetDataByName(RO_DataType.Texture, textureName);
        if (texture.IsChanged)
        {
          glUpdateTexture(textureName, texture);
          texture.IsChanged = false;
        }

        glActivateTexture(uniformName, textureName, slotId++);
      }

      // Bind uniforms
      foreach (var (varName, data) in UniformList)
      {
        var r = data();
        if (r is Matrix4x4 m)
        {
          OpenGL32.glUniformMatrix4fv(_shader.GetUniformLocation(varName), 1, 0, m.Raw);
        }
      }

      // Bind buffers
      foreach (var (bufferName, attribute) in _glBufferMapping)
      {
        OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, _glBuffer[bufferName]);
        glEnableAttribute("", attribute);
      }

      // Bind indices
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, _glBuffer["index"]);

      // Draw elements
      OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _indexAmount, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

      // Unbind vao
      OpenGL32.glBindVertexArray(0);
    }

    private void glCreateBuffer(string name)
    {
      uint bufferId = 0;
      OpenGL32.glGenBuffers(1, ref bufferId);
      if (bufferId == 0) throw new Exception("Can't create buffer");
      _glBuffer[name] = bufferId;
    }

    private void glUploadBuffer(string name, float[] data)
    {
      var bufferId = _glBuffer[name];
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);
      OpenGL32.glBufferData(OpenGL32.GL_ARRAY_BUFFER, data, OpenGL32.GL_STATIC_DRAW);
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
    }

    private void glUploadBuffer(string name, uint[] data)
    {
      var bufferId = _glBuffer[name];
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);
      OpenGL32.glBufferData(OpenGL32.GL_ARRAY_BUFFER, data, OpenGL32.GL_STATIC_DRAW);
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
    }

    private void glUploadElementBuffer(string name, uint[] data)
    {
      var bufferId = _glBuffer[name];

      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, bufferId);
      OpenGL32.glBufferData(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, data, OpenGL32.GL_STATIC_DRAW);
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    private void glCreateTexture(string name, byte[] pixels, TextureOptions options)
    {
      uint textureId = 0;
      OpenGL32.glCreateTextures(OpenGL32.GL_TEXTURE_2D, 1, ref textureId);
      if (textureId == 0) throw new Exception("Can't create texture");
      _glTexture.Add(name, textureId);

      // Internal opengl params
      var (internalFormat, srcFormat, srcType) = glGetTextureType(options);

      // At least 1 pixel size
      if (options.Width == 0) options.Width = 1;
      if (options.Height == 0) options.Height = 1;

      switch (options.Format)
      {
        case TextureFormat.BGR8:
          pixels ??= new byte[options.Width * options.Height * 3];
          break;
        case TextureFormat.RGB8:
          pixels ??= new byte[options.Width * options.Height * 3];
          break;
        case TextureFormat.RGBA8:
          pixels ??= new byte[options.Width * options.Height * 4];
          break;
        case TextureFormat.BGRA8:
          pixels ??= new byte[options.Width * options.Height * 4];
          break;
        case TextureFormat.R32F:
          pixels ??= new byte[options.Width * options.Height * 4];
          break;
        default:
          throw new Exception("Unsupported texture format");
      }

      // Store final options
      _glTextureOptions.Add(name, options);

      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Fill texture
      var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      OpenGL32.glTexImage2D(
        OpenGL32.GL_TEXTURE_2D,
        0,
        internalFormat,
        options.Width,
        options.Height,
        0,
        srcFormat,
        srcType,
        pixelPtr
      );

      // Do shit
      glSetTextureFiltrationMode(name, options.FiltrationMode);
      glSetTextureWrapMode(name, options.WrapMode);

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    private void glSetTextureFiltrationMode(string name, TextureFiltrationMode mode)
    {
      var textureId = _glTexture[name];
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      var textureOptions = _glTextureOptions[name];

      switch (mode)
      {
        case TextureFiltrationMode.Nearest:
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (int)OpenGL32.GL_NEAREST);
          OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (int)OpenGL32.GL_NEAREST);
          break;
        case TextureFiltrationMode.Linear when textureOptions.UseMipMaps:
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

    private void glSetTextureWrapMode(string name, TextureWrapMode mode)
    {
      var textureId = _glTexture[name];
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      if (mode == TextureWrapMode.Clamp)
      {
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_CLAMP_TO_EDGE);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_CLAMP_TO_EDGE);
      }

      if (mode == TextureWrapMode.Repeat)
      {
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_REPEAT);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_REPEAT);
      }

      if (mode == TextureWrapMode.Mirror)
      {
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_S, (int)OpenGL32.GL_MIRRORED_REPEAT);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_WRAP_T, (int)OpenGL32.GL_MIRRORED_REPEAT);
      }

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    private (int internalFormat, uint srcFormat, uint srcType) glGetTextureType(TextureOptions options)
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

    private void glActivateTexture(string varName, string textureName, uint slot)
    {
      // Activate slot texture
      OpenGL32.glActiveTexture(OpenGL32.GL_TEXTURE0 + slot);

      // Bind texture
      var textureId = _glTexture[textureName];
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Get shader.varName location
      var uniformLocation = OpenGL32.glGetUniformLocation(_shader.Id, varName);
      if (uniformLocation == -1) throw new Exception($"Uniform {varName} not found");

      // Set shader.varName = slot;
      OpenGL32.glUniform1i(uniformLocation, (int)slot);
    }

    private void glEnableAttribute(string shaderProgram, string attribute)
    {
      var tuple = attribute.Split(":");
      var name = tuple[0];
      var type = tuple[1];
      var size = 0;
      var attributeLocation = OpenGL32.glGetAttribLocation(_shader.Id, name);
      if (attributeLocation == -1) throw new Exception($"Attribute {attribute} not found");

      switch (type)
      {
        case "int":
        case "uint":
        case "float":
          size = 1;
          break;
        case "ivec2":
        case "uvec2":
        case "vec2":
          size = 2;
          break;
        case "ivec3":
        case "uvec3":
        case "vec3":
          size = 3;
          break;
        case "ivec4":
        case "uvec4":
        case "vec4":
          size = 4;
          break;
        default:
          throw new Exception($"Unknown type {tuple}");
      }

      switch (type)
      {
        case "float":
        case "vec2":
        case "vec3":
        case "vec4":
          OpenGL32.glVertexAttribPointer((uint)attributeLocation, size, OpenGL32.GL_FLOAT, 0, 0, IntPtr.Zero);
          OpenGL32.glEnableVertexAttribArray((uint)attributeLocation);
          break;
        case "int":
        case "ivec2":
        case "ivec3":
        case "ivec4":
          OpenGL32.glVertexAttribIPointer((uint)attributeLocation, size, OpenGL32.GL_INT, 0, IntPtr.Zero);
          OpenGL32.glEnableVertexAttribArray((uint)attributeLocation);
          break;
        case "uint":
        case "uvec2":
        case "uvec3":
        case "uvec4":
          OpenGL32.glVertexAttribIPointer((uint)attributeLocation, size, OpenGL32.GL_UNSIGNED_INT, 0, IntPtr.Zero);
          OpenGL32.glEnableVertexAttribArray((uint)attributeLocation);
          break;
        default:
          throw new Exception($"unknown type {type}");
      }
    }

    private void glUpdateTexture(string name, IntPtr dataPtr)
    {
      var textureId = _glTexture[name];
      var options = _glTextureOptions[name];
      var (internalFormat, srcFormat, srcType) = glGetTextureType(options);

      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Upload
      OpenGL32.glTexSubImage2D(
        OpenGL32.GL_TEXTURE_2D,
        0,
        0,
        0,
        options.Width,
        options.Height,
        srcFormat,
        srcType,
        dataPtr
      );

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    private void glUpdateTexture(string name, Texture_Base texture)
    {
      if (texture.Options.Format == TextureFormat.R32F)
      {
        var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(texture.RAW_FLOAT, 0);
        glUpdateTexture(name, pixelPtr);
      }
      else
      {
        var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(texture.RAW_BYTE, 0);
        glUpdateTexture(name, pixelPtr);
      }
    }

    private float[] Vec2ToGPU(IReadOnlyList<Vector2> l)
    {
      var v = new List<float>();
      for (var i = 0; i < l.Count; i++)
      {
        v.Add(l[i].X);
        v.Add(l[i].Y);
      }

      return v.ToArray();
    }

    private float[] Vec3ToGPU(IReadOnlyList<Vector3> l)
    {
      var v = new List<float>();
      for (var i = 0; i < l.Count; i++)
      {
        v.Add(l[i].X);
        v.Add(l[i].Y);
        v.Add(l[i].Z);
      }

      return v.ToArray();
    }
  }
}