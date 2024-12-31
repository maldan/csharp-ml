using System;
using System.Collections.Generic;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Texture;
using MegaLib.Voxel;

namespace MegaLib.Render.Layer;

public class Layer_Voxel : Layer_Base
{
  public override int Count => 0;
  public SparseVoxelMap8 VoxelMap;
  public Texture_2D<RGBA8> Texture;
}