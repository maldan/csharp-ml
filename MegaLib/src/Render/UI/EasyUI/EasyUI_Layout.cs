namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Layout : EasyUI_Element
{
  public LayoutDirection Direction = LayoutDirection.Vertical;
  public float Gap;

  public EasyUI_Layout()
  {
    Events.OnRender += (delta) =>
    {
      var w = Width();
      var totalH = 0f;

      for (var i = 0; i < Children.Count; i++)
      {
        Children[i].Style.Width = w - Gap * 2;
        Children[i].Style.Y = Gap + totalH;
        Children[i].Style.X = Gap;
        totalH += Children[i].Height() + Gap;
      }

      Style.Height = totalH;
    };
  }
}