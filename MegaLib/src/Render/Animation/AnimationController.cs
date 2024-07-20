using System.Collections.Generic;
using System.Linq;
using MegaLib.Runtime;

namespace MegaLib.Render.Animation;

public struct AnimationTransition
{
  public string From;
  public string To;
  public string ConditionString;
  public RuntimeExpression Condition;
}

public class AnimationController
{
  public Dictionary<string, object> Vars = new();
  public Dictionary<string, Animation> Animations = new();
  public Animation CurrentAnimation { get; private set; }
  private string _defaultAnimationName = "";
  public List<AnimationTransition> Transitions { get; private set; } = [];
  private bool _isTransitionTime;

  public AnimationController Clone()
  {
    var ac = new AnimationController();
    foreach (var (k, v) in Vars) ac.AddVar(k, v);
    foreach (var (k, v) in Animations) ac.AddAnimation(k, v.Clone());
    foreach (var t in ac.Transitions) ac.AddTransition(t.From, t.To, t.ConditionString);
    ac.SetDefaultAnimation(_defaultAnimationName);
    return ac;
  }

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
      Transitions.Add(new AnimationTransition
      {
        From = from,
        To = toState,
        ConditionString = condition,
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
    var trans = Transitions.FindAll(x => x.From == CurrentAnimation.Name);
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