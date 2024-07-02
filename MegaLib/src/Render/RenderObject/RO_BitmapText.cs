using System.Collections.Generic;
using System.Security.Cryptography;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.Text;

namespace MegaLib.Render.RenderObject;

public class RO_BitmapText : RO_Base
{
  public BitmapFont Font;

  private string _text = "";
  private bool _isChanged;

  public ListGPU<Vector3> VertexList;
  public ListGPU<Vector2> UV0List;
  public ListGPU<uint> IndexList;

  public RO_BitmapText()
  {
    Transform = new Transform();
    VertexList = [];
    UV0List = [];
    IndexList = [];
  }

  public string Text
  {
    get => _text;
    set
    {
      _text = value;
      _isChanged = true;
    }
  }

  private void Build()
  {
    VertexList.Clear();
    UV0List.Clear();
    IndexList.Clear();

    var offset = new Vector3(0, 0, 0);
    var indexOffset = 0u;

    for (var i = 0; i < _text.Length; i++)
    {
      var symbol = _text[i];
      var area = Font.GetSymbol(symbol);
      if (area.Area.IsEmpty) continue;

      // Console.WriteLine(area.Area.Width);

      VertexList.AddRange([
        new Vector3(0, 0, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset,
        new Vector3(1, 0, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset,
        new Vector3(1, 1, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset,
        new Vector3(0, 1, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset
      ]);

      var uv = area.Area.ToUV(256, 256);

      UV0List.AddRange([
        new Vector2(uv.FromX, uv.FromY),
        new Vector2(uv.ToX, uv.FromY),
        new Vector2(uv.ToX, uv.ToY),
        new Vector2(uv.FromX, uv.ToY)
      ]);
      IndexList.AddRange([
        0 + indexOffset, 1 + indexOffset, 2 + indexOffset, 0 + indexOffset, 2 + indexOffset, 3 + indexOffset
      ]);
      indexOffset += 4;

      offset.X += area.Area.Width;
    }
  }

  public override void Update(float delta)
  {
    if (!_isChanged) return;
    Build();
    // Console.WriteLine("X");
    _isChanged = false;
  }
}