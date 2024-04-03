using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject
{
  public enum RO_DataType
  {
    Buffer,
    Texture
  }

  public class RO_Base
  {
    public Transform Transform;

    public event EventHandler OnBeforeRender;

    protected RO_Base()
    {
    }

    public virtual dynamic GetDataByName(RO_DataType type, string name)
    {
      return null;
    }
  }
}