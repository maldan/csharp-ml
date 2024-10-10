using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Select : EasyUI_Element
{
  public List<object> Values = [1, 2, 3, 4, 5, 6];
  public List<string> Labels = ["sex", "rocl", "gavno", "pidor", "gas", "uas"];

  private EasyUI_Element _btn;

  private EasyUI_ScrollPane _scroll;
  private EasyUI_Layout _content;

  private bool _isContentVisible;
  private bool _isOver;

  public EasyUI_Select()
  {
    Style.BackgroundColor = new Vector4(0, 0, 0, 0.25f);
    Style.Width = 100;
    Style.Height = 24;
    Style.SetTextColor("#c0c0c0");
    Style.TextAlignment = TextAlignment.VerticalCenter;
    Style.SetBorderWidth(2f);
    Style.SetBorderColor(new Vector4(0, 0, 0, 0.5f));
    Style.TextOffset = new Vector2(5, 0);

    _btn = new EasyUI_Element();
    _btn.Text = @"▼";
    _btn.Style.Width = 24;
    _btn.Style.Height = 24;
    _btn.Style.SetTextColor("#c0c0c0");
    Events.OnClick += () => { _isContentVisible = !_isContentVisible; };
    Add(_btn);

    _scroll = new EasyUI_ScrollPane();
    _content = new EasyUI_Layout();
    _scroll.Add(_content);
    Add(_scroll);

    Events.OnBeforeRender += delta =>
    {
      _scroll.IsVisible = _isContentVisible;
      Refresh();

      for (var i = 0; i < Values.Count; i++)
      {
        if (Value == Values[i])
        {
          Text = Labels[i] ?? "...";
          break;
        }
      }
    };

    Events.OnRender += delta =>
    {
      /*if (Mouse.IsKeyDown(MouseKey.Left) && !_isOver)
      {
        _isOver = false;
        _isContentVisible = false;
      }*/
    };

    RefreshList();
  }

  private void RefreshList()
  {
    _content.Clear();
    for (var i = 0; i < Labels.Count; i++)
    {
      var el = new EasyUI_Element();
      el.Style.Height = 24;
      el.Style.BorderWidth = new Vector4(0, 0, 1, 0);
      el.Style.SetTextColor("#c0c0c0");
      el.Text = Labels[i];
      el.Style.TextOffset = new Vector2(5, 0);
      el.Style.TextAlignment = TextAlignment.VerticalCenter;

      el.Events.OnMouseOver += () =>
      {
        el.Style.SetBackgroundColor("#fe0000");
        _isOver = true;
      };
      el.Events.OnMouseOut += () =>
      {
        el.Style.SetBackgroundColor("#00000000");
        _isOver = false;
      };
      var i1 = i;
      el.Events.OnClick += () =>
      {
        try
        {
          Value = Values[i1];
        }
        catch
        {
          Value = null;
        }

        _isContentVisible = false;
      };
      _content.Add(el);
    }
  }

  private void Refresh()
  {
    _btn.Style.Width = 16;
    _btn.Style.Height = 16;
    _btn.Style.Y = Style.Height / 2 - _btn.Style.Height / 2;

    _btn.Style.X = Style.Width - 16;

    _scroll.Style.Y = Style.Height;
    _scroll.Style.Width = Style.Width;
    _scroll.Style.Height = 80;
    _content.Style.Width = _scroll.ContentWidth();
  }
}