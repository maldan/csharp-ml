using System;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Text : IMGUI_Element
{
  public string Text;
  public Func<string> OnText;

  public override uint Build(uint indexOffset = 0)
  {
    Clear();

    if (OnText != null)
      indexOffset = DoText(Position, OnText.Invoke(), indexOffset);
    else
      indexOffset = DoText(Position, Text, indexOffset);


    return indexOffset;
  }
}