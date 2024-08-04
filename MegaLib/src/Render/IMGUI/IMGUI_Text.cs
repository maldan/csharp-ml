using System;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Text : IMGUI_Element
{
  public string Text;

  public IMGUI_Text()
  {
    TextColor = new Vector4(1, 1, 1, 1);
  }

  public override IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };

    FontData = buildArgs.FontData;

    buildArgs.IndexOffset = DoText(Position, Text, TextColor, buildArgs.IndexOffset);

    return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset, Height = 20 };
  }
}