using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class ObjectInfo_OpenGL
  {
    public ulong MeshId;
    public ulong TextureId;
    public ulong NormalTextureId;
    public ulong RoughnessTextureId;
    public ulong MetallicTextureId;

    public string VertexBufferName;
    public string NormalBufferName;
    public string UV0BufferName;

    public string TangentBufferName;
    public string BiTangentBufferName;

    public string BoneWeightBufferName;
    public string BoneIndexBufferName;

    public Transform Transform;

    public string IndexBufferName;
    public string ShaderName;
    public string TextureName;
    public string NormalTextureName;
    public string RoughnessTextureName;
    public string MetallicTextureName;
    public string BoneMatrixTextureName;

    public int IndexAmount;
    public List<string> VertexAttributeList = new();
    private readonly Context_OpenGL _context;

    public ObjectInfo_OpenGL(Context_OpenGL context)
    {
      _context = context;
    }

    public void DrawElements()
    {
      // Enable attributes
      foreach (var tuple in VertexAttributeList.Select(attribute => attribute.Split(" -> ")))
      {
        _context.EnableAttribute(ShaderName, tuple[0], tuple[1]);
      }

      // Bind indices
      _context.BindElementBuffer(IndexBufferName);

      // Has texture
      uint slotId = 0;
      if (!string.IsNullOrEmpty(TextureName))
      {
        _context.ActivateTexture(ShaderName, "uTextureColor", TextureName, slotId++);
      }

      if (!string.IsNullOrEmpty(NormalTextureName))
      {
        _context.ActivateTexture(ShaderName, "uNormalColor", NormalTextureName, slotId++);
      }

      if (!string.IsNullOrEmpty(RoughnessTextureName))
      {
        _context.ActivateTexture(ShaderName, "uRoughnessColor", RoughnessTextureName, slotId++);
      }

      if (!string.IsNullOrEmpty(MetallicTextureName))
      {
        _context.ActivateTexture(ShaderName, "uMetallicColor", MetallicTextureName, slotId++);
      }

      if (!string.IsNullOrEmpty(BoneMatrixTextureName))
      {
        _context.ActivateTexture(ShaderName, "uBoneMatrix", BoneMatrixTextureName, slotId++);
      }

      if (_context.HasUniform(ShaderName, "uModelMatrix"))
      {
        _context.BindMatrix(ShaderName, "uModelMatrix", Transform.Matrix);
      }

      // Draw
      OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, IndexAmount, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);
    }
  }

  public class Context_OpenGL
  {
    private readonly Dictionary<string, uint> _bufferList = new();
    private readonly Dictionary<string, uint> _shaderList = new();
    private readonly Dictionary<string, uint> _textureList = new();
    private readonly Dictionary<string, uint> _cubeTextureList = new();
    private readonly Dictionary<string, TextureOptions> _textureOptionsList = new();
    private readonly Dictionary<ulong, ObjectInfo_OpenGL> _objectInfo = new();

    public ObjectInfo_OpenGL GetObjectInfo(ulong uid)
    {
      _objectInfo.TryGetValue(uid, out var objectInfo);
      return objectInfo;
    }

    public void SetObjectInfo(ulong uid, ObjectInfo_OpenGL objectInfo)
    {
      _objectInfo[uid] = objectInfo;
    }

    public void CreateBuffer(string name)
    {
      uint bufferId = 0;
      OpenGL32.glGenBuffers(1, ref bufferId);
      if (bufferId == 0) throw new Exception("Can't create buffer");
      _bufferList.Add(name, bufferId);
    }

    public void CreateShader(string name, string vertex, string fragment)
    {
      // Vertex shader 
      var vertexShaderId = OpenGL32.glCreateShader(OpenGL32.GL_VERTEX_SHADER);
      if (vertexShaderId == 0)
      {
        Console.WriteLine("Can't create shader");
        OpenGL32.PrintGlError();
        return;
      }

      Console.WriteLine("shaderId = " + vertexShaderId);
      OpenGL32.glShaderSource(vertexShaderId, vertex);
      OpenGL32.glCompileShader(vertexShaderId);
      Console.WriteLine(OpenGL32.glGetShaderInfoLog(vertexShaderId, 512));
      Console.WriteLine("Everything ok maybe??");

      // Fragment shader 
      var fragmentShaderId = OpenGL32.glCreateShader(OpenGL32.GL_FRAGMENT_SHADER);
      if (fragmentShaderId == 0)
      {
        OpenGL32.PrintGlError();
        return;
      }

      Console.WriteLine("fragmentShaderId = " + fragmentShaderId);
      OpenGL32.glShaderSource(fragmentShaderId, fragment);
      OpenGL32.glCompileShader(fragmentShaderId);
      Console.WriteLine(OpenGL32.glGetShaderInfoLog(fragmentShaderId, 512));
      Console.WriteLine("Everything ok maybe??");

      // Использование функции glCreateProgram()
      var shaderProgram = OpenGL32.glCreateProgram();
      Console.WriteLine(shaderProgram == 0
        ? "Не удалось создать программу OpenGL."
        : "Программа OpenGL успешно создана.");

      OpenGL32.glAttachShader(shaderProgram, vertexShaderId);
      OpenGL32.glAttachShader(shaderProgram, fragmentShaderId);
      OpenGL32.glLinkProgram(shaderProgram);

      OpenGL32.glGetProgramiv(shaderProgram, OpenGL32.GL_LINK_STATUS, out var success);
      Console.WriteLine(success == 0 ? OpenGL32.glGetProgramInfoLog(shaderProgram, 512) : "Everything ok maybe??");

      // Don't need anymore
      OpenGL32.glDeleteShader(vertexShaderId);
      OpenGL32.glDeleteShader(fragmentShaderId);

      _shaderList.Add(name, shaderProgram);
    }

    public void CreateCubeTexture(string name, byte[][] pixels, TextureOptions options)
    {
      uint textureId = 0;
      OpenGL32.glCreateTextures(OpenGL32.GL_TEXTURE_CUBE_MAP, 1, ref textureId);
      if (textureId == 0) throw new Exception("Can't create cube texture");
      _cubeTextureList.Add(name, textureId);

      // Internal opengl params

      // At least 1 pixel size
      var internalFormat = (int)OpenGL32.GL_RGBA;
      var srcFormat = OpenGL32.GL_RGBA;
      var srcType = OpenGL32.GL_UNSIGNED_BYTE;

      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_CUBE_MAP, textureId);

      // Fill texture
      for (var i = 0; i < 6; i++)
      {
        var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels[i], 0);
        OpenGL32.glTexImage2D(
          OpenGL32.GL_TEXTURE_CUBE_MAP_POSITIVE_X + (uint)i,
          0,
          internalFormat,
          options.Width,
          options.Height,
          0,
          srcFormat,
          srcType,
          pixelPtr
        );
      }


      // Filtration and wrap
      if (options.FiltrationMode == TextureFiltrationMode.Linear)
      {
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_MIN_FILTER,
          (int)OpenGL32.GL_LINEAR);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_MAG_FILTER,
          (int)OpenGL32.GL_LINEAR);
      }

      if (options.FiltrationMode == TextureFiltrationMode.Linear && options.UseMipMaps)
      {
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_MIN_FILTER,
          (int)OpenGL32.GL_LINEAR_MIPMAP_LINEAR);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_MAG_FILTER,
          (int)OpenGL32.GL_LINEAR_MIPMAP_LINEAR);
      }

      if (options.FiltrationMode == TextureFiltrationMode.Nearest)
      {
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_MIN_FILTER,
          (int)OpenGL32.GL_NEAREST);
        OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_MAG_FILTER,
          (int)OpenGL32.GL_NEAREST);
      }

      OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_WRAP_S,
        (int)OpenGL32.GL_CLAMP_TO_EDGE);
      OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_WRAP_T,
        (int)OpenGL32.GL_CLAMP_TO_EDGE);
      OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_CUBE_MAP, OpenGL32.GL_TEXTURE_WRAP_R,
        (int)OpenGL32.GL_CLAMP_TO_EDGE);

      // Генерация мипмапов для остальных уровней
      if (options.UseMipMaps) OpenGL32.glGenerateMipmap(OpenGL32.GL_TEXTURE_CUBE_MAP);

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_CUBE_MAP, 0);
    }

    public void DeleteTexture(string name)
    {
      var texture = _textureList[name];
      OpenGL32.glDeleteTextures(1, new[] { texture });
      _textureList.Remove(name);
      _textureOptionsList.Remove(name);
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

    public void CreateTexture(string name, byte[] pixels, TextureOptions options)
    {
      uint textureId = 0;
      OpenGL32.glCreateTextures(OpenGL32.GL_TEXTURE_2D, 1, ref textureId);
      if (textureId == 0) throw new Exception("Can't create texture");
      _textureList.Add(name, textureId);

      // Internal opengl params
      var (internalFormat, srcFormat, srcType) = GetTextureType(options);
      //int internalFormat;
      //uint srcFormat;
      //uint srcType;

      // At least 1 pixel size
      if (options.Width == 0) options.Width = 1;
      if (options.Height == 0) options.Height = 1;

      switch (options.Format)
      {
        case TextureFormat.BGR8:
          /*internalFormat = (int)OpenGL32.GL_RGBA;
          srcFormat = OpenGL32.GL_BGR;
          srcType = OpenGL32.GL_UNSIGNED_BYTE;*/
          // Fill pixels if null
          pixels ??= new byte[options.Width * options.Height * 3];
          break;
        case TextureFormat.RGB8:
          /*internalFormat = (int)OpenGL32.GL_RGBA;
          srcFormat = OpenGL32.GL_RGB;
          srcType = OpenGL32.GL_UNSIGNED_BYTE;*/
          // Fill pixels if null
          pixels ??= new byte[options.Width * options.Height * 3];
          break;
        case TextureFormat.RGBA8:
          /*internalFormat = (int)OpenGL32.GL_RGBA;
          srcFormat = OpenGL32.GL_RGBA;
          srcType = OpenGL32.GL_UNSIGNED_BYTE;*/
          // Fill pixels if null
          pixels ??= new byte[options.Width * options.Height * 4];
          break;
        case TextureFormat.BGRA8:
          /*internalFormat = (int)OpenGL32.GL_RGBA;
          srcFormat = OpenGL32.GL_BGRA;
          srcType = OpenGL32.GL_UNSIGNED_BYTE;*/
          // Fill pixels if null
          pixels ??= new byte[options.Width * options.Height * 4];
          break;
        case TextureFormat.R32F:
          /*internalFormat = (int)OpenGL32.GL_R32F;
          srcFormat = OpenGL32.GL_RED;
          srcType = OpenGL32.GL_FLOAT;*/
          // Fill pixels if null
          pixels ??= new byte[options.Width * options.Height * 4];
          break;
        default:
          throw new Exception("Unsupported texture format");
      }

      // Store final options
      _textureOptionsList.Add(name, options);

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
      SetTextureFiltrationMode(name, options.FiltrationMode);
      SetTextureWrapMode(name, options.WrapMode);

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    public void UpdateTexture(string name, byte[] pixels)
    {
      var textureId = _textureList[name];
      var options = _textureOptionsList[name];
      var (internalFormat, srcFormat, srcType) = GetTextureType(options);

      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Default
      var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      OpenGL32.glTexSubImage2D(
        OpenGL32.GL_TEXTURE_2D,
        0,
        0,
        0,
        options.Width,
        options.Height,
        srcFormat,
        srcType,
        pixelPtr
      );

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    public void UpdateTexture(string name, float[] pixels)
    {
      var textureId = _textureList[name];
      var options = _textureOptionsList[name];
      var (internalFormat, srcFormat, srcType) = GetTextureType(options);

      // Bind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Default
      var pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      OpenGL32.glTexSubImage2D(
        OpenGL32.GL_TEXTURE_2D,
        0,
        0,
        0,
        options.Width,
        options.Height,
        srcFormat,
        srcType,
        pixelPtr
      );

      // Unbind
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);
    }

    public void SetTextureFiltrationMode(string name, TextureFiltrationMode mode)
    {
      var textureId = _textureList[name];
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      var textureOptions = _textureOptionsList[name];

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

    public void SetTextureWrapMode(string name, TextureWrapMode mode)
    {
      var textureId = _textureList[name];
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

    public void UseProgram(string name)
    {
      var programId = _shaderList[name];
      OpenGL32.glUseProgram(programId);
    }

    public void UploadBuffer(string name, float[] data)
    {
      var bufferId = _bufferList[name];

      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);
      OpenGL32.glBufferData(OpenGL32.GL_ARRAY_BUFFER, data, OpenGL32.GL_STATIC_DRAW);
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
    }

    public void UploadBuffer(string name, uint[] data)
    {
      var bufferId = _bufferList[name];

      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);
      OpenGL32.glBufferData(OpenGL32.GL_ARRAY_BUFFER, data, OpenGL32.GL_STATIC_DRAW);
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
    }

    public void UploadElementBuffer(string name, uint[] data)
    {
      var bufferId = _bufferList[name];

      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, bufferId);
      OpenGL32.glBufferData(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, data, OpenGL32.GL_STATIC_DRAW);
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    public void BindBuffer(string name)
    {
      var bufferId = _bufferList[name];
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);
    }

    public void BindElementBuffer(string name)
    {
      var bufferId = _bufferList[name];
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, bufferId);
    }

    public void EnableAttribute(string shaderProgram, string bufferName, string attribute)
    {
      var programId = _shaderList[shaderProgram];
      var bufferId = _bufferList[bufferName];

      var tuple = attribute.Split(":");
      var name = tuple[0];
      var type = tuple[1];
      var size = 0;
      var attributeLocation = OpenGL32.glGetAttribLocation(programId, name);
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

      // Bind buffer
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);

      switch (type)
      {
        case "float":
        case "vec2":
        case "vec3":
        case "vec4":
          OpenGL32.glVertexAttribPointer((uint)attributeLocation, size, OpenGL32.GL_FLOAT, false, 0, IntPtr.Zero);
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

      // Unbind
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
    }

    public void BindVector(string shaderProgram, string varName, Vector3 vector)
    {
      var programId = _shaderList[shaderProgram];
      var uniformLocation = OpenGL32.glGetUniformLocation(programId, varName);
      if (uniformLocation == -1) throw new Exception($"Uniform {varName} not found");

      OpenGL32.glUniform3f(uniformLocation, vector.X, vector.Y, vector.Z);
    }

    public bool HasUniform(string shaderProgram, string varName)
    {
      var programId = _shaderList[shaderProgram];
      var uniformLocation = OpenGL32.glGetUniformLocation(programId, varName);
      return uniformLocation != -1;
    }

    public void BindMatrix(string shaderProgram, string varName, Matrix4x4 matrix)
    {
      var programId = _shaderList[shaderProgram];
      var uniformLocation = OpenGL32.glGetUniformLocation(programId, varName);
      if (uniformLocation == -1) throw new Exception($"Uniform {varName} not found");

      OpenGL32.glUniformMatrix4fv(uniformLocation, 1, false, matrix.Raw);
    }

    public void ActivateTexture(string shaderName, string varName, string textureName, uint slot)
    {
      // Activate slot texture
      OpenGL32.glActiveTexture(OpenGL32.GL_TEXTURE0 + slot);

      // Bind texture
      var textureId = _textureList[textureName];
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureId);

      // Get shader.varName location
      var programId = _shaderList[shaderName];
      var uniformLocation = OpenGL32.glGetUniformLocation(programId, varName);
      if (uniformLocation == -1) throw new Exception($"Uniform {varName} not found");

      // Set shader.varName = slot;
      OpenGL32.glUniform1i(uniformLocation, (int)slot);
    }

    public void ActivateCubeTexture(string shaderName, string varName, string textureName, uint slot)
    {
      // Activate slot texture
      OpenGL32.glActiveTexture(OpenGL32.GL_TEXTURE0 + slot);

      // Bind texture
      var textureId = _cubeTextureList[textureName];
      OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_CUBE_MAP, textureId);

      // Get shader.varName location
      var programId = _shaderList[shaderName];
      var uniformLocation = OpenGL32.glGetUniformLocation(programId, varName);
      if (uniformLocation == -1) throw new Exception($"Uniform {varName} not found");

      // Set shader.varName = slot;
      OpenGL32.glUniform1i(uniformLocation, (int)slot);
    }
  }
}