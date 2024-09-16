using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class MeshVertex : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;
  [ShaderFieldAttribute] public Vector3 aTangent;
  [ShaderFieldAttribute] public Vector3 aBiTangent;
  [ShaderFieldAttribute] public Vector2 aUV;
  [ShaderFieldAttribute] public Vector3 aNormal;

  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;
  [ShaderFieldUniform] public Matrix4x4 uModelMatrix;

  [ShaderFieldOut] public Vector3 vo_Position;
  [ShaderFieldOut] public Vector2 vo_UV;
  [ShaderFieldOut] public Matrix3x3 vo_TBN;
  [ShaderFieldOut] public Vector3 vo_CameraPosition;

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
  [ShaderFieldIn] public Vector3 vo_Position;
  [ShaderFieldIn] public Vector2 vo_UV;
  [ShaderFieldIn] public Matrix3x3 vo_TBN;
  [ShaderFieldIn] public Vector3 vo_CameraPosition;

  [ShaderFieldOut] public Vector4 color;

  [ShaderFieldUniform] public Texture_2D<RGBA<float>> uAlbedoTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA<float>> uNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA<float>> uRoughnessTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA<float>> uMetallicTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA<float>> uLightTexture;
  [ShaderFieldUniform] public Texture_Cube uSkybox;
  [ShaderFieldUniform] public Vector4 uTint;

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