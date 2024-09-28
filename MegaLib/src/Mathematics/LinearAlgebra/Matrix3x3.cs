using System;

namespace MegaLib.Mathematics.LinearAlgebra;

public struct Matrix3x3
{
  public float M00, M01, M02;
  public float M10, M11, M12;
  public float M20, M21, M22;

  public float[] Raw =>
  [
    M00, M01, M02,
    M10, M11, M12,
    M20, M21, M22
  ];

  public int Count => 9;

  // Перегрузка оператора [] для доступа к элементам матрицы по индексу
  public float this[int index]
  {
    get
    {
      return index switch
      {
        0 => M00,
        1 => M01,
        2 => M02,
        3 => M10,
        4 => M11,
        5 => M12,
        6 => M20,
        7 => M21,
        8 => M22,
        _ => 0
      };
    }
    set
    {
      switch (index)
      {
        case 0:
          M00 = value;
          break;
        case 1:
          M01 = value;
          break;
        case 2:
          M02 = value;
          break;
        case 3:
          M10 = value;
          break;
        case 4:
          M11 = value;
          break;
        case 5:
          M12 = value;
          break;
        case 6:
          M20 = value;
          break;
        case 7:
          M21 = value;
          break;
        case 8:
          M22 = value;
          break;
      }
    }
  }

  // Конструктор по элементам
  public Matrix3x3(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
  {
    M00 = m00;
    M01 = m01;
    M02 = m02;
    M10 = m10;
    M11 = m11;
    M12 = m12;
    M20 = m20;
    M21 = m21;
    M22 = m22;
  }

  // Конструктор, который создает Matrix3x3 из Matrix4x4
  public Matrix3x3(Matrix4x4 mat4)
  {
    M00 = mat4.M00;
    M01 = mat4.M01;
    M02 = mat4.M02;
    M10 = mat4.M10;
    M11 = mat4.M11;
    M12 = mat4.M12;
    M20 = mat4.M20;
    M21 = mat4.M21;
    M22 = mat4.M22;
  }

  // Конструктор для заполнения матрицы тремя векторами
  public Matrix3x3(Vector3 v1, Vector3 v2, Vector3 v3)
  {
    // Если считаем, что векторы задают столбцы матрицы
    M00 = v1.X;
    M01 = v2.X;
    M02 = v3.X;
    M10 = v1.Y;
    M11 = v2.Y;
    M12 = v3.Y;
    M20 = v1.Z;
    M21 = v2.Z;
    M22 = v3.Z;
  }

  // Метод для транспонирования матрицы 3x3
  public static Matrix3x3 Transpose(Matrix3x3 matrix)
  {
    return new Matrix3x3
    {
      M00 = matrix.M00, M01 = matrix.M10, M02 = matrix.M20,
      M10 = matrix.M01, M11 = matrix.M11, M12 = matrix.M21,
      M20 = matrix.M02, M21 = matrix.M12, M22 = matrix.M22
    };
  }

  public static Matrix3x3 Inverse(Matrix3x3 matrix)
  {
    var det = matrix.M00 * (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21)
              - matrix.M01 * (matrix.M10 * matrix.M22 - matrix.M12 * matrix.M20)
              + matrix.M02 * (matrix.M10 * matrix.M21 - matrix.M11 * matrix.M20);

    if (det == 0) return new Matrix3x3(); // Возвращаем нулевую матрицу в случае вырожденности

    var invDet = 1.0f / det;

    return new Matrix3x3
    {
      M00 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21) * invDet,
      M01 = (matrix.M02 * matrix.M21 - matrix.M01 * matrix.M22) * invDet,
      M02 = (matrix.M01 * matrix.M12 - matrix.M02 * matrix.M11) * invDet,
      M10 = (matrix.M12 * matrix.M20 - matrix.M10 * matrix.M22) * invDet,
      M11 = (matrix.M00 * matrix.M22 - matrix.M02 * matrix.M20) * invDet,
      M12 = (matrix.M02 * matrix.M10 - matrix.M00 * matrix.M12) * invDet,
      M20 = (matrix.M10 * matrix.M21 - matrix.M11 * matrix.M20) * invDet,
      M21 = (matrix.M01 * matrix.M20 - matrix.M00 * matrix.M21) * invDet,
      M22 = (matrix.M00 * matrix.M11 - matrix.M01 * matrix.M10) * invDet
    };
  }

  public static Vector3 operator *(Matrix3x3 matrix, Vector3 vector)
  {
    return new Vector3
    {
      X = matrix.M00 * vector.X + matrix.M01 * vector.Y + matrix.M02 * vector.Z,
      Y = matrix.M10 * vector.X + matrix.M11 * vector.Y + matrix.M12 * vector.Z,
      Z = matrix.M20 * vector.X + matrix.M21 * vector.Y + matrix.M22 * vector.Z
    };
  }

  public override string ToString()
  {
    return $@"Matrix3x3(
        {M00}, {M01}, {M02},
        {M10}, {M11}, {M12},
        {M20}, {M21}, {M22},
      )";
  }
}