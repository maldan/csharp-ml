using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using MegaLib.Media.Image;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.AssetLoader.GLTF;

public class GLTF_Image
{
  public GLTF Gltf;
  public string Name;
  public string MimeType;
  public int BufferView;

  public string URI;

  public bool IsBase64 => URI.Contains("data:application/octet-stream;base64,");

  public byte[] RawContent
  {
    get
    {
      if (string.IsNullOrEmpty(URI))
      {
        var view = Gltf.BufferViewList[BufferView];
        var segment = new ArraySegment<byte>(Gltf.BufferList[(int)view.Buffer].Content, (int)view.ByteOffset,
          (int)view.ByteLength);
        return segment.ToArray();
      }

      if (!IsBase64) return File.ReadAllBytes(Gltf.BaseURI + "/" + URI);
      var index = URI.IndexOf(',');
      return index != -1 ? Convert.FromBase64String(URI[(index + 1)..]) : File.ReadAllBytes(Gltf.BaseURI + "/" + URI);
    }
  }

  public GLTF_Image(GLTF gltf, JsonElement element)
  {
    Gltf = gltf;
    Name = element.GetProperty("name").GetString();
    MimeType = element.GetProperty("mimeType").GetString();

    if (element.TryGetProperty("uri", out var uri)) URI = uri.GetString();
    if (element.TryGetProperty("bufferView", out var bufferView)) BufferView = bufferView.GetInt32();
  }

  public BitmapData<T> ToBitmapData<T>() where T : struct
  {
    var content = RawContent;
    return BitmapData.FromFile<T>(content);
  }

  public Texture_2D<T> ToTexture2D<T>() where T : struct
  {
    var bmp = ToBitmapData<T>();

    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bmp.Pixels, 0);
    var texture = new Texture_2D<T>(bmp.Width, bmp.Height);
    texture.RAW.SetRaw(ptr, bmp.ByteSize);

    // texture.SaveToBMP($"C:/Users/black/Desktop/Chapter3/${ptr}.bmp");
    //if (typeof(T) == typeof(RGBA<byte>)) texture.Options.Format = TextureFormat.RGBA8;
    //if (typeof(T) == typeof(RGB<byte>)) texture.Options.Format = TextureFormat.RGB8;
    //if (typeof(T) == typeof(byte)) texture.Options.Format = TextureFormat.R8;

    return texture;
  }
}

public class GLTF_Texture
{
  public GLTF Gltf;
  public int Sampler;
  public int Source;

  public GLTF_Texture(GLTF gltf, JsonElement element)
  {
    Gltf = gltf;
    Sampler = element.GetProperty("sampler").GetInt32();
    Source = element.GetProperty("source").GetInt32();
  }

  public GLTF_Image Image => Gltf.ImageList[Source];
}

public class GLTF_Material
{
  public GLTF Gltf;
  public string Name;
  public bool IsDoubleSided;
  public int BaseColorTextureId = -1;
  public int NormalTextureId = -1;
  public int RoughnessTextureId = -1;
  public RGBA<float> BaseColor = new(1, 1, 1, 1);
  public float Roughness = 0.5f;

  public bool HasBaseColorTexture => BaseColorTextureId != -1;
  public bool HasNormalTexture => NormalTextureId != -1;
  public bool HasRoughnessTexture => RoughnessTextureId != -1;

  public GLTF_Texture BaseColorTexture => Gltf.TextureList[BaseColorTextureId];
  public GLTF_Texture NormalTexture => Gltf.TextureList[NormalTextureId];
  public GLTF_Texture RoughnessTexture => Gltf.TextureList[RoughnessTextureId];

  public GLTF_Material(GLTF gltf, JsonElement element)
  {
    Gltf = gltf;
    Name = element.GetProperty("name").GetString();

    if (element.TryGetProperty("doubleSided", out var ds)) IsDoubleSided = ds.GetBoolean();

    // Get base texture
    if (element.TryGetProperty("pbrMetallicRoughness", out var pbr))
    {
      if (pbr.TryGetProperty("baseColorTexture", out var bc)) BaseColorTextureId = bc.GetProperty("index").GetInt32();

      if (pbr.TryGetProperty("baseColorFactor", out var bcf))
        BaseColor = new RGBA<float>(bcf[0].GetSingle(), bcf[1].GetSingle(), bcf[2].GetSingle(), bcf[3].GetSingle());

      // BaseColor = bc.GetProperty("index").GetInt32();
      if (pbr.TryGetProperty("metallicRoughnessTexture", out var rt))
        RoughnessTextureId = rt.GetProperty("index").GetInt32();

      if (pbr.TryGetProperty("roughnessFactor", out var rf))
        Roughness = rf.GetSingle();
    }

    // Get normal
    if (element.TryGetProperty("normalTexture", out var normal))
      NormalTextureId = normal.GetProperty("index").GetInt32();
  }
}