using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Camera;

namespace MegaLib.Render.Core.Layer;

internal class IMGUI_Window : IMGUI_Element
{
  public string Title;
  public List<IMGUI_Element> Elements = [];

  public override uint Build(uint indexOffset = 0)
  {
    indexOffset = Rect(Position.X, Position.Y, 0, Size.X, Size.Y, indexOffset);

    var p = new Vector3(Position.X + 1, Position.Y + 1, 0.0001f);
    for (var i = 0; i < Elements.Count; i++)
    {
      Elements[i].Position = p;
      Elements[i].Size = new Vector2(Size.X - 2, 20);
      indexOffset = Elements[i].Build(indexOffset);
      p.Y += 20;
      p.Y += 1;

      Vertices.AddRange(Elements[i].Vertices);
      Colors.AddRange(Elements[i].Colors);
      Indices.AddRange(Elements[i].Indices);

      // for (var j = 0; j < Elements[i].Indices.Count; j++) Indices.Add(Elements[i].Indices[j] + maxIndex);
    }

    // Console.WriteLine(Elements.Count);

    return indexOffset;
  }
}

internal class IMGUI_Button : IMGUI_Element
{
  public string Text;

  public override uint Build(uint indexOffset = 0)
  {
    Collision.Add(new Triangle
    {
      A = new Vector3(Position.X, Position.Y, 0),
      B = new Vector3(Position.X + Size.X, Position.Y, 0),
      C = new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0)
    });
    Collision.Add(new Triangle
    {
      A = new Vector3(Position.X, Position.Y + Size.Y, 0),
      B = new Vector3(Position.X, Position.Y, 0),
      C = new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0)
    });

    var ray = new Ray(
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, -10),
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, 10)
    );

    var o = Rect(Position.X, Position.Y, Position.Z, Size.X, Size.Y, indexOffset);
    var isHit = false;
    for (var i = 0; i < Collision.Count; i++)
    {
      Collision[i].RayIntersection(ray, out _, out isHit);
      if (isHit) break;
    }

    if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
        for (var i = 0; i < Colors.Count; i++)
          Colors[i] = new Vector4(0.8f, 0.2f, 0.2f, 1.0f);
      else
        for (var i = 0; i < Colors.Count; i++)
          Colors[i] = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
    }
    else
    {
      for (var i = 0; i < Colors.Count; i++)
        Colors[i] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
    }

    return o;
  }
}

internal class IMGUI_Element
{
  public string Id;
  public Vector3 Position;
  public Vector2 Size;
  public List<Triangle> Collision = [];

  public List<Vector3> Vertices = [];
  public List<Vector4> Colors = [];
  public List<uint> Indices = [];

  public virtual uint Build(uint indexOffset = 0)
  {
    return indexOffset;
  }

  protected uint Rect(float x, float y, float z, float width, float height, uint indexOffset = 0)
  {
    var pivot = new Vector3(0.5f, 0.5f, 0);
    var size = new Vector3(width, height, 1);
    var offset = new Vector3(x, y, z);

    Vertices.AddRange([
      (new Vector3(-0.5f, -0.5f, 0) + pivot) * size + offset,
      (new Vector3(-0.5f, 0.5f, 0) + pivot) * size + offset,
      (new Vector3(0.5f, 0.5f, 0) + pivot) * size + offset,
      (new Vector3(0.5f, -0.5f, 0) + pivot) * size + offset
    ]);
    Colors.AddRange([
      new Vector4(0.5f, 0.5f, 0.5f, 1),
      new Vector4(0.5f, 0.5f, 0.5f, 1),
      new Vector4(1, 1, 1, 1),
      new Vector4(1, 1, 1, 1)
    ]);
    Indices.AddRange([
      0 + indexOffset, 1 + indexOffset, 2 + indexOffset,
      0 + indexOffset, 2 + indexOffset, 3 + indexOffset
    ]);
    return indexOffset + 4;
  }
}

public class Layer_IMGUI : Layer_Base
{
  public Camera_Orthographic Camera;
  private List<Vector3> _vertices = [];
  private List<Vector4> _colors = [];
  private List<uint> _indices = [];

  private List<IMGUI_Window> _windows = [];
  private IMGUI_Window _currentWindow;

  public bool Button(string text)
  {
    // X.Add("button");
    _currentWindow.Elements.Add(new IMGUI_Button()
    {
      Text = text
    });

    return false;
  }

  public void BeginWindow(string id, string name)
  {
    var win = new IMGUI_Window
    {
      Id = id,
      Title = name,
      Position = new Vector3(10, 10, 0),
      Size = new Vector2(64, 64)
    };
    _windows.Add(win);
    _currentWindow = win;
  }

  public void EndWindow()
  {
  }

  public void NewFrame()
  {
    _windows.Clear();
  }

  /*private void Rect(int x, int y, int width, int height)
  {
    var pivot = new Vector3(0.5f, 0.5f, 0);
    var size = new Vector3(width, height, 1);
    var offset = new Vector3(x, y, 0);

    _vertices.AddRange([
      (new Vector3(-0.5f, -0.5f, 0) + pivot) * size + offset,
      (new Vector3(-0.5f, 0.5f, 0) + pivot) * size + offset,
      (new Vector3(0.5f, 0.5f, 0) + pivot) * size + offset,
      (new Vector3(0.5f, -0.5f, 0) + pivot) * size + offset
    ]);
    _colors.AddRange([
      new Vector4(0.5f, 0.5f, 0.5f, 1),
      new Vector4(0.5f, 0.5f, 0.5f, 1),
      new Vector4(1, 1, 1, 1),
      new Vector4(1, 1, 1, 1)
    ]);
    _indices.AddRange([0, 1, 2, 0, 2, 3]);
  }*/

  public (Vector3[], Vector4[], uint[]) Build()
  {
    _vertices.Clear();
    _colors.Clear();
    _indices.Clear();

    /*for (var i = 0; i < X.Count; i++)
      if (X[i] == "window")
        Rect(10, 10, 32, 32);*/

    uint indexOffset = 0;
    for (var i = 0; i < _windows.Count; i++)
    {
      indexOffset = _windows[i].Build(indexOffset);
      _vertices.AddRange(_windows[i].Vertices);
      _colors.AddRange(_windows[i].Colors);
      _indices.AddRange(_windows[i].Indices);
    }

    //for (var i = 0; i < _indices.Count; i++) Console.Write($"{_indices[i]} ");
    //Console.WriteLine("");
    // Console.PrettyPrint(_indices);

    return (_vertices.ToArray(), _colors.ToArray(), _indices.ToArray());
  }
}