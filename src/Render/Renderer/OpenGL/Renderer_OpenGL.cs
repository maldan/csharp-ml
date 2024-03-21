using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class Renderer_OpenGL : IRenderer
  {
    private readonly Context_OpenGL _context = new();
    private Render_Scene _scene;

    public void SetScene(Render_Scene scene)
    {
      _scene = scene;

      foreach (var layer in scene.Pipeline)
      {
        switch (layer.Type)
        {
          case Render_LayerType.DynamicLine:
            InitLineLayer(layer);
            break;
          case Render_LayerType.StaticMesh:
            InitStaticMeshLayer(layer);
            break;
          default:
            throw new Exception("Unsupported layer type");
        }
      }
    }

    private void InitLineLayer(Render_Layer renderLayer)
    {
      // language=glsl
      var lineShaderVertex = @"#version 300 es
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec3 aVertex;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;

        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * vec4(aVertex.xyz, 1.0);
        }".Replace("\r", "");

      // language=glsl
      var lineShaderFragment = @"#version 300 es
          precision highp float;
          precision highp int;
          precision highp sampler2D;

          out vec4 color;

          void main()
          {
              color = vec4(1.0, 1.0, 1.0, 1.0);
          }".Replace("\r", "");

      // Create shader line
      _context.CreateShader(renderLayer.Name, lineShaderVertex, lineShaderFragment);

      // Create buffer vertex
      _context.CreateBuffer($"{renderLayer.Name}.vertex");

      // Create buffer color
    }

    private void InitStaticMeshLayer(Render_Layer renderLayer)
    {
      // language=glsl
      var shader = @"#version 300 es
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec3 aTangent;
        layout (location = 2) in vec3 aBiTangent;
        layout (location = 3) in vec2 aUV;
        layout (location = 4) in vec3 aNormal;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform mat4 uModelMatrix;

        uniform vec3 uLightPosition;
        uniform vec3 uCameraPosition;
        
        out vec3 vPosition;
        out vec2 vUV;
        out mat3 vTBN;
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            vPosition = aPosition;
            
            vUV = aUV;
            
            // TBN Matrix
            mat4 modelMatrix = uModelMatrix;
            vec3 T = normalize(vec3(modelMatrix * vec4(aTangent,   1.0)));
            vec3 B = normalize(vec3(modelMatrix * vec4(aBiTangent, 1.0)));
            vec3 N = normalize(vec3(modelMatrix * vec4(aNormal,    1.0)));
            vTBN = mat3(T, B, N);
        }
        // Fragment
        #version 300 es
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vPosition;
        in vec2 vUV;
        in mat3 vTBN;
        
        out vec4 color;

        uniform vec3 uLightPosition;
        uniform vec3 uCameraPosition;
        
        uniform sampler2D uTextureColor;
        uniform sampler2D uNormalColor;
                   
        void main()
        {
            vec4 texelColor = texture(uTextureColor, vUV);
            if (texelColor.a <= 0.01) {
                discard;
            }
            // if (int(gl_FragCoord.y) % 2 == 0) discard;
            
            // Считываем нормали из normal map
            vec3 normal = texture(uNormalColor, vUV).xyz * 2.0 - 1.0;
            normal = normalize(vTBN * normal);
            
            // Const
            vec3 vLightDirection = normalize(vec3(0.0, 1.0, 1.0));
            vec3 vAmbientColor = vec3(0.3, 0.3, 0.3);
            vec3 directionalLightColor = vec3(1.0, 1.0, 1.0);
            
            // Light
            float lightDot = dot(normalize(normal.xyz), normalize(vLightDirection));
            float lightPower = max(lightDot, 0.0);
            if (lightDot < 0.0) {
                vAmbientColor = mix(vAmbientColor, vec3(0.0, 0.0, 0.0), -lightDot);
                vec3 lighting = vAmbientColor + (directionalLightColor * lightPower);
                color = vec4(texelColor.rgb * lighting.rgb, 1.0);
                return;
            }

            // Spec
            vec3 viewDirection = normalize(uCameraPosition - vPosition);
            vec3 lightDirection = normalize(-vLightDirection); // Направление света
            vec3 reflectedLight = reflect(lightDirection, normal);
            float shininess = 28.0; // Параметр блеска (зависит от материала)
            float specularStrength = 0.75; // Степень блеска
            float specular = pow(max(dot(reflectedLight, viewDirection), 0.0), shininess);
            vec3 specularColor = specularStrength * specular * vec3(1.0, 1.0, 1.0);
            
            vec3 lighting = vAmbientColor + (directionalLightColor * lightPower);
            color = vec4(texelColor.rgb * lighting.rgb + specularColor.rgb, 1.0);
        }".Replace("\r", "");

      var tuple = shader.Split("// Fragment\n");
      _context.CreateShader(renderLayer.Name, tuple[0], tuple[1]);
      //_context.CreateBuffer($"{renderLayer.Name}.vertex");
      //_context.CreateBuffer($"{renderLayer.Name}.index");
    }

    private void DrawLineLayer(Render_Layer layer)
    {
      // User shader line
      _context.UseProgram(layer.Name);

      // gl blend fn
      OpenGL32.glEnable(OpenGL32.GL_BLEND);
      OpenGL32.glBlendFunc(OpenGL32.GL_SRC_ALPHA, OpenGL32.GL_ONE_MINUS_SRC_ALPHA);

      // bind matrix
      _context.BindMatrix(layer.Name, "uProjectionMatrix", _scene.Camera.ProjectionMatrix);
      _context.BindMatrix(layer.Name, "uViewMatrix", _scene.Camera.ViewMatrix);

      // gl enable depth test
      OpenGL32.glEnable(OpenGL32.GL_DEPTH_TEST);
      OpenGL32.glDepthFunc(OpenGL32.GL_LEQUAL);

      OpenGL32.glEnable(OpenGL32.GL_LINE_SMOOTH);
      OpenGL32.glLineWidth(2);

      // Build actual line arrays data
      var vertexList = new List<float>();
      layer.ForEach<RenderObject_Line>((line) =>
      {
        var from = line.From;
        var to = line.To;
        if (line.Transform != null)
        {
          from = line.From * line.Transform.Matrix;
          to = line.To * line.Transform.Matrix;
        }

        vertexList.Add(from.X);
        vertexList.Add(from.Y);
        vertexList.Add(from.Z);
        vertexList.Add(to.X);
        vertexList.Add(to.Y);
        vertexList.Add(to.Z);
      });

      // gl upload buffers
      _context.UploadBuffer($"{layer.Name}.vertex", vertexList.ToArray());

      // gl enable attributes
      _context.EnableAttribute(layer.Name, $"{layer.Name}.vertex", "aVertex:vec3");

      // gl draw arrays
      OpenGL32.glDrawArrays(OpenGL32.GL_LINES, 0, layer.Count * 2);

      // Clear list
      layer.Clear();
    }

    private void DrawStaticMeshLayer(Render_Layer layer)
    {
      // User shader line
      _context.UseProgram(layer.Name);

      // gl blend fn
      OpenGL32.glEnable(OpenGL32.GL_BLEND);
      OpenGL32.glBlendFunc(OpenGL32.GL_SRC_ALPHA, OpenGL32.GL_ONE_MINUS_SRC_ALPHA);

      // bind camera
      _context.BindVector(layer.Name, "uCameraPosition", _scene.Camera.Position);
      // _context.BindVector(layer.Name, "uLightPosition", new Vector3(0, 0, -2));
      _context.BindMatrix(layer.Name, "uProjectionMatrix", _scene.Camera.ProjectionMatrix);
      _context.BindMatrix(layer.Name, "uViewMatrix", _scene.Camera.ViewMatrix);

      // gl enable depth test
      OpenGL32.glEnable(OpenGL32.GL_DEPTH_TEST);
      OpenGL32.glDepthFunc(OpenGL32.GL_LEQUAL);

      // Draw each mesh
      layer.ForEach<RenderObject_Mesh>(mesh =>
      {
        var objectInfo = _context.GetObjectInfo(mesh.Id);
        if (objectInfo == null)
        {
          // Define object
          objectInfo = new ObjectInfo_OpenGL(_context)
          {
            VertexBufferName = $"{layer.Name}_{mesh.Id}.vertex",
            UV0BufferName = $"{layer.Name}_{mesh.Id}.uv0",
            NormalBufferName = $"{layer.Name}_{mesh.Id}.normal",

            TangentBufferName = $"{layer.Name}_{mesh.Id}.tangent",
            BiTangentBufferName = $"{layer.Name}_{mesh.Id}.biTangent",

            IndexBufferName = $"{layer.Name}_{mesh.Id}.index",
            IndexAmount = mesh.GpuIndexList.Length,
            ShaderName = layer.Name,
          };
          objectInfo.VertexAttributeList = new List<string>
          {
            $"{objectInfo.VertexBufferName} -> aPosition:vec3",
            $"{objectInfo.UV0BufferName} -> aUV:vec2",
            $"{objectInfo.NormalBufferName} -> aNormal:vec3",

            $"{objectInfo.TangentBufferName} -> aTangent:vec3",
            $"{objectInfo.BiTangentBufferName} -> aBiTangent:vec3",
          };

          // Create buffers
          _context.CreateBuffer(objectInfo.VertexBufferName);
          _context.CreateBuffer(objectInfo.UV0BufferName);
          _context.CreateBuffer(objectInfo.NormalBufferName);
          _context.CreateBuffer(objectInfo.TangentBufferName);
          _context.CreateBuffer(objectInfo.BiTangentBufferName);
          _context.CreateBuffer(objectInfo.IndexBufferName);

          // gl upload buffers
          _context.UploadBuffer(objectInfo.VertexBufferName, mesh.GpuVertexList);
          _context.UploadBuffer(objectInfo.UV0BufferName, mesh.GpuUVList);
          _context.UploadBuffer(objectInfo.NormalBufferName, mesh.GpuNormalList);
          _context.UploadBuffer(objectInfo.TangentBufferName, mesh.GpuTangentList);
          _context.UploadBuffer(objectInfo.BiTangentBufferName, mesh.GpuBiTangentList);
          _context.UploadElementBuffer(objectInfo.IndexBufferName, mesh.GpuIndexList);

          _context.BindMatrix(layer.Name, "uModelMatrix", mesh.Transform.Matrix);

          // Mesh has texture
          if (mesh.Texture != null)
          {
            _context.CreateTexture($"{layer.Name}_{mesh.Id}.texture", mesh.Texture.GPU_RAW, mesh.Texture.Options);
            objectInfo.TextureName = $"{layer.Name}_{mesh.Id}.texture";
          }

          if (mesh.NormalTexture != null)
          {
            _context.CreateTexture($"{layer.Name}_{mesh.Id}.normalTexture", mesh.NormalTexture.GPU_RAW,
              mesh.NormalTexture.Options);
            objectInfo.NormalTextureName = $"{layer.Name}_{mesh.Id}.normalTexture";
          }

          // Set object
          _context.SetObjectInfo(mesh.Id, objectInfo);
        }

        // gl draw arrays
        objectInfo.DrawElements();
      });

      // Unbind
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    public void Render()
    {
      for (var i = 0; i < _scene.Pipeline.Count; i++)
      {
        var layer = _scene.Pipeline[i];
        switch (layer.Type)
        {
          case Render_LayerType.DynamicLine:
            DrawLineLayer(layer);
            break;
          case Render_LayerType.StaticMesh:
            DrawStaticMeshLayer(layer);
            break;
          default:
            throw new Exception("Unsupported layer");
        }
      }
    }
  }
}