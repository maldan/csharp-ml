using System;
using MegaLib.Geometry;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Voxel;

public class VoxelArray<T> where T : struct
{
  public int Width;
  public int Height;
  public int Depth;

  public Vector3 Center => new(Width / 2, Height / 2, Depth / 2);

  private T[,,] _data;

  public VoxelArray(int w, int h, int d)
  {
    Width = w;
    Height = h;
    Depth = d;
    _data = new T[w, h, d];
  }

  public T this[int x, int y, int z]
  {
    get => IsInBounds(x, y, z) ? _data[x, y, z] : default;
    set
    {
      if (IsInBounds(x, y, z)) _data[x, y, z] = value;
    }
  }

  public bool HasDataAt(int x, int y, int z)
  {
    return _data switch
    {
      byte[,,] v => v[x, y, z] != 0,
      short[,,] v => v[x, y, z] != 0,
      int[,,] v => v[x, y, z] != 0,
      _ => false
    };
  }

  public bool IsInBounds(int x, int y, int z)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth;
  }

  public Mesh ToMesh()
  {
    var mesh = new Mesh();
    var grid = this;

    for (var x = 0; x < grid.Width - 1; x++)
    {
      for (var y = 0; y < grid.Height - 1; y++)
      {
        for (var z = 0; z < grid.Depth - 1; z++)
        {
          // Get the cube's corner states (8 corners)
          var cubeCorners = new bool[8];
          for (var i = 0; i < 8; i++)
          {
            var corner = MarchingCubes.CubeCorners[i];
            var dx = x + (int)corner.X;
            var dy = y + (int)corner.Y;
            var dz = z + (int)corner.Z;
            cubeCorners[i] = !grid.HasDataAt(dx, dy, dz);
          }

          // Determine cube configuration index
          var cubeIndex = 0;
          for (var i = 0; i < 8; i++)
          {
            if (cubeCorners[i]) cubeIndex |= 1 << i;
          }

          // Retrieve the edges for the current cube configuration from the triTable
          var edges = MarchingCubes.TriTable[cubeIndex];
          if (edges[0] == -1) continue; // No triangles for this configuration

          // Interpolate the vertices for the edges based on the triTable configuration
          for (var i = 0; edges[i] != -1; i += 3)
          {
            var v1 = MarchingCubes.InterpolateEdge(x, y, z, edges[i], grid);
            var v2 = MarchingCubes.InterpolateEdge(x, y, z, edges[i + 1], grid);
            var v3 = MarchingCubes.InterpolateEdge(x, y, z, edges[i + 2], grid);

            // Add the vertices to the mesh
            mesh.VertexList.Add(v1);
            mesh.VertexList.Add(v2);
            mesh.VertexList.Add(v3);

            // Add UVs and normals (you can compute them based on your needs)
            mesh.UV0List.Add(new Vector2(v1.X, v1.Y)); // Example UV mapping
            mesh.UV0List.Add(new Vector2(v2.X, v2.Y));
            mesh.UV0List.Add(new Vector2(v3.X, v3.Y));

            // For normals, use a simple cross-product of the triangle's edges
            var normal = Vector3.Cross(v2 - v1, v3 - v1).Normalized;
            mesh.NormalList.Add(normal);
            mesh.NormalList.Add(normal);
            mesh.NormalList.Add(normal);

            // Add indices for the triangle
            var baseIndex = (uint)(mesh.VertexList.Count - 3);
            mesh.IndexList.Add(baseIndex);
            mesh.IndexList.Add(baseIndex + 1);
            mesh.IndexList.Add(baseIndex + 2);
          }
        }
      }
    }

    return mesh;
  }
}