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

namespace MegaLib.Render.Shader;

[AttributeUsage(AttributeTargets.Field)] // Указываем, что атрибут можно применять только к полям
public class ShaderFieldAttribute(string description) : Attribute
{
  public string Description { get; } = description;
}

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

  // Класс для хранения информации о методе
  public struct MethodInfo
  {
    public string Name;
    public string ReturnType;
    public List<ParameterInfo> Parameters;
  }

  // Класс для хранения информации о параметрах метода
  public struct ParameterInfo
  {
    public string Name;
    public string Type;
  }

  public static Dictionary<string, string> Compile(string shaderName)
  {
    // language=C#
    var methodSource = GetShaderText($"MegaLib.src.Render.Shader.{shaderName}Shader.cs");

    // Парсим код метода с помощью Roslyn
    var tree = CSharpSyntaxTree.ParseText(methodSource);
    var root = tree.GetRoot() as CompilationUnitSyntax;
    var compilation = CSharpCompilation.Create("ShaderAnalysis")
      .AddSyntaxTrees(tree)
      .AddReferences(
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Vector3).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Vector4).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(MathF).Assembly.Location)
      );

    // Создаем семантическую модель для анализа типов
    var model = compilation.GetSemanticModel(tree);

    // Получаем список классов
    var classList = GetClasses(root);
    var shaders = new Dictionary<string, string>();
    for (var i = 0; i < classList.Count; i++)
    {
      shaders[classList[i].ShaderType] = CompileClass(root, model, classList[i]);
      Console.WriteLine(classList[i].ShaderType);
      Console.WriteLine(shaders[classList[i].ShaderType]);
      Console.WriteLine("");
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
      if (fieldList[i].Attributes.Count <= 0) throw new Exception("Need attribute");
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

    // Потом получаем все методы
    var methodList = GetMethods(root, classInfo.Name);
    for (var i = 0; i < methodList.Count; i++)
    {
      var methodName = methodList[i].Name;
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
      var statementList = GetMethodStatements(root, classInfo.Name, methodList[i].Name);
      for (var j = 0; j < statementList.Count; j++)
      {
        // Console.WriteLine(statementList[j].ToFullString());

        /*if (IsAssignment(statementList[j]))
        {
          var assignment =
            (AssignmentExpressionSyntax)((ExpressionStatementSyntax)statementList[j]).Expression;
          outShader.Add($"{assignment.Left} = {assignment.Right};");
        }
        else */
        // statementList[j] = TransformStatement(statementList[j]);


        /*if (IsVariableDeclaration(statementList[j]))
        {
          var declaration = (LocalDeclarationStatementSyntax)statementList[j];
          var variable = declaration.Declaration.Variables.First();

          // Используем семантическую модель для определения типа переменной
          var variableSymbol = model.GetDeclaredSymbol(variable) as ILocalSymbol;
          var variableType = variableSymbol?.Type;

          // Если тип найден
          if (variableType == null || variableType is IErrorTypeSymbol)
          {
            throw new Exception(
              $"unknown variable type. {variableType} {variable.Identifier.Text} = {variable.Initializer.Value}");
          }

          outShader.Add(
            $"{ReplaceTypes(variableType.Name)} {variable.Identifier.Text} = {variable.Initializer.Value};");
        }
        else if (IsReturnStatement(statementList[j]))
        {
          // Если мы в main методе у вертексного шейдера
          if (methodList[i].Name.ToLower() == "main" && classInfo.ShaderType == "vertex")
          {
            outShader.Add(statementList[j].ToString().Replace("return ", "gl_Position = "));
          }
          else
          {
            outShader.Add(statementList[j].ToString());
          }
        }
        else
        {

        }*/

        // 
        var newStatement = TransformStatement(statementList[j], model, classInfo, methodName).ToString();

        if (classInfo.ShaderType == "vertex" && methodName.ToLower() == "main" && newStatement.StartsWith("return"))
        {
          newStatement = newStatement.Replace("return ", "gl_Position = ");
        }

        // Если это векторные поля, то большие буквы XYZW меняем на маленькие xyzw
        for (var k = 0; k < fieldList.Count; k++)
        {
          if (fieldList[k].Type.Contains("Vector"))
          {
            var pattern = $@"\b({fieldList[k].Name})\.([XYZWRGBA])";
            newStatement = Regex.Replace(newStatement, pattern,
              m => $"{m.Groups[1].Value}.{m.Groups[2].Value.ToLower()}");
          }
        }

        // Прочие замены глобальных методов
        newStatement = newStatement.Replace("Matrix3x3.Transpose", "transpose");
        newStatement = newStatement.Replace("Matrix3x3.Inverse", "inverse");
        newStatement = newStatement.Replace("Vector3.Normalize", "normalize");
        newStatement = newStatement.Replace("Matrix4x4.Inverse", "inverse");
        newStatement = newStatement.Replace(".DropW()", ".xyz");

        outShader.Add(newStatement);
      }

      outShader.Add("}");
    }

    return string.Join("\n", outShader);
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

  // Функция для поиска всех полей класса и возвращения их в виде списка
  private static List<FieldInfo> GetFields(SyntaxNode root, string className)
  {
    var fieldsList = new List<FieldInfo>();

    // Находим объявление класса по имени
    var classDeclaration = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == className);

    if (classDeclaration != null)
    {
      // Извлекаем все поля в классе
      var fieldDeclarations = classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>();

      foreach (var field in fieldDeclarations)
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

  // Функция для поиска всех методов класса и возвращения их в виде списка
  private static List<MethodInfo> GetMethods(SyntaxNode root, string className)
  {
    var methodsList = new List<MethodInfo>();

    // Находим объявление класса по имени
    var classDeclaration = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == className);

    if (classDeclaration != null)
    {
      // Извлекаем все методы в классе
      var methodDeclarations =
        classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();

      foreach (var method in methodDeclarations)
      {
        // Имя метода
        var methodName = method.Identifier.Text;

        // Возвращаемый тип метода 
        var returnType = method.ReturnType.ToString();

        // Создаем объект для хранения информации о методе
        var methodInfo = new MethodInfo
        {
          Name = methodName,
          ReturnType = returnType,
          Parameters = []
        };

        // Извлекаем параметры метода
        foreach (var parameter in method.ParameterList.Parameters)
        {
          methodInfo.Parameters.Add(new ParameterInfo
          {
            Name = parameter.Identifier.Text,
            Type = parameter.Type.ToString()
          });
        }

        // Добавляем информацию о методе в список
        methodsList.Add(methodInfo);
      }
    }

    return methodsList;
  }

  // Метод для получения списка стейтментов заданной функции
  private static List<StatementSyntax> GetMethodStatements(SyntaxNode root, string className, string methodName)
  {
    // Находим объявление класса по его имени
    var classDeclaration = root.DescendantNodes()
      .OfType<ClassDeclarationSyntax>()
      .FirstOrDefault(c => c.Identifier.Text == className);

    if (classDeclaration != null)
    {
      // Находим метод внутри класса по его имени
      var methodDeclaration = classDeclaration.DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .FirstOrDefault(m => m.Identifier.Text == methodName);

      if (methodDeclaration != null && methodDeclaration.Body != null)
      {
        // Возвращаем список стейтментов из тела метода
        return methodDeclaration.Body.Statements.ToList();
      }
    }

    // Если метод не найден или тело метода пустое, возвращаем пустой список
    return [];
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
      { "Vector2", "vec2" }
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

  private static string ReplaceTypes(string sharpType)
  {
    return sharpType switch
    {
      "Matrix3x3" => "mat3",
      "Matrix4x4" => "mat4",
      "Vector2" => "vec2",
      "Vector3" => "vec3",
      "Vector4" => "vec4",
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
}