using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MegaLib.Runtime;

public class RuntimeExpression
{
  private readonly Func<Dictionary<string, object>, object> _compiledExpression;

  private RuntimeExpression(Func<Dictionary<string, object>, object> compiledExpression)
  {
    _compiledExpression = compiledExpression;
  }

  public static RuntimeExpression Compile(string expression, Dictionary<string, object> parameters)
  {
    // Для простоты вычислений, все типы приводим во float32, кроме bool
    foreach (var (key, value) in parameters)
    {
      if (value is bool) continue;
      parameters[key] = Convert.ToSingle(value);
    }

    var tokens = Tokenize(expression);
    var rpnTokens = InfixToPostfix(tokens);
    var param = Expression.Parameter(typeof(Dictionary<string, object>), "parameters");
    var body = BuildExpression(rpnTokens, param, parameters);
    var lambda =
      Expression.Lambda<Func<Dictionary<string, object>, object>>(Expression.Convert(body, typeof(object)), param);
    return new RuntimeExpression(lambda.Compile());
  }

  private static Expression BuildExpression(List<string> tokens, ParameterExpression param,
    Dictionary<string, object> parameters)
  {
    var stack = new Stack<Expression>();

    foreach (var token in tokens)
      if (parameters.ContainsKey(token))
      {
        var value = parameters[token];
        var accessExpr = Expression.Property(param, "Item", Expression.Constant(token));
        var convertExpr = Expression.Convert(accessExpr, value.GetType());
        stack.Push(convertExpr);
      }
      else if (bool.TryParse(token, out var boolValue))
      {
        stack.Push(Expression.Constant(boolValue));
      }
      else if (float.TryParse(token, out var floatValue))
      {
        stack.Push(Expression.Constant(floatValue));
      }
      else if (token == "&&" || token == "||" || token == "==" || token == "!=" || token == ">" || token == ">=" ||
               token == "<" || token == "<=" ||
               token == "+" || token == "-" || token == "*" || token == "/")
      {
        if (stack.Count < 2)
          throw new InvalidOperationException("Неправильное выражение.");

        var right = stack.Pop();
        var left = stack.Pop();

        switch (token)
        {
          case "&&":
            stack.Push(Expression.AndAlso(left, right));
            break;
          case "||":
            stack.Push(Expression.OrElse(left, right));
            break;
          case "==":
            stack.Push(Expression.Equal(left, right));
            break;
          case "!=":
            stack.Push(Expression.NotEqual(left, right));
            break;
          case ">":
            stack.Push(Expression.GreaterThan(left, right));
            break;
          case ">=":
            stack.Push(Expression.GreaterThanOrEqual(left, right));
            break;
          case "<":
            stack.Push(Expression.LessThan(left, right));
            break;
          case "<=":
            stack.Push(Expression.LessThanOrEqual(left, right));
            break;
          case "+":
            stack.Push(Expression.Add(left, right));
            break;
          case "-":
            stack.Push(Expression.Subtract(left, right));
            break;
          case "*":
            stack.Push(Expression.Multiply(left, right));
            break;
          case "/":
            stack.Push(Expression.Divide(left, right));
            break;
        }
      }
      else
      {
        throw new InvalidOperationException("Неизвестный токен: " + token);
      }

    if (stack.Count != 1)
      throw new InvalidOperationException("Неправильное выражение.");

    return stack.Pop();
  }

  private static List<string> Tokenize(string expression)
  {
    var tokens = new List<string>();
    var token = "";
    for (var i = 0; i < expression.Length; i++)
      if (char.IsWhiteSpace(expression[i]))
      {
        if (token.Length > 0)
        {
          tokens.Add(token);
          token = "";
        }
      }
      else if ("&|=!<>+-*/()".IndexOf(expression[i]) >= 0)
      {
        if (token.Length > 0)
        {
          tokens.Add(token);
          token = "";
        }

        if (i + 1 < expression.Length &&
            (expression[i] == '&' || expression[i] == '|' || expression[i] == '=' || expression[i] == '!' ||
             expression[i] == '<' || expression[i] == '>') && expression[i + 1] == expression[i])
        {
          tokens.Add(new string(expression[i], 2));
          i++;
        }
        else if ((expression[i] == '=' || expression[i] == '!' || expression[i] == '<' || expression[i] == '>') &&
                 i + 1 < expression.Length && expression[i + 1] == '=')
        {
          tokens.Add(expression[i].ToString() + "=");
          i++;
        }
        else
        {
          tokens.Add(expression[i].ToString());
        }
      }
      else
      {
        token += expression[i];
      }

    if (token.Length > 0) tokens.Add(token);
    return tokens;
  }

  private static List<string> InfixToPostfix(List<string> tokens)
  {
    var precedence = new Dictionary<string, int>
    {
      { "||", 1 },
      { "&&", 2 },
      { "==", 3 },
      { "!=", 3 },
      { ">", 4 },
      { ">=", 4 },
      { "<", 4 },
      { "<=", 4 },
      { "+", 5 },
      { "-", 5 },
      { "*", 6 },
      { "/", 6 }
    };

    var output = new List<string>();
    var operators = new Stack<string>();

    foreach (var token in tokens)
      if (precedence.ContainsKey(token))
      {
        while (operators.Count > 0 && precedence.ContainsKey(operators.Peek()) &&
               precedence[operators.Peek()] >= precedence[token]) output.Add(operators.Pop());
        operators.Push(token);
      }
      else if (token == "(")
      {
        operators.Push(token);
      }
      else if (token == ")")
      {
        while (operators.Count > 0 && operators.Peek() != "(") output.Add(operators.Pop());
        if (operators.Count == 0)
          throw new InvalidOperationException("Неправильное выражение.");
        operators.Pop();
      }
      else
      {
        output.Add(token);
      }

    while (operators.Count > 0)
    {
      var op = operators.Pop();
      if (op == "(" || op == ")")
        throw new InvalidOperationException("Неправильное выражение.");
      output.Add(op);
    }

    return output;
  }

  public object Invoke(Dictionary<string, object> parameters)
  {
    // Для простоты вычислений, все типы приводим во float32, кроме bool
    foreach (var (key, value) in parameters)
    {
      if (value is bool) continue;
      parameters[key] = Convert.ToSingle(value);
    }

    var v = _compiledExpression(parameters);
    return v is bool ? v : Convert.ToSingle(v);
  }
}