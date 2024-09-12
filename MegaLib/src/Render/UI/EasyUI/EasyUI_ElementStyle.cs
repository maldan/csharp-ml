using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ElementStyle
{
  public object Width;
  public object Height;
  public object TextColor = new Vector4(1, 1, 1, 1);
  public object BackgroundColor;
  public object X;
  public object Y;
  public string TextAlign;

  public void SetArea(float x, float y, float width, float height)
  {
    X = x;
    Y = y;
    Width = width;
    Height = height;
  }
}