using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.AssetLoader.GLTF
{
  public struct GLTF_AnimationSampler
  {
    public int Input;
    public string Interpolation;
    public int Output;
  }

  public class GLTF_AnimationSequence
  {
    public string Key;
    public string Type;
    public List<float> TimeList = new();
    public List<Vector4> ValueList = new();
  }

  public class GLTF_Animation
  {
    public GLTF Gltf;
    public string Name;
    public float Duration;
    public List<GLTF_AnimationSampler> SamplerList = new();
    public List<GLTF_AnimationSequence> SequenceList = new();

    public GLTF_Animation(GLTF gltf, JsonElement element)
    {
      Gltf = gltf;
      Name = element.GetProperty("name").GetString();

      // Parse samplers
      foreach (var samplers in element.GetProperty("samplers").EnumerateArray())
      {
        SamplerList.Add(new GLTF_AnimationSampler
        {
          Input = samplers.GetProperty("input").GetInt32(),
          Interpolation = samplers.GetProperty("interpolation").GetString(),
          Output = samplers.GetProperty("output").GetInt32(),
        });
      }

      // Channels
      foreach (var channel in element.GetProperty("channels").EnumerateArray())
      {
        var node = Gltf.NodeList[channel.GetProperty("target").GetProperty("node").GetInt32()];
        var path = channel.GetProperty("target").GetProperty("path").GetString();
        var samplerId = channel.GetProperty("sampler").GetInt32();
        var sampler = SamplerList[samplerId];

        var keyframes = Gltf.AccessorList[sampler.Input].ScalarFloat();
        Duration = keyframes.Max();

        var sequence = new GLTF_AnimationSequence
        {
          Key = node.Name,
          Type = path,
          TimeList = keyframes
        };

        if (sequence.Type == "translation")
        {
          var values = Gltf.AccessorList[sampler.Output].Vec3();
          foreach (var value in values)
            sequence.ValueList.Add(new Vector4(value.X, value.Y, value.Z, 0.0f));
        }

        if (sequence.Type == "rotation")
        {
          var values = Gltf.AccessorList[sampler.Output].Vec4();
          foreach (var value in values)
            sequence.ValueList.Add(new Vector4(value.X, value.Y, value.Z, value.W));
        }

        SequenceList.Add(sequence);
      }
    }
  }

  public class GLTF_Bone
  {
    public GLTF GLTF;

    public int Id;
    public string Name;
    public List<int> Children;

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public Matrix4x4 InverseBindMatrix;

    public GLTF_Bone(GLTF_Node node)
    {
      GLTF = node.GLTF;
      Id = node.Id;
      Name = node.Name;
      Children = node.Children;
      Position = node.Position;
      Rotation = node.Rotation;
      Scale = node.Scale;
    }
  }

  public class GLTF_Skin
  {
    public GLTF GLTF;
    public int Id;
    public string Name;
    public List<GLTF_Bone> BoneList = new();
    public List<GLTF_Mesh> MeshList = new();
    public Dictionary<string, GLTF_Bone> BoneMap = new();

    public GLTF_Skin(GLTF gltf, JsonElement element, int id)
    {
      GLTF = gltf;
      Id = id;
      Name = element.GetProperty("name").GetString();

      // Fill bones
      var joint = element.GetProperty("joints").EnumerateArray().Select(e => e.GetInt32()).ToList();
      foreach (var bone in joint.Select(i => new GLTF_Bone(GLTF.NodeList[i])))
      {
        BoneList.Add(bone);
        BoneMap.Add(bone.Name, bone);
      }

      // Fill meshes
      foreach (var node in GLTF.NodeList.Where(node => node.MeshId >= 0 && node.SkinId == Id))
      {
        if (node.MeshId != null) MeshList.Add(GLTF.MeshList[node.MeshId.Value]);
      }

      // Inverse Bind Matrices
      var ibm = element.GetProperty("inverseBindMatrices").GetInt32();
      var matrixList = GLTF.AccessorList[ibm].Mat4x4();
      for (var i = 0; i < matrixList.Count; i++) BoneList[i].InverseBindMatrix = matrixList[i];
    }
  }
}