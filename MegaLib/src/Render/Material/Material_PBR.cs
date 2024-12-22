using System;
using MegaLib.AssetLoader.GLTF;
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

  public void InitDefaultTextures()
  {
    var albedo = new Texture_2D<RGBA8>(2, 2)
    {
      RAW =
      {
        [0] = new RGBA8(255, 255, 255, 255),
        [1] = new RGBA8(128, 128, 128, 255),
        [2] = new RGBA8(128, 128, 128, 255),
        [3] = new RGBA8(255, 255, 255, 255)
      }
    };
    albedo.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    AlbedoTexture = albedo;

    var normal = new Texture_2D<RGB8>(1, 1)
    {
      RAW =
      {
        [0] = new RGB8(128, 128, 255)
      }
    };
    normal.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    NormalTexture = normal;

    var roughness = new Texture_2D<byte>(1, 1)
    {
      RAW =
      {
        [0] = 64
      }
    };
    roughness.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    RoughnessTexture = roughness;

    var metalic = new Texture_2D<byte>(1, 1)
    {
      RAW =
      {
        [0] = 0
      }
    };
    metalic.Options.FiltrationMode = TextureFiltrationMode.Nearest;
    MetallicTexture = metalic;
  }

  public Material_PBR FromGLTF(GLTF_MeshPrimitive gltfMeshPrimitive)
  {
    var mat = gltfMeshPrimitive.Material;

    if (mat is { HasBaseColorTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.BaseColorTexture.Image.URI;
      if (TextureManager.Has(texturePath))
      {
        AlbedoTexture = TextureManager.Get<RGBA8>(texturePath);
      }
      else
      {
        var texture = mat.BaseColorTexture.Image.ToTexture2D<RGBA8>();
        if (texture != null)
        {
          AlbedoTexture = texture;
          TextureManager.Add(texturePath, texture);
        }
      }
    }

    if (mat is { HasBaseColorTexture: false })
    {
      AlbedoTexture.RAW.Resize(1, 1);
      AlbedoTexture.RAW.SetPixels([
        new RGBA8((byte)(mat.BaseColor.R * 255),
          (byte)(mat.BaseColor.G * 255), (byte)(mat.BaseColor.B * 255),
          (byte)(mat.BaseColor.A * 255))
      ]);
    }

    // Базовую текстуру
    if (mat is { HasNormalTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.NormalTexture.Image.URI;
      Console.WriteLine(texturePath);
      if (TextureManager.Has(texturePath))
      {
        NormalTexture = TextureManager.Get<RGB8>(texturePath);
      }
      else
      {
        var texture = mat.NormalTexture.Image.ToTexture2D<RGB8>();
        if (texture != null)
        {
          NormalTexture = texture;
          TextureManager.Add(texturePath, texture);
        }
      }
    }

    // Базовую текстуру
    if (mat is { HasRoughnessTexture: true })
    {
      var texturePath = gltfMeshPrimitive.Gltf.BaseURI + "/" + mat.RoughnessTexture.Image.URI;
      if (TextureManager.Has(texturePath))
      {
        RoughnessTexture = TextureManager.Get<byte>(texturePath);
      }
      else
      {
        var texture = mat.RoughnessTexture.Image.ToTexture2D<byte>();
        if (texture != null)
        {
          RoughnessTexture = texture;
          TextureManager.Add(texturePath, texture);
        }
      }
    }

    return this;
  }
}