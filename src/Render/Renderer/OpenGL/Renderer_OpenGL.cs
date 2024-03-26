using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.Renderer.OpenGL.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL
{
  public class Renderer_OpenGL : IRenderer
  {
    private readonly Context_OpenGL _context = new();
    private Render_Scene _scene;
    private readonly Dictionary<string, GLRL_Base> _glLayer = new();

    public void SetScene(Render_Scene scene)
    {
      _scene = scene;

      foreach (var layer in scene.Pipeline)
      {
        switch (layer)
        {
          case RL_StaticLine:
            _glLayer[layer.Name] = new GLRL_Line(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          case RL_StaticMesh mesh:
            _glLayer[layer.Name] = new GLRL_StaticMesh(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          case RL_Skybox:
            _glLayer[layer.Name] = new GLRL_Skybox(_context, layer, _scene);
            _glLayer[layer.Name].Init();
            break;
          default:
            throw new Exception("Unsupported layer type");
        }
      }

      // Init cube map
      _context.CreateCubeTexture("main", new[]
      {
        _scene.Skybox.GPU_RIGHT,
        _scene.Skybox.GPU_LEFT,

        _scene.Skybox.GPU_TOP,
        _scene.Skybox.GPU_BOTTOM,

        _scene.Skybox.GPU_BACK,
        _scene.Skybox.GPU_FRONT,
      }, _scene.Skybox.Options);
    }

    private void InitStaticMeshLayer(RL_StaticMesh renderLayer)
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
        #pragma optimize(off)
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
        #pragma optimize(off)
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
        uniform sampler2D uMetallicColor;
        
        uniform samplerCube uSkybox;

        //const float PI = 3.1415;
        const float PI = 3.141592653589793;
        
        float ggxDistribution(float roughness, float nDotH) {
            float alpha2 = roughness * roughness * roughness * roughness;
            float d = nDotH * nDotH * (alpha2 - 1.0) + 1.0;
            return alpha2 / max((PI * d * d), 0.000001);
        }

        float geomSmith(float roughness, float dp) {
            float k = (roughness + 1.0) * (roughness + 1.0) / 8.0;
            float denom = max(dp * (1.0 - k) + k, 0.000001);
            return dp / denom;
        }
        
        vec3 schlickFresnel(vec3 F0, float vDotH) {
            return F0 + (1.0 - F0) * pow(clamp(1.0 - vDotH, 0.0, 1.0), 5.0);
        }

        vec3 blurReflection(vec3 viewDirection, vec3 normal, float power) {
            vec3 finalColor = vec3(0.0);
            for (int i = -1; i < 2; i++) {
              for (int j = -1; j < 2; j++) {
                vec3 reflectedDir = reflect(viewDirection, normal + vec3(float(i) * power, float(j) * power, 0.0));
                reflectedDir.y *= -1.0;
                reflectedDir.x *= -1.0;
                finalColor += textureLod(uSkybox, reflectedDir, power * 128.0).rgb;
              }
            }
            
            return finalColor / 9.0;
        }

        float remap(float value, float fromMin, float fromMax, float toMin, float toMax) {
            float normalizedValue = (value - fromMin) / (fromMax - fromMin);
            return mix(toMin, toMax, normalizedValue);
        }
        
        vec3 linearToSRGB(vec3 colorLinear) {
            vec3 colorSRGB;
            for (int i = 0; i < 3; ++i) {
                if (colorLinear[i] <= 0.0031308) {
                    colorSRGB[i] = 12.92 * colorLinear[i];
                } else {
                    colorSRGB[i] = 1.055 * pow(colorLinear[i], 1.0 / 2.4) - 0.055;
                }
            }
            return colorSRGB;
        }
            
        vec3 sRGBToLinear(vec3 colorSRGB) {
            vec3 colorLinear;
            for (int i = 0; i < 3; ++i) {
                if (colorSRGB[i] <= 0.04045) {
                    colorLinear[i] = colorSRGB[i] / 12.92;
                } else {
                    colorLinear[i] = pow((colorSRGB[i] + 0.055) / 1.055, 2.4);
                }
            }
            return colorLinear;
        }
        
        void main()
        {
            // Read albedo
            vec4 texelColor = texture(uTextureColor, vUV);
            if (texelColor.a <= 0.01) discard;
            texelColor.rgb = sRGBToLinear(texelColor.rgb);
            
            // Roughness
            float roughness = texture(uRoughnessColor, vUV).r;
            if (roughness < 0.06) roughness = 0.06;
            
            // Read normal
            vec3 normal = texture(uNormalColor, vUV).xyz * 2.0 - 1.0;
            normal = normalize(vTBN * normal);
            
            // Metallic
            bool metallic = texture(uMetallicColor, vUV).r >= 0.5;
            
            // Light
            // vec3 vLightDirection = normalize(vec3(0.0, 1.0, 1.0));
            // float nDotL = dot(normalize(normal.xyz), normalize(vLightDirection));
            vec3 lightIntensity = vec3(2.0);

            // Vectors
            vec3 viewDirection = normalize(vCameraPosition - vPosition);
            vec3 lightDirection = normalize(vec3(0.0, 1.0, 1.0));
            vec3 halfDir = normalize(lightDirection + viewDirection);
            
            // lightDirection = reflect(-viewDirection, normal);
            // lightIntensity = textureLod(uSkybox, lightDirection, 1.0).rgb * (1.0 - roughness);
            
            // Dots
            float nDotH = max(dot(normal, halfDir), 0.001);
            float vDotH = max(dot(viewDirection, halfDir), 0.001);
            float nDotL = dot(normal, lightDirection);
            float nDotV = max(dot(normal, viewDirection), 0.001);
            
            if (nDotL <= 0.05) nDotL = 0.05;
            
            // Reflection
            /*vec3 reflectedDir = reflect(viewDirection, normal);
            reflectedDir.y *= -1.0;
            reflectedDir.x *= -1.0;
            vec3 reflectedColor = textureLod(uSkybox, reflectedDir, roughness * 32.0).rgb;*/
            
            float sex = remap(roughness, 0.0, 1.0, 0.0, 4.0);
            vec3 reflectedColor = blurReflection(viewDirection, normal, sex * 0.02);

            // PBR
            vec3 FO = vec3(0.04);
            
            if (metallic) {
                FO = texelColor.rgb;
            }
            
            vec3 F = schlickFresnel(FO, vDotH);
            vec3 kS = F;
            vec3 kD = vec3(1.0) - kS;
            
            vec3 specBRDF_nom = ggxDistribution(roughness, nDotH) * 
                F * 
                geomSmith(roughness, nDotL) * 
                geomSmith(roughness, nDotV);
            float specBRDF_denom = 4.0 * nDotV * nDotL;
            vec3 specBRDF = (specBRDF_nom / max(specBRDF_denom, 0.00001));
            
            vec3 fLambert = vec3(0.0);
            if (!metallic) {
                fLambert = texelColor.rgb;
            }
            vec3 diffuseBRDF = kD * fLambert / PI;
            
            // Reflection
            if (!metallic) {
              reflectedColor *= pow(1.0 - roughness, 3.0) * 0.65;
              float cosTheta = dot(normal, viewDirection);
              float fresnelFactor = pow(1.0 - abs(cosTheta), 5.0);
              if (fresnelFactor < 0.2) fresnelFactor = 0.2;
              reflectedColor *= fresnelFactor;
            } else {
              float cosTheta = dot(normal, viewDirection);
              float fresnelFactor = pow(1.0 - abs(cosTheta), (roughness) * 4.0 + 1.0);
              if (fresnelFactor < 0.05) fresnelFactor = 0.05;
              reflectedColor *= fresnelFactor;
            }

            float xxx = remap(roughness, 0.0, 1.0, 0.0, 2.0);
            if (xxx > 1.0) xxx = 1.0;
            
            // specBRDF += vec3(1.0, 0.0, 0.0) * ((1.0 - nDotL) * 0.25);
            
            vec3 finalColor = (diffuseBRDF + specBRDF + reflectedColor) * lightIntensity * nDotL;
            
            // HDR
            finalColor = finalColor / (finalColor + vec3(1.0));
            
            // Gamma
            finalColor = pow(finalColor, vec3(1.0/2.2));
            
            color = vec4(finalColor, texelColor.a);
            // color = vec4(schlickFresnel(vec3(1.0), vDotH), 1.0);
        }".Replace("\r", "");

      var tuple = shaderPBR.Split("// Fragment\n");
      _context.CreateShader(renderLayer.Name, tuple[0], tuple[1]);
    }

    private void InitSkyboxLayer(RL_Skybox renderLayer)
    {
    }

    private void DrawStaticMeshLayer(RL_StaticMesh layer)
    {
    }

    private void DrawSkyboxLayer(RL_Skybox layer)
    {
    }

    public void Clear()
    {
      OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT);
      OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public void Render()
    {
      foreach (var layer in _scene.Pipeline)
      {
        if (_glLayer.ContainsKey(layer.Name)) _glLayer[layer.Name].Render();

        /*switch (layer)
        {
          case RL_StaticLine:
            _glLayer[layer.Name].Render();
            // DrawLineLayer(line);
            break;
          case RL_StaticMesh mesh:
            DrawStaticMeshLayer(mesh);
            break;
          case RL_Skybox:
            // DrawSkyboxLayer(skybox);
            break;
          default:
            throw new Exception("Unsupported layer");
        }*/
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