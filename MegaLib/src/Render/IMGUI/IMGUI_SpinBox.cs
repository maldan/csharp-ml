using System;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_SpinBox : IMGUI_Element
{
  private IMGUI_Button _left = new();
  private IMGUI_Text _text = new();
  private IMGUI_Button _right = new();
  private IMGUI_Container _container = new();
  public float Value = 0;
  public float Step = 0;

  public IMGUI_SpinBox()
  {
    _container.FlexDirection = FlexDirection.Row;
    _container.Elements.Add(_left);
    _container.Elements.Add(_text);
    _container.Elements.Add(_right);

    _left.Text = "-";
    _text.Text = "";
    _text.TextColor = new Vector4(0.75f, 0.75f, 0.75f, 1f);
    _right.Text = "+";

    _left.OnClick = () => { Value -= Step; };
    _right.OnClick = () => { Value += Step; };
  }

  public override IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };

    _text.Text = $"{Value:F}";

    _container.Position = Position;
    _container.Size = Size;
    buildArgs.IndexOffset = _container.Build(buildArgs).IndexOffset;
    CopyRenderDataFrom(_container);

    return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };
  }
}