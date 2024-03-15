using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    class Memory
    {
        readonly uint[] ram;
        readonly uint[] stack = new uint[0x4000]; // 2KB stack in 2 directions

        public Memory(uint[] ram)
        {
            this.ram = ram;
        }

        public uint Read(int vAddr)
        {
            return Read((uint) vAddr);
        }

        public uint Read(uint vAddr)
        {
            uint seg = vAddr >> 56;
            uint off = vAddr & 0x00ffffff;
            off /= 4;

            if (seg == 0x80)
            {
                return ram[off];
            }

            if (seg == 0x81)
            {
                return stack[off];
            }

            throw new ArithmeticException($"Unknown segment {seg}");
        }

        public void Write(int vAddr, byte val, int dataOff)
        {
            Write((uint)vAddr, val, dataOff);
        }

        public void Write(int vAddr, ushort val, int dataOff)
        {
            Write((uint)vAddr, val, dataOff);
        }

        public void Write(int vAddr, uint val)
        {
            Write((uint)vAddr, val);
        }

        public void Write(uint vAddr, byte val, int dataOff)
        {
            uint seg = vAddr >> 56;
            uint off = vAddr & 0x00ffffff;
            off /= 4;

            if (seg == 0x80)
            {
                //int data = ram[off];
                //data &= (0xff << (24 - 8 * dataOff));
                //data |= (val << (24 - 8 * dataOff));
                //ram[off] = data;
                return;
            }

            if (seg == 0x81)
            {
                uint data = stack[off];
                data &= (0xffU << (24 - 8 * dataOff));
                data |= (((uint)val) << (24 - 8 * dataOff));
                stack[off] = data;
                return;
            }

            throw new ArithmeticException($"Unknown segment {seg}");
        }

        public void Write(uint vAddr, ushort val, int dataOff)
        {
            uint seg = vAddr >> 48;
            uint off = vAddr & 0x00ffffff;
            off /= 4;

            if (seg == 0x80)
            {
                //int data = ram[off];
                //data &= (0xff << (16 - 16 * dataOff));
                //data |= (val << (16 - 16 * dataOff));
                //ram[off] = data;
                return;
            }

            if (seg == 0x81)
            {
                uint data = stack[off];
                data &= (0xffU << (16 - 16 * dataOff));
                data |= (((uint) val) << (16 - 16 * dataOff));
                stack[off] = data;
                return;
            }

            throw new ArithmeticException($"Unknown segment {seg}");
        }

        public void Write(uint vAddr, uint val)
        {
            uint seg = vAddr >> 48;
            uint off = vAddr & 0x00ffffff;
            off /= 4;

            if (seg == 0x80)
            {
                //ram[off] = val;
                return;
            }

            if (seg == 0x81)
            {
                stack[off] = val;
                return;
            }

            throw new ArithmeticException($"Unknown segment {seg}");
        }
    }
}
