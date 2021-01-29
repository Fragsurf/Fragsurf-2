
namespace Fragsurf.Utility
{
    public static class IntExtensions
    {
        public static bool HasFlag(this int a, int b)
        {
            return (a & b) == b;
        }

        public static int AddFlag(this int a, int b)
        {
            return a |= b;
        }

        public static int RemoveFlag(this int a, int b)
        {
            return a &= ~b;
        }
    }
}
