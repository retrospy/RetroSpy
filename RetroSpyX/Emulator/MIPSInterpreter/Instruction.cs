using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MIPSInterpreter
{
    struct Instruction
    {
        public Cmd cmd;
        public Register? rs;
        public Register? rt;
        public Register? rd;
        public int? shift;
        public short? imm;
        public int? off;
        public uint? jump;
        public Cop0Registers? cop0;
        public CacheOp? cache;

        public override string ToString()
        {
            string output = $"{cmd, -8}";
            if (rs.HasValue && rt.HasValue && off.HasValue)
            {
                if (cmd != Cmd.BEQ || cmd != Cmd.BEQL || cmd != Cmd.BNE || cmd != Cmd.BNEL)
                {
                    return $"{output} {rt.Value}, 0x{off.Value:X}({rs.Value})";
                }
            }

            string sep = " ";
            void Append(string app)
            {
                output += sep;
                sep = ", ";
                output += app;
            }

            if (cache.HasValue)
            {
                Append($"({cache.Value})");
            }
            if (rd.HasValue)
            {
                Append(rd.Value.ToString());
            }
            if (imm.HasValue)
            {
                // ADDIU and other imm instructions have rt and rs swapped
                if (rt.HasValue)
                {
                    Append(rt.Value.ToString());
                }
                if (rs.HasValue)
                {
                    Append(rs.Value.ToString());
                }
            }
            else
            {
                if (rs.HasValue)
                {
                    Append(rs.Value.ToString());
                }
                if (rt.HasValue)
                {
                    Append(rt.Value.ToString());
                }
            }
            if (off.HasValue)
            {
                Append($"0x{off.Value:X4}");
            }
            if (jump.HasValue)
            {
                Append($"0x{jump.Value:X8}");
            }
            if (imm.HasValue)
            {
                Append($"0x{imm.Value:X4}");
            }
            if (shift.HasValue)
            {
                Append(shift.Value.ToString());
            }
            if (cop0.HasValue)
            {
                Append(cop0.Value.ToString());
            }

            return output;
        }
    }
}
