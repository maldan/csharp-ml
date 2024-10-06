using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Check : EasyUI_Element
{
  private EasyUI_Element _text;
  // public bool Value;

  public EasyUI_Check()
  {
    var buttonWidth = 24;
    Style.Height = 21;

    var baseColor = "#232323";
    var leftCheck = new EasyUI_Element();
    leftCheck.Style.Width = 21;
    leftCheck.Style.Height = 21;
    //leftCheck.Style.BackgroundColor = baseColor;
    Children.Add(leftCheck);
    leftCheck.Events.OnClick += () =>
    {
      if (Value is bool v) Value = !v;
    };

    var isOver = false;
    leftCheck.Events.OnMouseOver += () =>
    {
      isOver = true;
      //leftCheck.Style.BackgroundColor = "#262626";
    };
    leftCheck.Events.OnMouseOut += () =>
    {
      isOver = false;
      //leftCheck.Style.BackgroundColor = baseColor;
    };

    leftCheck.Events.OnRender += (delta) =>
    {
      if (isOver) Mouse.Cursor = MouseCursor.Pointer;
    };

    var check = new EasyUI_Element();
    check.Style.Width = 21 - 8;
    check.Style.Height = 21 - 8;
    check.Style.X = 4;
    check.Style.Y = 4;
    // check.Style.BackgroundColor = "#ae5c00";
    Children.Add(check);

    _text = new EasyUI_Element();
    _text.Style.Width = buttonWidth * 2;
    _text.Style.Height = 24;
    _text.Style.X = buttonWidth;
    _text.Style.TextAlignment = TextAlignment.Center;
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