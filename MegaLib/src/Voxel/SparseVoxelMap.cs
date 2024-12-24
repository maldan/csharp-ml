using System;
using System.Collections.Generic;
using MegaLib.Geometry;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Voxel;

public class SparseVoxelMap<T> where T : struct
{
  private Dictionary<IVector3, VoxelArray<T>> _chunks = new();
  private int _chunkSize = 16;
  public int ChunkCount => _chunks.Count;

  // Adjusting for negative coordinates
  private IVector3 GetChunkPos(int x, int y, int z)
  {
    return new IVector3(
      x < 0 ? x / _chunkSize - 1 : x / _chunkSize,
      y < 0 ? y / _chunkSize - 1 : y / _chunkSize,
      z < 0 ? z / _chunkSize - 1 : z / _chunkSize
    );
  }

  public T this[int x, int y, int z]
  {
    get
    {
      var chunkPos = GetChunkPos(x, y, z);
      if (!_chunks.TryGetValue(chunkPos, out var chunk)) return default;

      // Normalize indices for chunk-local access
      var localX = (x % _chunkSize + _chunkSize) % _chunkSize;
      var localY = (y % _chunkSize + _chunkSize) % _chunkSize;
      var localZ = (z % _chunkSize + _chunkSize) % _chunkSize;

      return chunk[localX, localY, localZ];
    }
    set
    {
      var chunkPos = GetChunkPos(x, y, z);
      // Normalize indices for chunk-local access
      var localX = (x % _chunkSize + _chunkSize) % _chunkSize;
      var localY = (y % _chunkSize + _chunkSize) % _chunkSize;
      var localZ = (z % _chunkSize + _chunkSize) % _chunkSize;

      // Console.WriteLine($"G {x} {y} {z}, {chunkPos}");

      if (!_chunks.TryGetValue(chunkPos, out var chunk))
      {
        chunk = new VoxelArray<T>(_chunkSize, _chunkSize, _chunkSize);
        _chunks[chunkPos] = chunk;
      }

      chunk[localX, localY, localZ] = value;
    }
  }

  public VoxelArray<T> GetChunk(int x, int y, int z)
  {
    var chunkPos = GetChunkPos(x, y, z);
    return _chunks.GetValueOrDefault(chunkPos);
  }

  public Mesh ChunkToMesh(int xx, int yy, int zz)
  {
    var mesh = new Mesh();

    var chunkPos = new IVector3(xx, yy, zz);
    if (!_chunks.TryGetValue(chunkPos, out var chunk)) return null;
    // var chunk = GetChunk(xx, yy, zz);
    if (chunk == null) return null;

    // Console.WriteLine($"{chunk} - {chunk[15, 0, 0]}");
    // Console.WriteLine($"ToMesh {chunkPos}");
    var chunkOffset = (Vector3)chunkPos * _chunkSize;
    if (chunkOffset.X < 0) chunkOffset.X += 1;
    if (chunkOffset.Y < 0) chunkOffset.Y += 1;
    if (chunkOffset.Z < 0) chunkOffset.Z += 1;
    chunkOffset.X -= 0.5f;
    chunkOffset.Y -= 0.5f;
    chunkOffset.Z -= 0.5f;

    for (var x = 0; x < _chunkSize - 1; x++)
    {
      for (var y = 0; y < _chunkSize - 1; y++)
      {
        for (var z = 0; z < _chunkSize - 1; z++)
        {
          // Console.WriteLine(chunk[x, y, z]);

          // Get the cube's corner states (8 corners)
          var cubeCorners = new bool[8];
          for (var i = 0; i < 8; i++)
          {
            var corner = MarchingCubes.CubeCorners[i];
            var dx = x + (int)corner.X;
            var dy = y + (int)corner.Y;
            var dz = z + (int)corner.Z;
            // Console.WriteLine($"{dx} {dy} {dz}");
            cubeCorners[i] = !chunk.HasDataAt(dx, dy, dz);
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
            var v1 = MarchingCubes.InterpolateEdge(x, y, z, edges[i], chunk) + chunkOffset;
            var v2 = MarchingCubes.InterpolateEdge(x, y, z, edges[i + 1], chunk) + chunkOffset;
            var v3 = MarchingCubes.InterpolateEdge(x, y, z, edges[i + 2], chunk) + chunkOffset;

            // Add the vertices to the mesh
            mesh.VertexList.Add(v1 * 0.1f);
            mesh.VertexList.Add(v2 * 0.1f);
            mesh.VertexList.Add(v3 * 0.1f);

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