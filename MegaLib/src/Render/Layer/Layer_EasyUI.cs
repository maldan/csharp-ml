using System;
using System.Collections.Generic;
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

  public Layer_EasyUI()
  {
    _fontData = Font.Generate("Consolas", 15, 1);
    FontTexture = _fontData.Texture;

    FontTexture.Options.UseMipMaps = false;
    FontTexture.Options.WrapMode = TextureWrapMode.Clamp;
    FontTexture.Options.FiltrationMode = TextureFiltrationMode.Linear;

    _currentElement.Push(new EasyUI_Element()
    {
      Style = new EasyUI_ElementStyle()
      {
      }
    });
  }

  public void Add<T>(Action<T> onInit = null) where T : EasyUI_Element, new()
  {
    var cnt = _currentElement.Peek();
    var element = new T
    {
      // FontData = _fontData
    };
    cnt.Children.Add(element);

    // Проверка типа T
    _currentElement.Push(element);
    onInit?.Invoke(element);
    _currentElement.Pop();
  }

  public List<EasyUI_RenderData> Build(float delta)
  {
    var root = _currentElement.Peek();
    root.Build(new EasyUI_BuildIn
    {
      Delta = delta,
      FontData = _fontData
    });

    return root.RenderData;
  }
}