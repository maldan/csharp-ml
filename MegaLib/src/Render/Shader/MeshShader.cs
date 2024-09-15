using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class MeshVertexShader
{
  [ShaderField("attribute")] public Vector3 aPosition;
  [ShaderField("attribute")] public Vector3 aTangent;
  [ShaderField("attribute")] public Vector3 aBiTangent;
  [ShaderField("attribute")] public Vector2 aUV;
  [ShaderField("attribute")] public Vector3 aNormal;

  [ShaderField("uniform")] public Matrix4x4 uProjectionMatrix;
  [ShaderField("uniform")] public Matrix4x4 uViewMatrix;
  [ShaderField("uniform")] public Matrix4x4 uModelMatrix;

  [ShaderField("out")] public Vector3 vo_Position;
  [ShaderField("out")] public Vector2 vo_UV;
  [ShaderField("out")] public Matrix3x3 vo_TBN;
  [ShaderField("out")] public Vector3 vo_CameraPosition;

  private Vector3 normalize(Vector3 v)
  {
    return v.Normalized;
  }

  private Matrix3x3 inverse(Matrix3x3 m)
  {
    return Matrix3x3.Inverse(m);
  }

  private Matrix4x4 inverse(Matrix4x4 m)
  {
    return Matrix4x4.Inverse(m);
  }

  private Matrix3x3 transpose(Matrix3x3 m)
  {
    return Matrix3x3.Transpose(m);
  }

  public Vector4 Main()
  {
    // Применяем модельную и видовую матрицы для позиции
    var modelViewMatrix = uViewMatrix * uModelMatrix;
    var projectionMatrix = uProjectionMatrix;

    // Преобразуем нормали с использованием матрицы модели
    var normalMatrix = transpose(inverse(new Matrix3x3(uModelMatrix)));

    // Рассчитываем TBN матрицу (Tangent, Bitangent, Normal)
    var T = normalize(normalMatrix * aTangent); // Преобразуем тангенс
    var B = normalize(normalMatrix * aBiTangent); // Преобразуем битангенс
    var N = normalize(normalMatrix * aNormal); // Преобразуем нормаль

    // Передаём нормализованную TBN-матрицу в фрагментный шейдер
    vo_TBN = new Matrix3x3(T, B, N);

    // Позиция камеры в мировых координатах
    var cameraPosition = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).XYZ;

    // Преобразование позиции вершины в мировые координаты
    vo_Position = (uModelMatrix * new Vector4(aPosition, 1.0f)).XYZ;

    // Передаём текстурные координаты
    vo_UV = aUV;

    return projectionMatrix * modelViewMatrix * new Vector4(aPosition, 1.0f);
  }
}

public class MeshFragmentShader
{
  [ShaderField("in")] public Vector3 vo_Position;
  [ShaderField("in")] public Vector2 vo_UV;
  [ShaderField("in")] public Matrix3x3 vo_TBN;
  [ShaderField("in")] public Vector3 vo_CameraPosition;

  [ShaderField("out")] public Vector4 color;

  [ShaderField("uniform")] public Texture_2D<RGBA<float>> uAlbedoTexture;
  [ShaderField("uniform")] public Texture_2D<RGBA<float>> uNormalTexture;
  [ShaderField("uniform")] public Texture_2D<RGBA<float>> uRoughnessTexture;
  [ShaderField("uniform")] public Texture_2D<RGBA<float>> uMetallicTexture;
  [ShaderField("uniform")] public Texture_2D<RGBA<float>> uLightTexture;
  [ShaderField("uniform")] public Texture_Cube uSkybox;
  [ShaderField("uniform")] public Vector4 uTint;

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

  private float pow(float a, float b)
  {
    return MathF.Pow(a, b);
  }

  private Vector3 linearToSRGB(Vector3 colorLinear)
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

  private Vector3 sRGBToLinear(Vector3 colorSRGB)
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

  private Vector4 texture(Texture_2D<RGBA<float>> texture2D, Vector2 uv)
  {
    return new Vector4();
  }

  private void discard()
  {
  }

  private Material getMaterial()
  {
    var mat = new Material();

    // Read albedo
    var texelColor = texture(uAlbedoTexture, vo_UV);
    if (texelColor.A <= 0.01)
    {
      discard();
      return mat;
    }

    mat.Albedo = sRGBToLinear(texelColor.XYZ);
    mat.Alpha = texelColor.A;

    // Read normal
    var pp = texture(uNormalTexture, vo_UV).XYZ;
    var normal = pp * 2.0f - 1.0f;
    mat.Normal = normalize(vo_TBN * normal);

    // Roughness
    var roughness = texture(uRoughnessTexture, vo_UV).R;
    if (roughness < 0.06) roughness = 0.06f;
    mat.Roughness = roughness;

    // Metallic
    mat.IsMetallic = texture(uMetallicTexture, vo_UV).R > 0.0;

    return mat;
  }

  private Light createLight(Vector3 pos, Vector3 color, float intensity, bool isDir)
  {
    var light = new Light();

    light.Intensity = intensity;
    light.IsDirection = isDir;
    if (isDir) light.Vector = normalize(pos);
    else light.Vector = pos;
    light.Color = color;

    return light;
  }

  private IVector2 getLightTexelById(uint id, int offset)
  {
    var pixel = 1 + toInt(id) * 9 + offset;
    return new IVector2(pixel % 64, pixel / 64);
  }

  private int getLightAmount()
  {
    var m1 = texelFetch(uLightTexture, new IVector2(0, 0), 0);
    return toInt(m1.R);
  }

  private Light getLight(uint id)
  {
    var type = texelFetch(uLightTexture, getLightTexelById(id, 0), 0).R;

    var posX = texelFetch(uLightTexture, getLightTexelById(id, 1), 0).R;
    var posY = texelFetch(uLightTexture, getLightTexelById(id, 2), 0).R;
    var posZ = texelFetch(uLightTexture, getLightTexelById(id, 3), 0).R;

    var radius = texelFetch(uLightTexture, getLightTexelById(id, 4), 0).R;
    var intensity = texelFetch(uLightTexture, getLightTexelById(id, 5), 0).R;

    var colorR = texelFetch(uLightTexture, getLightTexelById(id, 6), 0).R;
    var colorG = texelFetch(uLightTexture, getLightTexelById(id, 7), 0).R;
    var colorB = texelFetch(uLightTexture, getLightTexelById(id, 8), 0).R;

    var ll = createLight(new Vector3(posX, posY, posZ), new Vector3(colorR, colorG, colorB), intensity, type <= 1.0);
    ll.Radius = radius;
    return ll;
  }

  private Vector3 fresnelSchlick(float cosTheta, Vector3 F0)
  {
    return F0 + (1.0f - F0) * pow(1.0f - cosTheta, 5.0f);
  }

  private float DistributionGGX(Vector3 N, Vector3 H, float roughness)
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

  private float GeometrySchlickGGX(float NdotV, float roughness)
  {
    var r = roughness + 1.0f;
    var k = r * r / 8.0f;

    var num = NdotV;
    var denom = NdotV * (1.0f - k) + k;

    return num / denom;
  }

  private float GeometrySmith(Vector3 N, Vector3 V, Vector3 L, float roughness)
  {
    var NdotV = max(dot(N, V), 0.0f);
    var NdotL = max(dot(N, L), 0.0f);
    var ggx2 = GeometrySchlickGGX(NdotV, roughness);
    var ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
  }

  private float length(Vector3 v)
  {
    return v.Length;
  }

  private Vector3 normalize(Vector3 v)
  {
    return v.Normalized;
  }

  private float dot(Vector3 v1, Vector3 v2)
  {
    return Vector3.Dot(v1, v2);
  }

  private float max(float v1, float v2)
  {
    return MathF.Max(v1, v2);
  }

  private Vector3 reflect(Vector3 a, Vector3 b)
  {
    return Vector3.Reflect(a, b);
  }

  private Vector3 PBR(Material mat, Light light)
  {
    // Нормализуем входные данные
    var N = normalize(mat.Normal); // Нормаль к поверхности
    var V = normalize(vo_CameraPosition - vo_Position); // Направление к камере

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
      L = normalize(light.Vector - vo_Position); // Направление к источнику света

      // Рассчитываем расстояние от источника света до объекта
      var dd = light.Vector - vo_Position;
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
    var NdotL = max(dot(N, L), 0.0f);
    var diffuse = kD * mat.Albedo / MathF.PI;

    // Основное освещение
    var color = (diffuse + specular) * radiance * NdotL;

    // Добавляем отражение с кубической карты
    var R = reflect(-V, N); // Отражение вектора взгляда относительно нормали
    var skyboxPixel = texture(uSkybox, R);
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

  private Vector4 texelFetch(object a, IVector2 uv, int hz)
  {
    return new Vector4();
  }

  private uint toUInt(int x)
  {
    return (uint)x;
  }

  private int toInt(uint x)
  {
    return (int)x;
  }

  private int toInt(float x)
  {
    return (int)x;
  }

  public Vector4 texture(Texture_Cube a, Vector3 v)
  {
    return new Vector4();
  }

  private Vector3 pow(Vector3 a, Vector3 b)
  {
    return Vector3.Pow(a, b);
  }

  public void Main()
  {
    var mat = getMaterial();

    // Light
    var finalColor = new Vector3(0.0f);
    var lightAmount = getLightAmount();
    for (var i = 0; i < lightAmount; i++)
    {
      var light1 = getLight(toUInt(i));
      finalColor += PBR(mat, light1);
    }

    // HDR
    finalColor = finalColor / (finalColor + new Vector3(1.0f));

    // Gamma
    finalColor = pow(finalColor, new Vector3(1.0f / 2.2f));

    color = new Vector4(finalColor, mat.Alpha) * uTint;
  }
}