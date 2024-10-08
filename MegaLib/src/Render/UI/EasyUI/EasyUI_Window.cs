using System;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Window : EasyUI_Element
{
  private EasyUI_Element _header;
  private EasyUI_Element _body;
  private EasyUI_Element _minimize;
  private EasyUI_Element _close;
  private EasyUI_Element _headerText;

  public string Title
  {
    set => _headerText.Text = value;
    get => _headerText.Text;
  }

  public new string Text
  {
    set => _body.Text = value;
    get => _body.Text;
  }

  public EasyUI_Window()
  {
    _header = new EasyUI_Element
    {
      Style =
      {
        Y = -20,
        Width = 80,
        Height = 20,
        TextAlignment = TextAlignment.VerticalCenter
      },
      Text = ""
    };
    _header.Style.SetBackgroundColor("#232323");
    _header.Style.SetBorderColor("#232323");
    _header.Style.SetTextColor("#c0c0c0");
    _header.Style.SetBorderWidth(1);
    _header.Style.BorderRadius = new Vector4(6, 6, 0, 0);

    _headerText = new EasyUI_Element();
    _headerText.Style.SetTextColor("#999999");
    _headerText.Style.TextAlignment = TextAlignment.VerticalCenter;
    _header.Add(_headerText);

    // Style.BorderWidth = 1;

    var isDrag = false;
    _header.Events.OnMouseOver += () => { LayerEasyUi.HoverElementList.Add(this); };
    _header.Events.OnMouseOut += () => { LayerEasyUi.HoverElementList.Remove(this); };
    _header.Events.OnMouseDown += () =>
    {
      if (LayerEasyUi.CheckHover(this))
      {
        // Проверка, что курсор над заголовком
        LayerEasyUi.AtTop(this);
        isDrag = true;
      }
    };
    _header.Events.OnMouseUp += () => { isDrag = false; };
    Children.Add(_header);

    _body = new EasyUI_Element
    {
      Style =
      {
        Y = 0,
        Width = 80,
        Height = 40
      }
    };
    _body.Style.SetBackgroundColor("#343434");
    _body.Style.SetBorderWidth(1);
    _body.Style.SetBorderColor("#232323");
    Children.Add(_body);

    _minimize = new EasyUI_Button();
    _minimize.Style.X = 80 - 16;
    _minimize.Style.Y = -20;
    _minimize.Style.Width = 16;
    _minimize.Style.Height = 20;
    _minimize.Style.SetTextColor("#c0c0c0");
    _minimize.Text = "-";
    _minimize.Style.TextAlignment = TextAlignment.Center;
    _minimize.Events.OnClick += () => { _body.IsVisible = !_body.IsVisible; };
    _minimize.Style.SetBorderRadius(0);
    Children.Add(_minimize);

    _close = new EasyUI_Button();
    _close.Style.SetTextColor("#c0c0c0");
    _close.Text = "x";
    _close.Style.Width = 16;
    _close.Style.Height = 20;
    _close.Style.TextAlignment = TextAlignment.Center;
    _close.Events.OnClick += () => { Parent.Remove(this); };
    _close.Style.SetBorderRadius(0);
    Children.Add(_close);

    Events.OnBeforeRender += delta =>
    {
      if (isDrag)
      {
        Mouse.Cursor = MouseCursor.Move;
        Style.X = Style.Position.X + Mouse.ClientDelta.X;
        Style.Y = Style.Position.Y + Mouse.ClientDelta.Y;
      }

      Refresh();
    };

    Style.Width = 100;
    Style.Height = 100;
    // SetSize(100, 100);
    Refresh();
  }

  /*public void SetSize(float width, float height)
  {
    _header.Style.Width = width;

    _minimize.Style.X = width - 16 - 2;
    _minimize.Style.Y = -18;
    _minimize.Style.Width = 16;
    _minimize.Style.Height = 16;

    _body.Style.Width = width;
    _body.Style.Height = height;
    Style.Width = width;
    Style.Height = height;
  }*/

  private void Refresh()
  {
    if (Style.Position.X < 0) Style.X = 0;
    if (Style.Position.Y < 20) Style.Y = 20;
    if (Style.Position.X + Style.Width > Window.Current.ClientWidth) Style.X = Window.Current.ClientWidth - Style.Width;
    if (Style.Position.Y + Style.Height > Window.Current.ClientHeight)
      Style.Y = Window.Current.ClientHeight - Style.Height;

    _close.Style.X = Style.Width - _close.Style.Width - 3;
    _close.Style.Y = -18;
    _close.Style.Width = 16;
    _close.Style.Height = 16;

    _minimize.Style.X = Style.Width - _close.Style.Width - _minimize.Style.Width - 3;
    _minimize.Style.Y = -18;
    _minimize.Style.Width = 16;
    _minimize.Style.Height = 16;

    _header.Style.Width = Style.Width;
    _headerText.Style.Width = _header.Style.Width - 10;
    _headerText.Style.X = 6;
    _headerText.Style.Y = 1;
    _headerText.Style.Height = _header.Style.Height;
    _body.Style.Width = Style.Width;
    _body.Style.Height = Style.Height;
  }

  public override void Add(EasyUI_Element element)
  {
    _body.Add(element);
  }
}