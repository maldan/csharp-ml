using System;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

public class VoxelVertexShader : Shader_Base
{
  [ShaderFieldAttribute] public Vector3 aPosition;
  [ShaderFieldAttribute] public int aVoxel;
  [ShaderFieldAttribute] public int aShadow;
  [ShaderFieldAttribute] public uint aColor;
  
  [ShaderFieldOut] public int vo_Voxel;
  [ShaderFieldOut] public int vo_Shadow;
  [ShaderFieldOut] public uint vo_Color;
  
  public Vector4 Main()
  {
    vo_Voxel = aVoxel;
    vo_Shadow = aShadow;
    vo_Color = aColor;
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
  [ShaderFieldOut] public Vector3 vo_Normal;
  [ShaderFieldOut] public float vo_Light;
  [ShaderFieldOut] public int go_Shadow;
  [ShaderFieldOut] public uint go_Color;
  
  [ShaderFieldIn] public int[] vo_Voxel;
  [ShaderFieldIn] public int[] vo_Shadow;
  [ShaderFieldIn] public uint[] vo_Color;
   
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

    var lightValue = 1.0f;  // 00100000 - Right face
    if ((vo_Voxel[0] & (1 << 6)) != 0)
    {
      lightValue = 1f;
    }
    
    // Emit vertices based on the face normal
    if (normal.Y == 1.0f || normal.X == -1.0f || normal.Z == -1.0f) // Front-facing
    {
      vo_WorldPosition = bottomLeft;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = bottomRight;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomRight, 1.0f);
      EmitVertex();

      vo_WorldPosition = topLeft;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = topRight;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topRight, 1.0f);
      EmitVertex();
    }
    else // Back-facing
    {
      vo_WorldPosition = bottomLeft;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = topLeft;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(topLeft, 1.0f);
      EmitVertex();

      vo_WorldPosition = bottomRight;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
      gl_Position = uProjectionMatrix * uViewMatrix * new Vector4(bottomRight, 1.0f);
      EmitVertex();

      vo_WorldPosition = topRight;
      vo_Normal = normal;
      vo_Light = lightValue;
      go_Shadow = vo_Shadow[0];
      go_Color = vo_Color[0];
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

    // Masks for each side
    // Bitmask for voxelInfo
    int backMask = 1 << 0;  // 00000001 - Back face
    int frontMask = 1 << 1; // 00000010 - Front face
    int bottomMask = 1 << 2; // 00000100 - Bottom face
    int topMask = 1 << 3;    // 00001000 - Top face
    int leftMask = 1 << 4;   // 00010000 - Left face
    int rightMask = 1 << 5;  // 00100000 - Right face
    
    if ((vo_Voxel[0] & backMask) != 0) GenerateFace(center, 0.5f, new Vector3(0, 0, -1));
    if ((vo_Voxel[0] & frontMask) != 0) GenerateFace(center, 0.5f, new Vector3(0, 0, 1));
    if ((vo_Voxel[0] & topMask) != 0) GenerateFace(center, 0.5f, new Vector3(0, 1, 0));
    if ((vo_Voxel[0] & bottomMask) != 0) GenerateFace(center, 0.5f, new Vector3(0, -1, 0));
    if ((vo_Voxel[0] & leftMask) != 0) GenerateFace(center, 0.5f, new Vector3(-1, 0, 0));
    if ((vo_Voxel[0] & rightMask) != 0) GenerateFace(center, 0.5f, new Vector3(1, 0, 0));
    
    /*GenerateFace(center, 0.5f, new Vector3(0, 1, 0));
    GenerateFace(center, 0.5f, new Vector3(0, -1, 0));
    GenerateFace(center, 0.5f, new Vector3(-1, 0, 0));
    GenerateFace(center, 0.5f, new Vector3(1, 0, 0));
    GenerateFace(center, 0.5f, new Vector3(0, 0, -1));
    GenerateFace(center, 0.5f, new Vector3(0, 0, 1));*/
  }
}

public class VoxelFragmentShader : Shader_Base
{
  [ShaderFieldIn] public Vector3 vo_WorldPosition;

  [ShaderFieldIn] public Vector2 vo_UV;

  //[ShaderFieldIn] public Vector3 vo_CameraPosition;
  [ShaderFieldIn] public Vector3 vo_Normal;
  [ShaderFieldIn] public float vo_Light;
  [ShaderFieldIn] public int go_Shadow;
  [ShaderFieldIn] public uint go_Color;

  [ShaderFieldOut] public Vector4 fragColor;
  [ShaderFieldOut] public Vector4 fragNormal;

  [ShaderFieldUniform] public Texture_2D<RGBA32F> uAlbedoTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uNormalTexture;
  [ShaderFieldUniform] public Texture_2D<RGBA32F> uLightTexture;
  [ShaderFieldUniform] public Vector3 uCameraPosition;

  [ShaderFieldUniform] public Vector4 uFogData;

  public void Main()
  {
    var r = ((go_Color >> 24) & toUInt(0xFF)) / 255.0f; // Extract Red
    var g = ((go_Color >> 16) & toUInt(0xFF)) / 255.0f; // Extract Green
    var b = ((go_Color >> 8) & toUInt(0xFF)) / 255.0f;  // Extract Blue
    var texelColor = new Vector4(r, g, b, 1.0f);
    
    // color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

    var fogStart = 64f;
    var fogEnd = 128f * 3f;

    // Calculate the distance from the camera to the fragment
    float distance = length(vo_WorldPosition - uCameraPosition);

    // Compute fog factor (linear fog)
    float fogFactor = clamp((fogEnd - distance) / (fogEnd - fogStart), 0.0f, 1.0f);

    // Interpolate between the fragment color and the fog color
    //var objectColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // Example object color
    var finalColor = mix(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), texelColor, fogFactor*0.001f+1f);

    // Light
    // Calculate normalized light direction
    //var lightPosition = normalize(new Vector3(0.0f, 1.0f, 1.0f));
    var lightDir = normalize(new Vector3(1.0f, 1.0f, 1.0f));

    // Normalize the normal (it should already be normalized if provided correctly)
    var normal = normalize(vo_Normal);

    // Lambertian reflection: max(0, dot(N, L))
    float diff = max(dot(normal, lightDir), 0.25f);

    // Final diffuse color
    //var diffuse = uDiffuseColor * uLightColor * diff;

    finalColor.R *= diff * vo_Light + go_Shadow * 0.0001f;
    finalColor.G *= diff * vo_Light + go_Shadow * 0.0001f;
    finalColor.B *= diff * vo_Light + go_Shadow * 0.0001f;

    // HDR
    //var finalColor = new Vector3(color.R, color.G, color.B);

    // HDR
    //finalColor = finalColor / (finalColor + new Vector3(1.0f));
    
    // Gamma
    //finalColor = pow(finalColor, new Vector3(1.0f / 2.2f));

    fragColor = finalColor;
    fragNormal = new Vector4(normal, 1.0f);
    
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