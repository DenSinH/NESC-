﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator
{
    partial class CPU
    {
        /*
         Code for the instructions of the MOS 6502 CPU
             */

        private int ADC(InstructionMode mode)
        {
            /*
            ADC  Add Memory to Accumulator with Carry
                    A + M + C -> A, C                N Z C I D V
                                                    + + + - - +
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     ADC #oper     69    2     2;
                    zeropage      ADC oper      65    2     3;
                    zeropage,X    ADC oper,X    75    2     4;
                    absolute      ADC oper      6D    3     4;
                    absolute,X    ADC oper,X    7D    3     4*
                    absolute,Y    ADC oper,Y    79    3     4*
                    (indirect,X)  ADC (oper,X)  61    2     6;
                    (indirect),Y  ADC (oper),Y  71    2     5*
            */

            int unsigned_result = this.ac + this[this.oper] + this.getFlag('C');
            int signed_result = unchecked((sbyte)this.ac + (sbyte)this[this.oper]) + this.getFlag('C');

            this.ac = (byte)(unsigned_result);

            this.setNZ(this.ac);
            this.setFlag('C', (byte)(unsigned_result > 0xff ? 1 : 0));
            this.setFlag('V', (byte)(signed_result > 127 || signed_result < -128 ? 1 : 0));

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX:
                case InstructionMode.abs: return 4;
                case InstructionMode.absX:
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int AND(InstructionMode mode)
        {
            /*
            AND  AND Memory with Accumulator
                    A AND M -> A                     N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     AND #oper     29    2     2;
                    zeropage      AND oper      25    2     3;
                    zeropage,X    AND oper,X    35    2     4;
                    absolute      AND oper      2D    3     4;
                    absolute,X    AND oper,X    3D    3     4*
                    absolute,Y    AND oper,Y    39    3     4*
                    (indirect,X)  AND (oper,X)  21    2     6;
                    (indirect),Y  AND (oper),Y  31    2     5*
            */

            this.ac &= this[this.oper];
            this.setNZ(this.ac);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ASL(InstructionMode mode)
        {
            /*
            ASL  Shift Left One Bit (Memory || Accumulator);
                    C <- [76543210] <- 0             N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    accumulator   ASL A         0A    1     2;
                    zeropage      ASL oper      06    2     5;
                    zeropage,X    ASL oper,X    16    2     6;
                    absolute      ASL oper      0E    3     6;
                    absolute,X    ASL oper,X    1E    3     7;
            */

            this.setFlag('C', (byte)(this[this.oper] >= 128 ? 1 : 0));
            this[this.oper] <<= 1;
            this.setNZ(this[this.oper]);

            switch (mode)
            {
                case InstructionMode.A: return 2;
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int BCC(InstructionMode mode)
        {
            /*
            BCC  Branch on Carry Clear
                    branch on C = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BCC oper      90    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('C') == 0)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BCS(InstructionMode mode)
        {
            /*
            BCS  Branch on Carry Set
                    branch on C = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BCS oper      B0    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('C') == 1)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BEQ(InstructionMode mode)
        {
            /*
            BEQ  Branch on Result Zero
                    branch on Z = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BEQ oper      F0    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('Z') == 1)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BIT(InstructionMode mode)
        {
            /*
            BIT  Test Bits in Memory with Accumulator
                    bits 7 && 6 of operand are transfered to bit 7 && 6 of SR (N,V);
                    the zeroflag is set to the result of operand AND accumulator.
                    A AND M, M7 -> N, M6 -> V        N Z C I D V
                                                    M7 + - - - M6;
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      BIT oper      24    2     3;
                    absolute      BIT oper      2C    3     4;
            */

            this.setFlag('N', (byte)((this[this.oper] & 0b1000_0000) >> 7));
            this.setFlag('V', (byte)((this[this.oper] & 0b0100_0000) >> 6));
            this.setFlag('Z', (byte)((this[this.oper] & this.ac) == 0 ? 1 : 0));

            switch (mode)
            {
                case InstructionMode.zpg: return 3;
                case InstructionMode.abs: return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int BMI(InstructionMode mode)
        {
            /*
            BMI  Branch on Result Minus
                    branch on N = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BMI oper      30    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('N') == 1)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BNE(InstructionMode mode)
        {
            /*
            BNE  Branch on Result not Zero
                    branch on Z = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BNE oper      D0    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('Z') == 0)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BPL(InstructionMode mode)
        {
            /*
            BPL  Branch on Result Plus
                    branch on N = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BPL oper      10    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('N') == 0)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BRK(InstructionMode mode)
        {
            /*
            BRK  Force Break
                    interrupt,                       N Z C I D V
                    push PC+2, push SR               - - - 1 - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       BRK           00    1     7;
            */

            this.incrPc(); this.incrPc();
            this.push(this.pc[0]);
            this.push(this.pc[1]);
            this.push((byte)(this.sr | 0b0011_0000));

            this.setFlag('I', 1);
            this.setPc(this[this.irqVector[0]], this[this.irqVector[1]]);

            if (mode == InstructionMode.impl)
            {
                return 7;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BVC(InstructionMode mode)
        {
            /*
            BVC  Branch on Overflow Clear
                    branch on V = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BVC oper      50    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('V') == 0)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BVS(InstructionMode mode)
        {
            /*
            BVS  Branch on Overflow Set
                    branch on V = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BVC oper      70    2     2**
            */

            if (mode == InstructionMode.rel)
            {
                if (this.getFlag('V') == 1)
                {
                    if (this.branch() != 0)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLC(InstructionMode mode)
        {
            /*
            CLC  Clear Carry Flag
                    0 -> C                           N Z C I D V
                                                    - - 0 - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLC           18    1     2;
            */

            this.setFlag('C', (byte)0);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLD(InstructionMode mode)
        {
            /*
            CLD  Clear Decimal Mode
                    0 -> D                           N Z C I D V
                                                    - - - - 0 -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLD           D8    1     2;
            */

            this.setFlag('D', (byte)0);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLI(InstructionMode mode)
        {
            /*
            CLI  Clear Interrupt Disable Bit
                    0 -> I                           N Z C I D V
                                                    - - - 0 - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLI           58    1     2;
            */

            this.setFlag('I', (byte)0);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLV(InstructionMode mode)
        {
            /*
            CLV  Clear Overflow Flag
                    0 -> V                           N Z C I D V
                                                    - - - - - 0;
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLV           B8    1     2;
            */

            this.setFlag('V', (byte)0);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CMP(InstructionMode mode)
        {
            /*
            CMP  Compare Memory with Accumulator
                    A - M                            N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     CMP #oper     C9    2     2;
                    zeropage      CMP oper      C5    2     3;
                    zeropage,X    CMP oper,X    D5    2     4;
                    absolute      CMP oper      CD    3     4;
                    absolute,X    CMP oper,X    DD    3     4*
                    absolute,Y    CMP oper,Y    D9    3     4*
                    (indirect,X)  CMP (oper,X)  C1    2     6;
                    (indirect),Y  CMP (oper),Y  D1    2     5*
            */

            this.setFlag('C', (byte)(this.ac >= this[this.oper] ? 1 : 0));
            this.setFlag('Z', (byte)(this.ac == this[this.oper] ? 1 : 0));
            this.setFlag('N', (byte)(((byte)(this.ac - this[this.oper]) >= 128) ? 1 : 0));

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int CPX(InstructionMode mode)
        {
            /*
            CPX  Compare Memory && Index X
                    X - M                            N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     CPX #oper     E0    2     2;
                    zeropage      CPX oper      E4    2     3;
                    absolute      CPX oper      EC    3     4;
            */

            this.setFlag('C', (byte)(this.x >= this[this.oper] ? 1 : 0));
            this.setFlag('Z', (byte)(this.x == this[this.oper] ? 1 : 0));
            this.setFlag('N', (byte)(((byte)(this.x - this[this.oper]) >= 128) ? 1 : 0));

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.abs: return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int CPY(InstructionMode mode)
        {
            /*
            CPY  Compare Memory && Index Y
                    Y - M                            N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     CPY #oper     C0    2     2;
                    zeropage      CPY oper      C4    2     3;
                    absolute      CPY oper      CC    3     4;
            */

            this.setFlag('C', (byte)(this.y >= this[this.oper] ? 1 : 0));
            this.setFlag('Z', (byte)(this.y == this[this.oper] ? 1 : 0));
            this.setFlag('N', (byte)(((byte)(this.y - this[this.oper]) >= 128) ? 1 : 0));

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.abs: return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int DEC(InstructionMode mode)
        {
            /*
            DEC  Decrement Memory by One
                    M - 1 -> M                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      DEC oper      C6    2     5;
                    zeropage,X    DEC oper,X    D6    2     6;
                    absolute      DEC oper      CE    3     6;
                    absolute,X    DEC oper,X    DE    3     7;
            */

            this[this.oper]--;
            this.setNZ(this[this.oper]);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int DEX(InstructionMode mode)
        {
            /*
            DEX  Decrement Index X by One
                    X - 1 -> X                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       DEC           CA    1     2;
            */

            this.x--;
            this.setNZ(this.x);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int DEY(InstructionMode mode)
        {
            /*
            DEY  Decrement Index Y by One
                    Y - 1 -> Y                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       DEC           88    1     2;
            */

            this.y--;
            this.setNZ(this.y);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int EOR(InstructionMode mode)
        {
            /*
            EOR  Exclusive-OR Memory with Accumulator
                    A EOR M -> A                     N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     EOR #oper     49    2     2;
                    zeropage      EOR oper      45    2     3;
                    zeropage,X    EOR oper,X    55    2     4;
                    absolute      EOR oper      4D    3     4;
                    absolute,X    EOR oper,X    5D    3     4*
                    absolute,Y    EOR oper,Y    59    3     4*
                    (indirect,X)  EOR (oper,X)  41    2     6;
                    (indirect),Y  EOR (oper),Y  51    2     5*
            */

            this.ac ^= this[this.oper];
            this.setNZ(this.ac);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int INC(InstructionMode mode)
        {
            /*
            INC  Increment Memory by One
                    M + 1 -> M                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      INC oper      E6    2     5;
                    zeropage,X    INC oper,X    F6    2     6;
                    absolute      INC oper      EE    3     6;
                    absolute,X    INC oper,X    FE    3     7;
            */

            this[this.oper]++;
            this.setNZ(this[this.oper]);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int INX(InstructionMode mode)
        {
            /*
            INX  Increment Index X by One
                    X + 1 -> X                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       INX           E8    1     2;
            */

            this.x++;
            this.setNZ(this.x);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int INY(InstructionMode mode)
        {
            /*
            INY  Increment Index Y by One
                    Y + 1 -> Y                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       INY           C8    1     2;
            */

            this.y++;
            this.setNZ(this.y);

            if (mode == InstructionMode.impl)

            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int JMP(InstructionMode mode)
        {
            /*
            JMP  Jump to New Location
                (PC+1) -> PCL                    N Z C I D V
                (PC+2) -> PCH                    - - - - - -
                addressing    assembler    opc  bytes  cyles
                --------------------------------------------
                absolute      JMP oper      4C    3     3
                indirect      JMP (oper)    6C    3     5
            */
            switch (mode)
            {
                case InstructionMode.abs:
                    {
                        this.setPc((byte)(this.oper & 0xff), (byte)(this.oper >> 8));
                        return 3;
                    }
                case InstructionMode.ind:
                    {
                        // indirect wraps around with the lower byte. This is a glitch/feature in the MOS6502 processor
                        this.setPc(
                                this[this.oper],
                                this[(this.oper >> 8) * 0x100 + (((this.oper & 0xff) + 1) % 0x100)]
                        );
                        return 5;
                    }
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            }
        }

        private int JSR (InstructionMode mode)
        {
            /*
            JSR  Jump to New Location Saving Return Address
                push (PC+2),                     N Z C I D V
                (PC+1) -> PCL                    - - - - - -
                (PC+2) -> PCH
                addressing    assembler    opc  bytes  cyles
                --------------------------------------------
                absolute      JSR oper      20    3     6
            */
            byte v = (byte)(this.pc[1] - 1 < 0 ? -1 : 0);
            this.push((byte)(this.pc[0] + v));
            this.push((byte)(this.pc[1] - 1));

            this.setPc((byte)(this.oper & 0xff), (byte)(this.oper >> 8));
            return 6;
        }

        private int LDA(InstructionMode mode)
        {
            /*
            LDA  Load Accumulator with Memory
                    M -> A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     LDA #oper     A9    2     2;
                    zeropage      LDA oper      A5    2     3;
                    zeropage,X    LDA oper,X    B5    2     4;
                    absolute      LDA oper      AD    3     4;
                    absolute,X    LDA oper,X    BD    3     4*
                    absolute,Y    LDA oper,Y    B9    3     4*
                    (indirect,X)  LDA (oper,X)  A1    2     6;
                    (indirect),Y  LDA (oper),Y  B1    2     5*
            */

            this.ac = this[this.oper];
            this.setNZ(this.ac);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LDX(InstructionMode mode)
        {
            /*
            LDX  Load Index X with Memory
                    M -> X                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     LDX #oper     A2    2     2;
                    zeropage      LDX oper      A6    2     3;
                    zeropage,Y    LDX oper,Y    B6    2     4;
                    absolute      LDX oper      AE    3     4;
                    absolute,Y    LDX oper,Y    BE    3     4*
            */

            this.x = this[this.oper];
            this.setNZ(this.x);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgY: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LDY(InstructionMode mode)
        {
            /*
            LDY  Load Index Y with Memory
                    M -> Y                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     LDY #oper     A0    2     2;
                    zeropage      LDY oper      A4    2     3;
                    zeropage,X    LDY oper,X    B4    2     4;
                    absolute      LDY oper      AC    3     4;
                    absolute,X    LDY oper,X    BC    3     4*
            */

            this.y = this[this.oper];
            this.setNZ(this.y);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LSR(InstructionMode mode)
        {
            /*
            LSR  Shift One Bit Right (Memory || Accumulator);
                0 -> [76543210] -> C             N Z C I D V
                                                0 + + - - -
                addressing    assembler    opc  bytes  cyles
                --------------------------------------------
                accumulator   LSR A         4A    1     2;
                zeropage      LSR oper      46    2     5;
                zeropage,X    LSR oper,X    56    2     6;
                absolute      LSR oper      4E    3     6;
                absolute,X    LSR oper,X    5E    3     7;
            */

            byte c = (byte)(this[this.oper] & 0x01);
            this[this.oper] >>= 1;
            this.setNZ(this[this.oper]);
            this.setFlag('C', c);

            switch (mode)
            {
                case InstructionMode.A: return 2;
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int NOP(InstructionMode mode)
        {
            /*
            NOP  No Operation
                    ---                              N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       NOP           EA    1     2;
            */

            switch (mode)
            {
                case InstructionMode.impl: return 2;
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.zpgY: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ORA(InstructionMode mode)
        {
            /*
            ORA  OR Memory with Accumulator
                    A OR M -> A                      N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     ORA #oper     09    2     2;
                    zeropage      ORA oper      05    2     3;
                    zeropage,X    ORA oper,X    15    2     4;
                    absolute      ORA oper      0D    3     4;
                    absolute,X    ORA oper,X    1D    3     4*
                    absolute,Y    ORA oper,Y    19    3     4*
                    (indirect,X)  ORA (oper,X)  01    2     6;
                    (indirect),Y  ORA (oper),Y  11    2     5*
            */

            this.ac |= this[this.oper];
            this.setNZ(this.ac);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int PHA(InstructionMode mode)
        {
            /*
            PHA  Push Accumulator on Stack
                    push A                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PHA           48    1     3;
            */

            this.push(this.ac);

            if (mode == InstructionMode.impl)
            {
                return 3;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int PHP(InstructionMode mode)
        {
            /*
            PHP  Push Processor Status on Stack
                    push SR                          N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PHP           08    1     3;
            */

            this.push((byte)(this.sr | 0b0011_0000));

            if (mode == InstructionMode.impl)
            {
                return 3;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int PLA(InstructionMode mode)
        {
            /*
            PLA  Pull Accumulator from Stack
                    pull A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PLA           68    1     4;
            */

            this.ac = this.pull();
            this.setNZ(this.ac);

            if (mode == InstructionMode.impl)
            {
                return 4;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int PLP(InstructionMode mode)
        {
            /*
            PLP  Pull Processor Status from Stack
                    pull SR                          N Z C I D V
                                                    from stack
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PLP           28    1     4;
            */

            this.sr = (byte)((this.pull() | 0x20) & 0xef);

            if (mode == InstructionMode.impl)
            {
                return 4;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int ROL(InstructionMode mode)
        {
            /*
            ROL  Rotate One Bit Left (Memory || Accumulator);
                    C <- [76543210] <- C             N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    accumulator   ROL A         2A    1     2;
                    zeropage      ROL oper      26    2     5;
                    zeropage,X    ROL oper,X    36    2     6;
                    absolute      ROL oper      2E    3     6;
                    absolute,X    ROL oper,X    3E    3     7;
            */

            byte c = this.getFlag('C');
            this.setFlag('C', (byte)(this[this.oper] >> 7));
            this[this.oper] <<= 1;
            this[this.oper] += c;
            this.setNZ(this[this.oper]);

            switch (mode)
            {
                case InstructionMode.A: return 2;
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ROR(InstructionMode mode)
        {
            /*
            ROR  Rotate One Bit Right (Memory || Accumulator);
                    C -> [76543210] -> C             N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    accumulator   ROR A         6A    1     2;
                    zeropage      ROR oper      66    2     5;
                    zeropage,X    ROR oper,X    76    2     6;
                    absolute      ROR oper      6E    3     6;
                    absolute,X    ROR oper,X    7E    3     7;
            */

            byte c = (byte)(this[this.oper] & 0x01);
            this[this.oper] >>= 1;
            this[this.oper] = (byte)(this[this.oper] + (this.getFlag('C') << 7));
            this.setFlag('C', c);
            this.setNZ(this[this.oper]);

            switch (mode)
            {
                case InstructionMode.A: return 2;
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int RTI(InstructionMode mode)
        {
            /*
                RTI  Return from Interrupt
                    pull SR, pull PC                 N Z C I D V
                                                    from stack
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       RTI           40    1     6;
                */

            this.sr = (byte)((this.pull() | 0x20) & 0xef);
            this.pc[1] = this.pull();
            this.pc[0] =this.pull();

            if (mode == InstructionMode.impl)
            {
                return 6;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");

        }

        private int RTS(InstructionMode mode)
        {
            /*
            RTS  Return from Subroutine
                    pull PC, PC+1 -> PC              N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       RTS           60    1     6;
            */

            this.pc[1] = this.pull();
            this.pc[0] = this.pull();

            this.incrPc();

            if (mode == InstructionMode.impl)
            {
                return 6;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SBC(InstructionMode mode)
        {
            /*
            SBC  Subtract Memory from Accumulator with Borrow
                    A - M - C -> A                   N Z C I D V
                                                    + + + - - +
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     SBC #oper     E9    2     2;
                    zeropage      SBC oper      E5    2     3;
                    zeropage,X    SBC oper,X    F5    2     4;
                    absolute      SBC oper      ED    3     4;
                    absolute,X    SBC oper,X    FD    3     4*
                    absolute,Y    SBC oper,Y    F9    3     4*
                    (indirect,X)  SBC (oper,X)  E1    2     6;
                    (indirect),Y  SBC (oper),Y  F1    2     5*
            */
            this.oper =  -0x200 - (this[this.oper] ^ 0xff);
            this.ADC(mode);

            switch (mode)
            {
                case InstructionMode.imm: return 2;
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 4 + pageChange;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SEC(InstructionMode mode)
        {
            /*
            SEC  Set Carry Flag
                    1 -> C                           N Z C I D V
                                                    - - 1 - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       SEC           38    1     2;
            */

            this.setFlag('C', (byte)1);

            switch (mode)
            {
                case InstructionMode.impl: return 2;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SED(InstructionMode mode)
        {
            /*
            SED  Set Decimal Flag
                    1 -> D                           N Z C I D V
                                                    - - - - 1 -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       SED           F8    1     2;
            */

            this.setFlag('D', (byte)1);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SEI(InstructionMode mode)
        {
            /*
            SEI  Set Interrupt Disable Status
                    1 -> I                           N Z C I D V
                                                    - - - 1 - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       SEI           78    1     2;
            */

            this.setFlag('I', (byte)1);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int STA(InstructionMode mode)
        {
            /*
            STA  Store Accumulator in Memory
                    A -> M                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      STA oper      85    2     3;
                    zeropage,X    STA oper,X    95    2     4;
                    absolute      STA oper      8D    3     4;
                    absolute,X    STA oper,X    9D    3     5;
                    absolute,Y    STA oper,Y    99    3     5;
                    (indirect,X)  STA (oper,X)  81    2     6;
                    (indirect),Y  STA (oper),Y  91    2     6;
            */

            this[this.oper] = this.ac;

            switch (mode)
            {
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absX: return 5;
                case InstructionMode.absY: return 5;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 6;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int STX(InstructionMode mode)
        {
            /*
            STX  Store Index X in Memory
                    X -> M                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      STX oper      86    2     3;
                    zeropage,Y    STX oper,Y    96    2     4;
                    absolute      STX oper      8E    3     4;
            */

            this[this.oper] = this.x;

            switch (mode)
            {
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgY: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int STY(InstructionMode mode)
        {
            /*
            STY  Sore Index Y in Memory
                    Y -> M                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      STY oper      84    2     3;
                    zeropage,X    STY oper,X    94    2     4;
                    absolute      STY oper      8C    3     4;
            */

            this[this.oper] = this.y;

            switch (mode)
            {
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.abs: return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int TAX(InstructionMode mode)
        {
            /*
            TAX  Transfer Accumulator to Index X
                    A -> X                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TAX           AA    1     2;
            */

            this.x = this.ac;
            this.setNZ(this.x);

            switch (mode)
            {
                case InstructionMode.impl: return 2;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int TAY(InstructionMode mode)
        {
            /*
            TAY  Transfer Accumulator to Index Y
                    A -> Y                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TAY           A8    1     2;
            */

            this.y = this.ac;
            this.setNZ(this.y);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int TSX(InstructionMode mode)
        {
            /*
            TSX  Transfer Stack Pointer to Index X
                    SP -> X                          N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TSX           BA    1     2;
            */

            this.x = this.sp;
            this.setNZ(this.x);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int TXA(InstructionMode mode)
        {
            /*
            TXA  Transfer Index X to Accumulator
                    X -> A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TXA           8A    1     2;
            */

            this.ac = this.x;
            this.setNZ(this.ac);

            switch (mode)
            {
                case InstructionMode.impl: return 2;
                case InstructionMode.intern: return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int TXS(InstructionMode mode)
        {
            /*
            TXS  Transfer Index X to Stack Register
                    X -> SP                          N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TXS           9A    1     2;
            */

            this.sp = this.x;

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int TYA(InstructionMode mode)
        {
            /*
            TYA  Transfer Index Y to Accumulator
                    Y -> A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TYA           98    1     2;
            */

            this.ac = this.y;
            this.setNZ(this.ac);

            if (mode == InstructionMode.impl)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SLO(InstructionMode mode)
        {
            /*
            ASO    ***    (SLO);
            This opcode ASLs the contents of a memory location && then ORs the result
            with the accumulator.

            Supported modes:

            ASO abcd        ;0F cd ab    ;No. Cycles= 6;
            ASO abcd,X      ;1F cd ab    ;            7;
            ASO abcd,Y      ;1B cd ab    ;            7;
            ASO ab          ;07 ab       ;            5;
            ASO ab,X        ;17 ab       ;            6;
            ASO (ab,X)      ;03 ab       ;            8;
            ASO (ab),Y      ;13 ab       ;            8;

            (Sub-instructions: ORA, ASL);

            Here is an example of how you might use this opcode:

            ASO $C010       ;0F 10 C0;

            Here is the same code using equivalent instructions.

            ASL $C010;
            ORA $C010;
            */

            this.ASL(InstructionMode.intern);
            this.ORA(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.absY: return 7;
                case InstructionMode.Xind: return 8;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int RLA(InstructionMode mode)
        {
            /*
            RLA    ***
            RLA ROLs the contents of a memory location && then ANDs the result with
            the accumulator.

            Supported modes:

            RLA abcd        ;2F cd ab    ;No. Cycles= 6;
            RLA abcd,X      ;3F cd ab    ;            7;
            RLA abcd,Y      ;3B cd ab    ;            7;
            RLA ab          ;27 ab       ;            5;
            RLA ab,X        ;37 ab       ;            6;
            RLA (ab,X)      ;23 ab       ;            8;
            RLA (ab),Y      ;33 ab       ;            8;

            (Sub-instructions: AND, ROL);

            Here's an example of how you might write it in a program.

            RLA $FC,X       ;37 FC

            Here's the same code using equivalent instructions.

            ROL $FC,X
            AND $FC,X
            */

            this.ROL(InstructionMode.intern);
            this.AND(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.absY: return 7;
                case InstructionMode.Xind: return 8;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SRE(InstructionMode mode)
        {
            /*
            LSE    ***   (SRE);
            LSE LSRs the contents of a memory location && then EORs the result with
            the accumulator.

            Supported modes:

            LSE abcd        ;4F cd ab    ;No. Cycles= 6;
            LSE abcd,X      ;5F cd ab    ;            7;
            LSE abcd,Y      ;5B cd ab    ;            7;
            LSE ab          ;47 ab       ;            5;
            LSE ab,X        ;57 ab       ;            6;
            LSE (ab,X)      ;43 ab       ;            8;
            LSE (ab),Y      ;53 ab       ;            8;

            (Sub-instructions: EOR, LSR);

            Example:

            LSE $C100,X     ;5F 00 C1;

            Here's the same code using equivalent instructions.

            LSR $C100,X
            EOR $C100,X
            */

            this.LSR(InstructionMode.intern);
            this.EOR(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.absY: return 7;
                case InstructionMode.Xind: return 8;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int RRA(InstructionMode mode)
        {
            /*
            RRA    ***
            RRA RORs the contents of a memory location && then ADCs the result with
            the accumulator.

            Supported modes:

            RRA abcd        ;6F cd ab    ;No. Cycles= 6;
            RRA abcd,X      ;7F cd ab    ;            7;
            RRA abcd,Y      ;7B cd ab    ;            7;
            RRA ab          ;67 ab       ;            5;
            RRA ab,X        ;77 ab       ;            6;
            RRA (ab,X)      ;63 ab       ;            8;
            RRA (ab),Y      ;73 ab       ;            8;

            (Sub-instructions: ADC, ROR);

            Example:

            RRA $030C       ;6F 0C 03;

            Equivalent instructions:

            ROR $030C
            ADC $030C
            */

            this.ROR(InstructionMode.intern);
            this.ADC(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.absY: return 7;
                case InstructionMode.Xind: return 8;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SAX(InstructionMode mode)
        {
            /*
            AXS    ***    (SAX);
            AXS ANDs the contents of the A && X registers (without changing the
            contents of either register) && stores the result in memory.
            AXS does not affect any flags in the processor status register.

            Supported modes:

            AXS abcd        ;8F cd ab    ;No. Cycles= 4;
            AXS ab          ;87 ab       ;            3;
            AXS ab,Y        ;97 ab       ;            4;
            AXS (ab,X)      ;83 ab       ;            6;

            (Sub-instructions: STA, STX);

            Example:

            AXS $FE         ;87 FE

            Here's the same code using equivalent instructions.

            STX $FE
            PHA
            AND $FE
            STA $FE
            PLA
            */

            this[this.oper] = (byte)(this.ac & this.x);

            switch (mode)
            {
                case InstructionMode.zpg: return 4;
                case InstructionMode.zpgY: return 3;
                case InstructionMode.abs: return 4;
                case InstructionMode.Xind: return 6;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LAX(InstructionMode mode)
        {
            /*
            LAX    ***
            This opcode loads both the accumulator && the X register with the contents
            of a memory location.

            Supported modes:

            LAX abcd        ;AF cd ab    ;No. Cycles= 4;
            LAX abcd,Y      ;BF cd ab    ;            4*
            LAX ab          ;A7 ab       ;*=add 1     3;
            LAX ab,Y        ;B7 ab       ;if page     4;
            LAX (ab,X)      ;A3 ab       ;boundary    6;
            LAX (ab),Y      ;B3 ab       ;is crossed  5*

            (Sub-instructions: LDA, LDX);

            Example:

            LAX $8400,Y     ;BF 00 84;

            Equivalent instructions:

            LDA $8400,Y
            LDX $8400,Y
            */

            this.LDA(InstructionMode.intern);
            this.LDX(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgY: return 4;
                case InstructionMode.abs: return 4;
                case InstructionMode.absY: return 4 + pageChange;
                case InstructionMode.Xind: return 6;
                case InstructionMode.indY: return 5 + pageChange;
                case InstructionMode.imm: return 2;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int DCP(InstructionMode mode)
        {
            /*
            DCM    ***    (DCP);
            This opcode DECs the contents of a memory location && then CMPs the result
            with the A register.

            Supported modes:

            DCM abcd        ;CF cd ab    ;No. Cycles= 6;
            DCM abcd,X      ;DF cd ab    ;            7;
            DCM abcd,Y      ;DB cd ab    ;            7;
            DCM ab          ;C7 ab       ;            5;
            DCM ab,X        ;D7 ab       ;            6;
            DCM (ab,X)      ;C3 ab       ;            8;
            DCM (ab),Y      ;D3 ab       ;            8;

            (Sub-instructions: CMP, DEC);

            Example:

            DCM $FF         ;C7 FF

            Equivalent instructions:

            DEC $FF
            CMP $FF
            */

            this.DEC(InstructionMode.intern);
            this.CMP(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.absY: return 7;
                case InstructionMode.Xind: return 8;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ISC(InstructionMode mode)
        {
            /*
            INS    ***    (ISC);
            This opcode INCs the contents of a memory location && then SBCs the result
            from the A register.

            Supported modes:

            INS abcd        ;EF cd ab    ;No. Cycles= 6;
            INS abcd,X      ;FF cd ab    ;            7;
            INS abcd,Y      ;FB cd ab    ;            7;
            INS ab          ;E7 ab       ;            5;
            INS ab,X        ;F7 ab       ;            6;
            INS (ab,X)      ;E3 ab       ;            8;
            INS (ab),Y      ;F3 ab       ;            8;

            (Sub-instructions: SBC, INC);

            Example:

            INS $FF         ;E7 FF

            Equivalent instructions:

            INC $FF
            SBC $FF
            */

            this.INC(InstructionMode.intern);
            this.SBC(InstructionMode.intern);

            switch (mode)
            {
                case InstructionMode.zpg: return 5;
                case InstructionMode.zpgX: return 6;
                case InstructionMode.abs: return 6;
                case InstructionMode.absX: return 7;
                case InstructionMode.absY: return 7;
                case InstructionMode.Xind: return 8;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ALR(InstructionMode mode)
        {
            /*
            ALR    ***
            This opcode ANDs the contents of the A register with an immediate value and
            then LSRs the result.

            One supported mode:

            ALR #ab         ;4B ab       ;No. Cycles= 2;

            Example:

            ALR #$FE        ;4B FE

            Equivalent instructions:

            AND #$FE
            LSR A
            */

            this.AND(InstructionMode.intern);
            this.oper = -0x100;
            this.LSR(InstructionMode.A);

            if (mode == InstructionMode.imm)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int ARR(InstructionMode mode)
        {
            /*
            ARR    ***
            This opcode ANDs the contents of the A register with an immediate value and
            then RORs the result.

            One supported mode:

            ARR #ab         ;6B ab       ;No. Cycles= 2;

            Here's an example of how you might write it in a program.

            ARR #$7F        ;6B 7F

            Here's the same code using equivalent instructions.

            AND #$7F
            ROR A
            */

            this.AND(InstructionMode.intern);
            this.oper = -0x100;
            this.ROR(InstructionMode.A);

            if (mode == InstructionMode.imm)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int XAA(InstructionMode mode)
        {
            /*
            XAA    ***
            XAA transfers the contents of the X register to the A register && then
            ANDs the A register with an immediate value.

            One supported mode:

            XAA #ab         ;8B ab       ;No. Cycles= 2;

            Example:

            XAA #$44        ;8B 44;

            Equivalent instructions:

            TXA
            AND #$44;
            */
            
            this.TXA(InstructionMode.impl);
            this.AND(InstructionMode.imm);

            if (mode == InstructionMode.imm)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int OAL(InstructionMode mode)
        {
            /*
            OAL    ***
            This opcode ORs the A register with #$EE, ANDs the result with an immediate
            value, && then stores the result in both A && X.

            One supported mode:

            OAL #ab         ;AB ab       ;No. Cycles= 2;

            Here's an example of how you might use this opcode:

            OAL #$AA        ;AB AA

            Here's the same code using equivalent instructions:

            ORA #$EE
            AND #$AA
            TAX
            */

            this.ac |= 0xee;
            this.ac &= (byte)(-this.oper - 0x200);
            this.x = this.ac;

            if (mode == InstructionMode.imm)
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SKB(InstructionMode mode)
        {
            /*
            SKB stands for skip next byte.
            Opcodes: 80, 82, C2, E2, 04, 14, 34, 44, 54, 64, 74, D4, F4.
            Takes 2, 3, || 4 cycles to execute.
            */

            this.incrPc();

            switch (mode)
            {
                case InstructionMode.zpg: return 3;
                case InstructionMode.zpgX: return 4;
                case InstructionMode.imm: return 6;
                case InstructionMode.indY: return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SKW(InstructionMode mode)
        {
            /*
            SKW skips next word (two bytes).
            Opcodes: 0C, 1C, 3C, 5C, 7C, DC, FC.
            Takes 4 cycles to execute.
            */

            this.incrPc(2);

            return 4;
        }
    }
}
