using System;
using System.Collections.Generic;
using MegaLib.Ext;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Physics;
using MegaLib.Render.Color;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Skin;

namespace MegaLib.Render.Layer;

public class Layer_Point : Layer_Base
{
  public override int Count => _objectList.Count;
  private List<RO_Point> _objectList = [];

  public void Add(RO_Point obj)
  {
    _objectList.Add(obj);
  }

  public void Remove(RO_Point obj)
  {
    _objectList.Remove(obj);
  }

  public void ForEach(Action<RO_Point> fn)
  {
    for (var i = 0; i < _objectList.Count; i++) fn(_objectList[i]);
  }

  public override void Clear()
  {
    _objectList.Clear();
  }

  public void Draw(VerletPoint point, RGBA<float> color, float size = 1.0f)
  {
    Add(new RO_Point()
    {
      Position = point.Position,
      Color = color,
      Size = size
    });
  }

  public void Draw(Vector3 point, RGBA<float> color, float size = 1.0f)
  {
    Add(new RO_Point()
    {
      Position = point,
      Color = color,
      Size = size
    });
  }

  public void Draw(Vector4 point, RGBA<float> color, float size = 1.0f)
  {
    Add(new RO_Point()
    {
      Position = point.XYZ,
      Color = color,
      Size = size
    });
  }

  public void DrawBoneInfluence(RO_Skin skin, Bone bone)
  {
    var boneIndex = skin.Skeleton.BoneList.IndexOf(bone);
    if (boneIndex == -1) return;

    foreach (var mesh in skin.MeshList)
    {
      for (var i = 0; i < mesh.VertexList.Count; i++)
      {
        RGBA<float> color;
        var weight = mesh.BoneWeightList[i];
        var bIndex = mesh.BoneIndexList[i].ToBytesBE();

        var r = new RGBA<float>(1, 0, 0, 1);
        var g = new RGBA<float>(0, 1, 0, 1);
        float size;

        if (boneIndex == bIndex.R)
        {
          color = RGBA<float>.Lerp(r, g, weight.R);
          size = weight.R;
        }
        else if (boneIndex == bIndex.G)
        {
          color = RGBA<float>.Lerp(r, g, weight.G);
          size = weight.G;
        }
        else if (boneIndex == bIndex.B)
        {
          color = RGBA<float>.Lerp(r, g, weight.B);
          size = weight.B;
        }
        else if (boneIndex == bIndex.A)
        {
          color = RGBA<float>.Lerp(r, g, weight.A);
          size = weight.A;
        }
        else
        {
          continue;
        }

        var bl = skin.Skeleton.BoneList;

        var mx1 = bl[bIndex.R].InverseBindMatrix * bl[bIndex.R].Matrix;
        var mx2 = bl[bIndex.G].InverseBindMatrix * bl[bIndex.G].Matrix;
        var mx3 = bl[bIndex.B].InverseBindMatrix * bl[bIndex.B].Matrix;
        var mx4 = bl[bIndex.A].InverseBindMatrix * bl[bIndex.A].Matrix;

        var skinMatrix = weight.R * mx1 +
                         weight.G * mx2 +
                         weight.B * mx3 +
                         weight.A * mx4;

        if (size < 0.5f) size = 0.5f;
        Draw((mesh.VertexList[i] + mesh.NormalList[i] * 0.001f).AddW(1.0f) * skinMatrix, color,
          MathF.Ceiling(size * 5f));

        Draw(bone.GetForwardPoint(bone.Length / 2), r, 10);
      }
    }
  }
}