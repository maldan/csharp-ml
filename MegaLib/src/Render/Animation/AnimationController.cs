using System.Collections.Generic;
using MegaLib.Runtime;

namespace MegaLib.Render.Animation;

public struct AnimationTransition
{
  public string From;
  public string To;
  public RuntimeExpression Condition;
}

public class AnimationController
{
  public Dictionary<string, object> Vars = new();
  public Dictionary<string, Animation> Animations = new();
  public Animation CurrentAnimation { get; private set; }
  private string _defaultAnimationName = "";
  private List<AnimationTransition> _transitions = [];
  private bool _isTransitionTime;

  public void AddVar(string name, object value)
  {
    Vars[name] = value;
  }

  public void SetVar(string name, object value)
  {
    Vars[name] = value;
  }

  public void SetDefaultAnimation(string name)
  {
    _defaultAnimationName = name;
  }

  public void AddAnimation(string name, Animation animation)
  {
    Animations[name] = animation;
    animation.Name = name;
  }

  public void AddTransition(string fromState, string toState, string condition)
  {
    var fromList = fromState.Split(",");
    foreach (var from in fromList)
      _transitions.Add(new AnimationTransition
      {
        From = from,
        To = toState,
        Condition = RuntimeExpression.Compile(condition, Vars)
      });
  }

  public void Tick(float delta)
  {
    // Берем дефолтную анимацию
    if (CurrentAnimation == null) CurrentAnimation = Animations[_defaultAnimationName];

    // Если нет, то идем нахуй
    if (CurrentAnimation == null) return;

    // Тикаем текущую анимацию
    CurrentAnimation.Tick(delta);

    // Пока идет транзиш анимация, не учитываем логику переходов
    if (_isTransitionTime) return;

    // Чекаем транзишены для выхода из нашей анимации в какие-нибудь другие анимации
    var trans = _transitions.FindAll(x => x.From == CurrentAnimation.Name);
    for (var i = 0; i < trans.Count; i++)
      // Чекаем срабатывает ли условие перехода в другую анимацию
      if (trans[i].Condition.Invoke(Vars) is true)
      {
        // Если да то устанавливаем другую анимацию
        // Если есть переход, то сначала создаем переход, а по его завершению включаем уже нужную анимацию
        _isTransitionTime = true;
        CurrentAnimation = CurrentAnimation.CreateBlendToAnimation(Animations[trans[i].To], 0.075f);
        CurrentAnimation.OnEnd = (o) =>
        {
          CurrentAnimation = Animations[trans[i].To];
          CurrentAnimation.Reset();
          _isTransitionTime = false;
        };
        break;
      }
  }
}