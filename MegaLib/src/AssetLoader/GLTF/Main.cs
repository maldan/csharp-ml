using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.AssetLoader.GLTF
{
  public class GLTF_Attribute
  {
    [JsonPropertyName("COLOR_0")] public int? COLOR_0 { get; set; }
    [JsonPropertyName("POSITION")] public int? POSITION { get; set; }
    [JsonPropertyName("NORMAL")] public int? NORMAL { get; set; }
    [JsonPropertyName("TEXCOORD_0")] public int? TEXCOORD_0 { get; set; }
    [JsonPropertyName("WEIGHTS_0")] public int? WEIGHTS_0 { get; set; }
    [JsonPropertyName("JOINTS_0")] public int? JOINTS_0 { get; set; }

    public override string ToString()
    {
      return $"Attribute {{ COLOR_0={COLOR_0}, POSITION={POSITION}, WEIGHTS_0={WEIGHTS_0} JOINTS_0={JOINTS_0} }}";
    }
  }

  public class GLTF_Node
  {
    public GLTF GLTF;

    public int Id;
    public string Name;

    public int? MeshId;
    public int? SkinId;
    public List<int> Children = new();

    public Vector3 Position;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale;

    public GLTF_Node(GLTF gltf, JsonElement element, int id)
    {
      GLTF = gltf;
      Id = id;
      Name = element.GetProperty("name").GetString();

      if (element.TryGetProperty("mesh", out var mesh)) MeshId = mesh.GetInt32();
      if (element.TryGetProperty("skin", out var skin)) SkinId = skin.GetInt32();

      // Children
      if (element.TryGetProperty("children", out var children))
      {
        foreach (var e in children.EnumerateArray()) Children.Add(e.GetInt32());
      }

      // Position
      if (element.TryGetProperty("translation", out var translation))
      {
        var list = translation.EnumerateArray().Select(x => x.GetSingle()).ToList();
        Position = new Vector3(list[0], list[1], list[2]);
      }

      // Rotation
      if (element.TryGetProperty("rotation", out var rotation))
      {
        var list = rotation.EnumerateArray().Select(x => x.GetSingle()).ToList();
        Rotation = new Quaternion(list[0], list[1], list[2], list[3]);
      }

      // Scale
      if (element.TryGetProperty("scale", out var scale))
      {
        var list = scale.EnumerateArray().Select(x => x.GetSingle()).ToList();
        Scale = new Vector3(list[0], list[1], list[2]);
      }
    }
  }

  public class GLTF
  {
    public string BaseURI;
    public List<GLTF_Image> ImageList = new();
    public List<GLTF_Texture> TextureList = new();
    public List<GLTF_Material> MaterialList = new();

    public List<GLTF_Mesh> MeshList = new();
    public List<GLTF_Node> NodeList = new();
    public List<GLTF_Accessor> AccessorList = new();
    public List<GLTF_BufferView> BufferViewList = new();
    public List<GLTF_Buffer> BufferList = new();
    public List<GLTF_Skin> SkinList = new();
    public List<GLTF_Animation> AnimationList = new();

    public const uint COMPONENT_TYPE_INT8 = 5120;
    public const uint COMPONENT_TYPE_UINT8 = 5121;
    public const uint COMPONENT_TYPE_INT16 = 5122;
    public const uint COMPONENT_TYPE_UINT16 = 5123;
    public const uint COMPONENT_TYPE_UINT32 = 5125;
    public const uint COMPONENT_TYPE_FLOAT = 5126;

    public const string SCALAR = "SCALAR";
    public const string VEC2 = "VEC2";
    public const string VEC3 = "VEC3";
    public const string VEC4 = "VEC4";
    public const string MAT2 = "MAT2";
    public const string MAT3 = "MAT3";
    public const string MAT4 = "MAT4";

    public GLTF_Animation GetAnimationByName(string name)
    {
      return AnimationList.FirstOrDefault(t => t.Name == name);
    }

    /*public byte[] ParseAccessor(int id)
    {
      var accessor = AccessorList[id];
      var finalView = BufferViewList[(int)accessor.BufferView];

      var componentAmount = NumberOfComponents(accessor.Type);
      var byteSize = ByteLength(accessor.ComponentType);
      var offset = finalView.ByteOffset;
      var buf = BufferList[(int)finalView.Buffer].Content;

      var outData = new List<byte>();
      for (var i = 0; i < accessor.Count; i++)
      {
        // Component reading
        for (var k = 0; k < componentAmount; k++)
        {
          // Byte reading
          for (var j = 0; j < byteSize; j++)
          {
            outData.Add(buf[offset + j]);
            offset += 1;
          }
        }
      }

      return outData.ToArray();
    }*/

    public uint NumberOfComponents(string t)
    {
      return t switch
      {
        SCALAR => 1,
        VEC2 => 2,
        VEC3 => 3,
        VEC4 => 4,
        MAT2 => 4,
        MAT3 => 9,
        MAT4 => 16,
        _ => 0
      };
    }

    public uint ByteLength(uint n)
    {
      return n switch
      {
        COMPONENT_TYPE_INT8 => 1, // byte
        COMPONENT_TYPE_UINT8 => 1, // unsigned byte
        COMPONENT_TYPE_INT16 => 2, // short
        COMPONENT_TYPE_UINT16 => 2, // unsigned short
        COMPONENT_TYPE_UINT32 => 4, // unsigned int
        COMPONENT_TYPE_FLOAT => 4, // float
        _ => 0
      };
    }

    public static GLTF FromFile(string path)
    {
      var jsonData = System.IO.File.ReadAllText(path);
      var jsonDocument = JsonDocument.Parse(jsonData);

      var gltf = new GLTF
      {
        BaseURI = Path.GetDirectoryName(path)
      };

      // Accessors
      foreach (var element in jsonDocument.RootElement.GetProperty("accessors").EnumerateArray())
      {
        var tmp = JsonSerializer.Deserialize<GLTF_Accessor>(element.GetRawText());
        tmp.Gltf = gltf;
        gltf.AccessorList.Add(tmp);
      }

      // Buffers
      foreach (var element in jsonDocument.RootElement.GetProperty("buffers").EnumerateArray())
      {
        var newBuffer = JsonSerializer.Deserialize<GLTF_Buffer>(element.GetRawText());
        if (newBuffer.Uri.Contains("data:application/octet-stream;base64,"))
        {
          var index = newBuffer.Uri.IndexOf(',');
          if (index != -1)
            newBuffer.Content = Convert.FromBase64String(newBuffer.Uri[(index + 1)..]);
        }
        else newBuffer.Content = File.ReadAllBytes(Path.GetDirectoryName(path) + "/" + newBuffer.Uri);

        gltf.BufferList.Add(newBuffer);
      }

      foreach (var bufferView in jsonDocument.RootElement.GetProperty("bufferViews").EnumerateArray())
      {
        var tmp = JsonSerializer.Deserialize<GLTF_BufferView>(bufferView.GetRawText());
        gltf.BufferViewList.Add(tmp);
      }

      // Parse mesh
      foreach (var mesh in jsonDocument.RootElement.GetProperty("meshes").EnumerateArray())
        gltf.MeshList.Add(new GLTF_Mesh(gltf, mesh));

      // Parse nodes
      var nodeId = 0;
      foreach (var node in jsonDocument.RootElement.GetProperty("nodes").EnumerateArray())
      {
        gltf.NodeList.Add(new GLTF_Node(gltf, node, nodeId));
        nodeId++;
      }

      // Textures
      if (jsonDocument.RootElement.TryGetProperty("textures", out var elementTextures))
      {
        foreach (var element in elementTextures.EnumerateArray())
          gltf.TextureList.Add(new GLTF_Texture(gltf, element));
      }

      // Materials
      if (jsonDocument.RootElement.TryGetProperty("materials", out var elementMaterials))
      {
        foreach (var element in elementMaterials.EnumerateArray())
          gltf.MaterialList.Add(new GLTF_Material(gltf, element));
      }

      // Images
      if (jsonDocument.RootElement.TryGetProperty("images", out var elementImages))
      {
        foreach (var element in elementImages.EnumerateArray())
          gltf.ImageList.Add(new GLTF_Image(gltf, element));
      }

      // Skins
      if (jsonDocument.RootElement.TryGetProperty("skins", out var elementSkins))
      {
        var id = 0;
        foreach (var element in elementSkins.EnumerateArray())
        {
          gltf.SkinList.Add(new GLTF_Skin(gltf, element, id));
          id += 1;
        }
      }

      // Animation
      if (jsonDocument.RootElement.TryGetProperty("animations", out var elementAnimations))
      {
        foreach (var element in elementAnimations.EnumerateArray())
        {
          gltf.AnimationList.Add(new GLTF_Animation(gltf, element));
        }
      }

      return gltf;
    }

    public override string ToString()
    {
      var str = "GLTF {\n";
      str += "MeshList [\n";
      for (var i = 0; i < MeshList.Count; i++)
      {
        str += MeshList[i] + "\n";
      }

      str += "]\n";
      str += "}\n";
      return str;
    }
  }
}