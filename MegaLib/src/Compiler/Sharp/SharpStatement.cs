using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Render.Shader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpStatement
{
  public string Text => _statementSyntax.ToString();

  public bool IsVarDeclaration => _statementSyntax is LocalDeclarationStatementSyntax;
  public bool IsIf => _statementSyntax is IfStatementSyntax;
  public bool IsFor => _statementSyntax is ForStatementSyntax;

  private StatementSyntax _statementSyntax;
  private SemanticModel _semanticModel;

  public SharpStatement(StatementSyntax statementSyntax, SemanticModel semanticModel)
  {
    _statementSyntax = statementSyntax;
    _semanticModel = semanticModel;
  }

  public string GetVariableType(StatementSyntax statementSyntax)
  {
    if (statementSyntax is not LocalDeclarationStatementSyntax localDeclaration) return string.Empty;
    var variableDeclaration = localDeclaration.Declaration;
    var typeInfo = _semanticModel.GetTypeInfo(variableDeclaration.Type);
    return typeInfo.Type?.ToString() ?? string.Empty;
  }

  public string GetTextWithExplicitVariableDeclaration()
  {
    var textList = new List<string>();
    GetTextWithExplicitVariableDeclaration(_statementSyntax, textList);
    return string.Join("\n", textList);
  }

  public void GetTextWithExplicitVariableDeclaration(StatementSyntax statementSyntax, List<string> outShader)
  {
    var newStatement = statementSyntax.ToString();

    if (statementSyntax is IfStatementSyntax ifStatement)
    {
      // Получение условия
      var condition = ifStatement.Condition.ToString();

      // Получение блока if
      var ifBlock = ifStatement.Statement as BlockSyntax;
      var ifStatements = ifBlock?.Statements.Select(s => s) ?? [];

      // Получение блока else, если есть
      var elseBlock = ifStatement.Else?.Statement as BlockSyntax;
      var elseStatements = elseBlock?.Statements.Select(s => s) ?? [];

      // Вывод
      outShader.Add($"if ({condition}) {{");
      if (ifStatement.Statement is BlockSyntax)
      {
        foreach (var stmt in ifStatements)
        {
          GetTextWithExplicitVariableDeclaration(stmt, outShader);
        }
      }
      else
      {
        GetTextWithExplicitVariableDeclaration(ifStatement.Statement, outShader);
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
            GetTextWithExplicitVariableDeclaration(stmt, outShader);
          }
        }
        else
        {
          GetTextWithExplicitVariableDeclaration(ifStatement.Else.Statement, outShader);
        }

        outShader.Add("}");
      }

      return;
    }

    if (statementSyntax is ForStatementSyntax forStatement)
    {
      var condition = forStatement.Condition?.ToString() ?? "";
      var increments = forStatement.Incrementors.Select(i => i.ToString()).ToList();
      var initializers = forStatement.Initializers.Select(i => i.ToString()).ToList();

      outShader.Add($"for ({string.Join(", ", initializers)}; {condition}; {string.Join(", ", increments)}) {{");

      // Проверяем, является ли Statement блоком или одиночным выражением
      if (forStatement.Statement is BlockSyntax block)
      {
        foreach (var stmt in block.Statements)
        {
          GetTextWithExplicitVariableDeclaration(stmt, outShader);
        }
      }
      else
      {
        // Если это не блок, то обрабатываем как одиночный Statement
        GetTextWithExplicitVariableDeclaration(forStatement.Statement, outShader);
      }

      outShader.Add("}");
      return;
    }

    if (statementSyntax is LocalDeclarationStatementSyntax)
    {
      var varType = GetVariableType(statementSyntax);
      newStatement = newStatement.Replace("var ", varType + " ");
    }

    outShader.Add(newStatement);
  }
}