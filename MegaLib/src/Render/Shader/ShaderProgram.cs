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
using MegaLib.Render.Color;
using MegaLib.Render.Texture;

namespace MegaLib.Render.Shader;

[AttributeUsage(AttributeTargets.Field)] // Указываем, что атрибут можно применять только к полям
public class ShaderFieldAttribute(string description) : Attribute
{
  public string Description { get; } = description;
}

[AttributeUsage(AttributeTargets.Method)] // Указываем, что атрибут можно применять только к полям
public class ShaderMethodAttribute(string description) : Attribute
{
  public string Description { get; } = description;
}

[AttributeUsage(AttributeTargets.Method)] // Указываем, что атрибут можно применять только к полям
public class ShaderBuiltinMethodAttribute : Attribute;

public class ShaderProgram
{
  // Класс для хранения информации о поле
  public struct FieldInfo
  {
    public string Name;
    public string Type;
    public List<AttributeSyntax> Attributes;
  }

  public struct ClassInfo
  {
    public string Name;
    public string ShaderType;
  }

  public struct StructInfo
  {
    public string Name;
    public List<FieldInfo> Fields;
  }

  // Класс для хранения информации о методе
  public struct MethodInfo
  {
    public string Name;
    public string ReturnType;
    public List<ParameterInfo> Parameters;
    public List<AttributeSyntax> Attributes;
  }

  // Класс для хранения информации о параметрах метода
  public struct ParameterInfo
  {
    public string Name;
    public string Type;
  }

  public static Dictionary<string, string> Compile2(string shaderName)
  {
    var sharpCompiler = new SharpCompiler();
    sharpCompiler.AddCode(GetShaderText($"MegaLib.src.Render.Shader.Shader_Base.cs"));
    sharpCompiler.AddCode(GetShaderText($"MegaLib.src.Render.Shader.Shader_PBR.cs"));
    sharpCompiler.AddCode(GetShaderText($"MegaLib.src.Render.Shader.Shader_{shaderName}.cs"));
    sharpCompiler.Parse();

    var dict = new Dictionary<string, string>();

    foreach (var sharpClass in sharpCompiler.ClassList)
    {
      if (sharpClass.Name.Contains("Vertex")) dict["vertex"] = CompileClass2(sharpClass);
      if (sharpClass.Name.Contains("Fragment")) dict["fragment"] = CompileClass2(sharpClass);
    }

    return dict;
  }

  private static string CompileClass2(SharpClass sharpClass)
  {
    var outShader = new List<string>();

    if (sharpClass.Parent != null)
    {
      outShader.Add(CompileClass2(sharpClass.Parent));
    }
    else
    {
      outShader.Add("#version 330 core");
      outShader.Add("");
      outShader.Add("precision highp float;");
      outShader.Add("precision highp int;");
      outShader.Add("precision highp usampler2D;");
      outShader.Add("precision highp sampler2D;");
      outShader.Add("");
    }

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

      var methodHeader = "";
      methodHeader += $"{ReplaceTypes(sharpMethod.ReturnType)} {sharpMethod.Name}";
      methodHeader += "(";
      methodHeader += string.Join(", ", sharpMethod.ParameterList.Select(x => $"{ReplaceTypes(x.Type)} {x.Name}"));
      methodHeader += ") {";
      outShader.Add(methodHeader);
      outShader.Add("}");
      outShader.Add("");
    }

    return string.Join("\n", outShader);
  }

  public static Dictionary<string, string> Compile(string shaderName)
  {
    // language=C#
    var fullSource = "";
    fullSource += GetShaderText($"MegaLib.src.Render.Shader.Shader_Base.cs") + "\n";
    fullSource += GetShaderText($"MegaLib.src.Render.Shader.Shader_PBR.cs") + "\n";
    fullSource += GetShaderText($"MegaLib.src.Render.Shader.Shader_{shaderName}.cs");

    // Парсим код метода с помощью Roslyn
    var tree = CSharpSyntaxTree.ParseText(fullSource);
    var root = tree.GetRoot() as CompilationUnitSyntax;
    var compilation = CSharpCompilation.Create("ShaderAnalysis")
      .AddSyntaxTrees(tree)
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
    var model = compilation.GetSemanticModel(tree);

    // Получаем список классов
    var classList = GetClasses(root);
    var shaders = new Dictionary<string, string>();
    for (var i = 0; i < classList.Count; i++)
    {
      shaders[classList[i].ShaderType] = CompileClass(root, model, classList[i]);
      /*Console.WriteLine(classList[i].ShaderType);
      Console.WriteLine(shaders[classList[i].ShaderType]);
      Console.WriteLine("");*/
    }

    return shaders;
  }

  private static string CompileClass(SyntaxNode root, SemanticModel model, ClassInfo classInfo)
  {
    var outShader = new List<string>();
    outShader.Add("#version 330 core");
    outShader.Add("");
    outShader.Add("precision highp float;");
    outShader.Add("precision highp int;");
    outShader.Add("precision highp usampler2D;");
    outShader.Add("precision highp sampler2D;");
    outShader.Add("");

    // Сначала все параметры прописываем
    var fieldList = GetFields(root, classInfo.Name);
    var locationCounter = 0;
    for (var i = 0; i < fieldList.Count; i++)
    {
      if (fieldList[i].Attributes.Count <= 0)
        throw new Exception($"Need attribute for field {fieldList[i].Type} {fieldList[i].Name}");
      if (fieldList[i].Attributes[0].Name.ToString() != "ShaderField")
        throw new Exception("Need ShaderField attribute");

      var fieldType = fieldList[i].Attributes[0].ArgumentList.Arguments[0].Expression.ToString();
      fieldType = fieldType.Substring(1, fieldType.Length - 2);

      outShader.Add(fieldType switch
      {
        "attribute" =>
          $"layout (location = {locationCounter++}) in {ReplaceTypes(fieldList[i].Type)} {fieldList[i].Name};",
        "uniform" => $"uniform {ReplaceTypes(fieldList[i].Type)} {fieldList[i].Name};",
        "in" => $"in {ReplaceTypes(fieldList[i].Type)} {fieldList[i].Name};",
        "out" => $"out {ReplaceTypes(fieldList[i].Type)} {fieldList[i].Name};",
        _ => throw new Exception("Unknown field type")
      });
    }

    outShader.Add("");

    // Структуры если есть
    var structList = GetStructs(root, classInfo.Name, true, model);
    for (var i = 0; i < structList.Count; i++)
    {
      outShader.Add($"struct {structList[i].Name} {{");
      for (var j = 0; j < structList[i].Fields.Count; j++)
      {
        outShader.Add($"{ReplaceTypes(structList[i].Fields[j].Type)} {structList[i].Fields[j].Name};");
      }

      outShader.Add("};");
      outShader.Add("");
    }

    // Потом получаем все методы
    var ignoreList = new string[]
    {
      "length", "pow", "toInt", "toUInt", "normalize", "max", "texelFetch", "discard", "texture", "dot", "reflect",
      "inverse", "transpose"
    };
    var methodList = GetMethods(root, classInfo.Name, model, true);
    for (var i = 0; i < methodList.Count; i++)
    {
      // Console.WriteLine(methodList[i].Attributes);

      var methodName = methodList[i].Name;
      Console.WriteLine(methodName);
      if (ignoreList.ToList().Contains(methodName)) continue;

      var methodReturnType = ReplaceTypes(methodList[i].ReturnType);

      if (methodName == "Main") methodName = "main";
      if (methodName == "main") methodReturnType = "void";

      var str = "";
      str += $"{methodReturnType} {methodName}(";
      for (var j = 0; j < methodList[i].Parameters.Count; j++)
      {
        str += $"{ReplaceTypes(methodList[i].Parameters[j].Type)} {methodList[i].Parameters[j].Name}";
        if (j < methodList[i].Parameters.Count - 1)
        {
          str += ", ";
        }
      }

      str += ")";
      str += "{";
      outShader.Add(str);

      // Внутри методов обрабатываем стейтменты
      var statementList = GetMethodStatements(root, classInfo.Name, methodList[i].Name, model, true);
      for (var j = 0; j < statementList.Count; j++)
      {
        CompileStatement(statementList[j], model, classInfo, methodName, outShader);
      }

      outShader.Add("}");

      for (var ii = 0; ii < outShader.Count; ii++)
      {
        var newStatement = outShader[ii];

        if (classInfo.ShaderType == "vertex" && methodName.ToLower() == "main" && newStatement.StartsWith("return"))
        {
          newStatement = newStatement.Replace("return ", "gl_Position = ");
        }

        // Прочие замены глобальных методов
        //newStatement = newStatement.Replace("Matrix3x3.Transpose", "transpose");
        //newStatement = newStatement.Replace("Matrix3x3.Inverse", "inverse");
        //newStatement = newStatement.Replace("Vector3.Normalize", "normalize");
        //newStatement = newStatement.Replace("Matrix4x4.Inverse", "inverse");
        //newStatement = newStatement.Replace(".DropW()", ".xyz");

        // newStatement = Regex.Replace(newStatement, $@"\= new vec(\d)\(\);", ";");

        foreach (var structInfo in structList)
        {
          newStatement = newStatement.Replace($"= new {structInfo.Name}();", ";");
        }

        newStatement = newStatement.Replace("Matrix3x3", "mat3");
        newStatement = newStatement.Replace("Matrix4x4", "mat4");

        newStatement = newStatement.Replace("toInt(", "int(");
        newStatement = newStatement.Replace("toUInt(", "uint(");

        newStatement = newStatement.Replace("for (var", "for (int");
        newStatement = newStatement.Replace("discard()", "discard");
        newStatement = newStatement.Replace("MathF.PI", $"{MathF.PI}".Replace(",", "."));

        newStatement = Regex.Replace(newStatement, $@"\bnew\b", "");
        newStatement = Regex.Replace(newStatement, $@"\bIVector(\d)\b", "ivec$1");
        newStatement = Regex.Replace(newStatement, $@"\bVector(\d)\b", "vec$1");

        // Заменяем в тексте
        newStatement = Regex.Replace(newStatement, @"\.([XYZWRGBA]+)(?=\b)", match =>
        {
          // Получаем совпадение
          var letter = match.Groups[1].Value;

          // Возвращаем замененный результат
          return $".{letter.ToLower()}";
        });

        outShader[ii] = newStatement;
      }
    }

    return string.Join("\n", outShader);
  }

  private static void CompileStatement(StatementSyntax statement, SemanticModel model, ClassInfo classInfo,
    string methodName, List<string> outShader)
  {
    var newStatement = statement.ToString();

    if (statement is IfStatementSyntax ifStatement)
    {
      // Получение условия
      var condition = ifStatement.Condition.ToString();

      // Получение блока if
      var ifBlock = ifStatement.Statement as BlockSyntax;
      var ifStatements = ifBlock?.Statements.Select(s => s) ?? Enumerable.Empty<StatementSyntax>();

      // Получение блока else, если есть
      var elseBlock = ifStatement.Else?.Statement as BlockSyntax;
      var elseStatements = elseBlock?.Statements.Select(s => s) ?? Enumerable.Empty<StatementSyntax>();

      // Вывод
      outShader.Add($"if ({condition}) {{");
      if (ifStatement.Statement is BlockSyntax)
      {
        foreach (var stmt in ifStatements)
        {
          CompileStatement(stmt, model, classInfo, methodName, outShader);
        }
      }
      else
      {
        CompileStatement(ifStatement.Statement, model, classInfo, methodName, outShader);
      }

      outShader.Add("}");

      // Вывод блока else, если есть
      if (ifStatement.Else != null)
      {
        outShader.Add("else {");
        if (ifStatement.Else.Statement is BlockSyntax)
        {
          foreach (var stmt in elseStatements)
          {
            CompileStatement(stmt, model, classInfo, methodName, outShader);
          }
        }
        else
        {
          CompileStatement(ifStatement.Else.Statement, model, classInfo, methodName, outShader);
        }

        outShader.Add("}");
      }

      return;
    }

    if (statement is ForStatementSyntax forStatement)
    {
      var condition = forStatement.Condition?.ToString() ?? "";
      var incrementors = forStatement.Incrementors.Select(i => i.ToString()).ToList();
      var initializers = forStatement.Initializers.Select(i => i.ToString()).ToList();

      var iin = string.Join(", ", initializers);
      if (iin == "")
      {
        iin = $"int {incrementors[0].Replace("++", "")} = 0";
      }

      outShader.Add($"for ({iin}; {condition}; {string.Join(", ", incrementors)}) {{");

      // Проверяем, является ли Statement блоком или одиночным выражением
      if (forStatement.Statement is BlockSyntax block)
      {
        foreach (var stmt in block.Statements)
        {
          CompileStatement(stmt, model, classInfo, methodName, outShader);
        }
      }
      else
      {
        // Если это не блок, то обрабатываем как одиночный Statement
        CompileStatement(forStatement.Statement, model, classInfo, methodName, outShader);
      }

      outShader.Add("}");
      return;
    }

    if (IsVariableDeclaration(statement))
    {
      var varType = GetVariableType(statement, model);
      if (varType == string.Empty) throw new Exception("FUCK!");
      newStatement = newStatement.Replace("var ", ReplaceTypes(varType) + " ");
    }


    outShader.Add(newStatement);
  }

  public static List<StructInfo> GetStructs(SyntaxNode root, string className, bool includeInherited = false,
    SemanticModel semanticModel = null)
  {
    var structsInfo = new List<StructInfo>();

    var classDeclaration = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == className);

    if (classDeclaration != null)
    {
      var classSymbol = semanticModel?.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

      // Получаем все вложенные структуры в текущем классе
      var structs = classDeclaration.DescendantNodes()
        .OfType<StructDeclarationSyntax>();

      foreach (var structDeclaration in structs)
      {
        var structInfo = new StructInfo
        {
          Name = structDeclaration.Identifier.Text,
          Fields = new List<FieldInfo>()
        };

        var fieldDeclarations = structDeclaration.DescendantNodes()
          .OfType<FieldDeclarationSyntax>();

        foreach (var field in fieldDeclarations)
        {
          var fieldType = field.Declaration.Type.ToString();
          var fieldNames = field.Declaration.Variables.Select(v => v.Identifier.Text);

          foreach (var name in fieldNames)
          {
            structInfo.Fields.Add(new FieldInfo
            {
              Name = name,
              Type = fieldType
            });
          }
        }

        structsInfo.Add(structInfo);
      }

      if (includeInherited && classSymbol != null)
      {
        AddInheritedStructs(classSymbol, root, structsInfo, semanticModel);
      }
    }

    return structsInfo;
  }

  private static void AddInheritedStructs(INamedTypeSymbol classSymbol, SyntaxNode root, List<StructInfo> structsInfo,
    SemanticModel semanticModel)
  {
    var baseType = classSymbol.BaseType;

    if (baseType != null)
    {
      var baseClassSyntax = root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .FirstOrDefault(c => c.Identifier.Text == baseType.Name);

      if (baseClassSyntax != null)
      {
        var baseStructs = baseClassSyntax.DescendantNodes()
          .OfType<StructDeclarationSyntax>()
          .Where(s => !structsInfo.Any(si => si.Name == s.Identifier.Text))
          .Select(s => new StructInfo
          {
            Name = s.Identifier.Text,
            Fields = s.DescendantNodes()
              .OfType<FieldDeclarationSyntax>()
              .SelectMany(f => f.Declaration.Variables.Select(v => new FieldInfo
              {
                Name = v.Identifier.Text,
                Type = f.Declaration.Type.ToString()
              })).ToList()
          });

        structsInfo.AddRange(baseStructs);

        AddInheritedStructs(baseType, root, structsInfo, semanticModel);
      }
    }
  }

  private static List<ClassInfo> GetClasses(SyntaxNode root)
  {
    // Извлекаем все классы из дерева синтаксических узлов
    var classDeclarations = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .ToList();

    // Преобразуем ClassDeclarationSyntax в ClassInfo, проверяя имена классов
    var classInfoList = classDeclarations
      .Where(classDecl =>
        classDecl.Identifier.Text.Contains("Vertex") || classDecl.Identifier.Text.Contains("Fragment"))
      .Select(classDecl => new ClassInfo
      {
        Name = classDecl.Identifier.Text,
        ShaderType = classDecl.Identifier.Text.Contains("Vertex") ? "vertex" :
          classDecl.Identifier.Text.Contains("Fragment") ? "fragment" : null
      })
      .ToList();

    return classInfoList;
  }

  private static List<FieldInfo> GetFields(SyntaxNode root, string className)
  {
    var fieldsList = new List<FieldInfo>();

    // Находим объявление класса по имени
    var classDeclaration = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == className);

    if (classDeclaration != null)
    {
      // Извлекаем только те поля, которые находятся непосредственно внутри целевого класса
      var fieldsInClass = classDeclaration.Members
        .OfType<FieldDeclarationSyntax>();

      foreach (var field in fieldsInClass)
      {
        // Тип поля
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
          fieldsList.Add(new FieldInfo
          {
            Name = name,
            Type = fieldType,
            Attributes = attributes
          });
        }
      }
    }

    return fieldsList;
  }

  private static List<MethodInfo> GetMethods(SyntaxNode root, string className, SemanticModel semanticModel,
    bool includeInherited = false)
  {
    var methodsList = new List<MethodInfo>();
    var classSymbol = GetClassSymbol(root, className, semanticModel);

    if (classSymbol != null)
    {
      var classDeclaration = root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .FirstOrDefault(c => c.Identifier.Text == className);

      if (includeInherited)
      {
        AddInheritedMethods(classSymbol, root, methodsList, semanticModel);
      }

      // Добавляем методы текущего класса
      AddClassMethods(classDeclaration, methodsList, semanticModel);
    }

    return methodsList;
  }

  private static void AddInheritedMethods(INamedTypeSymbol classSymbol, SyntaxNode root, List<MethodInfo> methodsList,
    SemanticModel semanticModel)
  {
    var baseType = classSymbol.BaseType;

    if (baseType != null)
    {
      var baseClassSyntax = root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .FirstOrDefault(c => c.Identifier.Text == baseType.Name);

      if (baseClassSyntax != null)
      {
        // Рекурсивно добавляем методы базового класса
        AddInheritedMethods(baseType, root, methodsList, semanticModel);

        // Добавляем методы базового класса
        AddClassMethods(baseClassSyntax, methodsList, semanticModel);
      }
    }
  }

  private static void AddClassMethods(ClassDeclarationSyntax classDeclaration, List<MethodInfo> methodsList,
    SemanticModel semanticModel)
  {
    var methodDeclarations = classDeclaration.DescendantNodes()
      .OfType<MethodDeclarationSyntax>();

    foreach (var method in methodDeclarations)
    {
      var methodInfo = new MethodInfo
      {
        Name = method.Identifier.Text,
        ReturnType = method.ReturnType.ToString(),
        Parameters = method.ParameterList.Parameters.Select(p => new ParameterInfo
        {
          Name = p.Identifier.Text,
          Type = p.Type.ToString()
        }).ToList(),
        Attributes = method.AttributeLists.SelectMany(a => a.Attributes).ToList()
      };
      Console.WriteLine(method.AttributeLists.SelectMany(a => a.Attributes).ToList().Count);
      methodsList.Add(methodInfo);
    }
  }

  private static INamedTypeSymbol GetClassSymbol(SyntaxNode root, string className, SemanticModel semanticModel)
  {
    var classDeclaration = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == className);

    return classDeclaration != null ? semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol : null;
  }

  private static List<StatementSyntax> GetMethodStatements(SyntaxNode root, string className, string methodName,
    SemanticModel semanticModel, bool includeInherited = false)
  {
    var statementsList = new List<StatementSyntax>();

    var classSymbol = GetClassSymbol(root, className, semanticModel);
    if (classSymbol != null)
    {
      var classDeclaration = root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .FirstOrDefault(c => c.Identifier.Text == className);

      if (includeInherited)
      {
        AddInheritedMethodStatements(classSymbol, methodName, root, statementsList, semanticModel);
      }

      // Добавляем стейтменты текущего класса
      AddMethodStatements(classDeclaration, methodName, statementsList);
    }

    return statementsList;
  }

  private static void AddInheritedMethodStatements(INamedTypeSymbol classSymbol, string methodName, SyntaxNode root,
    List<StatementSyntax> statementsList, SemanticModel semanticModel)
  {
    var baseType = classSymbol.BaseType;

    if (baseType != null)
    {
      var baseClassSyntax = root.DescendantNodes()
        .OfType<ClassDeclarationSyntax>()
        .FirstOrDefault(c => c.Identifier.Text == baseType.Name);

      if (baseClassSyntax != null)
      {
        // Рекурсивно добавляем стейтменты методов базового класса
        AddInheritedMethodStatements(baseType, methodName, root, statementsList, semanticModel);

        // Добавляем стейтменты базового класса, если метод найден
        AddMethodStatements(baseClassSyntax, methodName, statementsList);
      }
    }
  }

  private static void AddMethodStatements(ClassDeclarationSyntax classDeclaration, string methodName,
    List<StatementSyntax> statementsList)
  {
    var methodDeclaration = classDeclaration.DescendantNodes()
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(m => m.Identifier.Text == methodName);

    if (methodDeclaration != null && methodDeclaration.Body != null)
    {
      statementsList.AddRange(methodDeclaration.Body.Statements);
    }
  }

  private static StatementSyntax TransformStatement(StatementSyntax statement, SemanticModel model, ClassInfo classInfo,
    string methodName)
  {
    // Проверяем, является ли стейтмент объявлением переменной
    if (IsVariableDeclaration(statement))
    {
      var declaration = (LocalDeclarationStatementSyntax)statement;
      var variable = declaration.Declaration.Variables.FirstOrDefault();

      // Проверяем, объявлена ли переменная gl_PointSize
      if (variable != null && variable.Identifier.Text == "gl_PointSize")
      {
        // Преобразуем в присваивание: gl_PointSize = <value>;
        var assignment = SyntaxFactory.ExpressionStatement(
          SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxFactory.IdentifierName("gl_PointSize"), // Левое выражение
            variable.Initializer.Value // Правое выражение (значение присваивания)
          )
        );

        // Заменяем объявление переменной на присваивание в дереве
        statement = assignment;
      }
      else
      {
        // Используем семантическую модель для определения типа переменной
        var variableSymbol = model.GetDeclaredSymbol(variable) as ILocalSymbol;
        var variableType = variableSymbol?.Type;

        // Если тип найден
        if (variableType == null || variableType is IErrorTypeSymbol)
        {
          throw new Exception(
            $"unknown variable type. {variableType} {variable.Identifier.Text} = {variable.Initializer.Value}");
        }

        // Проверяем, объявлена ли переменная с использованием var
        if (declaration.Declaration.Type.IsVar)
        {
          // Создаем новый синтаксический узел с фактическим типом вместо var
          var typeSyntax = SyntaxFactory
            .ParseTypeName(variableType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            .WithTrailingTrivia(SyntaxFactory.Space); // Добавляем пробел после имени типа

          // Заменяем var на фактический тип
          var newDeclaration = declaration.WithDeclaration(
            declaration.Declaration.WithType(typeSyntax)
          );

          statement = newDeclaration;
        }
      }
    }

    // Проверяем, является ли стейтмент оператором return
    /*if (IsReturnStatement(statement))
    {
      var returnStatement = (ReturnStatementSyntax)statement;

      // Получаем выражение, которое возвращается в операторе return
      var returnExpression = returnStatement.Expression;

      // Если это вертексный шейдер и функция main
      if (classInfo.ShaderType == "vertex" && methodName.ToLower() == "main")
      {
        // Создаем присваивание: gl_Position = <выражение>;
        var assignment = SyntaxFactory.ExpressionStatement(
          SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxFactory.IdentifierName("gl_Position"), // Левое выражение
            returnExpression // Правое выражение (выражение из return)
          )
        );

        // Заменяем оператор return на присваивание
        statement = assignment;
      }
    }*/

    // Словарь для замены типов
    var typeReplacements = new Dictionary<string, string>
    {
      { "Matrix3x3", "mat3" },
      { "Matrix4x4", "mat4" },

      { "Vector4", "vec4" },
      { "Vector3", "vec3" },
      { "Vector2", "vec2" },

      { "IVector4", "ivec4" },
      { "IVector3", "ivec3" },
      { "IVector2", "ivec2" }
    };

    // Продолжаем обработку других выражений
    // Заменяем new <тип> на <заменённый тип> в правой части, используя словарь
    statement = statement.ReplaceNodes(
      statement.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
        .Where(creation => typeReplacements.ContainsKey(creation.Type.ToString())), // Проверяем, есть ли тип в словаре
      (oldNode, newNode) =>
      {
        // Получаем новый тип из словаря
        var newType = typeReplacements[oldNode.Type.ToString()];

        // Создаем вызов функции нового типа
        var newExpression = SyntaxFactory.InvocationExpression(
            SyntaxFactory.IdentifierName(newType))
          .WithArgumentList(oldNode.ArgumentList);

        return newExpression;
      });

    // Добавляем обработку в конце для замены типов в объявлении переменной
    if (statement is LocalDeclarationStatementSyntax localDeclaration)
    {
      // Проверяем, есть ли тип переменной в словаре
      if (typeReplacements.ContainsKey(localDeclaration.Declaration.Type.ToString()))
      {
        // Получаем новый тип из словаря
        var newType = typeReplacements[localDeclaration.Declaration.Type.ToString()];

        // Заменяем тип переменной на новый тип из словаря
        var typeSyntax = SyntaxFactory.IdentifierName(newType)
          .WithTrailingTrivia(SyntaxFactory.Space); // Добавляем пробел после имени типа

        // Заменяем тип в объявлении переменной
        var newDeclaration = localDeclaration.WithDeclaration(
          localDeclaration.Declaration.WithType(typeSyntax)
        );

        statement = newDeclaration;
      }
    }

    return statement;
  }

  // Метод для проверки, является ли стейтмент присваиванием
  private static bool IsAssignment(StatementSyntax statement)
  {
    // Проверяем, является ли стейтмент выражением
    if (statement is ExpressionStatementSyntax expressionStatement)
    {
      // Проверяем, является ли выражение присваиванием
      if (expressionStatement.Expression is AssignmentExpressionSyntax)
      {
        return true;
      }
    }

    return false;
  }

  // Метод для проверки, является ли стейтмент объявлением переменной с инициализацией
  private static bool IsVariableDeclaration(StatementSyntax statement)
  {
    // Проверяем, является ли стейтмент объявлением переменной
    if (statement is LocalDeclarationStatementSyntax localDeclaration)
    {
      // Проверяем, есть ли у переменной инициализатор
      var variable = localDeclaration.Declaration.Variables.FirstOrDefault();
      return variable?.Initializer != null;
    }

    return false;
  }

  public static bool IsReturnStatement(StatementSyntax statement)
  {
    // Проверяем, является ли стейтмент оператором return
    return statement is ReturnStatementSyntax;
  }

  public static bool IsExpression(StatementSyntax statement)
  {
    // Проверяем, является ли стейтмент выражением
    // Это может быть выражение присваивания или выражение, которое используется как часть выражения
    return statement is ExpressionStatementSyntax;
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
      "Vector4" => "vec4",
      "Texture_2D<RGBA<float>>" => "sampler2D",
      "Texture_2D<RGBA<byte>>" => "sampler2D",
      "Texture_Cube" => "samplerCube",
      _ => sharpType
    };
  }

  public static string GetShaderText(string resourceName)
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

  public static string GetVariableType(StatementSyntax statement, SemanticModel semanticModel)
  {
    // Проверка, является ли стейтмент объявлением переменной
    if (statement is LocalDeclarationStatementSyntax localDeclaration)
    {
      // Получаем тип переменной
      var variableDeclaration = localDeclaration.Declaration;
      var typeInfo = semanticModel.GetTypeInfo(variableDeclaration.Type);

      // Возвращаем строковое представление типа или пустую строку, если тип неизвестен
      return typeInfo.Type?.ToString() ?? string.Empty;
    }

    return string.Empty; // Если стейтмент не является объявлением переменной
  }
}