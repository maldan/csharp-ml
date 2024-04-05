namespace MegaLib.IO
{
  public static class Console
  {
    private static string Padding(this string input, int count)
    {
      return new string(' ', count) + input;
    }

    private static void PrintStruct(object any, int padding)
    {
      var type = any.GetType();
      Write($"struct {type.Name} {{\n".Padding(padding));

      var fields = type.GetFields();
      foreach (var field in fields)
      {
        WriteLine($"{field.FieldType} {field.Name}".Padding(padding + 2));
      }

      Write($"}}");
    }

    private static void PrintClass(object any, int padding)
    {
      var type = any.GetType();
      Write($"class {type.Name} {{\n".Padding(padding));

      var fields = type.GetFields();
      foreach (var field in fields)
      {
        WriteLine($"{field.FieldType} {field.Name}".Padding(padding + 2));
      }

      Write($"}}");
    }

    public static void PrettyPrint(object any)
    {
      if (any == null)
      {
        WriteLine("null");
        return;
      }

      var type = any.GetType();
      var isStruct = type.IsValueType && !type.IsEnum;
      var isClass = type.IsClass;

      if (isStruct) PrintStruct(any, 0);
      else if (isClass) PrintClass(any, 0);
      else WriteLine(any);
    }

    public static void Write(object value)
    {
      System.Console.Write(value);
    }

    public static void WriteLine(object value)
    {
      System.Console.WriteLine(value);
    }
  }
}