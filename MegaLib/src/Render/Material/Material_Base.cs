namespace MegaLib.Render.Material
{
  public class Material_Base
  {
    protected string _fragmentShader = "";
    protected string _vertexShader = "";
    public int Id;

    public Material_Base(string vertex, string fragment)
    {
      _vertexShader = vertex;
      _fragmentShader = fragment;
    }

    public Material_Base()
    {
    }
  }
}