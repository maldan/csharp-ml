using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Window : IMGUI_Element
{
  public string Title;
  public List<IMGUI_Element> Elements = [];
  private bool _isDrag = false;
  private bool _isClick = false;
  private Vector2 _startDrag;
  private Vector3 _startPos;

  public override uint Build(uint indexOffset = 0)
  {
    Clear();

    if (_isDrag)
    {
      var v = Mouse.ClientClamped - _startDrag;
      Position = _startPos + v;
    }

    InitCollision(Rectangle.FromLeftTopWidthHeight(Position.X, Position.Y, Size.X, 15));
    var isHit = CheckCollision();

    // Header
    indexOffset = DoRectangle(new Vector3(Position.X, Position.Y, 0), new Vector2(Size.X, 15),
      new Vector4(0.3f, 0.3f, 0.3f, 1),
      indexOffset);

    if (isHit)
    {
      if (Mouse.IsKeyDown(MouseKey.Left))
        if (!_isClick)
        {
          _isClick = true;
          _isDrag = true;
          _startDrag = Mouse.ClientClamped;
          _startPos = Position;
        }

      for (var i = 0; i < Colors.Count; i++)
        Colors[i] = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
    }
    else
    {
      for (var i = 0; i < Colors.Count; i++)
        Colors[i] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
    }


    if (!Mouse.IsKeyDown(MouseKey.Left))
    {
      _isClick = false;
      _isDrag = false;
    }

    // Body
    indexOffset = DoRectangle(new Vector3(Position.X, Position.Y + 15, 0), Size, new Vector4(0.2f, 0.2f, 0.2f, 1),
      indexOffset);

    var p = new Vector3(Position.X + 1, Position.Y + 1 + 15, 0.0001f);
    for (var i = 0; i < Elements.Count; i++)
    {
      Elements[i].Position = p;
      Elements[i].Size = new Vector2(Size.X - 2, 20);
      indexOffset = Elements[i].Build(indexOffset);
      p.Y += 20;
      p.Y += 1;

      Vertices.AddRange(Elements[i].Vertices);
      UV.AddRange(Elements[i].UV);
      Colors.AddRange(Elements[i].Colors);
      Indices.AddRange(Elements[i].Indices);

      // for (var j = 0; j < Elements[i].Indices.Count; j++) Indices.Add(Elements[i].Indices[j] + maxIndex);
    }

    // Console.WriteLine(Elements.Count);

    return indexOffset;
  }
}