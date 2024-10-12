using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.AssetLoader.GLTF;

public class GLTF_Accessor
{
  public GLTF Gltf;

  [JsonPropertyName("bufferView")] public uint BufferView { get; set; }
  [JsonPropertyName("componentType")] public uint ComponentType { get; set; }
  [JsonPropertyName("count")] public uint Count { get; set; }
  [JsonPropertyName("type")] public string Type { get; set; }

  private (GLTF_BufferView bufferView, uint componentAmount, uint byteSize, uint offset, byte[] buf) ViewData()
  {
    var finalView = Gltf.BufferViewList[(int)BufferView];

    var componentAmount = Gltf.NumberOfComponents(Type);
    var byteSize = Gltf.ByteLength(ComponentType);
    var offset = finalView.ByteOffset;
    var buf = Gltf.BufferList[(int)finalView.Buffer].Content;

    return (finalView, componentAmount, byteSize, offset, buf);
  }

  public List<float> ScalarFloat()
  {
    var view = ViewData();
    var outData = new List<float>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++) outData.Add(reader.ReadSingle());
    return outData;
  }

  public List<ushort> ScalarUInt16()
  {
    var view = ViewData();
    var outData = new List<ushort>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++) outData.Add(reader.ReadUInt16());
    return outData;
  }

  public List<uint> ScalarUInt32()
  {
    var view = ViewData();
    var outData = new List<uint>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++) outData.Add(reader.ReadUInt32());
    return outData;
  }

  public List<uint> ScalarUInt32Anyway()
  {
    var view = ViewData();
    var outData = new List<uint>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++)
    {
      switch (ComponentType)
      {
        case GLTF.COMPONENT_TYPE_UINT16:
          outData.Add(reader.ReadUInt16());
          break;
        case GLTF.COMPONENT_TYPE_UINT32:
          outData.Add(reader.ReadUInt32());
          break;
      }
    }

    return outData;
  }

  public List<IVector4> Vec4Int()
  {
    var view = ViewData();
    var outData = new List<IVector4>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++)
    {
      switch (ComponentType)
      {
        case GLTF.COMPONENT_TYPE_UINT8:
          outData.Add(new IVector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
          break;
        case GLTF.COMPONENT_TYPE_UINT16:
          outData.Add(new IVector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()));
          break;
        case GLTF.COMPONENT_TYPE_UINT32:
          outData.Add(new IVector4(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
            reader.ReadInt32()));
          break;
        default:
          throw new Exception("FUCK");
      }
    }

    return outData;
  }

  public List<Vector4> Vec4()
  {
    var view = ViewData();
    var outData = new List<Vector4>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++)
    {
      outData.Add(new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
    }

    return outData;
  }

  public List<Vector3> Vec3()
  {
    var view = ViewData();
    var outData = new List<Vector3>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);

    for (var i = 0; i < Count; i++)
    {
      outData.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
    }

    return outData;
  }

  public List<Vector2> Vec2()
  {
    var view = ViewData();
    var outData = new List<Vector2>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++)
    {
      outData.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
    }

    return outData;
  }

  public List<Matrix4x4> Mat4x4()
  {
    var view = ViewData();
    var outData = new List<Matrix4x4>();

    using var stream = new MemoryStream(view.buf);
    stream.Position = view.offset;
    using var reader = new BinaryReader(stream);
    for (var i = 0; i < Count; i++)
    {
      var mx = new Matrix4x4
      {
        M00 = reader.ReadSingle(),
        M01 = reader.ReadSingle(),
        M02 = reader.ReadSingle(),
        M03 = reader.ReadSingle(),

        M10 = reader.ReadSingle(),
        M11 = reader.ReadSingle(),
        M12 = reader.ReadSingle(),
        M13 = reader.ReadSingle(),

        M20 = reader.ReadSingle(),
        M21 = reader.ReadSingle(),
        M22 = reader.ReadSingle(),
        M23 = reader.ReadSingle(),

        M30 = reader.ReadSingle(),
        M31 = reader.ReadSingle(),
        M32 = reader.ReadSingle(),
        M33 = reader.ReadSingle()
      };

      outData.Add(mx);
    }

    return outData;
  }
}

public class GLTF_BufferView
{
  [JsonPropertyName("buffer")] public uint Buffer { get; set; }
  [JsonPropertyName("byteLength")] public uint ByteLength { get; set; }
  [JsonPropertyName("byteOffset")] public uint ByteOffset { get; set; }
  [JsonPropertyName("target")] public uint? Target { get; set; }
}

public class GLTF_Buffer
{
  [JsonPropertyName("byteLength")] public int ByteLength { get; set; }
  [JsonPropertyName("uri")] public string Uri { get; set; }

  public byte[] Content;
}