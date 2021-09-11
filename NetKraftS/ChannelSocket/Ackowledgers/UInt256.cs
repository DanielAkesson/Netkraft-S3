using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netkraft
{
    public struct Uint256
    {
        ulong[] data;
        public Uint256(ulong n)
        {
            data = new ulong[4];
            data[0] = n;
            data[1] = 0;
            data[2] = 0;
            data[3] = 0;
        }
        public Uint256(Uint256 n)
        {
            //Can be improved with array copy
            data = new ulong[4];
            data[0] = n.data[0];
            data[1] = n.data[1];
            data[2] = n.data[2];
            data[3] = n.data[3];
        }
        public ulong[] Longs()
        {
            return data;
        }
        //Equals
        public static bool operator ==(Uint256 a, Uint256 b)
        {
            return a.data[0] == b.data[0] && a.data[1] == b.data[1] && a.data[2] == b.data[2] && a.data[3] == b.data[3];
        }
        public static bool operator !=(Uint256 a, Uint256 b)
        {
            return !(a == b);
        }
        public static bool operator ==(Uint256 a, ulong b)
        {
            return a.data[0] == b;
        }
        public static bool operator !=(Uint256 a, ulong b)
        {
            return !(a == b);
        }

        //Bitwise
        public static Uint256 operator <<(Uint256 b, int a)
        {
            Uint256 n = new Uint256(b);
            n.LeftShift(3, a);
            n.LeftShift(2, a);
            n.LeftShift(1, a);
            n.LeftShift(0, a);
            return n;
        }
        private void LeftShift(int index, int amount)
        {
            int a = amount % 256;
            for(int i= index+1; i<4; i++)
            {
                int ca = a - ((i - index) * 64);
                data[i] |= overflowShift(data[index], ca);
            }
            data[index] = overflowShift(data[index], a);

            ulong overflowShift(ulong v, int i) { return (i >= 64 || i <= -64) ? 0 : i < 0 ? v >> Math.Abs(i) : v << i;}
        }
        public static Uint256 operator >>(Uint256 b, int a)
        {
            Uint256 n = new Uint256(b);
            n.RightShift(0, a);
            n.RightShift(1, a);
            n.RightShift(2, a);
            n.RightShift(3, a);
            return n;
        }
        private void RightShift(int index, int amount)
        {
            int a = amount % 256;
            for (int i = index - 1; i >= 0; i--)
            {
                int ca = a - ((index - i) * 64);
                data[i] |= overflowShift(data[index], ca);
            }
            data[index] = overflowShift(data[index], a);

            ulong overflowShift(ulong v, int i) { return (i >= 64 || i <= -64) ? 0 : i < 0 ? v << Math.Abs(i) : v >> i; }
        }
        //OR
        public static Uint256 operator |(Uint256 a, uint b)
        {
            Uint256 n = new Uint256(a);
            n.data[0] |= b;
            return n;
        }
        public static uint operator |(uint a, Uint256 b)
        {
            return a | (uint)b.data[0];
        }
        public static Uint256 operator |(Uint256 a, Uint256 b)
        {
            Uint256 n = new Uint256(0);
            n.data[0] = a.data[0] | b.data[0];
            n.data[1] = a.data[1] | b.data[1];
            n.data[2] = a.data[2] | b.data[2];
            n.data[3] = a.data[3] | b.data[3];
            return n;
        }
        //AND
        public static Uint256 operator &(Uint256 a, uint b)
        {
            Uint256 n = new Uint256(a);
            n.data[0] &= b;
            return n;
        }
        public static uint operator &(uint a, Uint256 b)
        {
            return a & (uint)b.data[0];
        }
        public static Uint256 operator &(Uint256 a, Uint256 b)
        {
            Uint256 n = new Uint256(0);
            n.data[0] = a.data[0] & b.data[0];
            n.data[1] = a.data[1] & b.data[1];
            n.data[2] = a.data[2] & b.data[2];
            n.data[3] = a.data[3] & b.data[3];
            return n;
        }
        //Not
        public static Uint256 operator ~(Uint256 b)
        {
            Uint256 n = new Uint256(b);
            n.data[0] = ~n.data[0];
            n.data[1] = ~n.data[1];
            n.data[2] = ~n.data[2];
            n.data[3] = ~n.data[3];
            return n;
        }
        //Explicit casts
        public static explicit operator byte(Uint256 a)
        {
            return (byte)a.data[0];
        }
        public static explicit operator ushort(Uint256 a)
        {
            return (ushort)a.data[0];
        }
        public static explicit operator uint(Uint256 a)
        {
            return (uint)a.data[0];
        }
        public static explicit operator ulong(Uint256 a)
        {
            return (ulong)a.data[0];
        }
    }
}
