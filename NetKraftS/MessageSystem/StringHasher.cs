namespace Netkraft.WritableSystem
{
    public static class StringHasher
    {
        static public ushort HashStringTo16Bit(ushort seed, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                seed += (ushort)(Rol3_16(seed) + value[i]);
            }
            return (ushort)(seed ^ value.Length);
        }
        static private ushort Rol3_16(ushort x)
        {
            return (ushort)((x << 3) | (x >> (16 - 3)));
        }

        static public uint HashStringTo32Bit(uint seed, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                seed += Rol3_32(seed) + value[i];
            }
            return (uint)(seed ^ value.Length);
        }
        static private uint Rol3_32(uint x)
        {
            return(x << 3) | (x >> (32 - 3));
        }
    }
}


