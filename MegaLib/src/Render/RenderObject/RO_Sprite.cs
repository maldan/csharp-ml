using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Buffer;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.RenderObject;

public class RO_Sprite : RO_Base
{
  public string Name;

  public ListGPU<Vector3> VertexList;
  public ListGPU<Vector2> UV0List;
  public ListGPU<uint> IndexList;

  public Texture_2D<RGBA8> Texture;

  public RGBA32F Tint = new(1, 1, 1, 1);

  public Vector3 Pivot = new();

  public float Width;
  public float Height;

  public RO_Sprite()
  {
    Transform = new Transform();
    VertexList =
    [
      new Vector3(-0.5f, -0.5f, 0),
      new Vector3(-0.5f, 0.5f, 0),
      new Vector3(0.5f, 0.5f, 0),
      new Vector3(0.5f, -0.5f, 0)
    ];
    UV0List =
    [
      new Vector2(0, 0),
      new Vector2(0, 1),
      new Vector2(1, 1),
      new Vector2(1, 0)
    ];
    IndexList = [0, 1, 2, 0, 2, 3];
    Texture = new Texture_2D<RGBA8>(1, 1)
    {
      RAW =
      {
        [0] = new RGBA8(255, 255, 255, 255)
      }
    };
  }

  public void Repeat(int x, int y)
  {
    SetUVRect(0, 0, Width * x, Height * y, Width, Height);
  }

  public void SetUVRect(float x, float y, float width, float height, float maxWidth, float maxHeight)
  {
    var uMin = x / maxWidth;
    var vMin = y / maxHeight;
    var uMax = (x + width) / maxWidth;
    var vMax = (y + height) / maxHeight;

    UV0List[0] = new Vector2(uMin, vMin);
    UV0List[1] = new Vector2(uMin, vMax);
    UV0List[2] = new Vector2(uMax, vMax);
    UV0List[3] = new Vector2(uMax, vMin);

    Width = width;
    Height = height;
  }

  public override void Update(float delta)
  {
    VertexList[0] = (new Vector3(0, 0, 0) + Pivot) * new Vector3(Width, Height, 1);
    VertexList[1] = (new Vector3(0, 1, 0) + Pivot) * new Vector3(Width, Height, 1);
    VertexList[2] = (new Vector3(1, 1, 0) + Pivot) * new Vector3(Width, Height, 1);
    VertexList[3] = (new Vector3(1, 0, 0) + Pivot) * new Vector3(Width, Height, 1);
  }
}