using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class MeshVertex : Shader_Base
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

public class MeshFragmentShader : Shader_PBR
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

  public void Main()
  {
    var mat = getMaterial(uAlbedoTexture, uNormalTexture, uRoughnessTexture, uMetallicTexture, vo_UV, vo_TBN);

    // Light
    var finalColor = new Vector3(0.0f);
    var lightAmount = getLightAmount(uLightTexture);
    for (var i = 0; i < lightAmount; i++)
    {
      var light1 = getLight(uLightTexture, toUInt(i));
      finalColor += PBR(mat, light1, uSkybox, vo_CameraPosition, vo_Position);
    }

    // HDR
    finalColor = finalColor / (finalColor + new Vector3(1.0f));

    // Gamma
    finalColor = pow(finalColor, new Vector3(1.0f / 2.2f));

    color = new Vector4(finalColor, mat.Alpha) * uTint;
  }
}