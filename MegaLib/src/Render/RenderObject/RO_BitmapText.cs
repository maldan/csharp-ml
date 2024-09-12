using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;
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
  public ListGPU<RGBA<float>> ColorList;

  public RO_BitmapText()
  {
    Transform = new Transform();
    VertexList = [];
    UV0List = [];
    IndexList = [];
    ColorList = [];
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
    ColorList.Clear();

    var offset = new Vector3(0, 0, 0);
    var indexOffset = 0u;
    var maxLineHeight = 0f;
    var color = new RGBA<float>(1, 1, 1, 1);

    for (var i = 0; i < _text.Length; i++)
    {
      if (_text[i] == '\x1B' && _text[i + 1] == '[')
      {
        // Full reset
        if (_text[i + 2] == '0' && _text[i + 3] == 'm')
        {
          i += 3;
          color = new RGBA<float>(1, 1, 1, 1);
          Font.FontSize = Font.DefaultStyle.Size;
          Font.FontStyle = BitmapFontStyleType.Normal;
          continue;
        }

        // Color
        if (_text[i + 2] == '3' && _text[i + 3] == '8' && _text[i + 4] == ';' && _text[i + 5] == '2' &&
            _text[i + 6] == ';')
        {
          i += 7;
          var str = "";
          for (var j = 0; j < 12; j++)
          {
            if (_text[i + j] == 'm') break;
            str += _text[i + j];
          }

          i += str.Length;

          var rgb = str.Split(";");
          color = new RGBA<float>(
            float.Parse(rgb[0]) / 255f,
            float.Parse(rgb[1]) / 255f,
            float.Parse(rgb[2]) / 255f, 1);

          continue;
        }

        // Font size
        if (_text[i + 3] == 's')
        {
          Font.FontSize = int.Parse(_text[i + 2] + "");
          i += 3;
          continue;
        }

        if (_text[i + 4] == 's')
        {
          Font.FontSize = int.Parse(_text[i + 2] + "" + _text[i + 3]);
          i += 4;
          continue;
        }

        // Italic
        if (_text[i + 2] == '3' && _text[i + 3] == 'm')
        {
          i += 3;
          Font.FontStyle = BitmapFontStyleType.Italic;
          continue;
        }

        // Bold
        if (_text[i + 2] == '1' && _text[i + 3] == 'm')
        {
          i += 3;
          Font.FontStyle = BitmapFontStyleType.Bold;
          continue;
        }
      }

      var symbol = _text[i];
      if (symbol == '\n')
      {
        offset.X = 0;
        offset.Y += maxLineHeight;
        maxLineHeight = 0;
        continue;
      }

      var area = Font.GetSymbol(symbol);
      if (area.Area.IsEmpty) continue;

      // Console.WriteLine(area.Area.Width);

      VertexList.AddRange([
        new Vector3(0, 0, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset,
        new Vector3(1, 0, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset,
        new Vector3(1, 1, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset,
        new Vector3(0, 1, 0) * new Vector3(area.Area.Width, area.Area.Height, 1) + offset
      ]);
      ColorList.AddRange([color, color, color, color]);

      var uv = area.Area.ToUV(Font.Width, Font.Height);
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

      offset.X += area.Width;
      maxLineHeight = Math.Max(maxLineHeight, area.Area.Height * 0.75f);
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