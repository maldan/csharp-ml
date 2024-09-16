using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpMethod
{
  public string Name => _methodDeclaration.Identifier.Text;
  public string ReturnType => _methodDeclaration.ReturnType.ToString();
  public string Text => _methodDeclaration.ToString();

  public string InnerText
  {
    get { return StatementList.Aggregate("", (current, statement) => current + statement.Text + "\n"); }
  }

  private MethodDeclarationSyntax _methodDeclaration;
  private SemanticModel _semanticModel;

  public SharpMethod(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
  {
    _methodDeclaration = methodDeclaration;
    _semanticModel = semanticModel;
  }

  public List<SharpStatement> StatementList
  {
    get
    {
      var body = _methodDeclaration.Body;
      return body != null ? body.Statements.Select(x => new SharpStatement(x, _semanticModel)).ToList() : [];
    }
  }

  public List<SharpAttribute> AttributeList =>
    (from attributeList in _methodDeclaration.AttributeLists
      from attribute in attributeList.Attributes
      select new SharpAttribute(attribute)).ToList();

  public bool HasAttribute(string name)
  {
    return AttributeList.Any(x => x.Name == name);
  }

  public SharpAttribute GetAttribute(string name)
  {
    return AttributeList.FirstOrDefault(x => x.Name == name);
  }

  public List<SharpParameter> ParameterList
  {
    get { return _methodDeclaration.ParameterList.Parameters.Select(param => new SharpParameter(param)).ToList(); }
  }
}