using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpField
{
  public string Name => _fieldDeclaration.Declaration.Variables[0].Identifier.Text;
  public string Type => _fieldDeclaration.Declaration.Type.ToString();

  public List<SharpAttribute> AttributeList =>
    (from attributeList in _fieldDeclaration.AttributeLists
      from attribute in attributeList.Attributes
      select new SharpAttribute(attribute)).ToList();

  private FieldDeclarationSyntax _fieldDeclaration;

  public SharpField(FieldDeclarationSyntax fieldDeclaration)
  {
    _fieldDeclaration = fieldDeclaration;
  }

  public bool HasAttribute(string name)
  {
    return AttributeList.Any(x => x.Name == name);
  }

  public SharpAttribute GetAttribute(string name)
  {
    return AttributeList.FirstOrDefault(x => x.Name == name);
  }
}