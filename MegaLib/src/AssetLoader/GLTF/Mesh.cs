using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.AssetLoader.GLTF;

public class GLTF_MeshPrimitive
{
  public GLTF Gltf;
  public int IndicesId;
  public int MaterialId = -1;
  public GLTF_Attribute Attribute;
  public List<GLTF_Attribute> Targets = new();

  public GLTF_Material Material => MaterialId == -1 ? null : Gltf.MaterialList[MaterialId];

  public GLTF_MeshPrimitive(GLTF gltf, JsonElement element)
  {
    Gltf = gltf;

    Attribute = JsonSerializer.Deserialize<GLTF_Attribute>(element.GetProperty("attributes").GetRawText());
    IndicesId = element.GetProperty("indices").GetInt32();

    if (element.TryGetProperty("material", out var elementMaterial))
    {
      MaterialId = elementMaterial.GetInt32();
    }

    Console.WriteLine(Attribute);
  }

  public List<Vector3> Vertices
  {
    get
    {
      var p = Attribute.POSITION;
      return p == null ? new List<Vector3>() : Gltf.AccessorList[p.Value].Vec3();
    }
  }

  public List<Vector3> Normals
  {
    get
    {
      var p = Attribute.NORMAL;
      return p == null ? new List<Vector3>() : Gltf.AccessorList[p.Value].Vec3();
    }
  }

  public List<Vector2> UV0
  {
    get
    {
      var p = Attribute.TEXCOORD_0;
      return p == null ? new List<Vector2>() : Gltf.AccessorList[p.Value].Vec2();
    }
  }

  public List<Vector4> BoneWeight
  {
    get
    {
      var p = Attribute.WEIGHTS_0;
      return p == null ? new List<Vector4>() : Gltf.AccessorList[p.Value].Vec4();
    }
  }

  public List<IVector4> BoneIndex
  {
    get
    {
      var p = Attribute.JOINTS_0;
      return p == null ? new List<IVector4>() : Gltf.AccessorList[p.Value].Vec4Int();
    }
  }

  public List<uint> Indices => Gltf.AccessorList[IndicesId].ScalarUInt32Anyway();

  public override string ToString()
  {
    var str = "MeshPrimitive {\n";
    str += $"IndicesId={IndicesId}\n";
    str += $"MaterialId={MaterialId}\n";
    str += $"Attribute={Attribute}\n";
    str += "}\n";
    return str;
  }
}

public class GLTF_Mesh
{
  public GLTF Gltf;

  //public string Id = "";
  public string Name;

  //public string NodeName = "";
  public List<GLTF_MeshPrimitive> Primitives = new();
  //public float[] Weights;

  public GLTF_Mesh(GLTF gltf, JsonElement element)
  {
    Gltf = gltf;
    Name = element.GetProperty("name").GetString();

    foreach (var primitive in element.GetProperty("primitives").EnumerateArray())
    {
      Primitives.Add(new GLTF_MeshPrimitive(gltf, primitive));
    }
  }

  public override string ToString()
  {
    var str = "Mesh {\n";
    str += $"\tName={Name}\n";
    str += "\tPrimitives={\n";
    foreach (var primitive in Primitives)
    {
      str += "\t\t" + primitive + "\n";
    }

    str += "\t}\n";
    str += "}\n";
    return str;
  }
}