using System;

namespace Fragsurf.Shared.Entity
{
    public static class Serializer
    {
        public static unsafe byte[] Serialize<T>(T value) where T : unmanaged
        {
            byte[] buffer = new byte[sizeof(T)];

            fixed (byte* bufferPtr = buffer)
            {
                Buffer.MemoryCopy(&value, bufferPtr, sizeof(T), sizeof(T));
            }

            return buffer;
        }

        public static unsafe T Deserialize<T>(byte[] buffer) where T : unmanaged, IEquatable<T>
        {
            T result = new T();

            fixed (byte* bufferPtr = buffer)
            {
                Buffer.MemoryCopy(bufferPtr, &result, sizeof(T), sizeof(T));
            }

            return result;
        }
        class U<T> where T : unmanaged { }
        public static bool IsUnmanaged(this Type t)
        {
            try { typeof(U<>).MakeGenericType(t); return true; }
            catch (Exception) { return false; }
        }
    }
}