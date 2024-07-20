using System;
using System.Collections.Generic;
using System.Threading;
using MegaLib.Audio;
using MegaLib.Render.Animation;
using NUnit.Framework;

namespace MegaTest.Render.Animation;

public class SpriteAnimationControllerTest
{
  [Test]
  public void TestBasic()
  {
    var ss = new SpriteAnimationController();
    ss.AddState("idle", new SpriteAnimation
    {
      IsLoop = true,
      Frames =
      [
        new SpriteAnimationFrame
        {
          Duration = 0.5f
        },
        new SpriteAnimationFrame
        {
          Duration = 0.5f
        },
        new SpriteAnimationFrame
        {
          Duration = 0.5f
        }
      ]
    });

    ss.AddState("walk", new SpriteAnimation
    {
      IsLoop = true,
      Frames =
      [
        new SpriteAnimationFrame
        {
          Duration = 0.5f
        },
        new SpriteAnimationFrame
        {
          Duration = 0.5f
        },
        new SpriteAnimationFrame
        {
          Duration = 0.5f
        }
      ]
    });

    ss.SetState("idle");
    ss.SetState("walk");

    for (var i = 0; i < 10; i++) ss.Update(0.016f);
  }
}