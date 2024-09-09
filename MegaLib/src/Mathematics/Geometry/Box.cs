using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Mathematics.Geometry;

public struct Box
{
  public Vector3 Size;

  /*public Vector3 V0;
  public Vector3 V1;
  public Vector3 V2;
  public Vector3 V3;
  public Vector3 V4;
  public Vector3 V5;
  public Vector3 V6;
  public Vector3 V7;

  public Box(Vector3 position, Vector3 size)
  {
    var sizeX = size.X / 2;
    var sizeY = size.Y / 2;
    var sizeZ = size.Z / 2;

    V0 = position + new Vector3(-sizeX, -sizeY, sizeZ);
    V1 = position + new Vector3(sizeX, -sizeY, sizeZ);
    V2 = position + new Vector3(sizeX, sizeY, sizeZ);
    V3 = position + new Vector3(-sizeX, sizeY, sizeZ);
    V4 = position + new Vector3(-sizeX, -sizeY, -sizeZ);
    V5 = position + new Vector3(sizeX, -sizeY, -sizeZ);
    V6 = position + new Vector3(sizeX, sizeY, -sizeZ);
    V7 = position + new Vector3(-sizeX, sizeY, -sizeZ);
  }*/
}