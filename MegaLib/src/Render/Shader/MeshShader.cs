using MegaLib.Mathematics.LinearAlgebra;

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

  public Vector4 Main()
  {
    // Применяем модельную и видовую матрицы для позиции
    var modelViewMatrix = uViewMatrix * uModelMatrix;
    var projectionMatrix = uProjectionMatrix;

    // Преобразуем нормали с использованием матрицы модели
    var normalMatrix = Matrix3x3.Transpose(Matrix3x3.Inverse(new Matrix3x3(uModelMatrix)));

    // Рассчитываем TBN матрицу (Tangent, Bitangent, Normal)
    var T = Vector3.Normalize(normalMatrix * aTangent); // Преобразуем тангенс
    var B = Vector3.Normalize(normalMatrix * aBiTangent); // Преобразуем битангенс
    var N = Vector3.Normalize(normalMatrix * aNormal); // Преобразуем нормаль

    // Передаём нормализованную TBN-матрицу в фрагментный шейдер
    vo_TBN = new Matrix3x3(T, B, N);

    // Позиция камеры в мировых координатах
    var cameraPosition = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    vo_CameraPosition = (Matrix4x4.Inverse(uViewMatrix) * cameraPosition).DropW();

    // Преобразование позиции вершины в мировые координаты
    vo_Position = (uModelMatrix * new Vector4(aPosition, 1.0f)).DropW();

    // Передаём текстурные координаты
    vo_UV = aUV;

    return projectionMatrix * modelViewMatrix * new Vector4(aPosition, 1.0f);
  }
}

public class MeshFragmentShader
{
  [ShaderField("in")] public Vector4 vo_Color;
  [ShaderField("out")] public Vector4 color;

  public void Main()
  {
    color = vo_Color;
  }
}