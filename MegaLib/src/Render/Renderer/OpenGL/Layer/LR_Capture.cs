using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Capture : LR_Base
{
  public OpenGL_Framebuffer Framebuffer;

  public LR_Capture(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
    Framebuffer = context.CreateFrameBuffer();

    //OpenGL32.glGenFramebuffers(1, ref fbo);
    //OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, fbo);

    // Создаем текстуру для цветного буфера
    /*uint textureColorbuffer = 0;
    OpenGL32.glGenTextures(1, ref textureColorbuffer);
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, textureColorbuffer);
    OpenGL32.glTexImage2D(OpenGL32.GL_TEXTURE_2D,
      0, (GLint)OpenGL32.GL_RGB, 1280, 720, 0, OpenGL32.GL_RGB, OpenGL32.GL_UNSIGNED_BYTE, 0);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MIN_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glTexParameteri(OpenGL32.GL_TEXTURE_2D, OpenGL32.GL_TEXTURE_MAG_FILTER, (GLint)OpenGL32.GL_LINEAR);
    OpenGL32.glBindTexture(OpenGL32.GL_TEXTURE_2D, 0);

    Console.WriteLine(textureColorbuffer);*/

    /*var ll = layer as Layer_Capture;

    ll.Texture = new Texture_2D<RGB<byte>>(1280, 720);
    Context.MapRenderTexture(ll.Texture);

    // Прикрепляем текстуру к фреймбуферу
    var tid = Context.GetTextureId(ll.Texture);
    Console.WriteLine(tid);
    OpenGL32.glFramebufferTexture2D(
      OpenGL32.GL_FRAMEBUFFER,
      OpenGL32.GL_COLOR_ATTACHMENT0, OpenGL32.GL_TEXTURE_2D, tid, 0);

    OpenGL32.PrintGlError();

    // Создаем рендербуфер для глубины и трафарета
    uint rbo = 0;
    OpenGL32.glGenRenderbuffers(1, ref rbo);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, rbo);
    OpenGL32.glRenderbufferStorage(OpenGL32.GL_RENDERBUFFER, OpenGL32.GL_DEPTH24_STENCIL8, 1280, 720);
    OpenGL32.glBindRenderbuffer(OpenGL32.GL_RENDERBUFFER, 0);

    OpenGL32.PrintGlError();

    // Прикрепляем рендербуфер к фреймбуферу
    OpenGL32.glFramebufferRenderbuffer(OpenGL32.GL_FRAMEBUFFER, OpenGL32.GL_DEPTH_STENCIL_ATTACHMENT,
      OpenGL32.GL_RENDERBUFFER, rbo);
    OpenGL32.PrintGlError();

    // Проверяем фреймбуфер на корректность
    var status = OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER);
    if (OpenGL32.glCheckFramebufferStatus(OpenGL32.GL_FRAMEBUFFER) != OpenGL32.GL_FRAMEBUFFER_COMPLETE)
      throw new Exception($"ERROR::FRAMEBUFFER:: Framebuffer is not complete! {status}");

    OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, 0);*/
  }

  public override void Init()
  {
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
        layout (location = 5) in vec4 aBoneWeight;
        layout (location = 6) in uint aBoneIndex;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform sampler2D uBoneMatrix;
        
        out vec3 vo_Position;
        out vec3 vo_CameraPosition;
        out vec2 vo_UV;
        out mat3 vo_TBN;
        out float vo_Depth;
        
        ivec2 getBoneTexelById(uint id, int ch) {
            int pixel = (int(id) * 16 + ch);
            return ivec2(pixel % 64, pixel / 64);
        }

        uvec4 unpackIndex(uint id) {
            uint r = uint((id >> 24u) & 0xFFu);
            uint g = uint((id >> 16u) & 0xFFu);
            uint b = uint((id >> 8u) & 0xFFu);
            uint a = uint(id & 0xFFu);
            return uvec4(r, g, b, a);
        }

        mat4 getInverseBindMatrix(uint id) {
            vec4 m1 = texelFetch(uBoneMatrix, getBoneTexelById(id, 0), 0);
            vec4 m2 = texelFetch(uBoneMatrix, getBoneTexelById(id, 1), 0);
            vec4 m3 = texelFetch(uBoneMatrix, getBoneTexelById(id, 2), 0);
          
            vec4 m5 = texelFetch(uBoneMatrix, getBoneTexelById(id, 4), 0);
            vec4 m6 = texelFetch(uBoneMatrix, getBoneTexelById(id, 5), 0);
            vec4 m7 = texelFetch(uBoneMatrix, getBoneTexelById(id, 6), 0);
          
            vec4 m9 = texelFetch(uBoneMatrix, getBoneTexelById(id, 8), 0);
            vec4 m10 = texelFetch(uBoneMatrix, getBoneTexelById(id, 9), 0);
            vec4 m11 = texelFetch(uBoneMatrix, getBoneTexelById(id, 10), 0);
          
            vec4 m13 = texelFetch(uBoneMatrix, getBoneTexelById(id, 12), 0);
            vec4 m14 = texelFetch(uBoneMatrix, getBoneTexelById(id, 13), 0);
            vec4 m15 = texelFetch(uBoneMatrix, getBoneTexelById(id, 14), 0);
          
            return mat4(
                m1.r, m2.r, m3.r, 0.0,
                m5.r, m6.r, m7.r, 0.0,
                m9.r, m10.r, m11.r, 0.0,
                m13.r, m14.r, m15.r, 1.0
            );
        }
        
        mat4 identity() {
           return mat4(
               1.0, 0.0, 0.0, 0.0,
               0.0, 1.0, 0.0, 0.0,
               0.0, 0.0, 1.0, 0.0,
               0.0, 0.0, 0.0, 1.0
           );
        }
        
        void main() {
            // Apply bone matrix
            uvec4 boneIndex = unpackIndex(aBoneIndex);
            mat4 bone1Matrix = getInverseBindMatrix(boneIndex.r);
            mat4 bone2Matrix = getInverseBindMatrix(boneIndex.g);
            mat4 bone3Matrix = getInverseBindMatrix(boneIndex.b);
            mat4 bone4Matrix = getInverseBindMatrix(boneIndex.a);
            
            // Make skin matrix
            mat4 skinMatrix = aBoneWeight.r * bone1Matrix +
            aBoneWeight.g * bone2Matrix +
            aBoneWeight.b * bone3Matrix +
            aBoneWeight.a * bone4Matrix;
            
            // Output
            gl_Position = uProjectionMatrix * uViewMatrix * skinMatrix * vec4(aPosition.xyz, 1.0);
            vo_Position = (skinMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            vo_UV = aUV;
            
            // TBN Matrix
            mat4 modelMatrix = skinMatrix;
            vec3 T = normalize(vec3(modelMatrix * vec4(aTangent,   0.0)));
            vec3 B = normalize(vec3(modelMatrix * vec4(aBiTangent, 0.0)));
            vec3 N = normalize(vec3(modelMatrix * vec4(aNormal,    0.0)));
            vo_TBN = mat3(T, B, N);

            // Camera position
            vec4 cameraPosition = vec4(0.0, 0.0, 0.0, 1.0);
            vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).xyz;
            
            // Нормализованное значение глубины
            vo_Depth = gl_Position.z / gl_Position.w;
        }";

    // language=glsl
    var fragment = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vo_Position;
        in vec3 vo_CameraPosition;
        in vec2 vo_UV;
        in mat3 vo_TBN;
        in float vo_Depth;
        
        out vec4 color;

        uniform vec3 uLightPosition;
        uniform vec3 uCameraPosition;
        
        uniform sampler2D uAlbedoTexture;
        uniform sampler2D uNormalTexture;
        uniform sampler2D uRoughnessTexture;
        uniform sampler2D uMetallicTexture;
        uniform sampler2D uBoneMatrix;
        
        uniform samplerCube uSkybox;
            
        uniform vec4 uTint;

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
        
        vec3 subsurfaceScatteringA(vec3 N, vec3 L, vec3 V, vec3 albedo, vec3 lightColor, vec3 sssColor, float roughness) {
            float cosTheta = max(dot(N, L), 0.0);
            float scattering = exp(-roughness / (cosTheta + 0.01));
            vec3 scatter = sssColor * scattering * 0.5;
            return scatter;
        }

        // Function to calculate Subsurface Scattering
        vec3 subsurfaceScattering(vec3 N, vec3 L, vec3 V, vec3 albedo, vec3 lightColor, float roughness) {
            // Простой подход к расчету SSS
            float cosTheta = max(dot(N, L), 0.0);
            float scattering = exp(-roughness / cosTheta);
            
            // Параметры для SSS
            float scale = 0.5; // Параметр, регулирующий интенсивность SSS
            vec3 scatterColor = albedo * lightColor * scattering * scale;

            return scatterColor;
        }

        vec3 subsurfaceScattering(vec3 N, vec3 L, vec3 V, vec3 scatterColor, float roughness) {
            // Вычисляем косинус угла между нормалью и направлением света
            float cosTheta = max(dot(N, L), 0.0);
            
            // Примерная модель для интенсивности рассеяния
            float scattering = exp(-roughness / (cosTheta + 0.01)); // Добавляем небольшой смещение, чтобы избежать деления на ноль
            
            // Вычисляем цвет рассеяния
            vec3 scatter = scatterColor * scattering;

            return scatter;
        }

        vec3 subsurfaceScatteringChristensenBurley(vec3 N, vec3 L, vec3 V, vec3 albedo, vec3 lightColor, vec3 sssColor, float roughness, float thickness) {
            float NdotL = max(dot(N, L), 0.0);
            float NdotV = max(dot(N, V), 0.0);

            // Расчет профиля рассеивания
            float sigma_s = thickness * 0.25;
            float s = sigma_s / (1.0 - roughness);
            float S = exp(-s * (1.0 - NdotL));

            // Компонента рассеивания
            vec3 scatter = sssColor * S * (1.0 - NdotL);

            return scatter;
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
            if (nDotL < 0.0) nDotL = 0.0;
            /*vec3 gaifin = vec3(0.0, 0.0, 0.0);
            if (nDotL < 0.1) {
                nDotL = mix(0.1, 0.0, (0.1 - nDotL) / (0.1 + 1.0));
                gaifin = vec3(1.0, 0.0, 0.0) * (1.0 - nDotL);
            }*/

            //float nDotL = (dot(mat.normal, light.vector) + 1.0) * 0.5; // Shift and scale to range [0, 1]  
            
            
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
            
            //vec3 scatter = subsurfaceScatteringA(mat.normal, light.vector, viewDirection, mat.albedo, lightColor, vec3(1.0, 0.0, 0.0), mat.roughness);
            
            
            //vec3 L = light.vector;
            //L.z *= -1.0;
            vec3 scatter = subsurfaceScatteringChristensenBurley(
                mat.normal, 
                -light.vector,
                viewDirection, 
                mat.albedo, lightColor, vec3(1.0, 0.2, 0.05) * 0.02, mat.roughness, 0.0001);
   
            return finalColor + scatter;
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
        
        float LinearizeDepth(float depth) {
            float near = 0.01;
            float far = 1.0;
            float z = depth * 2.0 - 1.0; // Обратно преобразуем в NDC
            return (2.0 * near * far) / (far + near - z * (far - near));    
        }

        void main()
        {
            Material mat = getMaterial();

            Light light1 = createLight(vec3(0.0, 0.0, 1.0), vec3(1.0), 0.5, true);
            Light light2 = createLight(vec3(-1.0, 1.0, 1.0), vec3(1.0), 0.5, true);
            Light light3 = createLight(vec3(0.0, 1.0, 1.0), vec3(1.0), 1.5, true);
            
            vec3 finalColor = vec3(0.0);
            finalColor += calcPbr(mat, light1);
            finalColor += calcPbr(mat, light2);
            finalColor += calcPbr(mat, light3);
            
             // Subsurface Scattering approximation
            //vec3 V = normalize(vo_CameraPosition - vo_Position);
            // vec3 L = normalize(light1.vector - vo_Position);
            
            //vec3 scatter = subsurfaceScattering(mat.normal, light1.vector, V, mat.albedo, light1.color, mat.roughness);
            // finalColor += scatter;
            
            // HDR
            finalColor = finalColor / (finalColor + vec3(1.0));
            
            // Gamma
            finalColor = pow(finalColor, vec3(1.0 / 2.2));
            //finalColor *= 0.00001;
            //finalColor.r += texture(uBoneMatrix, vUV).r;
            
            float linearDepth = LinearizeDepth(vo_Depth) / 1.0;
            color = vec4(finalColor, mat.alpha) * uTint * 0.00001 + vec4(linearDepth, linearDepth, linearDepth, 1.0);
            // color = vec4(texture(uBoneMatrix, vUV).r, 0.0, 0.0, 1.0);
        }";

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();
  }

  public override void Render()
  {
    // OpenGL32.glBindFramebuffer(OpenGL32.GL_FRAMEBUFFER, fbo);
    // OpenGL32.glViewport(0, 0, 1280, 720);
    // OpenGL32.glClear(OpenGL32.GL_COLOR_BUFFER_BIT | OpenGL32.GL_DEPTH_BUFFER_BIT);
    // OpenGL32.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);

    Framebuffer.Bind();
    Framebuffer.Clear();

    var layer = (Layer_Capture)Layer;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    var cp = Scene.Camera.Position;
    cp.Z *= -1;

    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);

    layer.LayerNames.ForEach(name =>
    {
      var l = Scene.GetLayer<Layer_Base>(name);
      if (l is Layer_SkinnedMesh ls) Render_SkinnedMesh(ls);
    });

    Framebuffer.Unbind();
  }

  private void Render_SkinnedMesh(Layer_SkinnedMesh layer)
  {
    // Draw each skinned mesh
    layer.ForEach(skin =>
    {
      skin.Update();

      Context.MapTexture(skin.BoneTexture);
      Shader.ActivateTexture(skin.BoneTexture, "uBoneMatrix", 11);

      skin.MeshList.ForEach(mesh =>
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
        Shader.EnableAttribute(mesh.BoneWeightList, "aBoneWeight");
        Shader.EnableAttribute(mesh.BoneIndexList, "aBoneIndex");

        // Texture
        if (mesh.Material != null)
        {
          if (mesh.Material.AlbedoTexture != null)
            Shader.ActivateTexture(mesh.Material.AlbedoTexture, "uAlbedoTexture", 0);
          if (mesh.Material.NormalTexture != null)
            Shader.ActivateTexture(mesh.Material.NormalTexture, "uNormalTexture", 1);
          if (mesh.Material.RoughnessTexture != null)
            Shader.ActivateTexture(mesh.Material.RoughnessTexture, "uRoughnessTexture", 2);
          if (mesh.Material.MetallicTexture != null)
            Shader.ActivateTexture(mesh.Material.MetallicTexture, "uMetallicTexture", 3);
        }

        if (mesh.Material != null)
        {
          Shader.SetUniform("uTint", (Vector4)mesh.Material.Tint);
        }
        else
        {
          Shader.SetUniform("uTint", new Vector4(1, 1, 1, 1));
        }

        // Bind indices
        OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(mesh.IndexList));

        // Draw
        OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

        // Unbind vao
        OpenGL32.glBindVertexArray(0);
      });
    });
  }
}