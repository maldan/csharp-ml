using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Tabs : EasyUI_Element
{
  private EasyUI_Layout _tabs;

  private Dictionary<string, EasyUI_Button> _tabsButton = new();
  private Dictionary<string, EasyUI_Layout> _tabsContent = new();
  private string _activeTabName = "";

  private EasyUI_Layout _activeTabContent =>
    _tabsContent.ContainsKey(_activeTabName) ? _tabsContent[_activeTabName] : null;

  private EasyUI_Button _activeTab =>
    _tabsButton.ContainsKey(_activeTabName) ? _tabsButton[_activeTabName] : null;

  public EasyUI_Tabs()
  {
    // Добавляем табы
    _tabs = new EasyUI_Layout
    {
      Direction = Direction.Horizontal
    };
    _tabs.Style.Height = 24;
    base.Add(_tabs);

    // Дефолтный размер
    Style.Height = 24;

    Events.OnBeforeRender += (delta) =>
    {
      if (_activeTabContent == null)
      {
        Style.Height = 24;
      }
      else
      {
        _activeTabContent.Style.Width = Style.Width;
        Style.Height = 24 + _activeTabContent.Style.Height;
      }

      if (_activeTab != null)
      {
        _activeTab.Style.SetBackgroundColor("#ae5c00");
      }
    };
  }

  public override void Add(EasyUI_Element element)
  {
    _activeTabContent?.Add(element);
  }

  public void ActivateTab(string name)
  {
    foreach (var (tabName, tab) in _tabsButton)
    {
      tab.Style.SetBackgroundColor("#545454");
    }

    foreach (var (tabName, tab) in _tabsContent)
    {
      tab.IsVisible = false;
    }

    _activeTabName = name;
    if (_activeTabContent != null) _activeTabContent.IsVisible = true;
  }

  public void AddTab(string name)
  {
    var tab = new EasyUI_Button
    {
      Text = name
    };
    tab.Style.Width = 20;
    tab.Style.Height = 24;
    tab.Style.BorderWidth = new Vector4(2, 2, 2, 2);
    tab.Style.BorderRadius = new Vector4(4f, 4f, 0, 0);
    tab.Style.TextAlignment = TextAlignment.Center;
    tab.Events.OnClick += () => { ActivateTab(name); };
    _tabsButton[name] = tab;

    // Табы
    _tabs.Add(tab);

    // Создаем контент для вкладки
    var tabContent = new EasyUI_Layout();
    tabContent.Direction = Direction.Vertical;
    tabContent.Style.SetBorderWidth(2f);
    tabContent.Style.SetBorderColor(new Vector4(0, 0, 0, 0.25f));
    tabContent.Style.Height = 48;
    tabContent.Gap = 5;
    tabContent.Style.Y = 24;

    // Добавляем в основную очередь
    _tabsContent[name] = tabContent;
    base.Add(tabContent);

    ActivateTab(name);
  }
}