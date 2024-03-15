using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    class Interpreter
    {
        public int[] gpr = new int[32];
        public int lo;
        public int hi;
        public uint pc;

        delegate void Performer(Instruction inst);

        readonly Memory memory;
        readonly Dictionary<Cmd, Performer> cmdToFunc;

        void Add(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rs] + gpr[rt];
        }

        void AddI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            short imm = inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = (int) gpr[rs] + imm;
        }

        void And(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rs] & gpr[rt];
        }

        void AndI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            short imm = inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = imm & gpr[rs];
        }

        void Div(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int rsVal = gpr[rs];
            int rtVal = gpr[rt];
            lo = rsVal % rtVal;
            hi = rsVal / rtVal;
        }

        void DivU(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            uint rsVal = (uint)gpr[rs];
            uint rtVal = (uint) gpr[rt];
            lo = (int) (rsVal % rtVal);
            hi = (int) (rsVal / rtVal);
        }

        void LB(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            int dataPos = vAddr & 0x3;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (char)(pAddr >> (24 - dataPos * 8));
        }

        void LBU(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            int dataPos = vAddr & 0x3;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (byte) (pAddr >> (24 - dataPos * 8));
        }

        void LH(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            int shortPos = vAddr & 0x1;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (short)(pAddr >> (16 - shortPos * 16));
        }

        void LHU(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            int shortPos = vAddr & 0x1;
            uint pAddr = memory.Read(vAddr);
            gpr[rt] = (ushort) (pAddr >> (16 - shortPos * 16));
        }

        void LUI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int imm = inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = imm << 16;
        }

        void LW(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            gpr[rt] = (int) memory.Read(vAddr);
        }

        void MFHI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = hi;
        }

        void MFLO(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = lo;
        }

        void MTHI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            hi = gpr[rs];
        }

        void MTLO(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            lo = gpr[rs];
        }

        void Mult(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            long rsVal = gpr[rs];
            long rtVal = gpr[rt];
            long val = rsVal * rtVal;
            lo = (int)val;
            hi = (int)(val >> 32);
        }

        void MultU(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            ulong rsVal = (uint)gpr[rs];
            ulong rtVal = (uint)gpr[rt];
            ulong val = rsVal * rtVal;
            lo = (int)val;
            hi = (int)(val >> 32);
        }

        void NOr(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.

            gpr[rd] = ~(gpr[rs] | gpr[rt]);
        }

        void Or(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rs] | gpr[rt];
        }

        void OrI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int imm = (ushort)inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = gpr[rs] | imm;
        }

        void SB(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            int dataPos = vAddr & 0x3;
            memory.Write(vAddr, (byte)gpr[rt], dataPos);
        }

        void SH(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            int shortPos = vAddr & 0x1;
            memory.Write(vAddr, (ushort)gpr[rt], shortPos);
        }

        void SLLV(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rt] << gpr[rs];
        }

        void SLT(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rs] < gpr[rt] ? 1 : 0;
        }

        void SLTI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int imm = inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = gpr[rs] < imm ? 1 : 0;
        }

        void SLTIU(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            uint imm = (ushort)inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = (uint)gpr[rs] < imm ? 1 : 0;
        }

        void SLTU(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = (uint)gpr[rs] < (uint)gpr[rt] ? 1 : 0;
        }

        void SRA(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int sa = inst.shift.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rt] >> sa;
        }

        void SRAV(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rt] >> gpr[rs];
        }

        void SRL(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int sa = inst.shift.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = (int)(((uint)gpr[rt]) >> sa);
        }

        void SRLV(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = (int)(((uint)gpr[rt]) >> gpr[rs]);
        }

        void Sub(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rs] - gpr[rt];
        }

        void SW(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int off = inst.off.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            int vAddr = off + gpr[rs];
            memory.Write(vAddr, (uint) gpr[rt]);
        }

        void Xor(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rd = (int)inst.rd.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rd] = gpr[rs] ^ gpr[rt];
        }

        void XorI(Instruction inst)
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            int rs = (int)inst.rs.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int rt = (int)inst.rt.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning disable CS8629 // Nullable value type may be null.
            int imm = (ushort)inst.imm.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            gpr[rt] = gpr[rs] ^ imm;
        }

        public Interpreter(uint[] ram)
        {
            memory = new Memory(ram);
            cmdToFunc = new Dictionary<Cmd, Performer>()
            {
                { Cmd.ADD,   Add },
                { Cmd.ADDU,  Add },
                { Cmd.ADDI,  AddI },
                { Cmd.ADDIU, AddI },
                { Cmd.AND,   And },
                { Cmd.ANDI,  AndI },
                { Cmd.DIV,   Div },
                { Cmd.DIVU,  DivU },
                { Cmd.LB,    LB },
                { Cmd.LBU,   LBU },
                { Cmd.LH,    LH },
                { Cmd.LHU,   LHU },
                { Cmd.LUI,   LUI },
                { Cmd.LW,    LW },
                { Cmd.LWU,   LW },
                { Cmd.MFHI,  MFHI },
                { Cmd.MFLO,  MFLO },
                { Cmd.MTHI,  MTHI },
                { Cmd.MTLO,  MTLO },
                { Cmd.MULT,  Mult },
                { Cmd.MULTU, MultU },
                { Cmd.NOR,   NOr },
                { Cmd.OR,    Or },
                { Cmd.ORI,   OrI },
                { Cmd.SB,    SB },
                { Cmd.SH,    SH },
                { Cmd.SLLV,  SLLV },
                { Cmd.SLT,   SLT },
                { Cmd.SLTI,  SLTI },
                { Cmd.SLTIU, SLTIU },
                { Cmd.SLTU,  SLTU },
                { Cmd.SRA,   SRA },
                { Cmd.SRAV,  SRAV },
                { Cmd.SRL,   SRL },
                { Cmd.SRLV,  SRLV },
                { Cmd.SUB,   Sub },
                { Cmd.SUBU,  Sub },
                { Cmd.SW,    SW },
                { Cmd.XOR,   Xor },
                { Cmd.XORI,  XorI },
            };
        }

        public void Execute(Instruction inst)
        {
            try
            {

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                if (cmdToFunc.TryGetValue(inst.cmd, out Performer perform))
                    perform(inst);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch (Exception) { }
        }

        public Instruction? GetInstruction()
        {
            uint cmd = memory.Read((int) pc);
            Instruction? inst = null;
            try
            {
                inst = Decompiler.Decode(cmd);
            }
            catch(Exception) 
            { }
            pc += 4;
            return inst;
        }
    }
}
