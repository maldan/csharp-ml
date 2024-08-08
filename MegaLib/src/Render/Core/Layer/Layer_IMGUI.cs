using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;
using MegaLib.Render.Color;
using MegaLib.Render.IMGUI;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Core.Layer;

public class Layer_IMGUI : Layer_Base
{
  public Camera_Orthographic Camera;
  public Texture_2D<RGBA<byte>> FontTexture;

  private FontData _fontData;
  private Stack<IMGUI_Element> _currentElement = new();

  public Layer_IMGUI()
  {
    _fontData = Font.Generate("Consolas", 15);
    FontTexture = _fontData.Texture;

    FontTexture.Options.UseMipMaps = false;
    FontTexture.Options.WrapMode = TextureWrapMode.Clamp;
    FontTexture.Options.FiltrationMode = TextureFiltrationMode.Linear;
    // FontTexture.SaveToBMP("baka.bmp");

    _currentElement.Push(new IMGUI_Element()
    {
      Style = new ElementStyle()
      {
        //Width = "100",
        //Height = "100",
        //BackgroundColor = "red"
        //Left = "10",
        //Top = "5"
      }
    });
  }

  /*public void Button(string text, Action<IMGUI_Button> onInit = null)
  {
    var cnt = _currentContainer.Peek();
    var btn = new IMGUI_Button
    {
      FontData = _fontData,
      Text = text
    };
    cnt.Elements.Add(btn);
    onInit?.Invoke(btn);
  }*/

  /*public void Add<T>(Action<T> onInit = null) where T : IMGUI_Element, new()
  {
    var cnt = _currentContainer.Peek();
    var element = new T
    {
      FontData = _fontData
    };
    cnt.Elements.Add(element);

    // Проверка типа T
    if (typeof(T) == typeof(IMGUI_Tree))
    {
      _currentContainer.Push((element as IMGUI_Tree)?.Content);
    }

    onInit?.Invoke(element);

    if (typeof(T) == typeof(IMGUI_Tree))
    {
      _currentContainer.Pop();
    }
  }*/

  /*public void BeginWindow(string name)
  {
    var win = new IMGUI_Window
    {
      FontData = _fontData,
      Title = name,
      Position = new Vector3(10 + _windows.Count * 140, 10, 0),
      Size = new Vector2(128, 128)
    };
    _windows.Add(win);
    _currentWindow.Push(win);
    _currentContainer.Push(win.Content);
  }

  public void BeginContainer(FlexDirection flexDirection)
  {
    var currentContainer = _currentContainer.Peek();
    var cnt = new IMGUI_Container
    {
      FlexDirection = flexDirection
    };
    _currentContainer.Push(cnt);
    currentContainer.Elements.Add(cnt);
  }

  public void EndContainer()
  {
    _currentContainer.Pop();
  }*/

  /*public void EndWindow()
  {
    _currentWindow.Pop();
  }*/

  public void Add<T>(Action<T> onInit = null) where T : IMGUI_Element, new()
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

  public List<RenderData> Build(float delta)
  {
    var root = _currentElement.Peek();
    root.Build(new BuildIn
    {
      Delta = delta,
      FontData = _fontData
    });

    return root.RenderData;
  }

  /*public (Vector3[], Vector2[], Vector4[], uint[]) Build()
  {
    _vertices.Clear();
    _colors.Clear();
    _uv.Clear();
    _indices.Clear();

    // uint indexOffset = 0;
    var args = new IMGUI_BuildArgs() { FontData = _fontData };
    for (var i = 0; i < _windows.Count; i++)
    {
      args.IndexOffset = _windows[i].Build(args).IndexOffset;
      _vertices.AddRange(_windows[i].Vertices);
      _uv.AddRange(_windows[i].UV);
      _colors.AddRange(_windows[i].Colors);
      _indices.AddRange(_windows[i].Indices);
    }

    return (_vertices.ToArray(), _uv.ToArray(), _colors.ToArray(), _indices.ToArray());
  }*/
}