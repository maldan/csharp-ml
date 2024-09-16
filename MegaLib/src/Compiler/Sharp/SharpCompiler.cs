using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.Render.Color;
using MegaLib.Render.Shader;
using MegaLib.Render.Texture;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpCompiler
{
  private string _code = "";
  private SyntaxTree _syntaxTree;
  private CompilationUnitSyntax _root;
  private SemanticModel _semanticModel;

  public List<SharpClass> ClassList
  {
    get
    {
      var classDeclarations = _root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .ToList();
      return classDeclarations.Select(cds => new SharpClass(_root, _semanticModel, cds)).ToList();
    }
  }

  public void AddCode(string code)
  {
    _code += code + "\n\n";
  }

  public void Parse()
  {
    // Парсим код метода с помощью Roslyn
    _syntaxTree = CSharpSyntaxTree.ParseText(_code);
    _root = _syntaxTree.GetRoot() as CompilationUnitSyntax;
    var compilation = CSharpCompilation.Create("ShaderAnalysis")
      .AddSyntaxTrees(_syntaxTree)
      .AddReferences(
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Vector2).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Vector3).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Vector4).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Texture_Cube).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Texture_2D<RGBA<float>>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(MathF).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Shader_Base).Assembly.Location)
      );

    // Создаем семантическую модель для анализа типов
    _semanticModel = compilation.GetSemanticModel(_syntaxTree);
  }
}