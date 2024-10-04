using System;
using System.Collections.Generic;
using System.Globalization;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Label : EasyUI_Element
{
  public EasyUI_Label()
  {
    Style.BackgroundColor = new Vector4(0, 0, 0, 0);
    Style.TextAlign = TextAlignment.Left | TextAlignment.VerticalCenter;
    Text = "";
    Style.Width = 20;
    Style.Height = 14;
    Style.TextColor = "#c0c0c0";
  }
}