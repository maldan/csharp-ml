using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;

namespace MegaLib.Compiler.Sharp;

public class SharpAttribute
{
  public string Name => _attributeSyntax.Name.ToString();

  public List<string> PositionalArguments => _attributeSyntax.ArgumentList.Arguments
    .Where(arg => arg.NameEquals == null)
    .Select(arg => arg.Expression.ToString())
    .ToList();

  public List<(string, string)> NamedArguments => _attributeSyntax.ArgumentList.Arguments
    .Where(arg => arg.NameEquals != null)
    .Select(arg => (arg.NameEquals.Name.ToString(), arg.Expression.ToString()))
    .ToList();

  private AttributeSyntax _attributeSyntax;

  public SharpAttribute(AttributeSyntax attributeSyntax)
  {
    _attributeSyntax = attributeSyntax;
  }

  public override string ToString()
  {
    var name = $"[{Name}(";

    name += Strings.Join(PositionalArguments.ToArray(), ",");

    name += ")]";
    return name;
  }
}