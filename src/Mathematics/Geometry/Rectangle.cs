namespace MegaLib.Mathematics.Geometry
{
  public struct Rectangle
  {
    public float FromX;
    public float FromY;
    public float ToX;
    public float ToY;

    public float Width => ToX - FromX;
    public float Height => ToY - FromY;

    public Rectangle(float x1, float y1, float x2, float y2)
    {
      FromX = x1;
      FromY = y1;
      ToX = x2;
      ToY = y2;
    }

    public static Rectangle FromLeftTopWidthHeight()
    {
      return new Rectangle();
    }
  }
}