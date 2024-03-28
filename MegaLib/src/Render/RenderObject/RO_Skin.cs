using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Skin;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject
{
  public class RO_Skin : RO_Base
  {
    public Skeleton Skeleton;
    public List<RO_Mesh> MeshList = new();
    public Texture_Base BoneTexture;

    public RO_Skin()
    {
      Skeleton = new Skeleton();
      BoneTexture = new Texture_Base();
      BoneTexture.Options.Width = 64;
      BoneTexture.Options.Height = 64;
      BoneTexture.Options.FiltrationMode = TextureFiltrationMode.Nearest;
      BoneTexture.Options.WrapMode = TextureWrapMode.Clamp;
      BoneTexture.Options.Format = TextureFormat.R32F;
      BoneTexture.Options.UseMipMaps = false;
    }

    public void Update()
    {
      Skeleton.Update();

      // Update bone texture
      var pixel = new float[64 * 64];
      foreach (var bone in Skeleton.BoneList)
      {
        var mx = Matrix4x4.Identity; // bone.Matrix * bone.InverseBindMatrix;
        var raw = mx.Raw;
        var id = 0;
        foreach (var v in raw)
        {
          pixel[id] = v;
          id += 1;
        }
      }

      /*var byteArray = new byte[pixel.Length * sizeof(float)];
      Buffer.BlockCopy(pixel, 0, byteArray, 0, byteArray.Length);
      BoneTexture.GPU_RAW = byteArray;*/
      BoneTexture.GPU_FLOAT = pixel;
      BoneTexture.IsChanged = true;
    }
  }
}