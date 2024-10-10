using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Texture;
using MegaLib.Render.UI.EasyUI;

namespace MegaLib.Render.Layer;

public class Layer_EasyUI : Layer_Base
{
  public Camera_Orthographic Camera;
  public Texture_2D<RGBA<byte>> FontTexture;

  private FontData _fontData;
  private Stack<EasyUI_Element> _currentElement = new();
  private EasyUI_Element _root;
  private List<EasyUI_RenderData> _renderData = [];

  public EasyUI_Element FocusedElement;
  public Stack<EasyUI_Element> ScrollElementStack = new();
  public List<EasyUI_Element> HoverElementList = new();

  public EasyUI_Element CollisionOverElementList = null;

  private List<Action> _delayedCalls = [];

  public Layer_EasyUI()
  {
    _fontData = Font.Generate("Consolas", 16, 1);
    FontTexture = _fontData.Texture;

    FontTexture.Options.UseMipMaps = false;
    FontTexture.Options.WrapMode = TextureWrapMode.Clamp;
    FontTexture.Options.FiltrationMode = TextureFiltrationMode.Nearest;

    _root = new EasyUI_Element();
    _currentElement.Push(_root);
  }

  public EasyUI_Label Label(string text)
  {
    return Add<EasyUI_Label>(label => { label.Text = text; });
  }

  public EasyUI_Window Window(string title, Action<EasyUI_Window> onInit = null)
  {
    var win = Add(onInit);
    win.Title = title;
    return win;
  }

  public EasyUI_Window WindowWithScroll(string title, Action<EasyUI_Window, EasyUI_Layout> onInit = null)
  {
    var win = Add<EasyUI_Window>(window =>
    {
      Add<EasyUI_ScrollPane>(scroll =>
      {
        Add<EasyUI_Layout>(layout =>
        {
          onInit?.Invoke(window, layout);

          window.Events.OnRender += (d) =>
          {
            scroll.Style.Width = window.Style.Width;
            scroll.Style.Height = window.Style.Height;

            layout.Style.Width = scroll.ContentWidth();
            // layout.Style.Height = window.Style.Height;
          };
        });
      });
    });
    win.Title = title;

    return win;
  }

  public EasyUI_Button Button(string title, Action onClick)
  {
    var btn = Add<EasyUI_Button>();
    btn.Text = title;
    btn.Events.OnClick += onClick;
    return btn;
  }

  public EasyUI_TextInput TextInput(TextInputType type, Action<EasyUI_TextInput> onInit = null)
  {
    var textInput = Add(onInit);
    textInput.InputType = type;
    return textInput;
  }

  public EasyUI_Slider Slider(float min, float max, Action<float> onChange = null)
  {
    var slider = Add<EasyUI_Slider>();
    slider.Min = min;
    slider.Max = max;
    slider.Events.OnChange += o =>
    {
      if (o is float f) onChange?.Invoke(f);
    };
    return slider;
  }

  public T Add<T>(Action<T> onInit = null) where T : EasyUI_Element, new()
  {
    var cnt = _currentElement.Peek();
    var element = new T();
    cnt.Add(element);

    // Проверка типа T
    _currentElement.Push(element);
    onInit?.Invoke(element);
    _currentElement.Pop();
    return element;
  }

  public List<EasyUI_RenderData> Build(float delta)
  {
    _renderData.Clear();
    CollisionOverElementList = null;
    var root = _currentElement.Peek();
    var changes = new List<int>();
    root.Build(new EasyUI_BuildIn
    {
      Delta = delta,
      FontData = _fontData,
      LayerEasyUi = this,
      RenderData = _renderData,
      Changes = changes,
      IsParentChanged = false
    });

    Console.WriteLine($"Changes: {changes.Count} | Elements: {_renderData.Count}");
    _delayedCalls.ForEach(x => { x.Invoke(); });
    _delayedCalls.Clear();

    return _renderData;
  }

  public void AtTop(EasyUI_Element me, bool isDelayed = true)
  {
    if (isDelayed)
    {
      _delayedCalls.Add(() => { AtTop(me, false); });
      return;
    }

    if (_root.Children.Contains(me))
    {
      var index = _root.Children.IndexOf(me);
      var a = _root.Children[^1];
      var b = _root.Children[index];
      _root.Children[index] = a;
      _root.Children[^1] = b;
    }
  }

  public bool CheckHover(EasyUI_Element me)
  {
    if (HoverElementList.Count == 0) return false;
    return HoverElementList.Last() == me;
  }
}