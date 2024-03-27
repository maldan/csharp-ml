using System;
using System.Collections.Generic;
using MegaLib.Render.Skin;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject
{
  public class RO_Skin : RO_Base
  {
    public Skeleton Skeleton;
    public List<RO_Mesh> MeshList;
    public Texture_Base BoneTexture;

    public RO_Skin()
    {
      BoneTexture = new Texture_Base();
      BoneTexture.Options.Width = 64;
      BoneTexture.Options.Height = 64;
      BoneTexture.Options.FiltrationMode = TextureFiltrationMode.Nearest;
      BoneTexture.Options.Format = TextureFormat.R32F;
    }

    public void Update()
    {
      Skeleton.Update();

      // Update bone texture
      var pixel = new float[64 * 64];
      foreach (var bone in Skeleton.BoneMap.Values)
      {
        var mx = bone.Matrix * bone.InverseBindMatrix;
        var raw = mx.Raw;
        for (var i = 0; i < raw.Length; i++)
        {
          pixel[bone.Index * 16 + i] = raw[i];
        }
      }

      var byteArray = new byte[pixel.Length * sizeof(float)];
      Buffer.BlockCopy(pixel, 0, byteArray, 0, byteArray.Length);
      BoneTexture.GPU_RAW = byteArray;
      BoneTexture.IsChanged = true;
    }
  }
}