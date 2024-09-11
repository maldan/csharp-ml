using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Mesh : LR_Base
{
  public LR_Mesh(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
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
            // Применяем модельную и видовую матрицы для позиции
            mat4 modelViewMatrix = uViewMatrix * uModelMatrix;
            mat4 projectionMatrix = uProjectionMatrix;
            
            // Применяем модифицированную матрицу проекции
            gl_Position = projectionMatrix * modelViewMatrix * vec4(aPosition.xyz, 1.0);
            
            // Преобразуем нормали с использованием матрицы модели
            mat3 normalMatrix = transpose(inverse(mat3(uModelMatrix)));
            
            // Рассчитываем TBN матрицу (Tangent, Bitangent, Normal)
            vec3 T = normalize(normalMatrix * aTangent);   // Преобразуем тангенс
            vec3 B = normalize(normalMatrix * aBiTangent); // Преобразуем битангенс
            vec3 N = normalize(normalMatrix * aNormal);    // Преобразуем нормаль
            
            // Передаём нормализованную TBN-матрицу в фрагментный шейдер
            vo_TBN = mat3(T, B, N);
            
            // Позиция камеры в мировых координатах
            vec4 cameraPosition = vec4(0.0, 0.0, 0.0, 1.0);
            vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).xyz;
            
            // Преобразование позиции вершины в мировые координаты
            vo_Position = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            
            // Передаём текстурные координаты
            vo_UV = aUV;
        }";

    #endregion

    #region fragment

    // language=glsl
    var pbrFunctions = @"
        vec3 fresnelSchlick(float cosTheta, vec3 F0)
        {
            return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
        }

        float DistributionGGX(vec3 N, vec3 H, float roughness)
        {
            float a = roughness * roughness;
            float a2 = a * a;
            float NdotH = max(dot(N, H), 0.0);
            float NdotH2 = NdotH * NdotH;

            float num = a2;
            float denom = (NdotH2 * (a2 - 1.0) + 1.0);
            denom = PI * denom * denom;

            return num / denom;
        }

        float GeometrySchlickGGX(float NdotV, float roughness)
        {
            float r = (roughness + 1.0);
            float k = (r * r) / 8.0;

            float num = NdotV;
            float denom = NdotV * (1.0 - k) + k;

            return num / denom;
        }

        float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
        {
            float NdotV = max(dot(N, V), 0.0);
            float NdotL = max(dot(N, L), 0.0);
            float ggx2 = GeometrySchlickGGX(NdotV, roughness);
            float ggx1 = GeometrySchlickGGX(NdotL, roughness);

            return ggx1 * ggx2;
        }
        
        vec3 PBR(Material mat, Light light) {
            // Нормализуем входные данные
            vec3 N = normalize(mat.normal);  // Нормаль к поверхности
            vec3 V = normalize(vo_CameraPosition - vo_Position);  // Направление к камере

            // Определяем тип источника света
            vec3 L;
            float attenuation = 1.0;

            if (light.isDirection) {
                // Направленный свет
                L = normalize(light.vector);  // Направление света, инвертированное
            } else {
                // Точечный свет (Point Light)
                L = normalize(light.vector - vo_Position);  // Направление к источнику света

                // Рассчитываем расстояние от источника света до объекта
                float distance = length(light.vector - vo_Position);

                // Проверяем, находится ли объект внутри радиуса действия света
                if (distance < light.radius) {
                    // Модель затухания с учётом радиуса
                    attenuation = light.intensity * (1.0 - distance / light.radius) / (distance * distance);
                } else {
                    attenuation = 0.0;  // Если объект за пределами радиуса, свет не воздействует
                }
            }

            vec3 H = normalize(V + L);  // Полу-угловой вектор

            // Диффузное освещение (Lambertian reflectance)
            vec3 F0 = vec3(0.04);  // Значение F0 для неметаллов
            //F0 = mix(F0, mat.albedo, mat.isMetallic ? 1.0 : 0.0);  // Для металлов F0 равен альбедо
            if (mat.isMetallic) F0 = mat.albedo;
            
            // Интенсивность света с учётом затухания
            vec3 radiance = light.color * light.intensity * attenuation;

            // Cook-Torrance BRDF
            float NDF = DistributionGGX(N, H, mat.roughness);  // Нормализованное распределение микрофасеток
            float G = GeometrySmith(N, V, L, mat.roughness);  // Геометрический термин
            vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);  // Френель

            vec3 numerator = NDF * G * F;
            float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001;
            vec3 specular = numerator / denominator;

            // kS - Спекулярный коэффициент, kD - диффузный коэффициент
            vec3 kS = F;
            vec3 kD = vec3(1.0) - kS;  // Энергосбережение: диффузное и спекулярное освещение вместе

            kD *= 1.0 - (mat.isMetallic ? 1.0 : 0.0);  // У металлов нет диффузного отражения

            // Диффузное освещение
            float NdotL = max(dot(N, L), 0.0);
            vec3 diffuse = kD * mat.albedo / PI;
            
             // Основное освещение
            vec3 color = (diffuse + specular) * radiance * NdotL;
            
            // Добавляем отражение с кубической карты
            vec3 R = reflect(-V, N);  // Отражение вектора взгляда относительно нормали
            vec3 environmentReflection = texture(uSkybox, R).rgb;

            // Учет шероховатости: чем выше roughness, тем меньше резкость отражения
            vec3 reflectionContribution = environmentReflection * (kS * (1.0 - mat.roughness));

            // Учет металличности: для металлов сильнее отражение, а для диэлектриков отражение меньше
            if (mat.isMetallic) {
                // Металлы почти полностью зависят от отражений
                color = reflectionContribution;
            } else {
                // Для диэлектриков добавляем как диффузное, так и отраженное освещение
                color = (diffuse + specular) * radiance * NdotL;
                color += reflectionContribution;
            }
            
            return color;
        }
    ";

    // language=glsl
    var baseStructs = @"
        struct Material {
            vec3 albedo;
            vec3 normal;
            float roughness;
            bool isMetallic;
            float alpha;
        };
        
        struct Light {
            bool isDirection;
            vec3 vector;
            float intensity;
            vec3 color;  
            float radius;
        };

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
            
            // Roughness
            float roughness = texture(uRoughnessTexture, vo_UV).r;
            if (roughness < 0.06) roughness = 0.06;
            mat.roughness = roughness;
            
            // Metallic
            mat.isMetallic = texture(uMetallicTexture, vo_UV).r > 0.0;
            
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
    ";

    // language=glsl
    var lightHelpers = @"
        ivec2 getLightTexelById(uint id, int offset) {
            int pixel = 1 + int(id) * 9 + offset;
            return ivec2(pixel % 64, pixel / 64);
        }
        
        int getLightAmount() {
            vec4 m1 = texelFetch(uLightTexture, ivec2(0, 0), 0);
            return int(m1.r);
        }
        
        Light getLight(uint id) {
            float type = texelFetch(uLightTexture, getLightTexelById(id, 0), 0).r;
            
            float posX = texelFetch(uLightTexture, getLightTexelById(id, 1), 0).r;
            float posY = texelFetch(uLightTexture, getLightTexelById(id, 2), 0).r;
            float posZ = texelFetch(uLightTexture, getLightTexelById(id, 3), 0).r;
            
            float radius = texelFetch(uLightTexture, getLightTexelById(id, 4), 0).r;
            float intensity = texelFetch(uLightTexture, getLightTexelById(id, 5), 0).r;
            
            float colorR = texelFetch(uLightTexture, getLightTexelById(id, 6), 0).r;
            float colorG = texelFetch(uLightTexture, getLightTexelById(id, 7), 0).r;
            float colorB = texelFetch(uLightTexture, getLightTexelById(id, 8), 0).r;
            
            Light ll = createLight(vec3(posX, posY, posZ), vec3(colorR, colorG, colorB), intensity, type <= 1.0);
            ll.radius = radius;
            return ll;
        }
    ";

    // language=glsl
    var otherHelpers = @"
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
    ";

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
        
        uniform sampler2D uLightTexture;
        uniform samplerCube uSkybox;
        
        uniform vec4 uTint;
        
        const float PI = 3.141592653589793;
        
        " + otherHelpers + @"
        
        " + baseStructs + @"

        " + lightHelpers + @"
        
        " + pbrFunctions + @"
        
        void main()
        {
            Material mat = getMaterial();

            // Light
            vec3 finalColor = vec3(0.0);
            int lightAmount = getLightAmount();
            for (int i = 0; i < lightAmount; i++) {
               Light light1 = getLight(uint(i));
               finalColor += PBR(mat, light1);
            }
            
            // HDR
            finalColor = finalColor / (finalColor + vec3(1.0));
            
            // Gamma
            finalColor = pow(finalColor, vec3(1.0/2.2));
            
            color = vec4(finalColor, mat.alpha) * uTint;
        }";

    #endregion

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();
  }

  public override void Render()
  {
    var layer = (Layer_StaticMesh)Layer;

    Shader.Use();
    //OpenGL32.glDepthRange(1.0, 0.0); // Инвертируем диапазон глубины
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);
    //OpenGL32.glDepthFunc(OpenGL32.GL_GREATER);

    //var cp = Scene.Camera.Position;
    //cp.Z *= -1;
    // cp.Y *= -1;
    // Shader.SetUniform("uCameraPosition", cp);
    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
    if (Scene.Skybox != null) Shader.ActivateTexture(Scene.Skybox, "uSkybox", 10);

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
      if (mesh.AlbedoTexture != null) Shader.ActivateTexture(mesh.AlbedoTexture, "uAlbedoTexture", 0);
      if (mesh.NormalTexture != null) Shader.ActivateTexture(mesh.NormalTexture, "uNormalTexture", 1);
      if (mesh.RoughnessTexture != null) Shader.ActivateTexture(mesh.RoughnessTexture, "uRoughnessTexture", 2);
      if (mesh.MetallicTexture != null) Shader.ActivateTexture(mesh.MetallicTexture, "uMetallicTexture", 3);

      // Текстура с источниками света
      Context.MapTexture(Scene.LightTexture);
      Shader.ActivateTexture(Scene.LightTexture, "uLightTexture", 12);

      Shader.SetUniform("uModelMatrix", mesh.Transform.Matrix);
      Shader.SetUniform("uTint", new Vector4(mesh.Tint.R, mesh.Tint.G, mesh.Tint.B, mesh.Tint.A));

      // Bind indices
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(mesh.IndexList));

      // Draw
      OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

      // Unbind vao
      OpenGL32.glBindVertexArray(0);
    });
  }
}