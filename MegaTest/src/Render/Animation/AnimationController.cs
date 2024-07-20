using System;
using System.Collections.Generic;
using System.Threading;
using MegaLib.Audio;
using MegaLib.Render.Animation;
using NUnit.Framework;

namespace MegaTest.Render.Animation;

public class AnimationControllerTest
{
  [Test]
  public void TestBasic()
  {
    var ac = new AnimationController();

    ac.AddAnimation("Idle", new MegaLib.Render.Animation.Animation());
    ac.AddAnimation("Walk", new MegaLib.Render.Animation.Animation());
    ac.SetDefaultAnimation("Idle");

    ac.AddVar("IsWalk", false);
    ac.SetVar("IsWalk", false);

    ac.AddTransition("Idle", "Walk", "IsWalk == true");
    ac.AddTransition("Walk", "Idle", "IsWalk == false");

    ac.Tick(0.016f);

    Console.WriteLine(ac.CurrentAnimation.Name);
  }
}