using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.RenderObject
{
  public class RenderObject_Base
  {
    private static ulong _nextId = 1;

    public ulong Id { get; }
    public int RenderOrderPriority;
    public Transform Transform;

    protected RenderObject_Base()
    {
      Id = _nextId++;
    }
  }
}