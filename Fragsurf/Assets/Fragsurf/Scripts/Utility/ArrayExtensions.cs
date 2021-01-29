using System;
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
}
