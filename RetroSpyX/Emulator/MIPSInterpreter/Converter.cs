using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    class Converter
    {
        public static Cmd ToCmd(Op op)
        {
            return Cmd.IMM | (Cmd)op;
        }

        public static Cmd ToCmd(Funct funct)
        {
            return Cmd.REG | (Cmd)funct;
        }

        public static Cmd ToCmd(FunctImm functImm)
        {
            return Cmd.REGIMM | (Cmd)functImm;
        }

        public static Cmd ToCmd(Cop cop, int num)
        {
            return (num == 0 ? Cmd.COP0 : Cmd.COP1) | (Cmd)cop;
        }

        public static Op ToOp(Cmd cmd)
        {
            return (Op)((int)cmd & 0xffffff);
        }

        public static Funct ToFunct(Cmd cmd)
        {
            return (Funct)((int)cmd & 0xffffff);
        }

        public static FunctImm ToFunctImm(Cmd cmd)
        {
            return (FunctImm)((int)cmd & 0xffffff);
        }

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0044 // Add readonly modifier
        static Dictionary<Cmd, Format> CmdToFormat = new Dictionary<Cmd, Format>()
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0090 // Use 'new(...)'
        {
            { Cmd.NOP, Format.NONE },

            { Cmd.ADDI, Format.REGIMM_ST },
            { Cmd.ADDIU, Format.REGIMM_ST },
            { Cmd.ANDI, Format.REGIMM_ST },
            { Cmd.BEQ, Format.REGOFF_ST },
            { Cmd.BEQL, Format.REGOFF_ST },
            { Cmd.BGTZ, Format.REGOFF_S },
            { Cmd.BGTZL, Format.REGOFF_S },
            { Cmd.BLEZ, Format.REGOFF_S },
            { Cmd.BLEZL, Format.REGOFF_S },
            { Cmd.BNE, Format.REGOFF_ST },
            { Cmd.BNEL, Format.REGOFF_ST },
            { Cmd.DADDI, Format.REGIMM_ST },
            { Cmd.DADDIU, Format.REGIMM_ST },
            { Cmd.J, Format.JUMP },
            { Cmd.JAL, Format.JUMP },
            { Cmd.LB, Format.REGOFF_ST },
            { Cmd.LBU, Format.REGOFF_ST },
            { Cmd.LD, Format.REGOFF_ST },
            { Cmd.LDL, Format.REGOFF_ST },
            { Cmd.LDR, Format.REGOFF_ST },
            { Cmd.LH, Format.REGOFF_ST },
            { Cmd.LHU, Format.REGOFF_ST },
            { Cmd.LL, Format.REGOFF_ST },
            { Cmd.LLD, Format.REGOFF_ST },
            { Cmd.LUI, Format.REGIMM_T },
            { Cmd.LW, Format.REGOFF_ST },
            { Cmd.LWL, Format.REGOFF_ST },
            { Cmd.LWR, Format.REGOFF_ST },
            { Cmd.LWU, Format.REGOFF_ST },
            { Cmd.ORI, Format.REGIMM_ST },
            { Cmd.SB, Format.REGOFF_ST },
            { Cmd.SC, Format.REGOFF_ST },
            { Cmd.SCD, Format.REGOFF_ST },
            { Cmd.SD, Format.REGOFF_ST },
            { Cmd.SDL, Format.REGOFF_ST },
            { Cmd.SDR, Format.REGOFF_ST },
            { Cmd.SH, Format.REGOFF_ST },
            { Cmd.SLTI, Format.REGIMM_ST },
            { Cmd.SLTIU, Format.REGIMM_ST },
            { Cmd.SW, Format.REGOFF_ST },
            { Cmd.SWL, Format.REGOFF_ST },
            { Cmd.SWR, Format.REGOFF_ST },
            { Cmd.XORI, Format.REGIMM_ST },

            { Cmd.ADD, Format.REG_STD },
            { Cmd.ADDU, Format.REG_STD },
            { Cmd.AND, Format.REG_STD },
//            { Cmd.BREAK, Format.REG_STD },
            { Cmd.DADD, Format.REG_STD },
            { Cmd.DADDU, Format.REG_STD },
            { Cmd.DDIV, Format.REG_ST },
            { Cmd.DDIVU, Format.REG_ST },
            { Cmd.DIV, Format.REG_ST },
            { Cmd.DIVU, Format.REG_ST },
            { Cmd.DMULT, Format.REG_ST },
            { Cmd.DMULTU, Format.REG_ST },
            { Cmd.DSLL, Format.REG_TDA },
            { Cmd.DSLL32, Format.REG_TDA },
            { Cmd.DSLLV, Format.REG_STD },
            { Cmd.DSRA, Format.REG_TDA },
            { Cmd.DSRA32, Format.REG_TDA },
            { Cmd.DSRAV, Format.REG_STD },
            { Cmd.DSRL, Format.REG_TDA },
            { Cmd.DSRL32, Format.REG_TDA },
            { Cmd.DSRLV, Format.REG_STD },
            { Cmd.DSUB, Format.REG_STD },
            { Cmd.DSUBU, Format.REG_STD },
            { Cmd.JALR, Format.REG_SD },
            { Cmd.JR, Format.REG_S },
            { Cmd.MFHI, Format.REG_D },
            { Cmd.MFLO, Format.REG_D },
            { Cmd.MTHI, Format.REG_S },
            { Cmd.MTLO, Format.REG_S },
            { Cmd.MULT, Format.REG_ST },
            { Cmd.MULTU, Format.REG_ST },
            { Cmd.NOR, Format.REG_STD },
            { Cmd.OR, Format.REG_STD },
            { Cmd.SLL, Format.REG_TDA },
            { Cmd.SLLV, Format.REG_STD },
            { Cmd.SLT, Format.REG_STD },
            { Cmd.SLTU, Format.REG_STD },
            { Cmd.SRA, Format.REG_TDA },
            { Cmd.SRAV, Format.REG_STD },
            { Cmd.SRL, Format.REG_TDA },
            { Cmd.SRLV, Format.REG_STD },
            { Cmd.SUB, Format.REG_STD },
            { Cmd.SUBU, Format.REG_STD },
            { Cmd.SYNC, Format.REG_A },
//            { Cmd.SYSCALL, Format.REG_STD },
            { Cmd.XOR, Format.REG_STD },

            { Cmd.BGEZ, Format.REGOFF_S },
            { Cmd.BGEZAL, Format.REGOFF_S },
            { Cmd.BGEZALL, Format.REGOFF_S },
            { Cmd.BGEZL, Format.REGOFF_S },
            { Cmd.BLTZ, Format.REGOFF_S },
            { Cmd.BLTZAL, Format.REGOFF_S },
            { Cmd.BLTZALL, Format.REGOFF_S },
            { Cmd.BLTZL, Format.REGOFF_S },

            { Cmd.MTC0, Format.REG_COP0 },
            { Cmd.MFC0, Format.REG_COP0 },

            { Cmd.CACHE, Format.REGOFF_CACHE },
        };

        public static Format ToFormat(Cmd cmd)
        {
            return CmdToFormat[cmd];
        }

        public static uint ToUInt(Instruction inst)
        {
            if (inst.cmd == Cmd.NOP)
                return 0;

            Format format = ToFormat(inst.cmd);

            uint ret = 0;

            if (format.HasFlag(Format.IMM))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (ushort)inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.OFF))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (ushort)(inst.off >> 2);
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.JUMP))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= inst.jump.Value >> 2;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.REG_S))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (uint)inst.rs.Value << 21;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.REG_T))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (uint)inst.rt.Value << 16;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.REG_D))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (uint)inst.rd.Value << 11;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.REG_A))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (uint)inst.shift.Value << 6;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.COP0_D))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (uint)inst.cop0.Value << 11;
#pragma warning restore CS8629 // Nullable value type may be null.
            }
            if (format.HasFlag(Format.CACHE_T))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                ret |= (uint)inst.cache.Value << 16;
#pragma warning restore CS8629 // Nullable value type may be null.
            }

            Cmd cmd = inst.cmd;
            uint cmdVal = (uint) cmd & 0b111111;
            if (cmd.HasFlag(Cmd.REGIMM))
            {
                uint regimm = 0b000001;
                ret |= regimm << 26;
                ret |= cmdVal << 16;
            }
            else if (cmd.HasFlag(Cmd.REG))
            {
                ret |= cmdVal;
            }
            else if (cmd.HasFlag(Cmd.IMM))
            {
                ret |= cmdVal << 26;
            }
            else if (cmd.HasFlag(Cmd.COP0))
            {
                uint cop0 = 0b010000;
                ret |= cop0   << 26;
                ret |= cmdVal << 21;
            }
            else
            {
                throw new ArithmeticException("Bad Cmd passed!");
            }

            return ret;
        }

        public static uint Extract(uint inst, int off, int bits)
        {
            uint data = inst >> off;
            uint mask = 1U << bits;
            return data & (mask - 1);
        }

        public static int ExtendZero(short val)
        {
            ushort uval = (ushort)val;
            return uval;
        }

        public static int ExtendSign(short val)
        {
            return val;
        }
    }
}
