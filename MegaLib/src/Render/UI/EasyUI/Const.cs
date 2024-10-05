using System;
using System.Collections.Generic;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Layer;

namespace MegaLib.Render.UI.EasyUI;

public struct EasyUI_BuildIn
{
  public FontData FontData;
  public EasyUI_Element Parent;
  public float Delta;
  public Vector2 ParentPosition;

  public List<Rectangle> StencilRectangleStack;

  public Layer_EasyUI LayerEasyUi;

  // public byte ParentStencilId;
}

public enum TextInputType
{
  Text,
  Integer,
  Float
}

public struct EasyUI_BuildOut
{
}

[Flags]
public enum TextAlignment
{
  Left = 1,
  Right = 2,
  Top = 4,
  Bottom = 8,

  HorizontalCenter = 16,
  VerticalCenter = 32,
  Center = 64
}

public enum Direction
{
  Vertical,
  Horizontal
}