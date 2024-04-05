using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class LR_Skybox : LR_Base
  {
    public LR_Skybox(OpenGL_Context context, RL_Base layer, Render_Scene scene) : base(context, layer, scene)
    {
    }

    public override void Init()
    {
      // language=glsl
      var vertex = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec3 aPosition;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform mat4 uModelMatrix;

        out vec3 vUV;
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            vUV = aPosition;
        }";

      // language=glsl
      var fragment = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vUV;
        out vec4 color;
        
        uniform samplerCube uSkybox;

        void main()
        {
            color = texture(uSkybox, vUV);
        }";

      Shader.ShaderCode["vertex"] = vertex;
      Shader.ShaderCode["fragment"] = fragment;
      Shader.Compile();

      // Base
      var skyboxCube = RO_Mesh.GenerateCube(32);
      skyboxCube.Transform = new Transform();
      Layer.Add(skyboxCube);
    }

    public override void Render()
    {
      var layer = (RL_Skybox)Layer;

      Shader.Use();
      Shader.Enable(OpenGL32.GL_BLEND);
      Shader.Enable(OpenGL32.GL_DEPTH_TEST);

      var cp = Scene.Camera.Position;
      cp.Z *= -1;
      // cp.Y *= -1;
      // Shader.SetUniform("uCameraPosition", cp);
      Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
      Shader.ActivateTexture(Scene.Skybox, "uSkybox", 10);

      // Draw each mesh
      layer.ForEach<RO_Mesh>(mesh =>
      {
        // Move mesh
        var p = Scene.Camera.Position;
        p.Z *= -1;
        mesh.Transform.Position = p;
        mesh.Transform.Scale = new Vector3(1.0f, 1.0f, -1.0f);

        Context.MapObject(mesh);

        // Bind vao
        OpenGL32.glBindVertexArray(Context.GetVaoId(mesh));

        // Buffer
        Shader.EnableAttribute(mesh.VertexList, "aPosition");
        Shader.SetUniform("uModelMatrix", mesh.Transform.Matrix);

        // Bind indices
        OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(mesh.IndexList));

        // Draw
        OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, mesh.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

        // Unbind vao
        OpenGL32.glBindVertexArray(0);
      });
    }
  }
}