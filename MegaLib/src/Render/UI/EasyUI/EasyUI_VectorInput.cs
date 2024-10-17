using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_VectorInput : EasyUI_Element
{
  private List<EasyUI_TextInput> _textInputs = [];
  public bool IsFocused => _textInputs.Any(t => t.IsFocused);

  public float ValueStep
  {
    get => _textInputs[0].ValueStep;
    set
    {
      foreach (var input in _textInputs) input.ValueStep = value;
    }
  }

  public EasyUI_VectorInput()
  {
    Style.Height = 48 + 8;
    Style.SetBorderWidth(2f);
    Style.SetBorderColor(new Vector4(0, 0, 0, 0.25f));
    // Style.BackgroundColor = new Vector4(0, 0, 0, 0.1f);

    var labelLayout = new EasyUI_Layout();
    labelLayout.Direction = Direction.Horizontal;
    labelLayout.IsAdjustChildrenSize = true;
    labelLayout.Style.Height = Style.Height;
    labelLayout.Gap = 5;
    labelLayout.Style.Height = 24;

    var l1 = new EasyUI_Label();
    l1.Text = "X";
    l1.Style.TextAlignment = TextAlignment.VerticalCenter;
    labelLayout.Add(l1);

    var l2 = new EasyUI_Label();
    l2.Text = "Y";
    labelLayout.Add(l2);

    var l3 = new EasyUI_Label();
    l3.Text = "Z";
    labelLayout.Add(l3);

    Children.Add(labelLayout);

    var valueLayout = new EasyUI_Layout
    {
      Direction = Direction.Horizontal,
      IsAdjustChildrenSize = true,
      Style =
      {
        Height = Style.Height
      },
      Gap = 5
    };
    valueLayout.Style.Height = Style.Height - labelLayout.Style.Height;

    var v1 = new EasyUI_TextInput
    {
      InputType = TextInputType.Float
    };
    valueLayout.Add(v1);

    var v2 = new EasyUI_TextInput
    {
      InputType = TextInputType.Float
    };
    valueLayout.Add(v2);

    var v3 = new EasyUI_TextInput
    {
      InputType = TextInputType.Float
    };
    valueLayout.Add(v3);

    _textInputs.Add(v1);
    _textInputs.Add(v2);
    _textInputs.Add(v3);

    Children.Add(labelLayout);
    Children.Add(valueLayout);

    Events.OnRender += delta =>
    {
      labelLayout.Style.Width = Style.Width;
      valueLayout.Style.Y = labelLayout.Style.Height - 5;
      valueLayout.Style.Width = Style.Width;
      Style.Height = 48 + 3;
    };
  }

  public void OnRead(Func<Vector3> read)
  {
    _textInputs[0].OnRead(() => read().X);
    _textInputs[1].OnRead(() => read().Y);
    _textInputs[2].OnRead(() => read().Z);
  }

  public void OnWrite<T>(Action<T> write) where T : struct
  {
    _textInputs[0].OnWrite<float>(v =>
    {
      write((T)(object)new Vector3(
        v,
        _textInputs[1].GetFloatValue(),
        _textInputs[2].GetFloatValue()
      ));
    });
    _textInputs[1].OnWrite<float>(v =>
    {
      write((T)(object)new Vector3(
        _textInputs[0].GetFloatValue(),
        v,
        _textInputs[2].GetFloatValue()
      ));
    });
    _textInputs[2].OnWrite<float>(v =>
    {
      write((T)(object)new Vector3(
        _textInputs[0].GetFloatValue(),
        _textInputs[1].GetFloatValue(),
        v
      ));
    });
  }
}