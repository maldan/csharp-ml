using System;
using System.Collections.Generic;
using MegaLib.Mathematics.Geometry;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Animation;

public struct SpriteAnimationFrame
{
  public Rectangle Area;
  public float Duration;
}

public class SpriteAnimation
{
  public string Name;
  public bool IsLoop;
  public List<SpriteAnimationFrame> Frames = [];
  public SpriteAnimationFrame CurrentFrame => Frames[_frameId];

  private float _timer;
  private int _frameId;
  private bool _isEnd;

  public Action OnEnd;

  public void Reset()
  {
    _isEnd = false;
    _frameId = 0;
    _timer = 0;
  }

  public void Update(float delta)
  {
    if (_isEnd) return;

    _timer += delta;

    // Используем цикл while для корректного переключения нескольких кадров
    while (_timer > CurrentFrame.Duration)
    {
      // Учитываем остаток времени
      _timer -= CurrentFrame.Duration;
      _frameId += 1;

      // Проверяем, если кадры закончились
      if (_frameId > Frames.Count - 1)
      {
        if (IsLoop)
        {
          // Если анимация зациклена, возвращаемся к первому кадру
          _frameId = 0;
          OnEnd?.Invoke();
        }
        else
        {
          // Если анимация не зациклена, останавливаемся на последнем кадре
          _frameId = Frames.Count - 1;
          _isEnd = true;
          OnEnd?.Invoke();
          break;
        }
      }
    }
  }
}

public class SpriteAnimationControllerTransition
{
  public SpriteAnimation From;
  public SpriteAnimation To;
  public SpriteAnimationControllerTransitionTrigger Trigger;
}

public enum SpriteAnimationControllerTransitionTrigger
{
  Start,
  End
}

public class SpriteAnimationController
{
  private Dictionary<string, SpriteAnimation> _animations = new();
  private List<SpriteAnimationControllerTransition> _transitions = [];
  private string _defaultState;
  public RO_Sprite Sprite;

  public string CurrentStateName { get; private set; }

  public void AddState(string name, SpriteAnimation animation)
  {
    animation.Name = name;
    _animations[name] = animation;
  }

  public void AddTransition(string from, string to, SpriteAnimationControllerTransitionTrigger trigger)
  {
    _transitions.Add(new SpriteAnimationControllerTransition
    {
      From = _animations[from],
      To = _animations[to],
      Trigger = trigger
    });
  }

  public void SetState(string name)
  {
    CurrentStateName = name;
  }

  public void Update(float delta)
  {
    _animations[CurrentStateName].Update(delta);
  }
}