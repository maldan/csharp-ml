namespace MegaLib.ECS;

public class ECS_Archetype
{
  public int[] IdList;
  public int Count { get; protected set; }

  public ECS_Archetype(int capacity)
  {
    IdList = new int[capacity];
  }
  
  protected void Add(int entityId)
  {
    IdList[Count] = entityId;
    Count++;
  }

  public void Remove(int entityId)
  {
    for (int i = 0; i < Count; i++)
    {
      if (IdList[i] == entityId)
      {
        IdList[i] = 0;
        return;
      }
    }
  }
}