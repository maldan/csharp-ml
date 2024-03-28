using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MegaLib.Server
{
  public struct RestHandlerArgs
  {
    public string Route;
    public string Path;
    public HttpListenerRequest Request;
    public HttpListenerResponse Response;

    public void WriteAndClose(string responseString, string contentType, int statusCode)
    {
      var buffer = Encoding.UTF8.GetBytes(responseString);
      WriteAndClose(buffer, contentType, statusCode);
    }

    public void WriteAndClose(byte[] buffer, string contentType, int statusCode)
    {
      Response.ContentType = contentType;
      Response.ContentLength64 = buffer.Length;
      Response.StatusCode = statusCode;

      var output = Response.OutputStream;
      output.Write(buffer, 0, buffer.Length);
      output.Close();
    }
  }

  /*public class RestHandler_Empty : RestHandler
  {
    public override void Handle(RestHandlerArgs args)
    {
      var responseString = "<html><body><h1>FUCK YOUUUU!!!</h1></body></html>";
      var buffer = Encoding.UTF8.GetBytes(responseString);

      args.Response.ContentType = "text/html";
      args.Response.ContentLength64 = buffer.Length;

      var output = args.Response.OutputStream;
      output.Write(buffer, 0, buffer.Length);
      output.Close();
    }
  }*/

  public struct RestServerError
  {
    public string Description;
    public int StatusCode;
  }

  public class RestServerCustomResponse
  {
    public Dictionary<string, string> Headers = new();
    public byte[] Body = Array.Empty<byte>();

    public RestServerCustomResponse SetBody(string str)
    {
      Body = Encoding.UTF8.GetBytes(str);
      return this;
    }
    
    public RestServerCustomResponse SetBody(byte[] body)
    {
      Body = body;
      return this;
    }

    public RestServerCustomResponse AddHeader(string name, string value)
    {
      Headers[name] = value;
      return this;
    }
  }

  public class RestServer
  {
    public int Port { get; }
    public Dictionary<string, RestHandler> Handler = new();

    public RestServer(int port)
    {
      Port = port;
    }

    public void AddHandler(string prefix, RestHandler gas)
    {
      Handler.Add(prefix, gas);
    }

    private (RestHandler, string) GetHandler(string url)
    {
      foreach (var (path, handler) in Handler)
        if (url.StartsWith(path))
          return (handler, path);
      return (new RestHandler(), "");
    }

    public async Task Run()
    {
      var listener = new HttpListener();
      listener.Prefixes.Add($"http://localhost:{Port}/");
      listener.Start();

      Console.WriteLine($"Http Server :{Port}");

      while (true)
      {
        // Get context
        var context = await listener.GetContextAsync();
        var request = context.Request;
        var response = context.Response;

        // Disable cors
        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "*");
        response.AddHeader("Access-Control-Allow-Headers", "*");

        // Fuck options
        if (request.HttpMethod == "OPTIONS")
        {
          response.StatusCode = 200;
          response.Close();
          continue;
        }

        // Get handler
        var (handler, route) = GetHandler(request.Url.LocalPath);

        // Handle
        var args = new RestHandlerArgs
        {
          Route = route,
          Request = request,
          Response = response
        };

        try
        {
          handler.Handle(args);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          var jsonString = JsonSerializer.Serialize(new
          {
            description = e.Message
          });
          args.WriteAndClose(jsonString, "application/json", 500);
        }
      }
    }
  }
}