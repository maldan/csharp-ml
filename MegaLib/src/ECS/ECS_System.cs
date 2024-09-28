namespace MegaLib.ECS;

public class ECS_System
{
  protected ECS_World World;

  public ECS_System(ECS_World world)
  {
    World = world;
  }

  public virtual void Tick(float delta)
  {
  }
}