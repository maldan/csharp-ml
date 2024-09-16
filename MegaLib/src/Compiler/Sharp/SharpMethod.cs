using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpMethod
{
  public string Name => _methodDeclaration.Identifier.Text;
  public string ReturnType => _methodDeclaration.ReturnType.ToString();

  private MethodDeclarationSyntax _methodDeclaration;

  public SharpMethod(MethodDeclarationSyntax methodDeclaration)
  {
    _methodDeclaration = methodDeclaration;
  }

  public List<StatementSyntax> StatementList
  {
    get
    {
      var body = _methodDeclaration.Body;
      return body != null ? body.Statements.ToList() : [];
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

  public List<SharpParameter> ParameterList
  {
    get { return _methodDeclaration.ParameterList.Parameters.Select(param => new SharpParameter(param)).ToList(); }
  }
}