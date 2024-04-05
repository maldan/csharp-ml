using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject
{
  /*public enum RO_DataType
  {
    Buffer,
    Texture
  }*/

  public class RO_Base
  {
    private static ulong _nextId = 1;

    public Transform Transform;
    public ulong Id;

    protected RO_Base()
    {
      Id = _nextId++;
    }

    /*public virtual dynamic GetDataByName(RO_DataType type, string name)
    {
      return null;
    }*/
  }
}