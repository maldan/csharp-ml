using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
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
  public bool IsResizingElement;

  public EasyUI_Element CollisionOverElementList = null;

  private List<Action> _delayedCalls = [];

  public bool HasFocusedField => FocusedElement != null;
  public bool HasFocusedScroll => ScrollElementStack.Count > 0;
  public bool HasHoveredElement => HoverElementList.Count > 0;

  public Vector3 PointerPosition;
  public Vector3 PointerPositionDelta;
  public bool IsPointerPrimaryDown;

  // private List<EasyUI_Element> _objectList = [];

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

  public EasyUI_TextInput TextInput(Func<float> onRead = null, Action<float> onWrite = null)
  {
    var textInput = Add<EasyUI_TextInput>();
    textInput.InputType = TextInputType.Float;
    textInput.OnRead(onRead);
    textInput.OnWrite(onWrite);
    return textInput;
  }

  public EasyUI_TextInput TextInput(Func<string> onRead = null, Action<string> onWrite = null)
  {
    var textInput = Add<EasyUI_TextInput>();
    textInput.InputType = TextInputType.Text;
    textInput.OnRead(onRead);
    textInput.OnWrite(onWrite);
    return textInput;
  }

  public EasyUI_VectorInput VectorInput(Func<Vector3> onRead = null, Action<Vector3> onWrite = null)
  {
    var textInput = Add<EasyUI_VectorInput>();
    textInput.OnRead(onRead);
    textInput.OnWrite(onWrite);
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

  public EasyUI_Check CheckBox(bool value, Action<bool> onChange = null)
  {
    var check = Add<EasyUI_Check>();
    check.Value = value;
    check.Events.OnChange += o =>
    {
      if (o is bool f) onChange?.Invoke(f);
    };
    return check;
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

  /*public T Create<T>() where T : EasyUI_Element, new()
  {
    var element = new T();
    return element;
  }*/

  public List<EasyUI_RenderData> Build(float delta)
  {
    PointerPosition = new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, 0);
    PointerPositionDelta = new Vector3(Mouse.ClientDelta.X, Mouse.ClientDelta.Y, 0);
    IsPointerPrimaryDown = Mouse.IsKeyDown(MouseKey.Left);

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

    // Console.WriteLine($"Changes: {changes.Count} | Elements: {_renderData.Count}");
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


  /*public void Add(EasyUI_Element element)
  {
    _objectList.Add(element);
  }

  public void Remove(EasyUI_Element element)
  {
    _objectList.Remove(element);
  }*/
}