using System;
using MegaLib.OS.Api;
using MegaLib.Render.Layer;
using MegaLib.Render.Mesh;
using MegaLib.Render.RenderObject;
using MegaLib.Render.Scene;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_Skybox : LR_Base
{
  private RO_Mesh _skybox;

  public LR_Skybox(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
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
        
        out vec3 vUV;
        
        void main() {
            // Преобразуем позицию вершины в пространство вида (без трансляции, только вращение)
            vUV = aPosition;
            vec4 pos = uProjectionMatrix * mat4(mat3(uViewMatrix)) * vec4(aPosition, 1.0);
            gl_Position = pos.xyww; // Применяем перспективу, но сохраняем W для корректного отображения
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
            color = vec4(texture(uSkybox, vUV).rgb, 1.0);
        }";

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();

    // Base
    _skybox = new RO_Mesh();
    _skybox.FromMesh(MeshGenerator.Cube(32));
  }

  public override void Render()
  {
    var layer = (Layer_Skybox)Layer;

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    var cp = Scene.Camera.Position;
    cp.Z *= -1;
    // cp.Y *= -1;
    // Shader.SetUniform("uCameraPosition", cp);
    Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
    Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);

    Context.MapTexture(Scene.Skybox);
    Shader.ActivateTexture(Scene.Skybox, "uSkybox", 10);

    // Draw each mesh
    // Move mesh
    var p = Scene.Camera.Position;
    //p.Z *= -1;
    //mesh.Transform.Position = p;
    //mesh.Transform.Scale = new Vector3(1.0f, 1.0f, -1.0f);

    Context.MapObject(_skybox);

    // Bind vao
    OpenGL32.glBindVertexArray(Context.GetVaoId(_skybox));

    // Buffer
    Shader.EnableAttribute(_skybox.VertexList, "aPosition");
    //Shader.EnableAttribute(mesh.UV0List, "aUV");
    // Shader.SetUniform("uModelMatrix", mesh.Transform.Matrix);

    // Bind indices
    OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(_skybox.IndexList));

    // Draw
    OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _skybox.IndexList.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

    // Unbind vao
    OpenGL32.glBindVertexArray(0);
  }
}