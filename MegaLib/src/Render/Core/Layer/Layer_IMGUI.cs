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

  private List<Vector3> _vertices = [];
  private List<Vector4> _colors = [];
  private List<Vector2> _uv = [];
  private List<uint> _indices = [];

  private List<IMGUI_Window> _windows = [];
  private Stack<IMGUI_Window> _currentWindow = new();

  private Dictionary<string, IMGUI_Element> _elements = new();

  public Layer_IMGUI()
  {
    _fontData = Font.Generate("Consolas", 14);
    FontTexture = _fontData.Texture;

    FontTexture.Options.UseMipMaps = false;
    FontTexture.Options.WrapMode = TextureWrapMode.Clamp;
    FontTexture.Options.FiltrationMode = TextureFiltrationMode.Linear;
    // FontTexture.SaveToBMP("baka.bmp");
  }

  public void GetElement()
  {
  }

  public void Text(string id, Func<string> text)
  {
    var win = _currentWindow.Peek();
    var txt = new IMGUI_Text
    {
      Id = id,
      FontData = _fontData,
      OnText = text
    };
    win.Elements.Add(txt);
    _elements[id] = txt;
  }

  public void Text(string id, string text)
  {
    var win = _currentWindow.Peek();
    var txt = new IMGUI_Text
    {
      Id = id,
      FontData = _fontData,
      Text = text
    };
    win.Elements.Add(txt);
    _elements[id] = txt;
  }

  public void Button(string text, Action onClick)
  {
    var win = _currentWindow.Peek();
    win.Elements.Add(new IMGUI_Button()
    {
      FontData = _fontData,
      Text = text,
      OnClick = onClick
    });
  }

  public void Pipi(float initial, float step, Action<float> onChange)
  {
    // var win = _currentWindow.Peek();
    BeginWindow("1", "1");

    _currentWindow.Peek().IsVerticalContent = false;
    _currentWindow.Peek().HeaderHeight = 0;
    _currentWindow.Peek().Padding = 0;
    _currentWindow.Peek().Size.Y = 0;

    var value = initial;
    Button("-", () =>
    {
      value -= step;
      onChange(value);
    });
    Text("yy", () => { return $"{value:F}"; });
    Button("+", () =>
    {
      value += step;
      onChange(value);
    });

    var cw = _currentWindow.Peek();

    EndWindow();

    _currentWindow.Peek().Elements.Add(cw);
    _windows.Remove(cw);

    /*win.Elements.Add(new IMGUI_Check()
    {
      FontData = _fontData,
      Text = text,
      OnClick = onClick,
      IsActive = initial
    });*/
  }

  public void Check(string text, bool initial, Func<bool, bool> onClick)
  {
    var win = _currentWindow.Peek();
    win.Elements.Add(new IMGUI_Check()
    {
      FontData = _fontData,
      Text = text,
      OnClick = onClick,
      IsActive = initial
    });
  }

  public void BeginWindow(string id, string name)
  {
    var win = new IMGUI_Window
    {
      FontData = _fontData,
      Id = id,
      Title = name,
      Position = new Vector3(10 + _windows.Count * 140, 10, 0),
      Size = new Vector2(128, 128)
    };
    _windows.Add(win);
    _currentWindow.Push(win);
  }

  public void EndWindow()
  {
    _currentWindow.Pop();
  }

  public (Vector3[], Vector2[], Vector4[], uint[]) Build()
  {
    _vertices.Clear();
    _colors.Clear();
    _uv.Clear();
    _indices.Clear();

    uint indexOffset = 0;
    for (var i = 0; i < _windows.Count; i++)
    {
      indexOffset = _windows[i].Build(indexOffset);
      _vertices.AddRange(_windows[i].Vertices);
      _uv.AddRange(_windows[i].UV);
      _colors.AddRange(_windows[i].Colors);
      _indices.AddRange(_windows[i].Indices);
    }

    return (_vertices.ToArray(), _uv.ToArray(), _colors.ToArray(), _indices.ToArray());
  }
}