using System.IO;

namespace MegaLib.FS;

public interface IBinarySerializable
{
  //public byte[] ToBytes();
  //public void FromBytes(byte[] bytes);

  public void ToWriter(BinaryWriter writer);
  public void FromReader(BinaryReader reader);
}