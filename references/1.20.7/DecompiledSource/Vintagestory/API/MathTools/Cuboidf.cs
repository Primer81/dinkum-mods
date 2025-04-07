#region Assembly VintagestoryAPI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\gwlar\AppData\Roaming\Vintagestory\VintagestoryAPI.dll
// Decompiled with ICSharpCode.Decompiler 8.2.0.7535
#endregion

using System;

namespace Vintagestory.API.MathTools;

//
// Summary:
//     Represents a three dimensional axis-aligned cuboid using two 3D coordinates.
//     Used for collision and selection boxes.
[DocumentAsJson]
public class Cuboidf : ICuboid<float, Cuboidf>, IEquatable<Cuboidf>
{
    //
    // Summary:
    //     Start X Pos
    [DocumentAsJson]
    public float X1;

    //
    // Summary:
    //     Start Y Pos
    [DocumentAsJson]
    public float Y1;

    //
    // Summary:
    //     Start Z Pos
    [DocumentAsJson]
    public float Z1;

    //
    // Summary:
    //     End X Pos
    [DocumentAsJson]
    public float X2;

    //
    // Summary:
    //     End Y Pos
    [DocumentAsJson]
    public float Y2;

    //
    // Summary:
    //     End Z Pos
    [DocumentAsJson]
    public float Z2;

    //
    // Summary:
    //     This is equivalent to width so long as X2 > X1, but could in theory be a negative
    //     number if the box has its corners the wrong way around
    public float XSize => X2 - X1;

    //
    // Summary:
    //     This is equivalent to height so long as Y2 > Y1, but could in theory be a negative
    //     number if the box has its corners the wrong way around
    public float YSize => Y2 - Y1;

    //
    // Summary:
    //     This is equivalent to length so long as Z2 > Z1, but could in theory be a negative
    //     number if the box has its corners the wrong way around
    public float ZSize => Z2 - Z1;

    public float Width => MaxX - MinX;

    public float Height => MaxY - MinY;

    public float Length => MaxZ - MinZ;

    public float MinX => Math.Min(X1, X2);

    public float MinY => Math.Min(Y1, Y2);

    public float MinZ => Math.Min(Z1, Z2);

    public float MaxX => Math.Max(X1, X2);

    public float MaxY => Math.Max(Y1, Y2);

    public float MaxZ => Math.Max(Z1, Z2);

    public float MidX => (X1 + X2) / 2f;

    public float MidY => (Y1 + Y2) / 2f;

    public float MidZ => (Z1 + Z2) / 2f;

    public float this[int index]
    {
        get
        {
            return index switch
            {
                0 => X1,
                1 => Y1,
                2 => Z1,
                3 => X2,
                4 => Y2,
                5 => Z2,
                _ => throw new ArgumentException("Out of bounds"),
            };
        }
        set
        {
            switch (index)
            {
                case 0:
                    X1 = value;
                    break;
                case 1:
                    Y1 = value;
                    break;
                case 2:
                    Z1 = value;
                    break;
                case 3:
                    X2 = value;
                    break;
                case 4:
                    Y2 = value;
                    break;
                case 5:
                    Z2 = value;
                    break;
                default:
                    throw new ArgumentException("Out of bounds");
            }
        }
    }

    //
    // Summary:
    //     True when all values are 0
    public bool Empty
    {
        get
        {
            if (X1 == 0f && Y1 == 0f && Z1 == 0f && X2 == 0f && Y2 == 0f)
            {
                return Z2 == 0f;
            }

            return false;
        }
    }

    public Vec3f Start => new Vec3f(MinX, MinY, MinZ);

    public Vec3f End => new Vec3f(MaxX, MaxY, MaxZ);

    public Vec3d Startd => new Vec3d(MinX, MinY, MinZ);

    public Vec3d Endd => new Vec3d(MaxX, MaxY, MaxZ);

    public Vec3d Center => new Vec3d(MidX, MidY, MidZ);

    public Cuboidf()
    {
    }

    public Cuboidf(float size)
    {
        X1 = (0f - size) / 2f;
        Y1 = (0f - size) / 2f;
        Z1 = (0f - size) / 2f;
        X2 = size / 2f;
        Y2 = size / 2f;
        Z2 = size / 2f;
    }

    public Cuboidf(double x1, double x2, double y1, double y2, double z1, double z2)
    {
        X1 = (float)x1;
        X2 = (float)x2;
        Y1 = (float)y1;
        Y2 = (float)y2;
        Z1 = (float)z1;
        Z2 = (float)z2;
    }

    public Cuboidf(Vec3f start, Vec3f end)
    {
        X1 = start.X;
        Y1 = start.Y;
        Z1 = start.Z;
        X2 = end.X;
        Y2 = end.Y;
        Z2 = end.Z;
    }

    public Cuboidf(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        Set(x1, y1, z1, x2, y2, z2);
    }

    //
    // Summary:
    //     Sets the minimum and maximum values of the cuboid
    public Cuboidf Set(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        X1 = x1;
        Y1 = y1;
        Z1 = z1;
        X2 = x2;
        Y2 = y2;
        Z2 = z2;
        return this;
    }

    //
    // Summary:
    //     Sets the minimum and maximum values of the cuboid
    public Cuboidf Set(IVec3 min, IVec3 max)
    {
        Set(min.XAsFloat, min.YAsFloat, min.ZAsFloat, max.XAsFloat, max.YAsFloat, max.ZAsFloat);
        return this;
    }

    public void Set(Cuboidf collisionBox)
    {
        X1 = collisionBox.X1;
        Y1 = collisionBox.Y1;
        Z1 = collisionBox.Z1;
        X2 = collisionBox.X2;
        Y2 = collisionBox.Y2;
        Z2 = collisionBox.Z2;
    }

    //
    // Summary:
    //     Adds the given offset to the cuboid
    public Cuboidf Translate(float posX, float posY, float posZ)
    {
        X1 += posX;
        Y1 += posY;
        Z1 += posZ;
        X2 += posX;
        Y2 += posY;
        Z2 += posZ;
        return this;
    }

    //
    // Summary:
    //     Adds the given offset to the cuboid
    public Cuboidf Translate(IVec3 vec)
    {
        Translate(vec.XAsFloat, vec.YAsFloat, vec.ZAsFloat);
        return this;
    }

    //
    // Summary:
    //     Substractes the given offset to the cuboid
    public Cuboidf Sub(float posX, float posY, float posZ)
    {
        X1 -= posX;
        Y1 -= posY;
        Z1 -= posZ;
        X2 -= posX;
        Y2 -= posY;
        Z2 -= posZ;
        return this;
    }

    //
    // Summary:
    //     Substractes the given offset to the cuboid
    public Cuboidf Sub(IVec3 vec)
    {
        Sub(vec.XAsFloat, vec.YAsFloat, vec.ZAsFloat);
        return this;
    }

    //
    // Summary:
    //     Returns if the given point is inside the cuboid
    public bool Contains(double x, double y, double z)
    {
        if (x >= (double)X1 && x <= (double)X2 && y >= (double)Y1 && y <= (double)Y2 && z >= (double)Z1)
        {
            return z <= (double)Z2;
        }

        return false;
    }

    //
    // Summary:
    //     Returns if the given point is inside the cuboid
    public bool ContainsOrTouches(float x, float y, float z)
    {
        if (x >= X1 && x <= X2 && y >= Y1 && y <= Y2 && z >= Z1)
        {
            return z <= Z2;
        }

        return false;
    }

    //
    // Summary:
    //     Returns if the given point is inside the cuboid
    public bool ContainsOrTouches(IVec3 vec)
    {
        return ContainsOrTouches(vec.XAsFloat, vec.YAsFloat, vec.ZAsFloat);
    }

    public Cuboidf OmniNotDownGrowBy(float size)
    {
        X1 -= size;
        Z1 -= size;
        X2 += size;
        Y2 += size;
        Z2 += size;
        return this;
    }

    public Cuboidf OmniGrowBy(float size)
    {
        X1 -= size;
        Y1 -= size;
        Z1 -= size;
        X2 += size;
        Y2 += size;
        Z2 += size;
        return this;
    }

    public Cuboidf ShrinkBy(float size)
    {
        X1 += size;
        Y1 += size;
        Z1 += size;
        X2 -= size;
        Y2 -= size;
        Z2 -= size;
        return this;
    }

    //
    // Summary:
    //     Grows the cuboid so that it includes the given block
    public Cuboidf GrowToInclude(int x, int y, int z)
    {
        X1 = Math.Min(X1, x);
        Y1 = Math.Min(Y1, y);
        Z1 = Math.Min(Z1, z);
        X2 = Math.Max(X2, x + 1);
        Y2 = Math.Max(Y2, y + 1);
        Z2 = Math.Max(Z2, z + 1);
        return this;
    }

    public Cuboidf ClampTo(Vec3f min, Vec3f max)
    {
        X1 = Math.Max(min.X, X1);
        Y1 = Math.Max(min.Y, Y1);
        Z1 = Math.Max(min.Z, Z1);
        X2 = Math.Min(max.X, X2);
        Y2 = Math.Min(max.Y, Y2);
        Z2 = Math.Min(max.Z, Z2);
        return this;
    }

    //
    // Summary:
    //     Grows the cuboid so that it includes the given block
    public Cuboidf GrowToInclude(IVec3 vec)
    {
        GrowToInclude(vec.XAsInt, vec.XAsInt, vec.ZAsInt);
        return this;
    }

    //
    // Summary:
    //     Returns the shortest distance between given point and any point inside the cuboid
    public double ShortestDistanceFrom(float x, float y, float z)
    {
        double num = GameMath.Clamp(x, X1, X2);
        double num2 = GameMath.Clamp(y, Y1, Y2);
        double num3 = GameMath.Clamp(z, Z1, Z2);
        return Math.Sqrt(((double)x - num) * ((double)x - num) + ((double)y - num2) * ((double)y - num2) + ((double)z - num3) * ((double)z - num3));
    }

    //
    // Summary:
    //     Returns the shortest distance between given point and any point inside the cuboid
    public double ShortestDistanceFrom(IVec3 vec)
    {
        return ShortestDistanceFrom(vec.XAsFloat, vec.YAsFloat, vec.ZAsFloat);
    }

    //
    // Summary:
    //     Returns a new x coordinate that's ensured to be outside this cuboid. Used for
    //     collision detection.
    public double pushOutX(Cuboidf from, float x, ref EnumPushDirection direction)
    {
        direction = EnumPushDirection.None;
        if (from.Y2 > Y1 && from.Y1 < Y2 && from.Z2 > Z1 && from.Z1 < Z2)
        {
            if ((double)x > 0.0 && from.X2 <= X1 && X1 - from.X2 < x)
            {
                direction = EnumPushDirection.Positive;
                x = X1 - from.X2;
            }
            else if ((double)x < 0.0 && from.X1 >= X2 && X2 - from.X1 > x)
            {
                direction = EnumPushDirection.Negative;
                x = X2 - from.X1;
            }
        }

        return x;
    }

    //
    // Summary:
    //     Returns a new y coordinate that's ensured to be outside this cuboid. Used for
    //     collision detection.
    public double pushOutY(Cuboidf from, float y, ref EnumPushDirection direction)
    {
        direction = EnumPushDirection.None;
        if (from.X2 > X1 && from.X1 < X2 && from.Z2 > Z1 && from.Z1 < Z2)
        {
            if ((double)y > 0.0 && from.Y2 <= Y1 && Y1 - from.Y2 < y)
            {
                direction = EnumPushDirection.Positive;
                y = Y1 - from.Y2;
            }
            else if ((double)y < 0.0 && from.Y1 >= Y2 && Y2 - from.Y1 > y)
            {
                direction = EnumPushDirection.Negative;
                y = Y2 - from.Y1;
            }
        }

        return y;
    }

    //
    // Summary:
    //     Returns a new z coordinate that's ensured to be outside this cuboid. Used for
    //     collision detection.
    public double pushOutZ(Cuboidf from, float z, ref EnumPushDirection direction)
    {
        direction = EnumPushDirection.None;
        if (from.X2 > X1 && from.X1 < X2 && from.Y2 > Y1 && from.Y1 < Y2)
        {
            if ((double)z > 0.0 && from.Z2 <= Z1 && Z1 - from.Z2 < z)
            {
                direction = EnumPushDirection.Positive;
                z = Z1 - from.Z2;
            }
            else if ((double)z < 0.0 && from.Z1 >= Z2 && Z2 - from.Z1 > z)
            {
                direction = EnumPushDirection.Negative;
                z = Z2 - from.Z1;
            }
        }

        return z;
    }

    //
    // Summary:
    //     Performs a 3-dimensional rotation on the cuboid and returns a new axis-aligned
    //     cuboid resulting from this rotation. Not sure it it makes any sense to use this
    //     for other rotations than 90 degree intervals.
    public Cuboidf RotatedCopy(float degX, float degY, float degZ, Vec3d origin)
    {
        float rad = degX * (MathF.PI / 180f);
        float rad2 = degY * (MathF.PI / 180f);
        float rad3 = degZ * (MathF.PI / 180f);
        float[] array = Mat4f.Create();
        Mat4f.Translate(array, array, (float)origin.X, (float)origin.Y, (float)origin.Z);
        Mat4f.RotateX(array, array, rad);
        Mat4f.RotateY(array, array, rad2);
        Mat4f.RotateZ(array, array, rad3);
        Mat4f.Translate(array, array, 0f - (float)origin.X, 0f - (float)origin.Y, 0f - (float)origin.Z);
        return TransformedCopy(array);
    }

    //
    // Summary:
    //     Performs a 3-dimensional rotation on the cuboid and returns a new axis-aligned
    //     cuboid resulting from this rotation. Not sure it it makes any sense to use this
    //     for other rotations than 90 degree intervals.
    public Cuboidf TransformedCopy(float[] matrix)
    {
        float[] array = Mat4f.MulWithVec4(matrix, X1, Y1, Z1, 1f);
        float[] array2 = Mat4f.MulWithVec4(matrix, X1, Y1, Z2, 1f);
        float[] array3 = Mat4f.MulWithVec4(matrix, X2, Y1, Z2, 1f);
        float[] array4 = Mat4f.MulWithVec4(matrix, X2, Y1, Z1, 1f);
        float[] array5 = Mat4f.MulWithVec4(matrix, X1, Y2, Z1, 1f);
        float[] array6 = Mat4f.MulWithVec4(matrix, X1, Y2, Z2, 1f);
        float[] array7 = Mat4f.MulWithVec4(matrix, X2, Y2, Z2, 1f);
        float[] array8 = Mat4f.MulWithVec4(matrix, X2, Y2, Z1, 1f);
        Cuboidf cuboidf = new Cuboidf();
        cuboidf.X1 = GameMath.Min(array[0], array2[0], array3[0], array4[0], array5[0], array6[0], array7[0], array8[0]);
        cuboidf.Y1 = GameMath.Min(array[1], array2[1], array3[1], array4[1], array5[1], array6[1], array7[1], array8[1]);
        cuboidf.Z1 = GameMath.Min(array[2], array2[2], array3[2], array4[2], array5[2], array6[2], array7[2], array8[2]);
        cuboidf.X2 = GameMath.Max(array[0], array2[0], array3[0], array4[0], array5[0], array6[0], array7[0], array8[0]);
        cuboidf.Y2 = GameMath.Max(array[1], array2[1], array3[1], array4[1], array5[1], array6[1], array7[1], array8[1]);
        cuboidf.Z2 = GameMath.Max(array[2], array2[2], array3[2], array4[2], array5[2], array6[2], array7[2], array8[2]);
        return cuboidf;
    }

    //
    // Summary:
    //     Performs a 3-dimensional rotation on the cuboid and returns a new axis-aligned
    //     cuboid resulting from this rotation. Not sure it it makes any sense to use this
    //     for other rotations than 90 degree intervals.
    public Cuboidf RotatedCopy(IVec3 vec, Vec3d origin)
    {
        return RotatedCopy(vec.XAsFloat, vec.YAsFloat, vec.ZAsFloat, origin);
    }

    //
    // Summary:
    //     Returns a new double precision cuboid offseted by given position
    public Cuboidf OffsetCopy(float x, float y, float z)
    {
        return new Cuboidf(X1 + x, Y1 + y, Z1 + z, X2 + x, Y2 + y, Z2 + z);
    }

    //
    // Summary:
    //     Returns a new cuboid offseted by given position
    public Cuboidf OffsetCopy(IVec3 vec)
    {
        return OffsetCopy(vec.XAsFloat, vec.YAsFloat, vec.ZAsFloat);
    }

    //
    // Summary:
    //     Returns a new cuboid offseted by given position
    public Cuboidd OffsetCopyDouble(double x, double y, double z)
    {
        return new Cuboidd((double)X1 + x, (double)Y1 + y, (double)Z1 + z, (double)X2 + x, (double)Y2 + y, (double)Z2 + z);
    }

    //
    // Summary:
    //     Returns a new cuboid offseted by given position
    public Cuboidd OffsetCopyDouble(IVec3 vec)
    {
        return OffsetCopyDouble(vec.XAsDouble, vec.YAsDouble, vec.ZAsDouble);
    }

    //
    // Summary:
    //     Expands this in the given direction by amount d
    public void Expand(BlockFacing face, float d)
    {
        switch (face.Index)
        {
            case 0:
                Z1 -= d;
                break;
            case 1:
                X2 += d;
                break;
            case 2:
                Z2 += d;
                break;
            case 3:
                X1 -= d;
                break;
            case 4:
                Y2 += d;
                break;
            case 5:
                Y1 -= d;
                break;
        }
    }

    //
    // Summary:
    //     Creates a copy of the cuboid
    public Cuboidf Clone()
    {
        return (Cuboidf)MemberwiseClone();
    }

    public Cuboidd ToDouble()
    {
        return new Cuboidd(X1, Y1, Z1, X2, Y2, Z2);
    }

    //
    // Summary:
    //     Returns a new cuboid with default size 1 width/height/length
    public static Cuboidf Default()
    {
        return new Cuboidf(0f, 0f, 0f, 1f, 1f, 1f);
    }

    //
    // Summary:
    //     Makes sure the collisionbox coords are multiples of 0.0001
    public void RoundToFracsOfOne10thousand()
    {
        X1 = RoundToOne10thousand(X1);
        Y1 = RoundToOne10thousand(Y1);
        Z1 = RoundToOne10thousand(Z1);
        X2 = RoundToOne10thousand(X2);
        Y2 = RoundToOne10thousand(Y2);
        Z2 = RoundToOne10thousand(Z2);
    }

    private static float RoundToOne10thousand(float val)
    {
        return (float)(int)(val * 10000f + 0.5f) / 10000f;
    }

    public bool Equals(Cuboidf other)
    {
        if (other.X1 == X1 && other.Y1 == Y1 && other.Z1 == Z1 && other.X2 == X2 && other.Y2 == Y2)
        {
            return other.Z2 == Z2;
        }

        return false;
    }

    public Cuboidi ConvertToCuboidi()
    {
        return new Cuboidi((int)X1, (int)Y1, (int)Z1, (int)X2, (int)Y2, (int)Z2);
    }

    public override string ToString()
    {
        return "[" + X1 + ", " + Y1 + ", " + Z1 + " => " + X2 + ", " + Y2 + ", " + Z2 + "]";
    }
}
#if false // Decompilation log
'182' items in cache
------------------
Resolve: 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Runtime.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Collections.dll'
------------------
Resolve: 'SkiaSharp, Version=2.88.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
Found single assembly: 'SkiaSharp, Version=2.88.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\SkiaSharp.dll'
------------------
Resolve: 'System.Collections.Concurrent, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Concurrent, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Collections.Concurrent.dll'
------------------
Resolve: 'Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'
Found single assembly: 'Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\Newtonsoft.Json.dll'
------------------
Resolve: 'protobuf-net, Version=2.4.0.0, Culture=neutral, PublicKeyToken=257b51d87d2e4d67'
Found single assembly: 'protobuf-net, Version=2.4.0.0, Culture=neutral, PublicKeyToken=257b51d87d2e4d67'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\protobuf-net.dll'
------------------
Resolve: 'System.Security.Cryptography, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Security.Cryptography.dll'
------------------
Resolve: 'System.Collections.Specialized, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Specialized, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Collections.Specialized.dll'
------------------
Resolve: 'System.Net.NetworkInformation, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.NetworkInformation, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Net.NetworkInformation.dll'
------------------
Resolve: 'System.Net.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Net.Primitives.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Text.RegularExpressions.dll'
------------------
Resolve: 'cairo-sharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'cairo-sharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\cairo-sharp.dll'
------------------
Resolve: 'System.Drawing.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Drawing.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Drawing.Primitives.dll'
------------------
Resolve: 'System.ObjectModel, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ObjectModel, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.ObjectModel.dll'
------------------
Resolve: 'System.ComponentModel.TypeConverter, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.TypeConverter, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.ComponentModel.TypeConverter.dll'
------------------
Resolve: 'OpenTK.Windowing.GraphicsLibraryFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'OpenTK.Windowing.GraphicsLibraryFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\OpenTK.Windowing.GraphicsLibraryFramework.dll'
------------------
Resolve: 'Microsoft.Data.Sqlite, Version=8.0.3.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.Data.Sqlite, Version=8.0.3.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\Microsoft.Data.Sqlite.dll'
------------------
Resolve: 'System.Data.Common, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Data.Common, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Data.Common.dll'
------------------
Resolve: 'System.Threading, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Threading.dll'
------------------
Resolve: 'OpenTK.Windowing.Desktop, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'OpenTK.Windowing.Desktop, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\gwlar\AppData\Roaming\Vintagestory\Lib\OpenTK.Windowing.Desktop.dll'
------------------
Resolve: 'System.Threading.Thread, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.Thread, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Threading.Thread.dll'
------------------
Resolve: 'System.Diagnostics.Process, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.Process, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Diagnostics.Process.dll'
------------------
Resolve: 'System.Linq, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Linq.dll'
------------------
Resolve: 'System.Console, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Console, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Console.dll'
------------------
Resolve: 'System.Net.NameResolution, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Net.NameResolution, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Net.NameResolution.dll'
------------------
Resolve: 'System.Diagnostics.StackTrace, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Diagnostics.StackTrace, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Diagnostics.StackTrace.dll'
------------------
Resolve: 'System.ComponentModel.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.ComponentModel.Primitives.dll'
------------------
Resolve: 'System.Threading.ThreadPool, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading.ThreadPool, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Threading.ThreadPool.dll'
------------------
Resolve: 'System.Collections.NonGeneric, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.NonGeneric, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Collections.NonGeneric.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.10\ref\net7.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif
