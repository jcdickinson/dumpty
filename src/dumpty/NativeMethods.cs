using System.Runtime.InteropServices;
using System.Security;

namespace Dumpty
{
    [SuppressUnmanagedCodeSecurity]
    static class NativeMethods
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool Equals(byte[] b1, byte[] b2)
        {
            if (b1 == null && b2 == null) return true;
            if (b1 == null || b2 == null) return false;

            return b1.Length == b2.Length && memcmp(b1, b2, b1.LongLength) == 0;
        }
    }
}
