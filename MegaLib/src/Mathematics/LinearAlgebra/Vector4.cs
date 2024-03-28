namespace MegaLib.Mathematics.LinearAlgebra
{
  public struct Vector4Int
  {
    public int X;
    public int Y;
    public int Z;
    public int W;

    public Vector4Int(int x, int y, int z, int w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    public override string ToString()
    {
      return $"Vector4Int({X}, {Y}, {Z}, {W})";
    }
  }

  public struct Vector4
  {
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(float x, float y, float z, float w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    public Vector3 DropW()
    {
      return new Vector3(X, Y, Z);
    }

    public static Vector4 operator *(Vector4 vector, Matrix4x4 matrix)
    {
      Vector4 result;
      result.X = matrix.M00 * vector.X + matrix.M10 * vector.Y + matrix.M20 * vector.Z + matrix.M30 * vector.W;
      result.Y = matrix.M01 * vector.X + matrix.M11 * vector.Y + matrix.M21 * vector.Z + matrix.M31 * vector.W;
      result.Z = matrix.M02 * vector.X + matrix.M12 * vector.Y + matrix.M22 * vector.Z + matrix.M32 * vector.W;
      result.W = matrix.M03 * vector.X + matrix.M13 * vector.Y + matrix.M23 * vector.Z + matrix.M33 * vector.W;
      return result;
    }

    public override string ToString()
    {
      return $"Vector4({X}, {Y}, {Z}, {W})";
    }
  }
}