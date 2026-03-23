// Simplex noise in C# (1D, 2D, 3D)
// Public domain implementation based on Stefan Gustavson's algorithm.

public static class SimplexNoise
{
    private static readonly int[] perm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,
        103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
        35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,
        168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,
        111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208,
        89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,
        186, 3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,
        // Repeat
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,
        103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
        35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,
        168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,
        111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208,
        89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,
        186, 3,64,52,217,226,250,124,123,5,202,38,147,118,126,255
    };

    private static float Grad(int hash, float x)
    {
        int h = hash & 15;
        float grad = 1f + (h & 7); // 1–8
        if ((h & 8) != 0) grad = -grad;
        return grad * x;
    }

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
    }

    private static float Grad(int hash, float x, float y, float z)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
    }

    // -------------------------
    // 1D Simplex Noise
    // -------------------------
    public static float Noise(float x)
    {
        int i0 = (int)MathF.Floor(x);
        int i1 = i0 + 1;

        float x0 = x - i0;
        float x1 = x0 - 1f;

        float t0 = 1f - x0 * x0;
        float t1 = 1f - x1 * x1;

        float n0 = 0f, n1 = 0f;

        if (t0 > 0)
        {
            t0 *= t0;
            n0 = t0 * t0 * Grad(perm[i0 & 255], x0);
        }

        if (t1 > 0)
        {
            t1 *= t1;
            n1 = t1 * t1 * Grad(perm[i1 & 255], x1);
        }

        return 0.395f * (n0 + n1); // scale to roughly [-1,1]
    }

    // -------------------------
    // 2D Simplex Noise
    // -------------------------
    public static float Noise(float x, float y)
    {
        const float F2 = 0.366025403f; // (sqrt(3)-1)/2
        const float G2 = 0.211324865f; // (3-sqrt(3))/6

        float s = (x + y) * F2;
        int i = (int)MathF.Floor(x + s);
        int j = (int)MathF.Floor(y + s);

        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;

        float x0 = x - X0;
        float y0 = y - Y0;

        int i1 = x0 > y0 ? 1 : 0;
        int j1 = x0 > y0 ? 0 : 1;

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;

        float x2 = x0 - 1f + 2f * G2;
        float y2 = y0 - 1f + 2f * G2;

        int ii = i & 255;
        int jj = j & 255;

        float n0, n1, n2;

        float t0 = 0.5f - x0 * x0 - y0 * y0;
        n0 = t0 < 0 ? 0 : (t0 *= t0) * t0 * t0 * Grad(perm[ii + perm[jj]], x0, y0);

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        n1 = t1 < 0 ? 0 : (t1 *= t1) * t1 * t1 * Grad(perm[ii + i1 + perm[jj + j1]], x1, y1);

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        n2 = t2 < 0 ? 0 : (t2 *= t2) * t2 * t2 * Grad(perm[ii + 1 + perm[jj + 1]], x2, y2);

        return 40f * (n0 + n1 + n2);
    }

    // -------------------------
    // 3D Simplex Noise
    // -------------------------
    public static float Noise(float x, float y, float z)
    {
        const float F3 = 1f / 3f;
        const float G3 = 1f / 6f;

        float s = (x + y + z) * F3;
        int i = (int)MathF.Floor(x + s);
        int j = (int)MathF.Floor(y + s);
        int k = (int)MathF.Floor(z + s);

        float t = (i + j + k) * G3;
        float X0 = i - t;
        float Y0 = j - t;
        float Z0 = k - t;

        float x0 = x - X0;
        float y0 = y - Y0;
        float z0 = z - Z0;

        int i1, j1, k1;
        int i2, j2, k2;

        if (x0 >= y0)
        {
            if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
            else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
        }
        else
        {
            if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
            else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
            else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
        }

        float x1 = x0 - i1 + G3;
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;

        float x2 = x0 - i2 + 2f * G3;
        float y2 = y0 - j2 + 2f * G3;
        float z2 = z0 - k2 + 2f * G3;

        float x3 = x0 - 1f + 3f * G3;
        float y3 = y0 - 1f + 3f * G3;
        float z3 = z0 - 1f + 3f * G3;

        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;

        float n0, n1, n2, n3;

        float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
        n0 = t0 < 0 ? 0 : (t0 *= t0) * t0 * t0 * Grad(perm[ii + perm[jj + perm[kk]]], x0, y0, z0);

        float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
        n1 = t1 < 0 ? 0 : (t1 *= t1) * t1 * t1 * Grad(perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]], x1, y1, z1);

        float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
        n2 = t2 < 0 ? 0 : (t2 *= t2) * t2 * t2 * Grad(perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]], x2, y2, z2);

        float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
        n3 = t3 < 0 ? 0 : (t3 *= t3) * t3 * t3 * Grad(perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]], x3, y3, z3);

        return 32f * (n0 + n1 + n2 + n3);
    }
}