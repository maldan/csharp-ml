using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Check : EasyUI_Element
{
  private EasyUI_Element _text;
  // public bool Value;

  public EasyUI_Check()
  {
    var buttonWidth = 24;

    var leftCheck = new EasyUI_Button();
    leftCheck.Style.Width = 24;
    leftCheck.Style.Height = 24;
    leftCheck.Style.BackgroundColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
    Children.Add(leftCheck);
    leftCheck.Events.OnClick += () =>
    {
      if (Value is bool v) Value = !v;
    };

    var check = new EasyUI_Element();
    check.Style.Width = 24 - 8;
    check.Style.Height = 24 - 8;
    check.Style.X = 4;
    check.Style.Y = 4;
    check.Style.BackgroundColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
    Children.Add(check);

    _text = new EasyUI_Element();
    _text.Style.Width = buttonWidth * 2;
    _text.Style.Height = 24;
    _text.Style.X = buttonWidth;
    _text.Style.TextAlign = TextAlignment.Center;
    _text.Text = $"0";
    Children.Add(_text);

    // Чекаем обновления
    Events.OnRender += (d) =>
    {
      if (Value is bool v) check.IsVisible = v;
    };
  }

  public void UpdateLabel(string text)
  {
    _text.Text = text;
  }
}