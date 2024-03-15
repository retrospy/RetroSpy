using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    internal class Cleanser
    {
        ISet<Register> _safeRegisters;

        public Cleanser(ISet<Register> safeRegisters)
        {
            _safeRegisters = safeRegisters;
        }
        
        public (Instruction, uint) Clean(Instruction inst)
        {
            uint mask = 0;
            const uint regMask = 0b11111;
            void Cleanse(ref Register? r, int off)
            {
                if (!r.HasValue)
                    return;

                if (r == Register.R0)
                    return;

                if (_safeRegisters.Contains(r.Value))
                    return;

                mask |= regMask << off;
                r = Register.R0;
            }

            Cleanse(ref inst.rs, 21);
            Cleanse(ref inst.rt, 16);
            Cleanse(ref inst.rd, 11);

            return (inst, mask);
        }
    }
}
