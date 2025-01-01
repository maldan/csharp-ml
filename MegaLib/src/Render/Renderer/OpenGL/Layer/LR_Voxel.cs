using System;
using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Shader;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Voxel : LR_Base
{
  private Dictionary<IVector3, RO_VoxelMesh> _chunks = new();

  public LR_Voxel(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
  }

  public override void Init()
  {
    var ss = ShaderProgram.Compile("Voxel");

    Shader.ShaderCode["vertex"] = ss["vertex"];
    Shader.ShaderCode["geometry"] = ss["geometry"];
    Shader.ShaderCode["fragment"] = ss["fragment"];
    Shader.Compile();
  }

  public override void Render()
  {
    var layer = (Layer_Voxel)Layer;

    if (layer.VoxelMap == null) return;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);
    //OpenGL32.glCullFace(OpenGL32.GL_BACK);
    Shader.Enable(OpenGL32.GL_CULL_FACE);

    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
    Shader.SetUniform("uCameraPosition", Scene.Camera.Position);
    if (Scene.Skybox != null) Shader.ActivateTexture(Scene.Skybox, "uSkybox", 10);

    var changed = layer.VoxelMap.BuildChanged();
    if (changed != null)
    {
      foreach (var (pos, mesh) in changed)
      {
        if (_chunks.ContainsKey(pos))
        {
          _chunks[pos] = mesh;
          
          /*_chunks[pos].NormalList.Clear();
          _chunks[pos].VertexList.Clear();
          _chunks[pos].UV0List.Clear();
          _chunks[pos].IndexList.Clear();

          _chunks[pos].NormalList.AddRange(mesh.NormalList);
          _chunks[pos].VertexList.AddRange(mesh.VertexList);
          _chunks[pos].UV0List.AddRange(mesh.UV0List);
          _chunks[pos].IndexList.AddRange(mesh.IndexList);

          _chunks[pos].CalculateTangent();
          _chunks[pos].CalculateBoundingBox();*/
        }
        else
        {
          // var m = new RO_Mesh().FromMesh(mesh);
          // m.InitDefaultTextures();
          _chunks[pos] = mesh;
        }
      }
    }

    // Draw each mesh
    foreach (var (pos, mesh) in _chunks)
    {
      // mesh.Material.AlbedoTexture = layer.Texture;
      Context.MapObject(mesh);

      // Bind vao
      OpenGL32.glBindVertexArray(Context.GetVaoId(mesh));

      // Buffer
      Shader.EnableAttribute(mesh.VertexList, "aPosition");
      
      //Shader.EnableAttribute(mesh.NormalList, "aNormal");
      //Shader.EnableAttribute(mesh.UV0List, "aUV");
      
      //Shader.EnableAttribute(mesh.TangentList, "aTangent");
      //Shader.EnableAttribute(mesh.BiTangentList, "aBiTangent");

      // Texture
      /*if (mesh.Material != null)
      {
        if (mesh.Material.AlbedoTexture != null)
          Shader.ActivateTexture(mesh.Material.AlbedoTexture, "uAlbedoTexture", 0);
        if (mesh.Material.NormalTexture != null)
          Shader.ActivateTexture(mesh.Material.NormalTexture, "uNormalTexture", 1);
        if (mesh.Material.RoughnessTexture != null)
          Shader.ActivateTexture(mesh.Material.RoughnessTexture, "uRoughnessTexture", 2);
        if (mesh.Material.MetallicTexture != null)
          Shader.ActivateTexture(mesh.Material.MetallicTexture, "uMetallicTexture", 3);
      }*/

      /*if (mesh.Material.AlbedoTexture != null)
      {
        Shader.ActivateTexture(mesh.Material.AlbedoTexture, "uAlbedoTexture", 0);
      }*/

      // Текстура с источниками света
      //Context.MapTexture(Scene.LightTexture);
      //Shader.ActivateTexture(Scene.LightTexture, "uLightTexture", 12);

      //Shader.SetUniform("uModelMatrix", mesh.Transform.Matrix);
      //Shader.SetUniform("uFogData", new Vector4(0.0f, 32f, 0, 0));

      /*if (mesh.Material != null)
      {
        Shader.SetUniform("uTint", (Vector4)mesh.Material.Tint);
      }
      else
      {
        Shader.SetUniform("uTint", new Vector4(1, 1, 1, 1));
      }*/

      // Bind indices
      // OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(mesh.IndexList));

      // Draw
      // OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

      OpenGL32.glDrawArrays(OpenGL32.GL_POINTS, 0, mesh.VertexList.Count);
      
      // Unbind vao
      OpenGL32.glBindVertexArray(0);
    }
  }
}