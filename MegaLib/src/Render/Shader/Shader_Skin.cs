using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class SkinVertex : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;
  [ShaderFieldAttribute] public Vector3 aTangent;
  [ShaderFieldAttribute] public Vector3 aBiTangent;
  [ShaderFieldAttribute] public Vector2 aUV;
  [ShaderFieldAttribute] public Vector3 aNormal;
  [ShaderFieldAttribute] public Vector4 aBoneWeight;
  [ShaderFieldAttribute] public uint aBoneIndex;

  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;
  [ShaderFieldUniform] public Matrix4x4 uModelMatrix;
  [ShaderFieldUniform] public Texture_2D<float> uBoneMatrix;

  [ShaderFieldOut] public Vector3 vo_Position;
  [ShaderFieldOut] public Vector2 vo_UV;
  [ShaderFieldOut] public Matrix3x3 vo_TBN;
  [ShaderFieldOut] public Vector3 vo_CameraPosition;
  [ShaderFieldOut] public Vector3 vo_ViewNormal;
  [ShaderFieldOut] public Vector4 vo_ViewPosition;

  public IVector2 getBoneTexelById(uint id, int ch)
  {
    var pixel = toInt(id) * 16 + ch;
    return new IVector2(pixel % 64, pixel / 64);
  }

  private IVector4 unpackIndex(uint id)
  {
    var r = (id >> 24) & 0xFFu;
    var g = (id >> 16) & 0xFFu;
    var b = (id >> 8) & 0xFFu;
    var a = id & 0xFFu;
    return new IVector4(r, g, b, a);
  }

  private Matrix4x4 getInverseBindMatrix(uint id)
  {
    var m1 = texelFetch(uBoneMatrix, getBoneTexelById(id, 0), 0);
    var m2 = texelFetch(uBoneMatrix, getBoneTexelById(id, 1), 0);
    var m3 = texelFetch(uBoneMatrix, getBoneTexelById(id, 2), 0);

    var m5 = texelFetch(uBoneMatrix, getBoneTexelById(id, 4), 0);
    var m6 = texelFetch(uBoneMatrix, getBoneTexelById(id, 5), 0);
    var m7 = texelFetch(uBoneMatrix, getBoneTexelById(id, 6), 0);

    var m9 = texelFetch(uBoneMatrix, getBoneTexelById(id, 8), 0);
    var m10 = texelFetch(uBoneMatrix, getBoneTexelById(id, 9), 0);
    var m11 = texelFetch(uBoneMatrix, getBoneTexelById(id, 10), 0);

    var m13 = texelFetch(uBoneMatrix, getBoneTexelById(id, 12), 0);
    var m14 = texelFetch(uBoneMatrix, getBoneTexelById(id, 13), 0);
    var m15 = texelFetch(uBoneMatrix, getBoneTexelById(id, 14), 0);

    return new Matrix4x4(
      m1.R, m2.R, m3.R, 0.0f,
      m5.R, m6.R, m7.R, 0.0f,
      m9.R, m10.R, m11.R, 0.0f,
      m13.R, m14.R, m15.R, 1.0f
    );
  }

  /*public Matrix4x4 identity()
  {
    return new Matrix4x4(
      1.0f, 0.0f, 0.0f, 0.0f,
      0.0f, 1.0f, 0.0f, 0.0f,
      0.0f, 0.0f, 1.0f, 0.0f,
      0.0f, 0.0f, 0.0f, 1.0f
    );
  }*/

  public Vector4 Main()
  {
    // Apply bone matrix
    var boneIndex = unpackIndex(aBoneIndex);
    var bone1Matrix = getInverseBindMatrix(toUInt(boneIndex.X));
    var bone2Matrix = getInverseBindMatrix(toUInt(boneIndex.Y));
    var bone3Matrix = getInverseBindMatrix(toUInt(boneIndex.Z));
    var bone4Matrix = getInverseBindMatrix(toUInt(boneIndex.W));

    // Make skin matrix
    var skinMatrix = aBoneWeight.R * bone1Matrix +
                     aBoneWeight.G * bone2Matrix +
                     aBoneWeight.B * bone3Matrix +
                     aBoneWeight.A * bone4Matrix;

    // Рассчитываем TBN матрицу (Tangent, Bitangent, Normal)
    var T = normalize((skinMatrix * new Vector4(aTangent.X, aTangent.Y, aTangent.Z, 0.0f)).XYZ); // Преобразуем тангенс
    var B = normalize((skinMatrix * new Vector4(aBiTangent.X, aBiTangent.Y, aBiTangent.Z, 0.0f))
      .XYZ); // Преобразуем битангенс
    var N = normalize((skinMatrix * new Vector4(aNormal.X, aNormal.Y, aNormal.Z, 0.0f)).XYZ); // Преобразуем нормаль

    // Передаём нормализованную TBN-матрицу в фрагментный шейдер
    vo_TBN = new Matrix3x3(T, B, N);

    // Позиция камеры в мировых координатах
    var cameraPosition = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    vo_CameraPosition = (inverse(uViewMatrix) * cameraPosition).XYZ;
    vo_CameraPosition.Z *= -1f;

    // Преобразование позиции вершины в мировые координаты
    vo_Position = (uModelMatrix * new Vector4(aPosition, 1.0f)).XYZ;

    // Передаём текстурные координаты
    vo_UV = aUV;

    vo_ViewPosition = uViewMatrix * skinMatrix * new Vector4(aPosition.X, aPosition.Y, aPosition.Z, 1.0f);
    vo_ViewNormal = (uViewMatrix * skinMatrix * new Vector4(aNormal.X, aNormal.Y, aNormal.Z, 0.0f)).XYZ;

    return uProjectionMatrix * uViewMatrix * skinMatrix * new Vector4(aPosition.X, aPosition.Y, aPosition.Z, 1.0f);
  }
}

public class SkinFragment : Shader_PBR
{
  [ShaderFieldIn] public Vector3 vo_Position;
  [ShaderFieldIn] public Vector2 vo_UV;
  [ShaderFieldIn] public Matrix3x3 vo_TBN;
  [ShaderFieldIn] public Vector3 vo_CameraPosition;

  [ShaderFieldIn] public Vector4 vo_ViewPosition;
  [ShaderFieldIn] public Vector3 vo_ViewNormal;

  [ShaderFieldOut] public Vector4 fragColor;
  [ShaderFieldOut] public Vector4 fragNormal;
  [ShaderFieldOut] public Vector4 fragPosition;
  [ShaderFieldOut] public Vector4 fragRME;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uAlbedoTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uRoughnessTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uMetallicTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uLightTexture;
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

    fragColor = new Vector4(finalColor, mat.Alpha) * uTint;

    var nx = Remap(vo_ViewNormal.X, -1.0f, 1.0f, 0.0f, 1.0f);
    var ny = Remap(vo_ViewNormal.Y, -1.0f, 1.0f, 0.0f, 1.0f);
    var nz = Remap(vo_ViewNormal.Z, -1.0f, 1.0f, 0.0f, 1.0f);
    fragNormal = new Vector4(nx, ny, nz, 1.0f);

    fragPosition = new Vector4(vo_ViewPosition.X, vo_ViewPosition.Y, vo_ViewPosition.Z, 1.0f);

    fragRME = new Vector4(0, 0, 0, 1f);
  }
}