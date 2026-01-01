using System;

namespace XZPToolv3
{
    /// <summary>
    /// Endian byte order conversion utilities
    /// </summary>
    internal class Endian
    {
        public static short SwapInt16(short v)
        {
            return (short)((v & 0xFF) << 8 | (v >> 8 & 0xFF));
        }

        public static int SwapInt32(int v)
        {
            return ((int)SwapInt16((short)v) & 0xFFFF) << 16 |
                   ((int)SwapInt16((short)(v >> 16)) & 0xFFFF);
        }
    }
}
