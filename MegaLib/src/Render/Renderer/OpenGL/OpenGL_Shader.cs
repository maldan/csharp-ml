using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class OpenGL_Shader
  {
    public readonly Dictionary<string, string> ShaderCode = new();
    public uint Id { get; private set; }
    public OpenGL_Context Context;

    public void Compile()
    {
      // Create program
      var shaderProgram = OpenGL32.glCreateProgram();
      if (shaderProgram == 0)
      {
        OpenGL32.PrintGlError();
        throw new Exception("Can't create shader program");
      }

      // Create shaders
      var typeMap = new Dictionary<string, uint>
      {
        { "vertex", OpenGL32.GL_VERTEX_SHADER },
        { "fragment", OpenGL32.GL_FRAGMENT_SHADER },
        { "geometry", OpenGL32.GL_GEOMETRY_SHADER },
        { "tesselationControl", OpenGL32.GL_TESS_CONTROL_SHADER },
        { "tesselationEvaluation", OpenGL32.GL_TESS_EVALUATION_SHADER },
      };
      var oldShaderList = new List<uint>();
      foreach (var (type, code) in ShaderCode)
      {
        var shaderId = OpenGL32.glCreateShader(typeMap[type]);
        if (shaderId == 0)
        {
          OpenGL32.PrintGlError();
          throw new Exception($"Can't create {type} shader");
        }

        Console.WriteLine($"{type} shader created");

        OpenGL32.glShaderSource(shaderId, code.Replace("\r", ""));
        OpenGL32.glCompileShader(shaderId);

        var log = OpenGL32.glGetShaderInfoLog(shaderId, 1024).Trim();
        if (log.Length > 0) Console.WriteLine($"{type} shader log: {log}");

        // Attach to program
        OpenGL32.glAttachShader(shaderProgram, shaderId);

        // Save for remove later
        oldShaderList.Add(shaderId);
      }

      // Link program
      OpenGL32.glLinkProgram(shaderProgram);

      // Check status
      var success = 0;
      OpenGL32.glGetProgramiv(shaderProgram, OpenGL32.GL_LINK_STATUS, ref success);
      if (success == 0) Console.WriteLine(OpenGL32.glGetProgramInfoLog(shaderProgram, 1024));

      // Remove old shaders, we don't need them anymore
      oldShaderList.ForEach(OpenGL32.glDeleteShader);

      // Store program
      Id = shaderProgram;
    }

    public void Use()
    {
      OpenGL32.glUseProgram(Id);
    }

    public void Enable(uint v)
    {
      OpenGL32.glEnable(v);

      if (v == OpenGL32.GL_BLEND)
      {
        OpenGL32.glBlendFunc(OpenGL32.GL_SRC_ALPHA, OpenGL32.GL_ONE_MINUS_SRC_ALPHA);
      }

      if (v == OpenGL32.GL_DEPTH_TEST)
      {
        OpenGL32.glDepthFunc(OpenGL32.GL_LEQUAL);
      }
    }

    public void Disable(uint v)
    {
      OpenGL32.glDisable(v);
    }

    public int GetUniformLocation(string name)
    {
      var uniformLocation = OpenGL32.glGetUniformLocation(Id, name);
      if (uniformLocation == -1) throw new Exception($"Uniform {name} not found");
      return uniformLocation;
    }

    public void SetUniform(string name, Vector3 v)
    {
      var id = GetUniformLocation(name);
      OpenGL32.glUniform3f(id, v.X, v.Y, v.Z);
    }

    public void SetUniform(string name, Matrix4x4 v)
    {
      var id = GetUniformLocation(name);
      OpenGL32.glUniformMatrix4fv(id, 1, 0, v.Raw);
    }

    public void EnableAttribute<T>(ListGPU<T> buffer, string attributeName)
    {
      var bufferId = Context.GetBufferId(buffer);

      var type = buffer switch
      {
        ListGPU<int> => "int",
        ListGPU<uint> => "uint",
        ListGPU<float> => "float",
        ListGPU<Vector2> => "vec2",
        ListGPU<Vector3> => "vec3",
        ListGPU<Vector4> => "vec4",
        ListGPU<Vector4Int> => "ivec4",
        _ => ""
      };

      var size = 0;
      var attributeLocation = OpenGL32.glGetAttribLocation(Id, attributeName);
      if (attributeLocation == -1) throw new Exception($"Attribute {attributeName} not found");

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
          throw new Exception($"Unknown type {type}");
      }

      // Bind buffer
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, bufferId);

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

      // Unbind
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
    }
  }
}