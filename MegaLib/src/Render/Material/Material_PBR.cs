using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Material;

public class Material_PBR
{
  public Texture_2D<RGBA8> AlbedoTexture;
  public Texture_2D<RGB8> NormalTexture;
  public Texture_2D<byte> RoughnessTexture;
  public Texture_2D<byte> MetallicTexture;

  public RGBA32F Tint = new(1, 1, 1, 1);
}