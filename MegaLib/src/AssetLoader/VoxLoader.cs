namespace MegaLib.AssetLoader;

using System;
using System.Collections.Generic;
using System.IO;

public class VoxLoader
{
    public class Voxel
    {
        public byte X { get; set; } 
        public byte Y { get; set; }
        public byte Z { get; set; }
        public byte ColorIndex { get; set; }
    }

    public struct Size
    {
        public int X;  
        public int Y;
        public int Z;
    }

    public Size ModelSize { get; private set; }
    public List<Voxel> Voxels { get; private set; } = new();
    public uint[] Palette { get; private set; } = new uint[256];

    public void Parse(string filePath)
    {
        using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
        {
            // Validate the header
            string magic = new string(reader.ReadChars(4));
            if (magic != "VOX ")
                throw new Exception("Invalid VOX file.");

            int version = reader.ReadInt32();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();
                int childrenSize = reader.ReadInt32();

                switch (chunkId)
                {
                    case "MAIN":
                        // Skip main chunk data (chunkSize bytes)
                        reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                        break;

                    case "SIZE":
                        ModelSize = new Size
                        {
                            X = reader.ReadInt32(),
                            Y = reader.ReadInt32(),
                            Z = reader.ReadInt32()
                        };
                        break;

                    case "XYZI":
                        int numVoxels = reader.ReadInt32();
                        for (int i = 0; i < numVoxels; i++)
                        {
                            Voxels.Add(new Voxel
                            {
                                X = reader.ReadByte(),
                                Y = reader.ReadByte(),
                                Z = reader.ReadByte(),
                                ColorIndex = reader.ReadByte()
                            });
                        }
                        break;

                    case "RGBA":
                        for (int i = 0; i < 256; i++)
                        {
                            byte r = reader.ReadByte();
                            byte g = reader.ReadByte();
                            byte b = reader.ReadByte();
                            byte a = reader.ReadByte();
                            Palette[i] = (uint)((r << 24) | (g << 16) | (b << 8) | a);
                        }
                        break;

                    default:
                        // Skip unknown chunks
                        reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                        break;
                }
            }
        }
    }
    
    public uint GetVoxelColor(Voxel voxel)
    {
        // Ensure ColorIndex is valid (1-based indexing)
        if (voxel.ColorIndex == 0 || voxel.ColorIndex > Palette.Length)
            throw new Exception($"Invalid ColorIndex: {voxel.ColorIndex}");

        // Subtract 1 to convert 1-based ColorIndex to 0-based array index
        return Palette[voxel.ColorIndex - 1];
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Model Size: {ModelSize.X}x{ModelSize.Y}x{ModelSize.Z}");
        Console.WriteLine($"Number of Voxels: {Voxels.Count}");
        //Console.WriteLine(Palette[45].ToString("X8"));
        
        for (int i = 0; i < Palette.Length; i++)
        {
            Console.WriteLine($"{i}: {Palette[i]:X8}");
        }
    }
}