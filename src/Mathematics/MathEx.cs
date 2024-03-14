namespace MegaLib.Mathematics
{
  public static class MathEx
  {
    public static float Lerp(float start, float end, float t)
    {
      return (1 - t) * start + t * end;
    }
  }

  public static class FloatExtensions
  {
    public static float DegToRad(this float number)
    {
      return (float)(number * (Math.PI / 180.0));
    }

    public static float RadToDeg(this float number)
    {
      return (float)(number * (180.0 / Math.PI));
    }

    public static float Sin(this float number)
    {
      return (float)(Math.Sin(number));
    }
  }
}