using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.AssetLoader.GLTF;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Animation;

public enum AnimationKeyType
{
  Translate,
  Rotate,
  Scale
}

public enum AnimationInterpolationType
{
  Step,
  Linear
}

public struct AnimationFrame
{
  public string Key;
  public AnimationKeyType Type;
  public Vector4 Value;
}

public class AnimationSequence
{
  public string Key;
  public AnimationKeyType Type;
  public List<float> TimeList = [];
  public List<Vector4> ValueList = [];
  public List<AnimationInterpolationType> Interpolation = [];

  public int PrevFrame;
  public int NextFrame;

  public AnimationSequence Clone()
  {
    return new AnimationSequence
    {
      Key = Key,
      Type = Type,
      TimeList = TimeList.ToArray().ToList(),
      ValueList = ValueList.ToArray().ToList(),
      Interpolation = Interpolation.ToArray().ToList()
    };
  }

  // Вычисляем текущий и следующий фрейм в зависимости от текущего времени
  public void CalculateFrames(float time)
  {
    PrevFrame = TimeList.Count - 1;
    NextFrame = 0;

    for (var i = 0; i < TimeList.Count - 1; i++)
      if (time >= TimeList[i] && time <= TimeList[i + 1])
      {
        PrevFrame = i;
        NextFrame = i + 1;
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
      AnimationKeyType.Translate or AnimationKeyType.Scale => Vector3.Lerp(
        ValueList[PrevFrame].DropW(),
        ValueList[NextFrame].DropW(),
        t).AddW(0.0f),
      AnimationKeyType.Rotate => Quaternion.Slerp(
        ValueList[PrevFrame].ToQuaternion(),
        ValueList[NextFrame].ToQuaternion(),
        t).ToVector4(),
      _ => Vector4.Zero
    };
  }
}

public class Animation
{
  public string Name;
  public float Duration;
  public float CurrentTime;
  public bool IsLoop;

  public Action<float> OnEnd;

  // Содержим ключи со значениями для текущего фрейма
  public List<AnimationFrame> CurrentFrame { get; private set; } = [];

  // Набор последовательностей с ключами. 
  // Например, hand.rotation или hand.translation и весь их тайм лайн
  public List<AnimationSequence> SequenceList { get; set; } = [];

  public void Reset()
  {
    CurrentTime = 0;
  }

  public Animation Clone()
  {
    var a = new Animation
    {
      Name = Name,
      Duration = Duration,
      IsLoop = IsLoop
    };

    foreach (var sequence in SequenceList)
      a.SequenceList.Add(sequence.Clone());

    return a;
  }

  public void Tick(float delta)
  {
    CurrentTime += delta;
    if (CurrentTime > Duration)
    {
      if (!IsLoop)
      {
        var offset = CurrentTime % Duration;
        CurrentTime = Duration;
        OnEnd?.Invoke(offset);
        return;
      }

      CurrentTime %= Duration;
    }

    CurrentFrame.Clear();
    foreach (var sequence in SequenceList)
    {
      // Вычисляем текущий и следующий фрейм в зависимости от текущего времени
      sequence.CalculateFrames(CurrentTime);

      CurrentFrame.Add(new AnimationFrame
      {
        Key = sequence.Key,
        Type = sequence.Type,
        Value = sequence.CalculateFrameValue(CurrentTime)
      });
    }
  }

  // Создаем новую анимацию из текущей, которая перейдет из текущего состояния в первые кадры анимации B
  // Так же указываем скорость перехода в time.
  public Animation CreateBlendToAnimation(Animation b, float time)
  {
    var blendAnimation = new Animation
    {
      Name = "Between",
      SequenceList = [],
      Duration = time
    };

    for (var i = 0; i < SequenceList.Count; i++)
    {
      // Берем секвенцию
      var currentSequence = SequenceList[i];

      // Находим такую же секвенцию в анимации Б
      var bSequenceIdx = b.SequenceList.FindIndex(x => x.Key == currentSequence.Key && x.Type == currentSequence.Type);
      if (bSequenceIdx == -1) continue;
      var bSequence = b.SequenceList[bSequenceIdx];
      if (bSequence.ValueList.Count == 0) continue;

      // Находим текущий кадр с такими же ключами как в секвенции
      var currentFrameIdx = CurrentFrame.FindIndex(x => x.Key == currentSequence.Key && x.Type == currentSequence.Type);
      if (currentFrameIdx == -1) continue;

      // Создаем новую секвенцию между текущим значением и первым значением в анимации Б
      blendAnimation.SequenceList.Add(new AnimationSequence
      {
        Key = currentSequence.Key,
        Type = currentSequence.Type,
        TimeList = [0, time],
        ValueList = [CurrentFrame[currentFrameIdx].Value, bSequence.ValueList[0]]
      });
    }

    return blendAnimation;
  }
}

public static class AnimationEx
{
  public static Animation FromGLTF(this Animation input, GLTF_Animation gltfAnimation)
  {
    input.Duration = gltfAnimation.Duration;
    input.Name = gltfAnimation.Name;

    foreach (var seqIn in gltfAnimation.SequenceList)
    {
      var seqOut = new AnimationSequence
      {
        Key = seqIn.Key
      };
      seqOut.Type = seqIn.Type switch
      {
        "translation" => AnimationKeyType.Translate,
        "rotation" => AnimationKeyType.Rotate,
        "scale" => AnimationKeyType.Scale,
        _ => seqOut.Type
      };

      // Копируем время и значения
      seqOut.TimeList = seqIn.TimeList.ToArray().ToList();
      seqOut.ValueList = seqIn.ValueList.ToArray().ToList();

      if (gltfAnimation.Gltf.IsZInverted)
      {
        if (seqOut.Type is AnimationKeyType.Translate)
        {
          for (var i = 0; i < seqOut.ValueList.Count; i++)
          {
            seqOut.ValueList[i] = seqOut.ValueList[i] with { Z = seqOut.ValueList[i].Z * -1 };
          }
        }

        if (seqOut.Type == AnimationKeyType.Rotate)
        {
          for (var i = 0; i < seqOut.ValueList.Count; i++)
          {
            seqOut.ValueList[i] = seqOut.ValueList[i] with
            {
              X = seqOut.ValueList[i].X * -1,
              Y = seqOut.ValueList[i].Y * -1
            };
          }
        }
      }

      // Добавляем в список секвенции
      input.SequenceList.Add(seqOut);
    }

    return input;
  }
}

public interface IAnimatable
{
  void Animate(Animation animation);
}