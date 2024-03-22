using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        out vec3 vCameraPosition;
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            vPosition = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            // vPosition = aPosition;
            
            vUV = aUV;
            
            // TBN Matrix
            mat4 modelMatrix = uModelMatrix;
            vec3 T = normalize(vec3(modelMatrix * vec4(aTangent,   1.0)));
            vec3 B = normalize(vec3(modelMatrix * vec4(aBiTangent, 1.0)));
            vec3 N = normalize(vec3(modelMatrix * vec4(aNormal,    1.0)));
            vTBN = mat3(T, B, N);

            vec4 cameraPosition = vec4(0.0, 0.0, 0.0, 1.0);
            vCameraPosition = (inverse(uViewMatrix) * cameraPosition).xyz;
        }
        // Fragment
        #version 300 es
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vPosition;
        in vec2 vUV;
        in mat3 vTBN;
        in vec3 vCameraPosition;
        
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
                color = vec4(texelColor.rgb * lighting.rgb, texelColor.a);
                return;
            }

            // Spec
            vec3 viewDirection = normalize(vCameraPosition - vPosition);
            vec3 lightDirection = normalize(-vLightDirection); // Направление света
            vec3 reflectedLight = reflect(lightDirection, normal);
            float shininess = 28.0; // Параметр блеска (зависит от материала)
            float specularStrength = 0.75; // Степень блеска
            float specular = pow(max(dot(reflectedLight, viewDirection), 0.0), shininess);
            vec3 specularColor = specularStrength * specular * vec3(1.0, 1.0, 1.0);
            
            vec3 lighting = vAmbientColor + (directionalLightColor * lightPower);
            color = vec4(texelColor.rgb * lighting.rgb + specularColor.rgb, texelColor.a);
        }".Replace("\r", "");

      // language=glsl
      var shaderPBR = @"#version 300 es
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
        out vec3 vCameraPosition;
        out vec2 vUV;
        out mat3 vTBN;
        
        mat4 identity() {
           return mat4(
               1.0, 0.0, 0.0, 0.0,
               0.0, 1.0, 0.0, 0.0,
               0.0, 0.0, 1.0, 0.0,
               0.0, 0.0, 0.0, 1.0
           );
        }
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            vPosition = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            //vPosition = aPosition;
            
            vUV = aUV;
            
            // TBN Matrix
            mat4 modelMatrix = identity(); //uModelMatrix;
            vec3 T = normalize(vec3(modelMatrix * vec4(aTangent,   1.0)));
            vec3 B = normalize(vec3(modelMatrix * vec4(aBiTangent, 1.0)));
            vec3 N = normalize(vec3(modelMatrix * vec4(aNormal,    1.0)));
            vTBN = mat3(T, B, N);
            
            // Camera position
            vec4 cameraPosition = vec4(0.0, 0.0, 0.0, 1.0);
            vCameraPosition = (inverse(uViewMatrix) * cameraPosition).xyz;
        }
        // Fragment
        #version 300 es
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vPosition;
        in vec3 vCameraPosition;
        in vec2 vUV;
        in mat3 vTBN;
        
        out vec4 color;

        uniform vec3 uLightPosition;
        uniform vec3 uCameraPosition;
        
        uniform sampler2D uTextureColor;
        uniform sampler2D uNormalColor;
        uniform sampler2D uRoughnessColor;
        
        const float PI = 3.1415;
        //const float PI = 3.141592653589793;
        
        float ggxDistribution(float roughness, float nDotH) {
            float alpha2 = roughness * roughness * roughness * roughness;
            float d = nDotH * nDotH * (alpha2 - 1.0) + 1.0;
            return alpha2 / (PI * d * d);
        }

        float geomSmith(float roughness, float dp) {
            float k = (roughness + 1.0) * (roughness + 1.0) / 8.0;
            float denom = dp * (1.0 - k) + k;
            return dp / denom;
        }
        
        vec3 schlickFresnel(vec3 F0, float vDotH) {
            return F0 + (1.0 - F0) * pow(clamp(1.0 - vDotH, 0.0, 1.0), 5.0);
        }
        
        void main()
        {
            // Read albedo
            vec4 texelColor = texture(uTextureColor, vUV);
            if (texelColor.a <= 0.01) discard;
            
            // Read normal
            vec3 normal = texture(uNormalColor, vUV).xyz * 2.0 - 1.0;
            normal = normalize(vTBN * normal);
            
            // Light
           // vec3 vLightDirection = normalize(vec3(0.0, 1.0, 1.0));
            // float nDotL = dot(normalize(normal.xyz), normalize(vLightDirection));
            vec3 lightIntensity = vec3(1.0);

            // Vectors
            vec3 viewDirection = normalize(vCameraPosition - vPosition);
            vec3 lightDirection = normalize(vec3(0.0, 1.0, 1.0));
            vec3 halfDir = normalize(lightDirection + viewDirection);
            
            // Dots
            float nDotH = max(dot(normal, halfDir), 0.0);
            float vDotH = max(dot(viewDirection, halfDir), 0.0);
            float nDotL = max(dot(normal, lightDirection), 0.0);
            float nDotV = max(dot(normal, viewDirection), 0.0);

            // PBR
            float roughness = texture(uRoughnessColor, vUV).r;
            vec3 F = schlickFresnel(vec3(0.04), vDotH);
            vec3 kS = F;
            vec3 kD = 1.0 - kS;
            
            vec3 specBRDF_nom = ggxDistribution(roughness, nDotH) * F * geomSmith(roughness, nDotL) * geomSmith(roughness, nDotV);
            float specBRDF_denom = 4.0 * dot(normal, viewDirection) * dot(normal, lightDirection) + 0.0001;
            vec3 specBRDF = specBRDF_nom / specBRDF_denom;
            
            vec3 lambert = texelColor.rgb;
            vec3 diffuseBRDF = kD * lambert / PI;
            
            vec3 finalColor = (diffuseBRDF + specBRDF) * lightIntensity * nDotL;
            
            // HDR
            finalColor = finalColor / (finalColor + vec3(1.0));
            
            // Gamma
            finalColor = pow(finalColor, vec3(1.0/2.2));
            
            color = vec4(finalColor, texelColor.a);
        }".Replace("\r", "");

      var tuple = shaderPBR.Split("// Fragment\n");
      _context.CreateShader(renderLayer.Name, tuple[0], tuple[1]);
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
      var cp = _scene.Camera.Position.Inverted;

      // _context.BindVector(layer.Name, "uCameraPosition", cp);
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

            Transform = mesh.Transform,
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

          // _context.BindMatrix(layer.Name, "uModelMatrix", mesh.Transform.Matrix);

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

          if (mesh.RoughnessTexture != null)
          {
            _context.CreateTexture($"{layer.Name}_{mesh.Id}.roughnessTexture", mesh.RoughnessTexture.GPU_RAW,
              mesh.RoughnessTexture.Options);
            objectInfo.RoughnessTextureName = $"{layer.Name}_{mesh.Id}.roughnessTexture";
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

    public void Clear()
    {
      OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT);
      OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
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

    public byte[] GetScreen()
    {
      var width = 800;
      var height = 600;
      var pixels = new byte[width * height * 4];
      var pixelsPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
      OpenGL32.glReadPixels(0, 0, width, height, OpenGL32.GL_RGBA, OpenGL32.GL_UNSIGNED_BYTE, pixelsPtr);

      return pixels;
    }
  }
}