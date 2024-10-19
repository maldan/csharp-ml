using System;
using System.IO;
using System.Runtime.InteropServices;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;

namespace MegaLib.Render.Texture;

public class Texture_Cube
{
  public ulong Id;

  public ImageGPU<RGB8> FRONT { get; set; }
  public ImageGPU<RGB8> BACK { get; set; }

  public ImageGPU<RGB8> TOP { get; set; }
  public ImageGPU<RGB8> BOTTOM { get; set; }

  public ImageGPU<RGB8> LEFT { get; set; }
  public ImageGPU<RGB8> RIGHT { get; set; }

  public TextureOptions Options;

  public event EventHandler<ulong> OnDestroy;

  public Texture_Cube()
  {
    Options = new TextureOptions
    {
      Format = TextureFormat.RGB8,
      FiltrationMode = TextureFiltrationMode.Linear,
      WrapMode = TextureWrapMode.Clamp,
      UseMipMaps = true
    };

    TOP = new ImageGPU<RGB8>(4, 4);
    BOTTOM = new ImageGPU<RGB8>(4, 4);
    LEFT = new ImageGPU<RGB8>(4, 4);
    RIGHT = new ImageGPU<RGB8>(4, 4);
    BACK = new ImageGPU<RGB8>(4, 4);
    FRONT = new ImageGPU<RGB8>(4, 4);

    var l = new[] { RIGHT, LEFT, TOP, BOTTOM, BACK, FRONT };
    foreach (var ll in l)
    {
      ll.SetPixels(new RGB8[]
      {
        new(128, 128, 128), new(255, 255, 255), new(128, 128, 128), new(255, 255, 255),
        new(255, 255, 255), new(128, 128, 128), new(255, 255, 255), new(128, 128, 128),
        new(128, 128, 128), new(255, 255, 255), new(128, 128, 128), new(255, 255, 255),
        new(255, 255, 255), new(128, 128, 128), new(255, 255, 255), new(128, 128, 128)
      });
    }

    Id = TextureId.NextId();
  }

  public static Vector4 ReadPixel(Texture_Cube textureCube, Vector3 position)
  {
    return new Vector4();
  }
}