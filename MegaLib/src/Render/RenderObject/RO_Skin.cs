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
    public Texture_2D<float> BoneTexture;

    public RO_Skin()
    {
      Skeleton = new Skeleton();
      BoneTexture = new Texture_2D<float>();
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
      // var pixel = new float[64 * 64];
      var id = 0;

      foreach (var bone in Skeleton.BoneList)
      {
        var mx = bone.InverseBindMatrix * bone.Matrix;
        var raw = mx.Raw;

        foreach (var v in raw)
        {
          BoneTexture.RAW[id] = v;
          id += 1;
        }
      }

      //BoneTexture.SetPixels(pixel);
      //BoneTexture.IsChanged = true;
    }
  }
}