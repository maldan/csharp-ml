using System.Security.Cryptography;
using System.Text;

namespace MegaLib.Crypto
{
  public static class UID
  {
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";

    public static string Generate(int length = 8)
    {
      using var rng = new RNGCryptoServiceProvider();
      var chars = Alphabet.ToCharArray();
      var data = new byte[length];
      var result = new StringBuilder(length);
      
      rng.GetBytes(data);
      foreach (var b in data) result.Append(chars[b % (chars.Length)]);
      return result.ToString();
    }
  }
}