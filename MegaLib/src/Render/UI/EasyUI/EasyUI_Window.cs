using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Window : EasyUI_Element
{
  private EasyUI_Element _header;
  private EasyUI_Element _body;
  private EasyUI_Element _minimize;

  public string Title
  {
    set => _header.Text = value;
    get => _header.Text;
  }

  public EasyUI_Window()
  {
    _header = new EasyUI_Element();
    _header.Style.Y = -20;
    _header.Style.Width = 80;
    _header.Style.Height = 20;
    _header.Style.BackgroundColor = "#232323";
    _header.Style.BorderWidth = 1;
    _header.Style.BorderColor = "#232323";
    _header.Style.TextColor = "#c0c0c0";
    _header.Text = "Window";
    _header.Style.TextAlign = TextAlignment.VerticalCenter;
    // Style.BorderWidth = 1;

    var isDrag = false;
    var isHover = false;
    _header.Events.OnMouseOver += () => { isHover = true; };
    _header.Events.OnMouseOut += () => { isHover = false; };
    _header.Events.OnMouseDown += () =>
    {
      if (isHover)
      {
        // Проверка, что курсор над заголовком
        isDrag = true;
      }
    };
    _header.Events.OnMouseUp += () => { isDrag = false; };
    Children.Add(_header);

    _body = new EasyUI_Element();
    _body.Style.Y = 0;
    _body.Style.Width = 80;
    _body.Style.Height = 40;
    _body.Style.BackgroundColor = "#343434";
    _body.Style.BorderWidth = 1;
    _body.Style.BorderColor = "#232323";
    Children.Add(_body);

    _minimize = new EasyUI_Button();
    _minimize.Style.X = 80 - 16;
    _minimize.Style.Y = -20;
    _minimize.Style.Width = 16;
    _minimize.Style.Height = 20;
    _minimize.Style.TextColor = "#c0c0c0";
    // _minimize.Style.BackgroundColor = "#545454";
    _minimize.Text = "_";
    _minimize.Events.OnClick += () => { _body.IsVisible = !_body.IsVisible; };
    Children.Add(_minimize);

    Events.OnRender += delta =>
    {
      if (isDrag)
      {
        Mouse.Cursor = MouseCursor.Move;
        Style.X = Position().X + Mouse.ClientDelta.X;
        Style.Y = Position().Y + Mouse.ClientDelta.Y;
      }

      if (Position().X < 0) Style.X = 0;
      if (Position().Y < 20) Style.Y = 20;
      if (Position().X + Width() > Window.Current.ClientWidth) Style.X = Window.Current.ClientWidth - Width();
      if (Position().Y + Height() > Window.Current.ClientHeight) Style.Y = Window.Current.ClientHeight - Height();
    };

    SetSize(100, 100);
  }

  public void SetSize(float width, float height)
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
  }

  public override void Add(EasyUI_Element element)
  {
    _body.Add(element);
  }
}