using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class Shader_PBR : Shader_Base
{
  public struct Light
  {
    public bool IsDirection;
    public Vector3 Vector;
    public float Intensity;
    public Vector3 Color;
    public float Radius;
  }

  public struct Material
  {
    public Vector3 Albedo;
    public Vector3 Normal;
    public float Roughness;
    public bool IsMetallic;
    public float Alpha;
  }

  public Vector3 linearToSRGB(Vector3 colorLinear)
  {
    var colorSRGB = new Vector3(0.0f);
    for (var i = 0; i < 3; ++i)
    {
      if (colorLinear[i] <= 0.0031308)
      {
        colorSRGB[i] = 12.92f * colorLinear[i];
      }
      else
      {
        colorSRGB[i] = 1.055f * pow(colorLinear[i], 1.0f / 2.4f) - 0.055f;
      }
    }

    return colorSRGB;
  }

  public Vector3 sRGBToLinear(Vector3 colorSRGB)
  {
    var colorLinear = new Vector3(0.0f);
    for (var i = 0; i < 3; ++i)
    {
      if (colorSRGB[i] <= 0.04045)
      {
        colorLinear[i] = colorSRGB[i] / 12.92f;
      }
      else
      {
        colorLinear[i] = pow((colorSRGB[i] + 0.055f) / 1.055f, 2.4f);
      }
    }

    return colorLinear;
  }

  public Material getMaterial(
    Texture_2D<RGBA32F> albedoTexture,
    Texture_2D<RGBA32F> normalTexture,
    Texture_2D<RGBA32F> roughnessTexture,
    Texture_2D<RGBA32F> metallicTexture,
    Vector2 uv,
    Matrix3x3 tbn
  )
  {
    var mat = new Material();

    // Read albedo
    var texelColor = texture(albedoTexture, uv);
    if (texelColor.A <= 0.01)
    {
      discard();
      return mat;
    }

    mat.Albedo = sRGBToLinear(texelColor.XYZ);
    mat.Alpha = texelColor.A;

    // Read normal
    var pp = texture(normalTexture, uv).XYZ;
    var normal = pp * 2.0f - 1.0f;
    mat.Normal = normalize(tbn * normal);
    mat.Normal.Z *= -1f;

    // Roughness
    var roughness = texture(roughnessTexture, uv).R;
    if (roughness < 0.06) roughness = 0.06f;
    mat.Roughness = roughness;

    // Metallic
    mat.IsMetallic = texture(metallicTexture, uv).R > 0.0;

    return mat;
  }

  public Light createLight(Vector3 pos, Vector3 color, float intensity, bool isDir)
  {
    var light = new Light();

    light.Intensity = intensity;
    light.IsDirection = isDir;
    if (isDir) light.Vector = normalize(pos);
    else light.Vector = pos;
    light.Color = color;

    return light;
  }

  public IVector2 getLightTexelById(uint id, int offset)
  {
    var pixel = 1 + toInt(id) * 9 + offset;
    return new IVector2(pixel % 64, pixel / 64);
  }

  public int getLightAmount(Texture_2D<RGBA32F> lightTexture)
  {
    var m1 = texelFetch(lightTexture, new IVector2(0, 0), 0);
    return toInt(m1.R);
  }

  public Light getLight(Texture_2D<RGBA32F> lightTexture, uint id)
  {
    var type = texelFetch(lightTexture, getLightTexelById(id, 0), 0).R;

    var posX = texelFetch(lightTexture, getLightTexelById(id, 1), 0).R;
    var posY = texelFetch(lightTexture, getLightTexelById(id, 2), 0).R;
    var posZ = texelFetch(lightTexture, getLightTexelById(id, 3), 0).R;

    var radius = texelFetch(lightTexture, getLightTexelById(id, 4), 0).R;
    var intensity = texelFetch(lightTexture, getLightTexelById(id, 5), 0).R;

    var colorR = texelFetch(lightTexture, getLightTexelById(id, 6), 0).R;
    var colorG = texelFetch(lightTexture, getLightTexelById(id, 7), 0).R;
    var colorB = texelFetch(lightTexture, getLightTexelById(id, 8), 0).R;

    var ll = createLight(new Vector3(posX, posY, posZ), new Vector3(colorR, colorG, colorB), intensity, type <= 1.0);
    ll.Radius = radius;
    return ll;
  }

  public Vector3 fresnelSchlick(float cosTheta, Vector3 F0)
  {
    return F0 + (1.0f - F0) * pow(1.0f - cosTheta, 5.0f);
  }

  public float DistributionGGX(Vector3 N, Vector3 H, float roughness)
  {
    var a = roughness * roughness;
    var a2 = a * a;
    var NdotH = max(dot(N, H), 0.0f);
    var NdotH2 = NdotH * NdotH;

    var num = a2;
    var denom = NdotH2 * (a2 - 1.0f) + 1.0f;
    denom = MathF.PI * denom * denom;

    return num / denom;
  }

  public float GeometrySchlickGGX(float NdotV, float roughness)
  {
    var r = roughness + 1.0f;
    var k = r * r / 8.0f;

    var num = NdotV;
    var denom = NdotV * (1.0f - k) + k;

    return num / denom;
  }

  public float GeometrySmith(Vector3 N, Vector3 V, Vector3 L, float roughness)
  {
    var NdotV = max(dot(N, V), 0.0f);
    var NdotL = max(dot(N, L), 0.0f);
    var ggx2 = GeometrySchlickGGX(NdotV, roughness);
    var ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
  }

  public Vector3 PBR(
    Material mat,
    Light light,
    Texture_Cube skybox,
    Vector3 cameraPosition,
    Vector3 fragmentPosition
  )
  {
    // Нормализуем входные данные
    var N = normalize(mat.Normal); // Нормаль к поверхности
    var V = normalize(cameraPosition - fragmentPosition); // Направление к камере

    // Определяем тип источника света
    Vector3 L;
    var attenuation = 1.0f;

    if (light.IsDirection)
    {
      // Направленный свет
      L = normalize(light.Vector); // Направление света, инвертированное
    }
    else
    {
      // Точечный свет (Point Light)
      L = normalize(light.Vector - fragmentPosition); // Направление к источнику света

      // Рассчитываем расстояние от источника света до объекта
      var dd = light.Vector - fragmentPosition;
      var distance = length(dd);

      // Проверяем, находится ли объект внутри радиуса действия света
      if (distance < light.Radius)
      {
        // Модель затухания с учётом радиуса
        attenuation = light.Intensity * (1.0f - distance / light.Radius) / (distance * distance);
      }
      else
      {
        attenuation = 0.0f; // Если объект за пределами радиуса, свет не воздействует
      }
    }

    var H = normalize(V + L); // Полу-угловой вектор

    // Диффузное освещение (Lambertian reflectance)
    var F0 = new Vector3(0.04f); // Значение F0 для неметаллов
    //F0 = mix(F0, mat.albedo, mat.isMetallic ? 1.0 : 0.0);  // Для металлов F0 равен альбедо
    if (mat.IsMetallic) F0 = mat.Albedo;

    // Интенсивность света с учётом затухания
    var radiance = light.Color * light.Intensity * attenuation;

    // Cook-Torrance BRDF
    var NDF = DistributionGGX(N, H, mat.Roughness); // Нормализованное распределение микрофасеток
    var G = GeometrySmith(N, V, L, mat.Roughness); // Геометрический термин
    var F = fresnelSchlick(max(dot(H, V), 0.0f), F0); // Френель

    var numerator = NDF * G * F;
    var denominator = 4.0f * max(dot(N, V), 0.0f) * max(dot(N, L), 0.0f) + 0.001f;
    var specular = numerator / denominator;

    // kS - Спекулярный коэффициент, kD - диффузный коэффициент
    var kS = F;
    var kD = new Vector3(1.0f) - kS; // Энергосбережение: диффузное и спекулярное освещение вместе

    kD *= 1.0f - (mat.IsMetallic ? 1.0f : 0.0f); // У металлов нет диффузного отражения

    // Диффузное освещение
    //var NdotL = max(dot(N, L), 0.0f);
    var NdotL = dot(N, L);
    if (NdotL < 0.03)
    {
      NdotL = mix(0.03f, 0.0f, -(NdotL - 0.1f));
    }

    var diffuse = kD * mat.Albedo / MathF.PI;

    // Основное освещение
    var color = (diffuse + specular) * radiance * NdotL;

    // Добавляем отражение с кубической карты
    var R = reflect(-V, N); // Отражение вектора взгляда относительно нормали
    var skyboxPixel = texture(skybox, R);
    var environmentReflection = skyboxPixel.XYZ;

    // Учет шероховатости: чем выше roughness, тем меньше резкость отражения
    var reflectionContribution = environmentReflection * (kS * (1.0f - mat.Roughness));

    // Учет металличности: для металлов сильнее отражение, а для диэлектриков отражение меньше
    if (mat.IsMetallic)
    {
      // Металлы почти полностью зависят от отражений
      color = reflectionContribution;
    }
    else
    {
      // Для диэлектриков добавляем как диффузное, так и отраженное освещение
      color = (diffuse + specular) * radiance * NdotL;
      color += reflectionContribution;
    }

    return color;
  }
}