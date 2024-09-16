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

  public int AA => _methodDeclaration.AttributeLists.SelectMany(a => a.Attributes).ToList().Count;

  public List<SharpAttribute> AttributeList =>
    _methodDeclaration.AttributeLists.SelectMany(a => a.Attributes).Select(x => new SharpAttribute(x)).ToList();
}