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
    Padding = 0;
  }

  public override IMGUI_BuildOut Build(IMGUI_BuildArgs buildArgs)
  {
    Clear();
    if (!IsVisible) return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };

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
        buildArgs.IndexOffset = Elements[i].Build(buildArgs).IndexOffset;
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
      var eachItemWidth = (Size.X - Padding * 2) / Elements.Count - Gap / Elements.Count * (Elements.Count - 1);
      var p = new Vector3(Position.X + Padding, Position.Y + Padding, 0.0001f);
      for (var i = 0; i < Elements.Count; i++)
      {
        Elements[i].Position = p;
        Elements[i].Size = new Vector2(eachItemWidth, 20);
        buildArgs.IndexOffset = Elements[i].Build(buildArgs).IndexOffset;
        p.X += eachItemWidth + Gap;

        Vertices.AddRange(Elements[i].Vertices);
        UV.AddRange(Elements[i].UV);
        Colors.AddRange(Elements[i].Colors);
        Indices.AddRange(Elements[i].Indices);
      }
    }

    return new IMGUI_BuildOut() { IndexOffset = buildArgs.IndexOffset };
  }
}