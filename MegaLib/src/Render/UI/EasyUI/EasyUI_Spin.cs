namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Spin : EasyUI_Element
{
  // public Action<float> OnChange;
  private EasyUI_Element _center;

  public EasyUI_Spin()
  {
    var buttonWidth = 24;

    var leftButton = new EasyUI_Button();
    leftButton.Style.Width = buttonWidth;
    leftButton.Text = "-";
    leftButton.Events.OnClick += () => { Events.OnChange?.Invoke(-1); };

    _center = new EasyUI_Element();
    _center.Style.Width = buttonWidth * 2;
    _center.Style.Height = 24;
    _center.Style.X = buttonWidth;
    _center.Style.TextAlign = TextAlignment.Center;
    _center.Text = $"0";

    var rightButton = new EasyUI_Button();
    rightButton.Style.Width = buttonWidth;
    rightButton.Style.X = buttonWidth + buttonWidth * 2;
    rightButton.Text = "+";
    rightButton.Events.OnClick += () => { Events.OnChange?.Invoke(1); };

    Children.Add(leftButton);
    Children.Add(_center);
    Children.Add(rightButton);
  }

  public void UpdateLabel(string text)
  {
    _center.Text = text;
  }
}