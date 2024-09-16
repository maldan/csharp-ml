using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MegaLib.Compiler.Sharp;

public class SharpParameter
{
  public string Name => _parameterSyntax.Identifier.Text;
  public string Type => _parameterSyntax.Type.ToString();

  private ParameterSyntax _parameterSyntax;

  public SharpParameter(ParameterSyntax parameterSyntax)
  {
    _parameterSyntax = parameterSyntax;
  }
}