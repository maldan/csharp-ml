using System;
using System.Collections.Generic;
using MegaLib.Geometry;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Voxel;

public class SparseVoxelMap8
{
  private Dictionary<IVector3, VoxelArray8> _chunks = new();
  private HashSet<IVector3> _changed = new();
  
  private int _chunkSize;
  public int ChunkCount => _chunks.Count;
  public float VoxelSize = 1f;
  
  public SparseVoxelMap8(int chunkSize = 16)
  {
    _chunkSize = chunkSize;
  }
  
  // Adjusting for negative coordinates
  public IVector3 GetChunkPosition(int x, int y, int z)
  {
    return new IVector3(
      x < 0 ? x / _chunkSize - 1 : x / _chunkSize,
      y < 0 ? y / _chunkSize - 1 : y / _chunkSize,
      z < 0 ? z / _chunkSize - 1 : z / _chunkSize
    );
  }

  public IVector3 GetChunkLocalPosition(int x, int y, int z)
  {
    // Normalize indices for chunk-local access
    var localX = (x % _chunkSize + _chunkSize) % _chunkSize;
    var localY = (y % _chunkSize + _chunkSize) % _chunkSize;
    var localZ = (z % _chunkSize + _chunkSize) % _chunkSize;

    return new IVector3(localX, localY, localZ);
  }

  public byte this[IVector3 p]
  {
    get => this[p.X, p.Y, p.Z];
    set => this[p.X, p.Y, p.Z] = value;
  }

  public byte this[int x, int y, int z]
  {
    get
    {
      var chunkPos = GetChunkPosition(x, y, z);
      if (!_chunks.TryGetValue(chunkPos, out var chunk)) return default;

      // Normalize indices for chunk-local access
      var localX = (x % _chunkSize + _chunkSize) % _chunkSize;
      var localY = (y % _chunkSize + _chunkSize) % _chunkSize;
      var localZ = (z % _chunkSize + _chunkSize) % _chunkSize;

      return chunk[localX, localY, localZ];
    }
    set
    {
      var chunkPos = GetChunkPosition(x, y, z);
      // Normalize indices for chunk-local access
      var localX = (x % _chunkSize + _chunkSize) % _chunkSize;
      var localY = (y % _chunkSize + _chunkSize) % _chunkSize;
      var localZ = (z % _chunkSize + _chunkSize) % _chunkSize;

      // Console.WriteLine($"G {x} {y} {z}, {chunkPos}");

      if (!_chunks.TryGetValue(chunkPos, out var chunk))
      {
        chunk = new VoxelArray8(_chunkSize, _chunkSize, _chunkSize);
        _chunks[chunkPos] = chunk;
      }

      chunk[localX, localY, localZ] = value;
      _changed.Add(chunkPos);
    }
  }

  public bool HasDataAt(int x, int y, int z)
  {
    var chunkPos = GetChunkPosition(x, y, z);
    var localX = (x % _chunkSize + _chunkSize) % _chunkSize;
    var localY = (y % _chunkSize + _chunkSize) % _chunkSize;
    var localZ = (z % _chunkSize + _chunkSize) % _chunkSize;

    return _chunks.TryGetValue(chunkPos, out var chunk) && chunk.HasDataAt(localX, localY, localZ);
  }

  public VoxelArray8 GetChunk(int x, int y, int z)
  {
    var chunkPos = GetChunkPosition(x, y, z);
    return _chunks.GetValueOrDefault(chunkPos);
  }
  
  public Dictionary<IVector3, Mesh> BuildChanged()
  {
    if (_changed.Count <= 0) return null;
    
    var outList = new Dictionary<IVector3, Mesh>();

    Console.WriteLine($"CHANGED: {_changed.Count}");
    
    foreach (var chunk in _changed)
    {
      Console.WriteLine($"Ch: {chunk}");
      var mesh = ChunkToMeshR(chunk);
      if (mesh == null) continue;
      if (mesh.VertexList.Count == 0) continue;
      outList.Add(chunk, mesh);
    }
    
    _changed.Clear();
    
    return outList;
  }

  public Mesh ChunkToMeshR(IVector3 position)
  {
    return ChunkToMeshR(position.X, position.Y, position.Z);
  }

  public Mesh ChunkToMeshR(int xx, int yy, int zz)
  {
    var mesh = new Mesh();

    // Define cube face directions and offsets
    Vector3[] faceNormals =
    [
      new(0, 0, -1), // Back
      new(0, 0, 1), // Front
      new(0, -1, 0), // Bottom
      new(0, 1, 0), // Top
      new(-1, 0, 0), // Left
      new(1, 0, 0) // Right
    ];

    Vector3[,] faceVertices =
    {
      // Back
      { new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0) },
      // Front
      { new(1, 0, 1), new(0, 0, 1), new(0, 1, 1), new(1, 1, 1) },
      // Bottom
      { new(0, 0, 1), new(1, 0, 1), new(1, 0, 0), new(0, 0, 0) },
      // Top
      { new(0, 1, 0), new(1, 1, 0), new(1, 1, 1), new(0, 1, 1) },
      // Left
      { new(0, 0, 1), new(0, 0, 0), new(0, 1, 0), new(0, 1, 1) },
      // Right
      { new(1, 0, 0), new(1, 0, 1), new(1, 1, 1), new(1, 1, 0) }
    };

    uint[] faceTriangles = [0, 1, 2, 0, 2, 3]; // Two triangles per face

    var chunkOffset = new IVector3(xx, yy, zz) * _chunkSize;
    Console.WriteLine(chunkOffset);
    // var chunkLocalPosition = GetChunkPosition(xx, yy, zz);
    
    //var chunkOffset = GetChunkPos(xx, yy, zz) * _chunkSize;
    var rnd = new Mathematics.Random();
    

    for (var x = 0; x < _chunkSize; x++)
    {
      for (var y = 0; y < _chunkSize; y++)
      {
        for (var z = 0; z < _chunkSize; z++)
        {
          var gx = x + chunkOffset.X;
          var gy = y + chunkOffset.Y;
          var gz = z + chunkOffset.Z;
          
          if (!HasDataAt(gx, gy, gz)) continue;

          var vv = this[gx, gy, gz];
          
          // Add visible faces
          for (var i = 0; i < 6; i++)
          {
            var neighborPos = new Vector3(gx, gy, gz) + faceNormals[i];
            if (!HasDataAt((int)neighborPos.X, (int)neighborPos.Y, (int)neighborPos.Z))
            {
              // Generate face
              var vertices = new Vector3[4];
              for (var v = 0; v < 4; v++)
              {
                vertices[v] = faceVertices[i, v] + new Vector3(gx, gy, gz);
                vertices[v] *= VoxelSize;
                //vertices[v] -= 0.5f;
              }

              mesh.AddQuad(
                vertices,
                faceTriangles,
                faceNormals[i],
                NumberToQuadUV(vv, 128)
              );
            }
          }
        }
      }
    }

    return mesh;
  }

  public static List<Vector2> NumberToQuadUV(int n, int gridSize)
  {
    // Calculate the row and column of the quad
    int row = n / gridSize;
    int col = n % gridSize;

    // Calculate normalized coordinates for the bottom-left corner
    float u = (float)col / gridSize;
    float v = (float)row / gridSize;

    // Create the list of UV coordinates for the quad corners
    return new List<Vector2>
    {
      new Vector2(u, v),                          // BottomLeft
      new Vector2(u + 1.0f / gridSize, v),        // BottomRight
      new Vector2(u + 1.0f / gridSize, v + 1.0f / gridSize), // TopRight
      new Vector2(u, v + 1.0f / gridSize)         // TopLeft
    };
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

  public void AddSphere(IVector3 center, float radius, byte value)
  {
    var areaSize = (int)(radius);
    
    for (var i = -areaSize; i < areaSize; i++)
    {
      for (var j = -areaSize; j < areaSize; j++)
      {
        for (var k = -areaSize; k < areaSize; k++)
        {
          var pos = center + new IVector3(i, j, k);
          
          if (Vector3.Distance((Vector3)center, (Vector3)pos) < radius * 0.5f)
          {
            this[pos] = value;
          }
        }
      }
    }
  }
}