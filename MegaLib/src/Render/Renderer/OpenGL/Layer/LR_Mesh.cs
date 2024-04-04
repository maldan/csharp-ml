using System;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class LR_Mesh : LR_Base
  {
    public LR_Mesh(OpenGL_Context context, RL_Base layer, Render_Scene scene) : base(context, layer, scene)
    {
    }

    public override void Init()
    {
      # region vertex

      // language=glsl
      var vertex = @"#version 330 core
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
        
        out vec3 vo_Position;
        out vec2 vo_UV;
        out mat3 vo_TBN;
        out vec3 vo_CameraPosition;
        
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
            
            // TBN Matrix
            mat4 modelMatrix = identity();
            vec3 T = normalize(vec3(modelMatrix * vec4(aTangent,   1.0)));
            vec3 B = normalize(vec3(modelMatrix * vec4(aBiTangent, 1.0)));
            vec3 N = normalize(vec3(modelMatrix * vec4(aNormal,    1.0)));
            
            // Out
            vec4 cameraPosition = vec4(0.0, 0.0, 0.0, 1.0);
            vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).xyz;
            
            vo_Position = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            vo_UV = aUV;
            vo_TBN = mat3(T, B, N);
        }";

      #endregion

      #region fragment

      // language=glsl
      var fragment = @"
        #version 330 core
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vo_Position;
        in vec2 vo_UV;
        in mat3 vo_TBN;
        in vec3 vo_CameraPosition;
        
        out vec4 color;

        uniform vec3 uLightPosition;
        // uniform vec3 uCameraPosition;
        
        uniform sampler2D uAlbedoTexture;
        uniform sampler2D uNormalTexture;
        uniform sampler2D uRoughnessTexture;
        uniform sampler2D uMetallicTexture;
        
        uniform samplerCube uSkybox;

        //const float PI = 3.1415;
        const float PI = 3.141592653589793;
        
        struct Material {
            vec3 albedo;
            vec3 normal;
            vec3 softNormal;
            float roughness;
            bool isMetallic;
            float alpha;
        };
        
        struct Light {
            bool isDirection;
            vec3 vector;
            float intensity;
            vec3 color;  
        };

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
        
        vec3 calcPbr(Material mat, Light light) {
            if (!light.isDirection) {
                vec3 l = light.vector - vo_Position;
                float dist = length(l);
                l = normalize(l);
                light.intensity /= (dist * dist);
                light.vector = l;
            }
            
            // Vectors
            vec3 viewDirection = normalize(vo_CameraPosition - vo_Position);
            vec3 halfDir = normalize(light.vector + viewDirection);
            
            // lightDirection = reflect(-viewDirection, normal);
            // lightIntensity = textureLod(uSkybox, lightDirection, 1.0).rgb * (1.0 - roughness);
            
            // Dots
            /*float softNDotL = dot(mat.softNormal, light.vector);
            if (softNDotL < 0.1) {
                softNDotL = mix(0.1, 0.0, (0.1 - softNDotL) / (0.1 + 1.0));
            }
            mat.normal = mix(mat.normal, mat.softNormal, 1.0 - softNDotL);*/
            
            float nDotL = dot(mat.normal, light.vector);
            if (nDotL < 0.1) {
                nDotL = mix(0.1, 0.0, (0.1 - nDotL) / (0.1 + 1.0));
            }  
            
            
            float nDotH = max(dot(mat.normal, halfDir), 0.001);
            float vDotH = max(dot(viewDirection, halfDir), 0.001);
            float nDotV = max(dot(mat.normal, viewDirection), 0.001);

            // if (nDotL <= 0.05) nDotL = 0.05;
            
            // Reflection
            /*vec3 reflectedDir = reflect(viewDirection, normal);
            reflectedDir.y *= -1.0;
            reflectedDir.x *= -1.0;
            vec3 reflectedColor = textureLod(uSkybox, reflectedDir, roughness * 32.0).rgb;*/
            
            float sex = remap(mat.roughness, 0.0, 1.0, 0.0, 4.0);
            vec3 reflectedColor = blurReflection(viewDirection, mat.normal, sex * 0.02);

            // PBR
            vec3 FO = vec3(0.04);
            
            if (mat.isMetallic) {
                FO = mat.albedo;
            }
            
            vec3 F = schlickFresnel(FO, vDotH);
            vec3 kS = F;
            vec3 kD = vec3(1.0) - kS;
            
            vec3 specBRDF_nom = ggxDistribution(mat.roughness, nDotH) * 
                F * 
                geomSmith(mat.roughness, nDotL) * 
                geomSmith(mat.roughness, nDotV);
            float specBRDF_denom = 4.0 * nDotV * nDotL;
            vec3 specBRDF = (specBRDF_nom / max(specBRDF_denom, 0.00001));
            
            vec3 fLambert = vec3(0.0);
            if (!mat.isMetallic) {
                fLambert = mat.albedo;
            }
            vec3 diffuseBRDF = kD * fLambert / PI;
            
            // Reflection
            if (!mat.isMetallic) {
              reflectedColor *= pow(1.0 - mat.roughness, 3.0) * 0.65;
              float cosTheta = dot(mat.normal, viewDirection);
              float fresnelFactor = pow(1.0 - abs(cosTheta), 5.0);
              if (fresnelFactor < 0.2) fresnelFactor = 0.2;
              reflectedColor *= fresnelFactor;
            } else {
              float cosTheta = dot(mat.normal, viewDirection);
              float fresnelFactor = pow(1.0 - abs(cosTheta), (mat.roughness) * 4.0 + 1.0);
              if (fresnelFactor < 0.05) fresnelFactor = 0.05;
              reflectedColor *= fresnelFactor;
            }

            float xxx = remap(mat.roughness, 0.0, 1.0, 0.0, 2.0);
            if (xxx > 1.0) xxx = 1.0;
            
            // specBRDF += vec3(1.0, 0.0, 0.0) * ((1.0 - nDotL) * 0.25);
            
            vec3 lightColor = light.color * light.intensity;
            vec3 finalColor = (diffuseBRDF + specBRDF + reflectedColor) * lightColor * nDotL;
            
            return finalColor;
        }
        
        Material getMaterial() {
            Material mat;
            
            // Read albedo
            vec4 texelColor = texture(uAlbedoTexture, vo_UV);
            if (texelColor.a <= 0.01) discard;
            mat.albedo = sRGBToLinear(texelColor.rgb);
            mat.alpha = texelColor.a;
            
            // Read normal
            vec3 normal = texture(uNormalTexture, vo_UV).xyz * 2.0 - 1.0;
            mat.normal = normalize(vo_TBN * normal);
            vec3 softNormal = textureLod(uNormalTexture, vo_UV, 10.0).xyz * 2.0 - 1.0;
            mat.softNormal = normalize(vo_TBN * softNormal);
            
            // Roughness
            float roughness = texture(uRoughnessTexture, vo_UV).r;
            if (roughness < 0.06) roughness = 0.06;
            mat.roughness = roughness;
            
            // Metallic
            mat.isMetallic = texture(uMetallicTexture, vo_UV).r >= 0.5;
            
            return mat;
        }
        
        Light createLight(vec3 pos, vec3 color, float intensity, bool isDir) {
            Light light;
            
            light.intensity = intensity;
            light.isDirection = isDir;
            if (isDir) light.vector = normalize(pos);
            else light.vector = pos;
            light.color = color;
            
            return light;
        }
        
        void main()
        {
            Material mat = getMaterial();

            Light light1 = createLight(vec3(0.0, 1.0, 1.0), vec3(1.0), 0.5, true);
            //Light light2 = createLight(vec3(-1.0, 1.0, 1.0), vec3(1.0), 0.5, true);
            //Light light3 = createLight(vec3(0.0, 1.0, 1.0), vec3(1.0), 1.5, true);
            
            vec3 finalColor = vec3(0.0);
            finalColor += calcPbr(mat, light1);
            //finalColor += calcPbr(mat, light2);
            //finalColor += calcPbr(mat, light3);
            
            // HDR
            finalColor = finalColor / (finalColor + vec3(1.0));
            
            // Gamma
            finalColor = pow(finalColor, vec3(1.0/2.2));
            
            // finalColor += uAlbedoTexture;
            
            color = vec4(finalColor, mat.alpha);
        }";

      #endregion

      Shader.ShaderCode["vertex"] = vertex;
      Shader.ShaderCode["fragment"] = fragment;
      Shader.Compile();
    }

    public override void Render()
    {
      var layer = (RL_StaticMesh)Layer;

      Shader.Use();
      Shader.Enable(OpenGL32.GL_BLEND);
      Shader.Enable(OpenGL32.GL_DEPTH_TEST);

      var cp = Scene.Camera.Position;
      cp.Z *= -1;
      // cp.Y *= -1;
      // Shader.SetUniform("uCameraPosition", cp);
      Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
      Shader.ActivateTexture(Scene.Skybox, "uSkybox", 10);

      // Draw each mesh
      layer.ForEach<RO_Mesh>(mesh =>
      {
        Context.MapObject(mesh);

        // Bind vao
        OpenGL32.glBindVertexArray(Context.GetVaoId(mesh));

        // Buffer
        Shader.EnableAttribute(mesh.VertexList, "aPosition");
        Shader.EnableAttribute(mesh.NormalList, "aNormal");
        Shader.EnableAttribute(mesh.UV0List, "aUV");
        Shader.EnableAttribute(mesh.TangentList, "aTangent");
        Shader.EnableAttribute(mesh.BiTangentList, "aBiTangent");

        // Texture
        Shader.ActivateTexture(mesh.AlbedoTexture, "uAlbedoTexture", 0);
        Shader.ActivateTexture(mesh.NormalTexture, "uNormalTexture", 1);
        Shader.ActivateTexture(mesh.RoughnessTexture, "uRoughnessTexture", 2);
        Shader.ActivateTexture(mesh.MetallicTexture, "uMetallicTexture", 3);

        Shader.SetUniform("uModelMatrix", mesh.Transform.Matrix);

        // Bind indices
        OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(mesh.IndexList));

        // Draw
        OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

        // Unbind vao
        OpenGL32.glBindVertexArray(0);
      });
    }
  }
}