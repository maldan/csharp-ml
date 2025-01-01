using System;
using MegaLib.Mathematics.LinearAlgebra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MegaLib.Compiler.Sharp;

namespace MegaLib.Render.Shader;

[AttributeUsage(AttributeTargets.Field)]
public class ShaderFieldUniformAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class ShaderFieldInAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class ShaderFieldOutAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class ShaderFieldAttributeAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ShaderBuiltinMethodAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class ShaderEmbedText(string value) : Attribute
{
  public string Value { get; } = value;
}

public class ShaderProgram
{
  // Класс для хранения информации о параметрах метода
  public struct ParameterInfo
  {
    public string Name;
    public string Type;
  }

  public static Dictionary<string, string> Compile(string shaderName)
  {
    var sharpCompiler = new SharpCompiler();
    sharpCompiler.AddCode(GetShaderText($"MegaLib.src.Render.Shader.Shader_Base.cs"));
    sharpCompiler.AddCode(GetShaderText($"MegaLib.src.Render.Shader.Shader_PBR.cs"));
    sharpCompiler.AddCode(GetShaderText($"MegaLib.src.Render.Shader.Shader_{shaderName}.cs"));
    sharpCompiler.Parse();

    var dict = new Dictionary<string, string>();

    foreach (var sharpClass in sharpCompiler.ClassList)
    {
      if (sharpClass.Name.Contains("Vertex")) dict["vertex"] = CompileClass(sharpClass);
      if (sharpClass.Name.Contains("Geometry")) dict["geometry"] = CompileClass(sharpClass);
      if (sharpClass.Name.Contains("Fragment")) dict["fragment"] = CompileClass(sharpClass);
    }

    return dict;
  }

  private static string CompileClass(SharpClass sharpClass, int classIndex = 0)
  {
    var outShader = new List<string>();

    // Самый первый класс
    if (classIndex == 0)
    {
      outShader.Add("#version 330 core");
      outShader.Add("");
      outShader.Add("precision highp float;");
      outShader.Add("precision highp int;");
      outShader.Add("precision highp usampler2D;");
      outShader.Add("precision highp sampler2D;");
      outShader.Add("");

      if (sharpClass.HasAttribute("ShaderEmbedText"))
      {
        var attr = sharpClass.GetAttribute("ShaderEmbedText");
        foreach (var x in attr.PositionalArguments)
        {
          var lines = x[1..^1].Split("\\n");
          foreach (var line in lines)
          {
            outShader.Add(line);
          }
        }
        outShader.Add("");
      }

      var locationCounter = 0;
      var locationOutCounter = 0;
      foreach (var sharpField in sharpClass.FieldList)
      {
        if (sharpField.HasAttribute("ShaderFieldUniform"))
          outShader.Add($"uniform {ReplaceTypes(sharpField.Type)} {sharpField.Name};");
        if (sharpField.HasAttribute("ShaderFieldAttribute"))
          outShader.Add(
            $"layout (location = {locationCounter++}) in {ReplaceTypes(sharpField.Type)} {sharpField.Name};");
        
        // In
        if (sharpField.HasAttribute("ShaderFieldIn"))
        {
          var flat = "";
          var type = ReplaceTypes(sharpField.Type.Replace("[]", ""));
          if (type == "int" || type == "uint") flat = "flat";
          
          if (sharpField.Type.Contains("[]"))
          {
            outShader.Add($"{flat} in {type} {sharpField.Name}[];");
          }
          else
          {
            outShader.Add($"{flat} in {type} {sharpField.Name};");
          }
        }
        
        // Out
        if (sharpField.HasAttribute("ShaderFieldOut"))
        {
          var layout = "";
          if (sharpClass.Name.Contains("Fragment"))
          {
            layout = $"layout(location = {locationOutCounter++})";
          }
          var type = ReplaceTypes(sharpField.Type);
          var flat = "";
          if (type == "int" || type == "uint") flat = "flat";
          outShader.Add($"{layout} {flat} out {type} {sharpField.Name};");
        }
      }

      outShader.Add("");
    }

    // Если есть базовый класс
    if (sharpClass.Parent != null) outShader.Add(CompileClass(sharpClass.Parent, classIndex + 1));

    foreach (var sharpStruct in sharpClass.StructList)
    {
      outShader.Add($"struct {sharpStruct.Name} {{");
      outShader.AddRange(sharpStruct.FieldList.Select(sharpField =>
        $"    {ReplaceTypes(sharpField.Type)} {sharpField.Name};"));
      outShader.Add("};");
      outShader.Add("");
    }

    foreach (var sharpMethod in sharpClass.MethodList)
    {
      if (sharpMethod.HasAttribute("ShaderBuiltinMethod")) continue;

      var methodName = sharpMethod.Name;
      var returnType = ReplaceTypes(sharpMethod.ReturnType);
      if (methodName.ToLower() == "main")
      {
        methodName = "main";
        returnType = "void";
      }

      var methodHeader = "";
      methodHeader += $"{returnType} {methodName}";
      methodHeader += "(";
      methodHeader += string.Join(", ", sharpMethod.ParameterList.Select(x => $"{ReplaceTypes(x.Type)} {x.Name}"));
      methodHeader += ") {";
      outShader.Add(methodHeader);

      var methodText = "";

      foreach (var sharpStatement in sharpMethod.StatementList)
      {
        methodText += sharpStatement.GetTextWithExplicitVariableDeclaration() + "\n";
      }

      // Заменяем декларацию переменные типа Vector3 normal = на vec3 normal = 
      methodText = Regex.Replace(methodText, $@"([a-zA-Z0-9\.]+) ([a-zA-Z0-9]+) =",
        (match) => $"{ReplaceTypes(match.Groups[1].Value)} {match.Groups[2].Value} =");
      
      // Заменяем декларацию переменные типа Vector3[] normal = new[] на vec3 normal[] = vec3[]
      methodText = Regex.Replace(methodText, @" = new\[\]\s*\{([\s\S]*?)\};",
        (match) => @" = new[] (" +match.Groups[1].Value+");");
      
      // Заменяем декларацию переменные типа Vector3[] normal = new[] на vec3 normal[] = vec3[]
      methodText = Regex.Replace(methodText, $@"([a-zA-Z0-9\.]+)\[\] ([a-zA-Z0-9]+) = new\[\]",
        (match) => $"{ReplaceTypes(match.Groups[1].Value)} {match.Groups[2].Value}[] = {ReplaceTypes(match.Groups[1].Value)}[]");
      
      // Заменяем new Vector3( на vec3(
      methodText = Regex.Replace(methodText, $@"\bnew ([a-zA-Z0-9]+)\(",
        (match) => $"{ReplaceTypes(match.Groups[1].Value)}(");

      // Заменяем new Vector3 X; на vec3 X;
      methodText = Regex.Replace(methodText, $@"([a-zA-Z0-9_]+) ([a-zA-Z0-9_]+);",
        (match) => $"{ReplaceTypes(match.Groups[1].Value)} {match.Groups[2].Value};");

      // Заменяем Light light = Light();
      methodText = Regex.Replace(methodText, @"([a-zA-Z0-9_]+) ([a-zA-Z0-9_]+) = \1\(\);", "$1 $2;");

      // Заменяем .XYZW на .xyzw
      methodText = Regex.Replace(methodText, @"\.([XYZWRGBA]+)(?=\b)", match => $".{match.Groups[1].Value.ToLower()}");

      // Удаляем ненужное
      methodText = methodText.Replace("MegaLib.Render.Shader.MegaLib.Render.Shader.Shader_", "");

      // Константы
      methodText = methodText.Replace("MathF.PI", MathF.PI.ToString().Replace(",", "."));

      // Привидение типов
      methodText = methodText.Replace("toInt(", "int(");
      methodText = methodText.Replace("toUInt(", "uint(");

      // Для размера точек
      methodText = methodText.Replace("float gl_PointSize", "gl_PointSize");

      // Прочие фичи
      methodText = methodText.Replace("for (var", "for (int");
      methodText = Regex.Replace(methodText, $@"for \(\; ([a-zA-Z0_9]+)\b",
        (match) => $"for (int {match.Groups[1].Value} = 0; {match.Groups[1].Value}");
      methodText = methodText.Replace("discard()", "discard");

      // В главном методе делаем так
      if (methodName == "main") methodText = methodText.Replace("return ", "gl_Position = ");

      outShader.Add(methodText);

      outShader.Add("}");
      outShader.Add("");
    }

    return string.Join("\n", outShader);
  }

  private static string ReplaceTypes(string sharpType)
  {
    sharpType = sharpType.Split(".").Last();
    
    return sharpType switch
    {
      "Matrix3x3" => "mat3",
      "Matrix4x4" => "mat4",

      "IVector2" => "ivec2",
      "IVector3" => "ivec3",
      "IVector4" => "ivec4",

      "Vector2" => "vec2",
      "Vector3" => "vec3",
      "Vector3[]" => "vec3",
      "Vector4" => "vec4",
      "Texture_2D<float>" => "sampler2D",
      "Texture_2D<RGBA32F>" => "sampler2D",
      "Texture_2D<RGBA8>" => "sampler2D",
      "Texture_Cube" => "samplerCube",
      _ => sharpType
    };
  }

  private static string GetShaderText(string resourceName)
  {
    var assembly = Assembly.GetExecutingAssembly();

    using (var stream = assembly.GetManifestResourceStream(resourceName))
    {
      if (stream != null)
      {
        using (var reader = new StreamReader(stream))
        {
          return reader.ReadToEnd();
        }
      }
      else
      {
        throw new Exception($"Ресурс {resourceName} не найден.");
      }
    }
  }
}