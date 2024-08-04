using System;
using System.Collections.Generic;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.IMGUI;

public class IMGUI_Container : IMGUI_Element
{
  public List<IMGUI_Element> Elements = [];
  public float Gap = 5;
  public FlexDirection FlexDirection = FlexDirection.Column;

  public IMGUI_Container()
  {
    Padding = 5;
  }

  public override uint Build(uint indexOffset = 0)
  {
    Clear();
    if (!IsVisible) return indexOffset;

    // Вертикальный контейнер, сверху вниз
    if (FlexDirection == FlexDirection.Column)
    {
      var p = new Vector3(Position.X + Padding, Position.Y + Padding, Position.Z);
      var totalH = 0f;
      for (var i = 0; i < Elements.Count; i++)
      {
        // Пропускаем невидимые
        if (!Elements[i].IsVisible) continue;

        Elements[i].Position = p;
        var h = Elements[i].Size.Y > 0 ? Elements[i].Size.Y : 20;
        Elements[i].Size = new Vector2(Size.X - Padding * 2, h);
        indexOffset = Elements[i].Build(indexOffset);
        p.Y += h;
        p.Y += Gap;
        totalH += h + Gap;

        Vertices.AddRange(Elements[i].Vertices);
        UV.AddRange(Elements[i].UV);
        Colors.AddRange(Elements[i].Colors);
        Indices.AddRange(Elements[i].Indices);
      }

      Size.Y = totalH + Padding * 2;
    }
    else
    {
      // Горизонтальный
      var eachItemWidth = (Size.X - Padding * 2) / Elements.Count;
      var p = new Vector3(Position.X + Padding, Position.Y + Padding, 0.0001f);
      for (var i = 0; i < Elements.Count; i++)
      {
        Elements[i].Position = p;
        Elements[i].Size = new Vector2(eachItemWidth, 20);
        indexOffset = Elements[i].Build(indexOffset);
        p.X += eachItemWidth;

        Vertices.AddRange(Elements[i].Vertices);
        UV.AddRange(Elements[i].UV);
        Colors.AddRange(Elements[i].Colors);
        Indices.AddRange(Elements[i].Indices);
      }
    }

    return indexOffset;
  }
}