namespace MegaLib.Render.Texture
{
  public class Texture_Cube
  {
    public byte[] GPU_FRONT { get; set; }
    public byte[] GPU_BACK { get; set; }

    public byte[] GPU_TOP { get; set; }
    public byte[] GPU_BOTTOM { get; set; }

    public byte[] GPU_LEFT { get; set; }
    public byte[] GPU_RIGHT { get; set; }

    public ushort Width;
    public ushort Height;

    public Texture_Cube()
    {
      Width = 4;
      Height = 4;

      GPU_TOP = new byte[]
      {
        128, 128, 128, 255, 255, 255, 255, 255, 128, 128, 128, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 128, 128, 128, 255, 255, 255, 255, 255, 128, 128, 128, 255,
        128, 128, 128, 255, 255, 255, 255, 255, 128, 128, 128, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 128, 128, 128, 255, 255, 255, 255, 255, 128, 128, 128, 255,
      };
      GPU_FRONT = GPU_TOP;
      GPU_BACK = GPU_TOP;
      GPU_LEFT = GPU_TOP;
      GPU_RIGHT = GPU_TOP;
      GPU_BOTTOM = GPU_TOP;
    }
  }
}