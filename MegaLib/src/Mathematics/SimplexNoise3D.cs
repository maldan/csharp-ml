namespace MegaLib.Mathematics;

using System;

using System;

public class SimplexNoise3D
{
    // Hash function
    private static int Hash(int x, int y, int z)
    {
        int n = x + y * 57 + z * 131;
        n = (n << 13) ^ n;
        return (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
    }

    // Gradient dot product
    private static float Dot(int[] g, float x, float y, float z)
    {
        return g[0] * x + g[1] * y + g[2] * z;
    }

    // Generate a gradient vector from hash
    private static int[] Gradient(int hash)
    {
        switch (hash & 0xF)
        {
            case 0x0: return new int[] { 1, 1, 0 };
            case 0x1: return new int[] { -1, 1, 0 };
            case 0x2: return new int[] { 1, -1, 0 };
            case 0x3: return new int[] { -1, -1, 0 };
            case 0x4: return new int[] { 1, 0, 1 };
            case 0x5: return new int[] { -1, 0, 1 };
            case 0x6: return new int[] { 1, 0, -1 };
            case 0x7: return new int[] { -1, 0, -1 };
            case 0x8: return new int[] { 0, 1, 1 };
            case 0x9: return new int[] { 0, -1, 1 };
            case 0xA: return new int[] { 0, 1, -1 };
            case 0xB: return new int[] { 0, -1, -1 };
            case 0xC: return new int[] { 1, 1, 0 };
            case 0xD: return new int[] { -1, 1, 0 };
            case 0xE: return new int[] { 0, -1, 1 };
            case 0xF: return new int[] { 0, -1, -1 };
            default: return new int[] { 0, 0, 0 };
        }
    }

    // Fade function for smooth interpolation
    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    // Linear interpolation
    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    // Main noise function
    public static float Noise(float x, float y, float z)
    {
        // Floor to find cell coordinates
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;
        int Z = (int)Math.Floor(z) & 255;

        // Relative position within the cell
        float xf = x - (float)Math.Floor(x);
        float yf = y - (float)Math.Floor(y);
        float zf = z - (float)Math.Floor(z);

        // Fade values for interpolation
        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        // Hash gradients
        int[] grad000 = Gradient(Hash(X, Y, Z));
        int[] grad001 = Gradient(Hash(X, Y, Z + 1));
        int[] grad010 = Gradient(Hash(X, Y + 1, Z));
        int[] grad011 = Gradient(Hash(X, Y + 1, Z + 1));
        int[] grad100 = Gradient(Hash(X + 1, Y, Z));
        int[] grad101 = Gradient(Hash(X + 1, Y, Z + 1));
        int[] grad110 = Gradient(Hash(X + 1, Y + 1, Z));
        int[] grad111 = Gradient(Hash(X + 1, Y + 1, Z + 1));

        // Dot products with relative positions
        float x1 = Lerp(Dot(grad000, xf, yf, zf), Dot(grad100, xf - 1, yf, zf), u);
        float x2 = Lerp(Dot(grad010, xf, yf - 1, zf), Dot(grad110, xf - 1, yf - 1, zf), u);
        float y1 = Lerp(x1, x2, v);

        float x3 = Lerp(Dot(grad001, xf, yf, zf - 1), Dot(grad101, xf - 1, yf, zf - 1), u);
        float x4 = Lerp(Dot(grad011, xf, yf - 1, zf - 1), Dot(grad111, xf - 1, yf - 1, zf - 1), u);
        float y2 = Lerp(x3, x4, v);

        // Final interpolation along z-axis
        return Lerp(y1, y2, w); 
    }
}


public class SimpleNoise
{
    private int[] permutation;

    public SimpleNoise(int seed)
    {
        var rand = new System.Random(seed);
        permutation = new int[256];
        for (int i = 0; i < 256; i++)
        {
            permutation[i] = rand.Next(256);
        }
    }

    public float Noise(float x, float y, float z)
    {
        // Get unit cube corners
        int x0 = (int)Math.Floor(x) & 255;
        int y0 = (int)Math.Floor(y) & 255;
        int z0 = (int)Math.Floor(z) & 255;

        int x1 = (x0 + 1) & 255;
        int y1 = (y0 + 1) & 255;
        int z1 = (z0 + 1) & 255;

        // Compute local positions
        float fx = x - (int)Math.Floor(x);
        float fy = y - (int)Math.Floor(y);
        float fz = z - (int)Math.Floor(z);

        // Smoothstep fade
        float u = Fade(fx);
        float v = Fade(fy);
        float w = Fade(fz);

        // Get noise values for cube corners
        float n000 = RandomValue(x0, y0, z0);
        float n001 = RandomValue(x0, y0, z1);
        float n010 = RandomValue(x0, y1, z0);
        float n011 = RandomValue(x0, y1, z1);
        float n100 = RandomValue(x1, y0, z0);
        float n101 = RandomValue(x1, y0, z1);
        float n110 = RandomValue(x1, y1, z0);
        float n111 = RandomValue(x1, y1, z1);

        // Interpolate
        float nx00 = Lerp(n000, n100, u);
        float nx01 = Lerp(n001, n101, u);
        float nx10 = Lerp(n010, n110, u);
        float nx11 = Lerp(n011, n111, u);

        float nxy0 = Lerp(nx00, nx10, v);
        float nxy1 = Lerp(nx01, nx11, v);

        return Lerp(nxy0, nxy1, w);
    }

    private float Fade(float t)
    {
        // Smoothstep fade function
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    private float RandomValue(int x, int y, int z)
    {
        // Hash function based on permutation
        int hash = permutation[(x + permutation[(y + permutation[z]) & 255]) & 255];
        return hash / 255.0f;
    }
}
