using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class _VoxelVertexShader : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;
  [ShaderFieldAttribute] public Vector2 aUV;
  [ShaderFieldAttribute] public Vector3 aNormal;

  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;
  
  [ShaderFieldOut] public Vector3 vo_WorldPosition;
  [ShaderFieldOut] public Vector2 vo_UV;
  [ShaderFieldOut] public Vector3 vo_CameraPosition;
  [ShaderFieldOut] public Vector3 vo_Normal;

  public Vector4 Main()
  {
    // Применяем модельную и видовую матрицы для позиции
    var modelViewMatrix = uViewMatrix;
    var projectionMatrix = uProjectionMatrix;
    
    // Преобразуем нормали с использованием матрицы модели
    //var normalMatrix = transpose(inverse(new Matrix3x3(uModelMatrix)));
    
    // Позиция камеры в мировых координатах
    var cameraPosition = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).XYZ;

    // Преобразование позиции вершины в мировые координаты
    vo_WorldPosition = (new Vector4(aPosition, 1.0f)).XYZ;
    
    // Передаём текстурные координаты
    vo_UV = aUV;
    
    vo_Normal = aNormal; // Transform normal to world space
    
    return projectionMatrix * modelViewMatrix * new Vector4(aPosition, 1.0f);
  }
}

[ShaderEmbedText("layout(points) in;\nlayout(triangle_strip, max_vertices = 14) out;")]
public class _VoxelGeometryShader : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;
 
  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;

  [ShaderFieldOut] public Vector3 vo_WorldPosition;
  [ShaderFieldOut] public Vector2 vo_UV;
  [ShaderFieldOut] public Vector3 vo_CameraPosition;

  public void Main()
  {
    
  }
}

public class _VoxelFragmentShader : Shader_Base
{
  [ShaderFieldIn] public Vector3 vo_WorldPosition;
  [ShaderFieldIn] public Vector2 vo_UV;
  [ShaderFieldIn] public Vector3 vo_CameraPosition;
  [ShaderFieldIn] public Vector3 vo_Normal;

  [ShaderFieldOut] public Vector4 color;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uAlbedoTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uLightTexture;
  
  [ShaderFieldUniform] public Vector4 uFogData;

  public void Main()
  {
    var texelColor = texture(uAlbedoTexture, vo_UV);
    
    // color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

    var fogStart = 0;
    var fogEnd = 128f*3f;
    
    // Calculate the distance from the camera to the fragment
    float distance = length(vo_WorldPosition - vo_CameraPosition);

    // Compute fog factor (linear fog)
    float fogFactor = clamp((fogEnd - distance) / (fogEnd - fogStart), 0.0f, 1.0f);

    // Interpolate between the fragment color and the fog color
    // var objectColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // Example object color
    color = mix(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), texelColor, fogFactor);
    
    // Light
    // Calculate normalized light direction
    //var lightPosition = normalize(new Vector3(0.0f, 1.0f, 1.0f));
    var lightDir = normalize(new Vector3(0.0f, 1.0f, 1.0f));

    // Normalize the normal (it should already be normalized if provided correctly)
    var normal = normalize(vo_Normal);

    // Lambertian reflection: max(0, dot(N, L))
    float diff = max(dot(normal, lightDir), 0.5f);

    // Final diffuse color
    //var diffuse = uDiffuseColor * uLightColor * diff;

    color.R *= diff;
    color.G *= diff;
    color.B *= diff;
    
    /*var mat = getMaterial(uAlbedoTexture, uNormalTexture, uRoughnessTexture, uMetallicTexture, vo_UV, vo_TBN);

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

    color = new Vector4(finalColor, mat.Alpha) * uTint;*/
  }
}