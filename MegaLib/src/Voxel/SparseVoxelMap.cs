using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaLib.Geometry;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.RenderObject;

namespace MegaLib.Voxel;

public class SparseVoxelMap8
{
  private Dictionary<IVector3, VoxelArray8> _chunks = new();
  private HashSet<IVector3> _changed = new();
  private VoxelArray8 _lastChunk;
  private IVector3 _lastChunkPosition;
  
  private int _chunkSize;
  public int ChunkCount => _chunks.Count;
  public float VoxelSize = 1f;

  public SparseVoxelMap8(int chunkSize = 16)
  {
    _chunkSize = chunkSize;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IVector3 GetChunkPosition(int x, int y, int z)
  {
    return new IVector3(
      x < 0 ? x / _chunkSize - 1 : x / _chunkSize,
      y < 0 ? y / _chunkSize - 1 : y / _chunkSize,
      z < 0 ? z / _chunkSize - 1 : z / _chunkSize
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
      if (chunkPos == _lastChunkPosition && _lastChunk != null)
      {
        var localPos = GetChunkLocalPosition(x, y, z);
        return _lastChunk[localPos];
      }
      else
      {
        if (!_chunks.TryGetValue(chunkPos, out var chunk)) return 0;
        var localPos = GetChunkLocalPosition(x, y, z);
        _lastChunkPosition = chunkPos;
        _lastChunk = chunk;
        return chunk[localPos];
      }
    }
    set
    {
      var chunkPos = GetChunkPosition(x, y, z);
      var localPos = GetChunkLocalPosition(x, y, z);

      if (chunkPos == _lastChunkPosition && _lastChunk != null)
      {
        _lastChunk[localPos] = value;
      }
      else
      {
        if (!_chunks.TryGetValue(chunkPos, out var chunk))
        {
          chunk = new VoxelArray8(_chunkSize, _chunkSize, _chunkSize);
          _chunks[chunkPos] = chunk;
        }

        chunk[localPos] = value;
        _lastChunk = chunk;
      }
      
      _changed.Add(chunkPos);
      _lastChunkPosition = chunkPos;
    }
  }

  public bool HasDataAt(IVector3 p)
  {
    return HasDataAt(p.X, p.Y, p.Z);
  }
  
  public bool HasDataAt(int x, int y, int z)
  {
    /*var chunkPos = GetChunkPosition(x, y, z);
    if (chunkPos == _lastChunkPosition && _lastChunk != null)
    {
      var localPos = GetChunkLocalPosition(x, y, z);
      return _lastChunk.HasDataAt(localPos);
    }
    else
    {
      var localPos = GetChunkLocalPosition(x, y, z);
      if (_chunks.TryGetValue(chunkPos, out var chunk))
      {
        _lastChunk = chunk;
        _lastChunkPosition = chunkPos;
        return chunk.HasDataAt(localPos);
      }
      return false;
    }*/
    
    var chunkPos = GetChunkPosition(x, y, z);
    var localPos = GetChunkLocalPosition(x, y, z);
    if (_chunks.TryGetValue(chunkPos, out var chunk))
    {
      return chunk.HasDataAt(localPos);
    }
    return false;
  }

  public VoxelArray8 GetChunk(int x, int y, int z)
  {
    var chunkPos = GetChunkPosition(x, y, z);
    return _chunks.GetValueOrDefault(chunkPos);
  }

  public Dictionary<IVector3, RO_VoxelMesh> BuildChanged()
  {
    if (_changed.Count <= 0) return null;

    var outList = new Dictionary<IVector3, RO_VoxelMesh>();

    Console.WriteLine($"CHANGED: {_changed.Count}");
    var tt = new Stopwatch();
    tt.Start();

    foreach (var chunk in _changed)
    {
      Console.WriteLine($"Ch: {chunk}");
      var tt2 = new Stopwatch();
      tt2.Start();
      var mesh = ChunkToMeshR2(chunk);
      if (mesh == null) continue;
      if (mesh.VertexList.Count == 0) continue;
      outList.Add(chunk, mesh);
      tt2.Stop();
      Console.WriteLine($"ChDone: {tt2.ElapsedTicks}");
    }

    _changed.Clear();

    tt.Stop();
    Console.WriteLine($"Done: {tt.ElapsedTicks}");

    return outList;
  }

  public Mesh ChunkToMeshR(IVector3 position)
  {
    return ChunkToMeshR(position.X, position.Y, position.Z);
  }
  
  public RO_VoxelMesh ChunkToMeshR2(IVector3 position)
  {
    return ChunkToMeshR2(position.X, position.Y, position.Z);
  }

  public RO_VoxelMesh ChunkToMeshR2(int xx, int yy, int zz)
  {
    var mesh = new RO_VoxelMesh();

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
                vertices[v] -= 0.5f;
              }
              
              /*mesh.AddQuad(
                vertices,
                faceTriangles,
                faceNormals[i],
                NumberToQuadUV(vv, 8)
              );*/
            }
          }
          
          mesh.VertexList.Add(new Vector3(gx, gy, gz));
        }
      }
    }

    return mesh;
  }

  
  public Mesh ChunkToMeshR(int xx, int yy, int zz)
  {
    var mesh = new Mesh(4096);

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
                vertices[v] -= 0.5f;
              }

              mesh.AddQuad(
                vertices,
                faceTriangles,
                faceNormals[i],
                NumberToQuadUV(vv, 8)
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
    var row = n / gridSize;
    var col = n % gridSize;

    // Calculate normalized coordinates for the bottom-left corner
    var u = (float)col / gridSize;
    var v = (float)row / gridSize;

    // Create the list of UV coordinates for the quad corners
    return new List<Vector2>
    {
      new(u, v), // BottomLeft
      new(u + 1.0f / gridSize, v), // BottomRight
      new(u + 1.0f / gridSize, v + 1.0f / gridSize), // TopRight
      new(u, v + 1.0f / gridSize) // TopLeft
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
    var mx = Matrix4x4.Identity;
    mx = mx.Translate(center.X, center.Y, center.Z);
    AddSphere(mx, radius, value);
    /*var areaSize = (int)radius;

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
    }*/
  }

  public void AddSphere(Matrix4x4 matrix, float radius, byte value)
  {
    // Define the sphere's local bounding box size
    var localRadius = radius;

    // Define the local-space sphere corners (bounding box corners)
    var localCorners = new Vector3[]
    {
      new(-localRadius, -localRadius, -localRadius),
      new(localRadius, -localRadius, -localRadius),
      new(-localRadius, localRadius, -localRadius),
      new(localRadius, localRadius, -localRadius),
      new(-localRadius, -localRadius, localRadius),
      new(localRadius, -localRadius, localRadius),
      new(-localRadius, localRadius, localRadius),
      new(localRadius, localRadius, localRadius)
    };

    // Transform the corners to world space using the matrix
    var worldCorners = localCorners.Select(corner => Vector3.Transform(corner, matrix)).ToArray();

    // Calculate the bounding box in world space
    var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

    foreach (var corner in worldCorners)
    {
      min = Vector3.Min(min, corner);
      max = Vector3.Max(max, corner);
    }

    // Iterate over the bounding box
    var minVoxel = new IVector3((int)Math.Floor(min.X), (int)Math.Floor(min.Y), (int)Math.Floor(min.Z));
    var maxVoxel = new IVector3((int)Math.Ceiling(max.X), (int)Math.Ceiling(max.Y), (int)Math.Ceiling(max.Z));

    // Precompute the inverse of the matrix
    var inverseMatrix = matrix.Inverted;

    // Iterate over the bounding box
    for (var x = minVoxel.X; x <= maxVoxel.X; x++)
    {
      for (var y = minVoxel.Y; y <= maxVoxel.Y; y++)
      {
        for (var z = minVoxel.Z; z <= maxVoxel.Z; z++)
        {
          // Transform the voxel position back to local space to check if it lies within the sphere
          var worldPos = new Vector3(x, y, z);
          var localPos = Vector3.Transform(worldPos, inverseMatrix);

          // Check if the local position is inside the sphere
          if (localPos.Length <= radius)
          {
            this[new IVector3(x, y, z)] = value;
          }
        }
      }
    }
  }

  public void AddCube(Matrix4x4 matrix, IVector3 size, byte value)
  {
    // Define half-size for local cube bounds
    var halfSize = new Vector3(size.X * 0.5f, size.Y * 0.5f, size.Z * 0.5f);

    // Define the local-space cube corners
    var localCorners = new Vector3[]
    {
      new(-halfSize.X, -halfSize.Y, -halfSize.Z),
      new(halfSize.X, -halfSize.Y, -halfSize.Z),
      new(-halfSize.X, halfSize.Y, -halfSize.Z),
      new(halfSize.X, halfSize.Y, -halfSize.Z),
      new(-halfSize.X, -halfSize.Y, halfSize.Z),
      new(halfSize.X, -halfSize.Y, halfSize.Z),
      new(-halfSize.X, halfSize.Y, halfSize.Z),
      new(halfSize.X, halfSize.Y, halfSize.Z)
    };

    // Transform the corners to world space using the matrix
    var worldCorners = localCorners.Select(corner => Vector3.Transform(corner, matrix)).ToArray();

    // Calculate the bounding box in world space
    var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

    foreach (var corner in worldCorners)
    {
      min = Vector3.Min(min, corner);
      max = Vector3.Max(max, corner);
    }

    // Iterate over the bounding box
    var minVoxel = new IVector3((int)Math.Floor(min.X), (int)Math.Floor(min.Y), (int)Math.Floor(min.Z));
    var maxVoxel = new IVector3((int)Math.Ceiling(max.X), (int)Math.Ceiling(max.Y), (int)Math.Ceiling(max.Z));

    // Precompute the inverse of the matrix
    var inverseMatrix = matrix.Inverted;

    // Iterate over the bounding box
    for (var x = minVoxel.X; x <= maxVoxel.X; x++)
    {
      for (var y = minVoxel.Y; y <= maxVoxel.Y; y++)
      {
        for (var z = minVoxel.Z; z <= maxVoxel.Z; z++)
        {
          // Transform the voxel position back to local space to check if it lies within the cube
          var worldPos = new Vector3(x, y, z);
          var localPos = Vector3.Transform(worldPos, inverseMatrix);

          // Check if the local position is inside the cube
          if (Math.Abs(localPos.X) <= halfSize.X &&
              Math.Abs(localPos.Y) <= halfSize.Y &&
              Math.Abs(localPos.Z) <= halfSize.Z)
          {
            this[new IVector3(x, y, z)] = value;
          }
        }
      }
    }
  }
}