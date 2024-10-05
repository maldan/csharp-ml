using System;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_ElementEvents
{
  public Action OnClick;
  public Action OnDoubleClick;
  public Action OnMouseOver;
  public Action OnMouseOut;
  public Action OnMouseDown;
  public Action OnMouseUp;

  public Action OnFocus;
  public Action OnBlur;

  public Action<object> OnChange;
  public Action<float> OnBeforeRender;
  public Action<float> OnRender;
}