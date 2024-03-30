using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.AssetLoader.GLTF
{
  public class GLTF_Bone
  {
    public GLTF GLTF;

    public int JointId;
    public int SkinId;
    public int NodeId;
    public string Name;
    public List<int> ChildrenId;
    public List<GLTF_Bone> ChildrenBone = new();

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public Matrix4x4 InverseBindMatrix;

    public GLTF_Bone(GLTF_Node node)
    {
      GLTF = node.GLTF;
      NodeId = node.Id;
      Name = node.Name;
      ChildrenId = node.Children;
      Position = node.Position;
      Rotation = node.Rotation;
      Scale = node.Scale;
    }
  }

  public class GLTF_Skin
  {
    public GLTF GLTF;
    public int Id;
    public string Name;
    public List<GLTF_Bone> BoneList = new();
    public List<GLTF_Mesh> MeshList = new();
    public Dictionary<string, GLTF_Bone> BoneMap = new();

    public GLTF_Skin(GLTF gltf, JsonElement element, int id)
    {
      GLTF = gltf;
      Id = id;
      Name = element.GetProperty("name").GetString();

      // Fill bones
      var joint = element.GetProperty("joints").EnumerateArray().Select(e => e.GetInt32()).ToList();
      var ii = 0;
      foreach (var bone in joint.Select(i => new GLTF_Bone(GLTF.NodeList[i])))
      {
        bone.SkinId = id;
        bone.JointId = ii;
        BoneList.Add(bone);
        BoneMap.Add(bone.Name, bone);
        ii += 1;
      }

      // Fill children
      foreach (var mainBone in BoneList)
      {
        foreach (var childId in mainBone.ChildrenId)
        {
          var childBone = BoneList.Find(x => x.NodeId == childId);
          mainBone.ChildrenBone.Add(childBone);
        }
      }

      // Fill meshes
      foreach (var node in GLTF.NodeList.Where(node => node.MeshId >= 0 && node.SkinId == Id))
      {
        if (node.MeshId != null) MeshList.Add(GLTF.MeshList[node.MeshId.Value]);
      }

      // Inverse Bind Matrices
      var ibm = element.GetProperty("inverseBindMatrices").GetInt32();
      var matrixList = GLTF.AccessorList[ibm].Mat4x4();
      for (var i = 0; i < matrixList.Count; i++) BoneList[i].InverseBindMatrix = matrixList[i];
    }
  }
}