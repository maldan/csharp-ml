namespace MegaLib.Ext;

public static class StringEx
{
  public static string Title(this string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    var chars = input.ToCharArray();
    chars[0] = char.ToUpper(chars[0]);
    return new string(chars);
  }
}