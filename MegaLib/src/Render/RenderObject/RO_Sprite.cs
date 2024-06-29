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

  public Texture_2D<RGBA<byte>> Texture;

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
    Texture = new Texture_2D<RGBA<byte>>(1, 1)
    {
      RAW =
      {
        [0] = new RGBA<byte>(255, 255, 255, 255)
      }
    };
  }

  public override void Update(float delta)
  {
    VertexList[0] = new Vector3(-0.5f * Width, -0.5f * Height, 0);
    VertexList[1] = new Vector3(-0.5f * Width, 0.5f * Height, 0);
    VertexList[2] = new Vector3(0.5f * Width, 0.5f * Height, 0);
    VertexList[3] = new Vector3(0.5f * Width, -0.5f * Height, 0);
  }
}