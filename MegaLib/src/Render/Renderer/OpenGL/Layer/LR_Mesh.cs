using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;
using MegaLib.Render.Shader;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Mesh : LR_Base
{
  public LR_Mesh(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
  }

  public override void Init()
  {
    var ss = ShaderProgram.Compile("Mesh");

    Shader.ShaderCode["vertex"] = ss["vertex"];
    Shader.ShaderCode["fragment"] = ss["fragment"];
    Shader.Compile();
  }

  public override void Render()
  {
    var layer = (Layer_StaticMesh)Layer;

    Shader.Use();
    //OpenGL32.glDepthRange(1.0, 0.0); // Инвертируем диапазон глубины
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);
    //OpenGL32.glDepthFunc(OpenGL32.GL_GREATER);

    //var cp = Scene.Camera.Position;
    //cp.Z *= -1;
    // cp.Y *= -1;
    // Shader.SetUniform("uCameraPosition", cp);
    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
    if (Scene.Skybox != null) Shader.ActivateTexture(Scene.Skybox, "uSkybox", 10);

    // Draw each mesh
    layer.ForEach(mesh =>
    {
      Context.MapObject(mesh);

      // Bind vao
      OpenGL32.glBindVertexArray(Context.GetVaoId(mesh));

      // Buffer
      Shader.EnableAttribute(mesh.VertexList, "aPosition");
      Shader.EnableAttribute(mesh.NormalList, "aNormal");
      Shader.EnableAttribute(mesh.UV0List, "aUV");
      Shader.EnableAttribute(mesh.TangentList, "aTangent");
      Shader.EnableAttribute(mesh.BiTangentList, "aBiTangent");

      // Texture
      if (mesh.Material != null)
      {
        if (mesh.Material.AlbedoTexture != null)
          Shader.ActivateTexture(mesh.Material.AlbedoTexture, "uAlbedoTexture", 0);
        if (mesh.Material.NormalTexture != null)
          Shader.ActivateTexture(mesh.Material.NormalTexture, "uNormalTexture", 1);
        if (mesh.Material.RoughnessTexture != null)
          Shader.ActivateTexture(mesh.Material.RoughnessTexture, "uRoughnessTexture", 2);
        if (mesh.Material.MetallicTexture != null)
          Shader.ActivateTexture(mesh.Material.MetallicTexture, "uMetallicTexture", 3);
      }

      // Текстура с источниками света
      Context.MapTexture(Scene.LightTexture);
      Shader.ActivateTexture(Scene.LightTexture, "uLightTexture", 12);

      Shader.SetUniform("uModelMatrix", mesh.Transform.Matrix);
      if (mesh.Material != null)
      {
        Shader.SetUniform("uTint", (Vector4)mesh.Material.Tint);
      }
      else
      {
        Shader.SetUniform("uTint", new Vector4(1, 1, 1, 1));
      }


      // Bind indices
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(mesh.IndexList));

      // Draw
      OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

      // Unbind vao
      OpenGL32.glBindVertexArray(0);
    });
  }
}