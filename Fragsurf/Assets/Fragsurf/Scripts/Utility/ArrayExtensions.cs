using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

public static class ArrayExtensions
{
    public static void ShiftRight<T>(this T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            arr[i] = arr[i - 1];
        }
    }
    public static void ShiftRightPrepend<T>(this T[] arr, T o)
    {
        ShiftRight(arr);
        arr[0] = o;
    }
    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }
    public static T ReadAtPosition<T>(this byte[] buffer, int position)
        where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        var bytes = new byte[size];

        Array.Copy(buffer, position, bytes, 0, size);
        T stuff;
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }
        return stuff;
    }

    private const int COMPRESSION_BUFFER_SIZE = 64 * 1024;

    public static byte[] Compress(this byte[] inputData)
    {
        if (inputData == null)
            throw new ArgumentNullException("inputData must be non-null");

        using (var compressIntoMs = new MemoryStream())
        {
            using (var gzs = new BufferedStream(new GZipStream(compressIntoMs,
             CompressionMode.Compress), COMPRESSION_BUFFER_SIZE))
            {
                gzs.Write(inputData, 0, inputData.Length);
            }
            return compressIntoMs.ToArray();
        }
    }

    public static byte[] Decompress(this byte[] inputData)
    {
        if (inputData == null)
            throw new ArgumentNullException("inputData must be non-null");

        using (var compressedMs = new MemoryStream(inputData))
        {
            using (var decompressedMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                 CompressionMode.Decompress), COMPRESSION_BUFFER_SIZE))
                {
                    gzs.CopyTo(decompressedMs);
                }
                return decompressedMs.ToArray();
            }
        }
    }

}
