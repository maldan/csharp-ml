using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject
{
  public class RO_Base
  {
    private static ulong _nextId = 1;

    public ulong Id { get; }
    public int RenderOrderPriority;
    public Transform Transform;

    protected RO_Base()
    {
      Id = _nextId++;
    }
  }
}