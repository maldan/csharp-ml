using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class GLRL_StaticMesh : GLRL_Base
  {
    public GLRL_StaticMesh(Context_OpenGL context, RL_Base layer, Render_Scene scene) : base(context, layer, scene)
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
        // layout (location = 5) in uint aTriangleId;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform mat4 uModelMatrix;

        uniform vec3 uLightPosition;
        uniform vec3 uCameraPosition;
        // 
        
        out vec3 vo_Position;
        // out vec3 vo_CameraPosition;
        out vec2 vo_UV;
        out mat3 vo_TBN;
        out mat4 vo_Matrix;
        
        out vec3 vo_Normal;
        out vec3 vo_Tangent;
        out vec3 vo_BiTangent;
        //flat out uint vo_TriangleId;
        
        
        
        void main() {
            //ivec2 xx = ivec2(floor(aUV.x * 128.0), floor(aUV.y * 128.0));
            //vec3 pos = texelFetch(uPositionTexture, xx, 0).rgb * 3.0 - 1.5;
            //vec3 pos = texture(uPositionTexture, aUV).rgb * 3.0 - 1.5;

            //gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);

            // gl_Position += vec4(aTriangleId) * 0.00000000001;
            //vPosition = (uModelMatrix * vec4(pos.xyz, 1.0)).xyz;
            
            
            
            // Camera position
            //vec4 cameraPosition = vec4(0.0, 0.0, 0.0, 1.0);
            
            // Out
            vo_Position = aPosition;
            vo_UV = aUV;
            //vo_TBN = mat3(T, B, N);
            vo_Matrix = uProjectionMatrix * uViewMatrix * uModelMatrix;
            
            vo_Normal = aNormal;
            vo_Tangent = aTangent;
            vo_BiTangent = aBiTangent;
            
            //vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).xyz;
            // vo_TriangleId = uint(gl_VertexID / 3);
        }";

      #endregion

      #region geometry

      // language=glsl
      var geometry = @"#version 450 core
        
      layout (triangles) in;
      layout (triangle_strip, max_vertices = 3 * 16) out;
        
      in vec3 vo_Position[];
      in vec2 vo_UV[];
      in mat3 vo_TBN[];
      in mat4 vo_Matrix[];
      
      in vec3 vo_Normal[];
      in vec3 vo_Tangent[];
      in vec3 vo_BiTangent[];
      
      out vec3 go_Position;
      out vec2 go_UV;
      out mat3 go_TBN;
      
      uniform sampler2D uPositionTexture;
      uniform vec3 uCameraPosition;
      
      mat4 identity() {
         return mat4(
             1.0, 0.0, 0.0, 0.0,
             0.0, 1.0, 0.0, 0.0,
             0.0, 0.0, 1.0, 0.0,
             0.0, 0.0, 0.0, 1.0
         );
      }
        
      ivec2 getTexelPositionById(uint id, int ch) {
        int pixel = (int(id) * 1 + ch);
        return ivec2(pixel % 64, pixel / 64);
      }
      
      float remap(float value, float fromMin, float fromMax, float toMin, float toMax) {
        float normalizedValue = (value - fromMin) / (fromMax - fromMin);
        return mix(toMin, toMax, normalizedValue);
      }
      
      mat3 getTBN(vec3 a, vec3 b, vec3 c, vec3 tangent, vec3 biTangent) {
        // Вычисление векторов сторон треугольника
        vec3 side1 = b.xyz - a.xyz;
        vec3 side2 = c.xyz - a.xyz;
        
        // Вычисление нормали как векторного произведения сторон треугольника
        vec3 normal = normalize(cross(side1, side2));
    
        // TBN Matrix
        mat4 modelMatrix = identity(); //uModelMatrix;
        vec3 T = normalize(vec3(modelMatrix * vec4(tangent,   1.0)));
        vec3 B = normalize(vec3(modelMatrix * vec4(biTangent, 1.0)));
        vec3 N = normalize(vec3(modelMatrix * vec4(normal,    1.0)));
        return mat3(T, B, N);
      }

      mat3 getTBN(vec3 tangent, vec3 biTangent, vec3 normal) {
        // TBN Matrix
        mat4 modelMatrix = identity(); //uModelMatrix;
        vec3 T = normalize(vec3(modelMatrix * vec4(tangent,   1.0)));
        vec3 B = normalize(vec3(modelMatrix * vec4(biTangent, 1.0)));
        vec3 N = normalize(vec3(modelMatrix * vec4(normal,    1.0)));
        return mat3(T, B, N);
      }

      void original() {
        mat3 TBN = getTBN(vo_Tangent[0], vo_BiTangent[0], vo_Normal[0]);
        
        for (int i = 0; i < gl_in.length(); ++i) {
          gl_Position = vo_Matrix[0] * vec4(vo_Position[i].xyz, 1.0);
          go_Position = gl_Position.xyz;
          go_UV = vo_UV[i];
          go_TBN = TBN;
          
          EmitVertex();
        }
        EndPrimitive();
      }
      
      void subdiv() {
        // Get offset
        ivec2 offsetPos = getTexelPositionById(gl_PrimitiveIDIn, 0);
        vec4 sasa = texelFetch(uPositionTexture, offsetPos, 0);
        uvec4 sasax = uvec4(sasa.r * 255, sasa.g * 255, sasa.b * 255, sasa.a * 255);
        sasax.r = (sasax.g << 8) | sasax.r;
        
        // Read amount of vertices
        ivec2 dataPos = getTexelPositionById(sasax.r, 0);
        uint amount = uint(texelFetch(uPositionTexture, dataPos, 0).r * 255);

        // Read vertices
        for (int i = 0; i < amount; i += 3) {
            // Read triangle
            dataPos = getTexelPositionById(sasax.r + 1 + i + 0, 0);
            vec3 v = texelFetch(uPositionTexture, dataPos, 0).rgb;
            vec3 a = vec3(remap(v.r, 0.0, 1.0, -1.5, 1.5), remap(v.g, 0.0, 1.0, -1.5, 1.5), remap(v.b, 0.0, 1.0, -1.5, 1.5));
            
            dataPos = getTexelPositionById(sasax.r + 1 + i + 1, 0);
            v = texelFetch(uPositionTexture, dataPos, 0).rgb;
            vec3 b = vec3(remap(v.r, 0.0, 1.0, -1.5, 1.5), remap(v.g, 0.0, 1.0, -1.5, 1.5), remap(v.b, 0.0, 1.0, -1.5, 1.5));
            
            dataPos = getTexelPositionById(sasax.r + 1 + i + 2, 0);
            v = texelFetch(uPositionTexture, dataPos, 0).rgb;
            vec3 c = vec3(remap(v.r, 0.0, 1.0, -1.5, 1.5), remap(v.g, 0.0, 1.0, -1.5, 1.5), remap(v.b, 0.0, 1.0, -1.5, 1.5));
            
            // A
            gl_Position = vo_Matrix[0] * vec4(a, 1.0);
            go_Position = gl_Position.xyz;
            go_UV = vo_UV[0];
            go_TBN = getTBN(a, b, c, vo_Tangent[0], vo_BiTangent[0]);
            EmitVertex();
            
            // B
            gl_Position = vo_Matrix[0] * vec4(b, 1.0);
            go_Position = gl_Position.xyz;
            go_UV = vo_UV[0];
            go_TBN = getTBN(a, b, c, vo_Tangent[1], vo_BiTangent[1]);
            EmitVertex();
            
            // C
            gl_Position = vo_Matrix[0] * vec4(c, 1.0);
            go_Position = gl_Position.xyz;
            go_UV = vo_UV[0];
            go_TBN = getTBN(a, b, c, vo_Tangent[2], vo_BiTangent[2]);
            EmitVertex();
            
            EndPrimitive();
        }
      }
        
      void main() {
        if (uCameraPosition.z < -3.0) original();
        else subdiv();
      }";

      #endregion

      #region fragment

      // language=glsl
      var fragment = @"
        #version 330 core
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 go_Position;
        //in vec3 go_CameraPosition;
        in vec2 go_UV;
        in mat3 go_TBN;
        //flat in uint go_TriangleId;
        //flat in uvec4 go_Sex;
        
        out vec4 color;

        uniform vec3 uLightPosition;
        uniform vec3 uCameraPosition;
        
        uniform sampler2D uTextureColor;
        uniform sampler2D uNormalColor;
        uniform sampler2D uRoughnessColor;
        uniform sampler2D uMetallicColor;
        uniform sampler2D uPositionTexture;
        
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
                vec3 l = light.vector - go_Position;
                float dist = length(l);
                l = normalize(l);
                light.intensity /= (dist * dist);
                light.vector = l;
            }
            
            // Vectors
            vec3 viewDirection = normalize(uCameraPosition - go_Position);
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
            vec4 texelColor = texture(uTextureColor, go_UV);
            if (texelColor.a <= 0.01) discard;
            mat.albedo = sRGBToLinear(texelColor.rgb);
            mat.alpha = texelColor.a;
            
            // Read normal
            vec3 normal = texture(uNormalColor, go_UV).xyz * 2.0 - 1.0;
            mat.normal = normalize(go_TBN * normal);
            vec3 softNormal = textureLod(uNormalColor, go_UV, 10.0).xyz * 2.0 - 1.0;
            mat.softNormal = normalize(go_TBN * softNormal);
            
            // Roughness
            float roughness = texture(uRoughnessColor, go_UV).r;
            if (roughness < 0.06) roughness = 0.06;
            mat.roughness = roughness;
            
            // Metallic
            mat.isMetallic = texture(uMetallicColor, go_UV).r >= 0.5;
            
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
            
            
                
            //finalColor.rgb *= 0.0001;
            //finalColor.rgb += texture(uPositionTexture, vUV).rgb;
            
            color = vec4(finalColor, mat.alpha);
        }";

      #endregion

      Context.CreateShader(Layer.Name, new Dictionary<string, string>
      {
        { "vertex", vertex },
        { "geometry", geometry },
        { "fragment", fragment }
      });
    }

    public override void Render()
    {
      var layer = (RL_StaticMesh)Layer;

      // User shader line
      Context.UseProgram(layer.Name);

      // gl blend fn
      OpenGL32.glEnable(OpenGL32.GL_BLEND);
      OpenGL32.glBlendFunc(OpenGL32.GL_SRC_ALPHA, OpenGL32.GL_ONE_MINUS_SRC_ALPHA);

      // bind camera
      // var cp = _scene.Camera.Position.Inverted;
      //var cp = new Vector4(0, 0, 0, 1);
      //cp *= Scene.Camera.ViewMatrix;
      // Console.WriteLine(Scene.Camera.Position);
      Context.BindVector(layer.Name, "uCameraPosition", Scene.Camera.Position);
      // _context.BindVector(layer.Name, "uLightPosition", new Vector3(0, 0, -2));
      Context.BindMatrix(layer.Name, "uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Context.BindMatrix(layer.Name, "uViewMatrix", Scene.Camera.ViewMatrix);

      Context.ActivateCubeTexture(layer.Name, "uSkybox", "main", 10);

      // gl enable depth test
      OpenGL32.glEnable(OpenGL32.GL_DEPTH_TEST);
      OpenGL32.glDepthFunc(OpenGL32.GL_LEQUAL);

      // Draw each mesh
      layer.ForEach<RO_Mesh>(mesh =>
      {
        var objectInfo = Context.GetObjectInfo(mesh.Id);
        if (objectInfo == null)
        {
          // Define object
          objectInfo = new ObjectInfo_OpenGL(Context)
          {
            // Id
            MeshId = mesh.Id,

            VaoName = $"{layer.Name}_{mesh.Id}",

            VertexBufferName = $"{layer.Name}_{mesh.Id}.vertex",
            UV0BufferName = $"{layer.Name}_{mesh.Id}.uv0",
            NormalBufferName = $"{layer.Name}_{mesh.Id}.normal",

            TangentBufferName = $"{layer.Name}_{mesh.Id}.tangent",
            BiTangentBufferName = $"{layer.Name}_{mesh.Id}.biTangent",

            // TriangleIdBufferName = $"{layer.Name}_{mesh.Id}.triangleId",

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

            //$"{objectInfo.TriangleIdBufferName} -> aTriangleId:uint",
          };

          // Create vao
          Context.CreateVAO(objectInfo.VaoName);

          // Create buffers
          Context.CreateBuffer(objectInfo.VertexBufferName);
          Context.CreateBuffer(objectInfo.UV0BufferName);
          Context.CreateBuffer(objectInfo.NormalBufferName);
          Context.CreateBuffer(objectInfo.TangentBufferName);
          Context.CreateBuffer(objectInfo.BiTangentBufferName);
          // Context.CreateBuffer(objectInfo.TriangleIdBufferName);
          Context.CreateBuffer(objectInfo.IndexBufferName);

          // gl upload buffers
          Context.UploadBuffer(objectInfo.VertexBufferName, mesh.GpuVertexList);
          Context.UploadBuffer(objectInfo.UV0BufferName, mesh.GpuUVList);
          Context.UploadBuffer(objectInfo.NormalBufferName, mesh.GpuNormalList);
          Context.UploadBuffer(objectInfo.TangentBufferName, mesh.GpuTangentList);
          Context.UploadBuffer(objectInfo.BiTangentBufferName, mesh.GpuBiTangentList);
          //Context.UploadBuffer(objectInfo.TriangleIdBufferName, mesh.GpuTriangleId);
          Context.UploadElementBuffer(objectInfo.IndexBufferName, mesh.GpuIndexList);
          //Console.WriteLine("G " + mesh.GpuTriangleId.Length);
          // _context.BindMatrix(layer.Name, "uModelMatrix", mesh.Transform.Matrix);

          // Mesh has texture
          /*if (mesh.Texture != null)
          {
            objectInfo.TextureId = mesh.Texture.Id;
            objectInfo.TextureName = $"{layer.Name}_{mesh.Texture.Id}.texture";
            _context.CreateTexture(objectInfo.TextureName, mesh.Texture.GPU_RAW, mesh.Texture.Options);
          }

          if (mesh.NormalTexture != null)
          {
            objectInfo.NormalTextureId = mesh.NormalTexture.Id;
            objectInfo.NormalTextureName = $"{layer.Name}_{mesh.NormalTexture.Id}.normalTexture";
            _context.CreateTexture(objectInfo.NormalTextureName, mesh.NormalTexture.GPU_RAW,
              mesh.NormalTexture.Options);
          }

          if (mesh.RoughnessTexture != null)
          {
            _context.CreateTexture($"{layer.Name}_{mesh.Texture.Id}.roughnessTexture", mesh.RoughnessTexture.GPU_RAW,
              mesh.RoughnessTexture.Options);
            objectInfo.RoughnessTextureName = $"{layer.Name}_{mesh.Texture.Id}.roughnessTexture";
          }

          if (mesh.MetallicTexture != null)
          {
            _context.CreateTexture($"{layer.Name}_{mesh.Texture.Id}.metallicTexture", mesh.MetallicTexture.GPU_RAW,
              mesh.MetallicTexture.Options);
            objectInfo.MetallicTextureName = $"{layer.Name}_{mesh.Texture.Id}.metallicTexture";
          }*/

          // Set object
          Context.SetObjectInfo(mesh.Id, objectInfo);
        }

        if (mesh.Texture != null)
        {
          if (objectInfo.TextureId != mesh.Texture.Id)
          {
            // Remove old
            if (!string.IsNullOrEmpty(objectInfo.TextureName)) Context.DeleteTexture(objectInfo.TextureName);

            // Create new
            objectInfo.TextureId = mesh.Texture.Id;
            objectInfo.TextureName = $"{layer.Name}_{mesh.Texture.Id}.texture";
            Context.CreateTexture(objectInfo.TextureName, mesh.Texture.GPU_RAW, mesh.Texture.Options);
          }
        }

        if (mesh.NormalTexture != null)
        {
          if (objectInfo.NormalTextureId != mesh.NormalTexture.Id)
          {
            // Remove old
            if (!string.IsNullOrEmpty(objectInfo.NormalTextureName))
              Context.DeleteTexture(objectInfo.NormalTextureName);

            // Create new
            objectInfo.NormalTextureId = mesh.NormalTexture.Id;
            objectInfo.NormalTextureName = $"{layer.Name}_{mesh.NormalTexture.Id}.normalTexture";
            Context.CreateTexture(objectInfo.NormalTextureName, mesh.NormalTexture.GPU_RAW,
              mesh.NormalTexture.Options);
          }
        }

        if (mesh.RoughnessTexture != null)
        {
          if (objectInfo.RoughnessTextureId != mesh.RoughnessTexture.Id)
          {
            // Remove old
            if (!string.IsNullOrEmpty(objectInfo.RoughnessTextureName))
              Context.DeleteTexture(objectInfo.RoughnessTextureName);

            // Create new
            objectInfo.RoughnessTextureId = mesh.RoughnessTexture.Id;
            objectInfo.RoughnessTextureName = $"{layer.Name}_{mesh.RoughnessTexture.Id}.roughnessTexture";
            Context.CreateTexture(objectInfo.RoughnessTextureName, mesh.RoughnessTexture.GPU_RAW,
              mesh.RoughnessTexture.Options);
          }
        }

        if (mesh.MetallicTexture != null)
        {
          if (objectInfo.MetallicTextureId != mesh.MetallicTexture.Id)
          {
            // Remove old
            if (!string.IsNullOrEmpty(objectInfo.MetallicTextureName))
              Context.DeleteTexture(objectInfo.MetallicTextureName);

            // Create new
            objectInfo.MetallicTextureId = mesh.MetallicTexture.Id;
            objectInfo.MetallicTextureName = $"{layer.Name}_{mesh.MetallicTexture.Id}.metallicTexture";
            Context.CreateTexture(
              objectInfo.MetallicTextureName,
              mesh.MetallicTexture.GPU_RAW,
              mesh.MetallicTexture.Options);
          }
        }

        if (mesh.PositionTexture != null)
        {
          if (objectInfo.PositionTextureId != mesh.PositionTexture.Id)
          {
            // Remove old
            if (!string.IsNullOrEmpty(objectInfo.PositionTextureName))
              Context.DeleteTexture(objectInfo.PositionTextureName);

            // Create new
            objectInfo.PositionTextureId = mesh.PositionTexture.Id;
            objectInfo.PositionTextureName = $"{layer.Name}_{mesh.PositionTexture.Id}.positionTexture";
            Context.CreateTexture(objectInfo.PositionTextureName, mesh.PositionTexture.GPU_RAW,
              mesh.PositionTexture.Options);
          }
        }

        // gl draw arrays
        objectInfo.DrawElements();
      });

      // Unbind
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, 0);
    }
  }
}