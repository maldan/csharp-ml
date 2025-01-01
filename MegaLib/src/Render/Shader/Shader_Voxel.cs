using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class VoxelVertexShader : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;

  public Vector4 Main()
  {
    return new Vector4(aPosition, 1.0f);
  }
}

[ShaderEmbedText("layout(points) in;\nlayout(triangle_strip, max_vertices = 24) out;")]
public class VoxelGeometryShader : Shader_Base
{
  [ShaderFieldUniform] public Matrix4x4 uProjectionMatrix;
  [ShaderFieldUniform] public Matrix4x4 uViewMatrix;
  [ShaderFieldUniform] public Vector3 uCameraPosition;

  [ShaderFieldOut] public Vector3 vo_WorldPosition;
  [ShaderFieldOut] public Vector2 vo_UV;

  private void GenerateTop(Vector3 center, float halfSize)
  {
    // Calculate the four vertices of the top face
    var bottomLeft = new Vector3(center.X - halfSize, center.Y + halfSize, center.Z - halfSize);
    var bottomRight = new Vector3(center.X + halfSize, center.Y + halfSize, center.Z - halfSize);
    var topLeft = new Vector3(center.X - halfSize, center.Y + halfSize, center.Z + halfSize);
    var topRight = new Vector3(center.X + halfSize, center.Y + halfSize, center.Z + halfSize);

    // Emit the vertices
    vo_WorldPosition = bottomLeft;
    gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomLeft, 1.0f);
    EmitVertex();

    vo_WorldPosition = bottomRight;
    gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomRight, 1.0f);
    EmitVertex();

    vo_WorldPosition = topLeft;
    gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topLeft, 1.0f);
    EmitVertex();

    vo_WorldPosition = topRight;
    gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topRight, 1.0f);
    EmitVertex();
  }

  private void GenerateFace(Vector3 center, float halfSize, Vector3 normal)
  {
    // Calculate the tangent and bitangent vectors for the face
    var tangent = new Vector3();
    var bitangent = new Vector3();

    if (abs(normal.Y) == 1.0f) // Top or bottom face
    {
      tangent = new Vector3(1, 0, 0);
      bitangent = new Vector3(0, 0, 1);
    }
    else if (abs(normal.X) == 1.0f) // Left or right face
    {
      tangent = new Vector3(0, 1, 0);
      bitangent = new Vector3(0, 0, 1);
    }
    else // Front or back face
    {
      tangent = new Vector3(1, 0, 0);
      bitangent = new Vector3(0, 1, 0);
    }

    // Scale the tangent and bitangent vectors by halfSize
    tangent *= halfSize;
    bitangent *= halfSize;

    // Offset the center of the face along the normal direction
    /*if (abs(normal.Z) == 1.0f)
    {
      normal = -normal;
    }*/
    Vector3 faceCenter = center + (normal * halfSize);

    // Calculate the four vertices of the face
    var bottomLeft = faceCenter - tangent - bitangent;
    var bottomRight = faceCenter + tangent - bitangent;
    var topLeft = faceCenter - tangent + bitangent;
    var topRight = faceCenter + tangent + bitangent;

    // Emit vertices based on the face normal
    if (normal.Y == 1.0f || normal.X == -1.0f || normal.Z == -1.0f) // Front-facing
    {
      vo_WorldPosition = bottomLeft;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = bottomRight;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomRight, 1.0f);
      EmitVertex();

      vo_WorldPosition = topLeft;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = topRight;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topRight, 1.0f);
      EmitVertex();
    }
    else // Back-facing
    {
      vo_WorldPosition = bottomLeft;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = topLeft;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = bottomRight;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomRight, 1.0f);
      EmitVertex();

      vo_WorldPosition = topRight;
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topRight, 1.0f);
      EmitVertex();
    }
    
    EndPrimitive();
  }

  public void Main()
  {
    var center = gl_in[0].gl_Position.XYZ;
    var center4 = new Vector4(center, 1.0f);
    var centerClip = uProjectionMatrix * uViewMatrix * center4;
    var aa = centerClip.XYZ;
    var bb = centerClip.W;
    var centerNDC = aa / bb; // Perform perspective divide

    if (centerNDC.X < -0.9f || centerNDC.X > 0.9f)
    {
      return;
    }

    if (centerNDC.Y < -0.9f || centerNDC.Y > 0.9f)
    {
      return;
    }

    GenerateFace(center, 0.5f, new Vector3(0, 1, 0));
    GenerateFace(center, 0.5f, new Vector3(0, -1, 0));
    GenerateFace(center, 0.5f, new Vector3(-1, 0, 0));
    GenerateFace(center, 0.5f, new Vector3(1, 0, 0));
    GenerateFace(center, 0.5f, new Vector3(0, 0, -1));
    GenerateFace(center, 0.5f, new Vector3(0, 0, 1));
    
    // Half-size of the cube
    /*float halfSize = 0.5f;

    // Cube vertices offsets from the center
    var offsets = new[]
    {
      new Vector3(-halfSize, -halfSize, -halfSize), // 0
      new Vector3(halfSize, -halfSize, -halfSize), // 1
      new Vector3(halfSize, halfSize, -halfSize), // 2
      new Vector3(-halfSize, halfSize, -halfSize), // 3
      new Vector3(-halfSize, -halfSize, halfSize), // 4
      new Vector3(halfSize, -halfSize, halfSize), // 5
      new Vector3(halfSize, halfSize, halfSize), // 6
      new Vector3(-halfSize, halfSize, halfSize) // 7
    };

    // Cube faces (triangle strip order)
    var indices = new[]
    {
      0, 1, 3, 2, // Front face
      7, 6, 4, 5, // Back face
      0, 1, 4, 5, // Bottom face
      3, 2, 7, 6 // Top face
    };

    // Emit vertices for the cube
    for (int i = 0; i < 14; i++)
    {
      var position = center + offsets[indices[i]];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(position, 1.0f);
      vo_WorldPosition = position;

      EmitVertex();
    }*/

   
  }
}

public class VoxelFragmentShader : Shader_Base
{
  [ShaderFieldIn] public Vector3 vo_WorldPosition;

  [ShaderFieldIn] public Vector2 vo_UV;

  //[ShaderFieldIn] public Vector3 vo_CameraPosition;
  [ShaderFieldIn] public Vector3 vo_Normal;

  [ShaderFieldOut] public Vector4 color;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uAlbedoTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uLightTexture;
  [ShaderFieldUniform] public Vector3 uCameraPosition;

  [ShaderFieldUniform] public Vector4 uFogData;

  public void Main()
  {
    var texelColor = texture(uAlbedoTexture, vo_UV);

    // color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

    var fogStart = 0;
    var fogEnd = 128f * 3f;

    // Calculate the distance from the camera to the fragment
    float distance = length(vo_WorldPosition - uCameraPosition);

    // Compute fog factor (linear fog)
    float fogFactor = clamp((fogEnd - distance) / (fogEnd - fogStart), 0.0f, 1.0f);

    // Interpolate between the fragment color and the fog color
    var objectColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // Example object color
    color = mix(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), objectColor, fogFactor);

    // Light
    // Calculate normalized light direction
    //var lightPosition = normalize(new Vector3(0.0f, 1.0f, 1.0f));
    //var lightDir = normalize(new Vector3(0.0f, 1.0f, 1.0f));

    // Normalize the normal (it should already be normalized if provided correctly)
    //var normal = normalize(vo_Normal);

    // Lambertian reflection: max(0, dot(N, L))
    //float diff = max(dot(normal, lightDir), 0.5f);

    // Final diffuse color
    //var diffuse = uDiffuseColor * uLightColor * diff;

    /*color.R *= diff;
    color.G *= diff;
    color.B *= diff;*/

    /*var mat = getMaterial(uAlbedoTexture, uNormalTexture, uRoughnessTexture, uMetallicTexture, vo_UV, vo_TBN);

    // Light
    var finalColor = new Vector3(0.0f);
    var lightAmount = getLightAmount(uLightTexture);
    for (var i = 0; i < lightAmount; i++)
    {
      var light1 = getLight(uLightTexture, toUInt(i));
      finalColor += PBR(mat, light1, uSkybox, vo_CameraPosition, vo_Position);
    }

    // HDR
    finalColor = finalColor / (finalColor + new Vector3(1.0f));

    // Gamma
    finalColor = pow(finalColor, new Vector3(1.0f / 2.2f));

    color = new Vector4(finalColor, mat.Alpha) * uTint;*/
  }
}