using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpStruct
{
  public string Name => _structDeclaration.Identifier.Text;

  public List<SharpField> FieldList
  {
    get
    {
      // Извлекаем только те поля, которые находятся непосредственно внутри целевого класса
      var fieldsInClass = _structDeclaration.Members
        .OfType<FieldDeclarationSyntax>();
      return fieldsInClass.Select(field => new SharpField(field)).ToList();
    }
  }

  public List<SharpMethod> MethodList
  {
    get
    {
      // Извлекаем только те методы, которые находятся непосредственно внутри целевого класса
      var methodsInClass = _structDeclaration.Members
        .OfType<MethodDeclarationSyntax>();
      return methodsInClass.Select(method => new SharpMethod(method)).ToList();
    }
  }

  public List<SharpAttribute> AttributeList =>
    (from attributeList in _structDeclaration.AttributeLists
      from attribute in attributeList.Attributes
      select new SharpAttribute(attribute)).ToList();

  private StructDeclarationSyntax _structDeclaration;
  private CompilationUnitSyntax _syntaxTree;
  private SemanticModel _semanticModel;

  public SharpStruct(CompilationUnitSyntax syntaxTree, SemanticModel semanticModel,
    StructDeclarationSyntax structDeclaration)
  {
    _structDeclaration = structDeclaration;
    _syntaxTree = syntaxTree;
    _semanticModel = semanticModel;
  }

  // Add any additional properties or methods you need for structures here
}