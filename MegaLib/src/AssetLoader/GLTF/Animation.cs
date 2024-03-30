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

    public int PrevFrame;
    public int NextFrame;

    public void CalculateFrames(float time)
    {
      PrevFrame = TimeList.Count - 1;
      NextFrame = 0;

      for (var i = 0; i < TimeList.Count - 1; i++)
      {
        if (time >= TimeList[i] && time <= TimeList[i + 1])
        {
          PrevFrame = i;
          NextFrame = i + 1;
        }
      }
    }

    public Vector4 CalculateFrameValue(float time)
    {
      if (ValueList.Count == 0) return Vector4.Zero;

      var t =
        (time - TimeList[PrevFrame]) /
        (TimeList[NextFrame] - TimeList[PrevFrame]);

      if (PrevFrame < 0)
        return ValueList[0];

      if (NextFrame > ValueList.Count - 1)
        return ValueList.Last();

      return Type switch
      {
        "translation" or "scale" => Vector3.Lerp(
          ValueList[PrevFrame].DropW(),
          ValueList[NextFrame].DropW(),
          t).AddW(0.0f),
        "rotation" => Quaternion.Lerp(
          ValueList[PrevFrame].ToQuaternion(),
          ValueList[NextFrame].ToQuaternion(),
          t).ToVector4(),
        _ => Vector4.Zero
      };
    }
  }

  public struct GLTF_AnimationFrame
  {
    public string Key;
    public string Type;
    public Vector4 Value;
  }

  public class GLTF_Animation
  {
    public GLTF Gltf;
    public string Name;
    public float Duration;
    public float CurrentTime;
    public List<GLTF_AnimationFrame> CurrentFrame = new();
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

    public void Tick(float delta)
    {
      CurrentTime += delta;
      if (CurrentTime > Duration)
      {
        CurrentTime %= Duration;
      }

      CurrentFrame.Clear();
      for (var i = 0; i < SequenceList.Count; i++)
      {
        SequenceList[i].CalculateFrames(CurrentTime);

        var key = SequenceList[i].Key;
        var value = SequenceList[i].CalculateFrameValue(CurrentTime);
        var type = SequenceList[i].Type;

        if (type == "translation")
        {
          /*const retarget = this.retargetTranslation[this.sequenceList[i].key];
          if (retarget) {
            (value as Vector3).add_(retarget);
          }*/
        }

        if (type == "rotation")
        {
          /*const retarget = this.retargetRotation[this.sequenceList[i].key];
          if (retarget) {
            (value as Quaternion).mul_(retarget);
          }*/
        }

        CurrentFrame.Add(new GLTF_AnimationFrame
        {
          Key = key,
          Type = type,
          Value = value,
        });
      }
    }
  }
}