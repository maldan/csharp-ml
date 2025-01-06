using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpClass
{
  public bool IsPublic;
  public string Name => _classDeclaration.Identifier.Text;

  public List<SharpField> FieldList
  {
    get
    {
      var fieldsList = new List<SharpField>();

      // Извлекаем только те поля, которые находятся непосредственно внутри целевого класса
      var fieldsInClass = _classDeclaration.Members
        .OfType<FieldDeclarationSyntax>();

      foreach (var field in fieldsInClass)
      {
        fieldsList.Add(new SharpField(field));

        /*// Тип поля
        var fieldType = field.Declaration.Type.ToString();

        // Имена полей (если объявлено несколько через запятую)
        var fieldNames = field.Declaration.Variables.Select(v => v.Identifier.Text);

        foreach (var name in fieldNames)
        {
          // Список атрибутов для данного поля
          var attributes = new List<AttributeSyntax>();

          // Проверяем наличие атрибутов у поля
          foreach (var attributeList in field.AttributeLists)
          {
            foreach (var attribute in attributeList.Attributes)
            {
              // Добавляем атрибут в список
              attributes.Add(attribute);
            }
          }

          // Добавляем информацию о поле и его атрибутах
          fieldsList.Add(new SharpField
          {
            Name = name,
            Type = fieldType
          });
        }*/
      }

      return fieldsList;
    }
  }

  public List<SharpField> AllFieldList
  {
    get
    {
      var allFields = new List<SharpField>();

      // Process the current class
      CollectFieldsFromClass(_classDeclaration, allFields);

      // Traverse the inheritance chain
      var classSymbol = _semanticModel.GetDeclaredSymbol(_classDeclaration) as INamedTypeSymbol;
      while (classSymbol != null && classSymbol.BaseType != null)
      {
        // Get the syntax node of the base type if available
        var baseTypeSymbol = classSymbol.BaseType;
        var baseTypeSyntax =
          baseTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;

        if (baseTypeSyntax != null)
        {
          // Collect fields from the base type
          CollectFieldsFromClass(baseTypeSyntax, allFields);
        }

        // Move up the inheritance chain
        classSymbol = baseTypeSymbol;
      }

      return allFields;
    }
  }

  public List<SharpMethod> MethodList
  {
    get
    {
      // Извлекаем только те методы, которые находятся непосредственно внутри целевого класса
      var methodsInClass = _classDeclaration.Members
        .OfType<MethodDeclarationSyntax>();
      return methodsInClass.Select(method => new SharpMethod(method, _semanticModel)).ToList();
    }
  }

  public List<SharpAttribute> AttributeList =>
    (from attributeList in _classDeclaration.AttributeLists
      from attribute in attributeList.Attributes
      select new SharpAttribute(attribute)).ToList();

  public List<SharpStruct> StructList
  {
    get
    {
      var structsList = new List<SharpStruct>();

      // Извлекаем только те структуры, которые находятся непосредственно внутри целевого класса
      var structsInClass = _classDeclaration.Members
        .OfType<StructDeclarationSyntax>();

      foreach (var structDeclaration in structsInClass)
      {
        structsList.Add(new SharpStruct(_syntaxTree, _semanticModel, structDeclaration));
      }

      return structsList;
    }
  }

  public SharpClass Parent
  {
    get
    {
      var baseList = _classDeclaration.BaseList;

      if (baseList != null)
      {
        var baseType = baseList.Types.FirstOrDefault();

        if (baseType != null)
        {
          var baseTypeSyntax = baseType.Type;
          var baseSymbol = _semanticModel.GetSymbolInfo(baseTypeSyntax).Symbol as INamedTypeSymbol;

          if (baseSymbol != null)
          {
            var parentClassDeclaration = baseSymbol.DeclaringSyntaxReferences
              .Select(reference => reference.GetSyntax() as ClassDeclarationSyntax)
              .FirstOrDefault();

            if (parentClassDeclaration != null)
            {
              return new SharpClass(_syntaxTree, _semanticModel, parentClassDeclaration);
            }
          }
        }
      }

      return null;
    }
  }

  private ClassDeclarationSyntax _classDeclaration;
  private CompilationUnitSyntax _syntaxTree;
  private SemanticModel _semanticModel;

  public SharpClass(CompilationUnitSyntax syntaxTree, SemanticModel semanticModel,
    ClassDeclarationSyntax classDeclaration)
  {
    _syntaxTree = syntaxTree;
    _classDeclaration = classDeclaration;
    _semanticModel = semanticModel;
  }

  public bool HasAttribute(string name)
  {
    return AttributeList.Any(x => x.Name == name);
  }

  public SharpAttribute GetAttribute(string name)
  {
    return AttributeList.FirstOrDefault(x => x.Name == name);
  }

  private void CollectFieldsFromClass(ClassDeclarationSyntax classDeclaration, List<SharpField> fieldsList)
  {
    var fieldsInClass = classDeclaration.Members.OfType<FieldDeclarationSyntax>();

    foreach (var field in fieldsInClass)
    {
      var sharpField = new SharpField(field);
      fieldsList.Add(sharpField);
    }
  }
}