using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MegaLib.Server.Handler
{
  public class RestHandler_Api : RestHandler
  {
    public readonly Dictionary<string, dynamic> Controller = new();

    private static string Title(string str)
    {
      if (string.IsNullOrEmpty(str))
        return str;

      var firstChar = char.ToUpper(str[0]);
      return firstChar + str[1..];
    }

    public override void Handle(RestHandlerArgs args)
    {
      // Get controller name
      var path = args.Request.Url.LocalPath.Replace(args.Route, "").Split("/");
      if (path.Length <= 1) throw new Exception("controller not specified");
      var controllerName = path[1];
      Console.WriteLine($"Controller: {controllerName}");

      // Get method name
      var methodName = "";
      if (path.Length > 2) methodName = path[2];
      if (methodName == "") methodName = "Index";
      methodName = Title(args.Request.HttpMethod.ToLower()) + Title(methodName);
      Console.WriteLine($"Method: {methodName}");

      // Get real controller
      var controller = Controller[controllerName];
      var queryArgs = new Dictionary<string, string>();
      foreach (string v in args.Request.QueryString.Keys) queryArgs[v] = args.Request.QueryString.GetValues(v)?[0];

      // Parse json body
      JsonDocument jsonDocument = null;
      if (args.Request.Headers["Content-Type"] == "application/json")
      {
        using var reader = new StreamReader(args.Request.InputStream);
        jsonDocument = JsonDocument.Parse(reader.ReadToEnd());
      }

      // Find real method
      var dynamicType = controller.GetType();
      var methods = dynamicType.GetMethods();
      foreach (var method in methods)
        if (method.Name == methodName)
        {
          // Получаем параметры метода
          var parameters = method.GetParameters();
          var parameterValues = new object[parameters.Length];

          // Выводим информацию о каждом параметре
          var parameterId = 0;
          foreach (var parameter in parameters)
          {
            // Convert
            if (queryArgs.ContainsKey(parameter.Name))
            {
              if (parameter.ParameterType == typeof(string))
                parameterValues[parameterId] = queryArgs[parameter.Name];
              else if (parameter.ParameterType == typeof(int))
                parameterValues[parameterId] = int.Parse(queryArgs[parameter.Name]);
              else if (parameter.ParameterType == typeof(float))
                parameterValues[parameterId] = float.Parse(queryArgs[parameter.Name]);
              else if (parameter.ParameterType == typeof(bool))
                parameterValues[parameterId] = queryArgs[parameter.Name] == "true";
            }

            // Get from json
            if (jsonDocument != null)
            {
              if (parameter.ParameterType == typeof(string))
                parameterValues[parameterId] = jsonDocument.RootElement.GetProperty(parameter.Name).GetString();
              if (parameter.ParameterType == typeof(int))
                parameterValues[parameterId] = jsonDocument.RootElement.GetProperty(parameter.Name).GetInt32();
              if (parameter.ParameterType == typeof(float))
                parameterValues[parameterId] = jsonDocument.RootElement.GetProperty(parameter.Name).GetFloat32();
            }

            parameterId += 1;
          }

          var result = method.Invoke(controller, parameterValues);

          if (result is RestServerCustomResponse r)
            args.WriteAndClose(r.Body, r.Headers["Content-Type"] ?? "application/octet-stream", 200);
          else
          {
            if (result == null) args.WriteAndClose("", "application/json", 200);
            else
            {
              var jsonString = JsonSerializer.Serialize(result);
              args.WriteAndClose(jsonString.ToString(), "application/json", 200);
            }
            
            //else args.WriteAndClose(result.ToString(), "application/json", 200);
          }

          return;
        }

      throw new Exception($"Method '{methodName}' not found");
    }
  }
}