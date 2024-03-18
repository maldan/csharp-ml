using System.Text;

namespace MegaLib.Server
{
  public class RestHandler
  {
    public virtual void Handle(RestHandlerArgs args)
    {
      var responseString = "<html><body><h1>404 not found</h1></body></html>";
      var buffer = Encoding.UTF8.GetBytes(responseString);

      args.Response.ContentType = "text/html";
      args.Response.ContentLength64 = buffer.Length;
      args.Response.StatusCode = 404;

      var output = args.Response.OutputStream;
      output.Write(buffer, 0, buffer.Length);
      output.Close();
    }
  }
}