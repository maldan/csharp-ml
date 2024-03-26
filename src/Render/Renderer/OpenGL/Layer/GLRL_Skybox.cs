using System.Collections.Generic;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS.Api;
using MegaLib.Render.Core;
using MegaLib.Render.Core.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.Renderer.OpenGL.Layer
{
  public class GLRL_Skybox : GLRL_Base
  {
    public GLRL_Skybox(Context_OpenGL context, RL_Base layer, Render_Scene scene) : base(context, layer, scene)
    {
    }

    public override void Init()
    {
      // language=glsl
      var shaderPBR = @"#version 300 es
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
        }
        // Fragment
        #version 300 es
        
        precision highp float;
        precision highp int;
        precision highp sampler2D;

        in vec3 vUV;
        out vec4 color;
        
        uniform samplerCube uSkybox;

        void main()
        {
            color = texture(uSkybox, vUV);
        }".Replace("\r", "");

      var tuple = shaderPBR.Split("// Fragment\n");
      Context.CreateShader(Layer.Name, tuple[0], tuple[1]);

      // Base
      var skyboxCube = RO_Mesh.GenerateCube(32);
      skyboxCube.Transform = new Transform();
      Layer.Add(skyboxCube);
    }

    public override void Render()
    {
      var layer = (RL_Skybox)Layer;

      // User shader line
      Context.UseProgram(layer.Name);

      // gl blend fn
      OpenGL32.glEnable(OpenGL32.GL_BLEND);
      OpenGL32.glBlendFunc(OpenGL32.GL_SRC_ALPHA, OpenGL32.GL_ONE_MINUS_SRC_ALPHA);

      // bind camera
      var cp = Scene.Camera.Position.Inverted;

      Context.BindMatrix(layer.Name, "uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Context.BindMatrix(layer.Name, "uViewMatrix", Scene.Camera.ViewMatrix);
      Context.ActivateCubeTexture(layer.Name, "uSkybox", "main", 10);

      // gl enable depth test
      OpenGL32.glEnable(OpenGL32.GL_DEPTH_TEST);
      OpenGL32.glDepthFunc(OpenGL32.GL_LEQUAL);

      // Draw each mesh
      layer.ForEach<RO_Mesh>(mesh =>
      {
        var objectInfo = Context.GetObjectInfo(mesh.Id);
        if (objectInfo == null)
        {
          // Define object
          objectInfo = new ObjectInfo_OpenGL(Context)
          {
            VertexBufferName = $"{layer.Name}_{mesh.Id}.vertex",
            IndexBufferName = $"{layer.Name}_{mesh.Id}.index",
            IndexAmount = mesh.GpuIndexList.Length,
            ShaderName = layer.Name,

            Transform = mesh.Transform,
          };
          objectInfo.VertexAttributeList = new List<string>
          {
            $"{objectInfo.VertexBufferName} -> aPosition:vec3",
          };

          // Create buffers
          Context.CreateBuffer(objectInfo.VertexBufferName);
          Context.CreateBuffer(objectInfo.IndexBufferName);

          // gl upload buffers
          Context.UploadBuffer(objectInfo.VertexBufferName, mesh.GpuVertexList);
          Context.UploadElementBuffer(objectInfo.IndexBufferName, mesh.GpuIndexList);

          // Set object
          Context.SetObjectInfo(mesh.Id, objectInfo);
        }

        var p = Scene.Camera.Position;
        p.Z *= -1;
        mesh.Transform.Position = p;
        mesh.Transform.Scale = new Vector3(1.0f, 1.0f, -1.0f);

        // gl draw arrays
        objectInfo.DrawElements();
      });

      // Unbind
      OpenGL32.glBindBuffer(OpenGL32.GL_ARRAY_BUFFER, 0);
      OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, 0);
    }
  }
}