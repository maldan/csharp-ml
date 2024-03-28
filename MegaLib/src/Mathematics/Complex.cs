namespace MegaLib.Mathematics
{
  public struct Complex
  {
    public float Real;
    public float Imaginary;

    // Add
    public static Complex operator +(Complex a, Complex b)
    {
      return new Complex
      {
        Real = a.Real + b.Real,
        Imaginary = a.Imaginary + b.Imaginary
      };
    }

    // Sub
    public static Complex operator -(Complex a, Complex b)
    {
      return new Complex
      {
        Real = a.Real - b.Real,
        Imaginary = a.Imaginary - b.Imaginary
      };
    }

    // Mul
    public static Complex operator *(Complex a, Complex b)
    {
      return new Complex
      {
        Real = a.Real * b.Real - a.Imaginary * b.Imaginary,
        Imaginary = a.Real * b.Imaginary + a.Imaginary * b.Real
      };
    }
  }
}