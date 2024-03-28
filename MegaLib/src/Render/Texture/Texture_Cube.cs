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

    public TextureOptions Options;

    public Texture_Cube()
    {
      Options = new TextureOptions();
      Options.Width = 4;
      Options.Height = 4;
      Options.FiltrationMode = TextureFiltrationMode.Linear;
      Options.UseMipMaps = true;

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