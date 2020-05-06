using System;

namespace NesEmulator
{
    public enum MirrorType
    {
        Horizontal,
        Vertical,
        SingleScreen,
        FourScreen
    }

    public enum InstructionMode
    {
        A,
        abs,
        absX,
        absY,
        imm,
        impl,
        ind,
        Xind,
        indY,
        rel,
        zpg,
        zpgX,
        zpgY,
        intern
    }

    public class InstructionCaller
    {
        // wrapper for an instruction with mode

        private Func<InstructionMode, int> method;
        public InstructionMode mode;
        /*
        Modes:
        0: A        ....	Accumulator	 	        OPC A	 	    operand is AC (implied single byte instruction)
        1: abs      ....	absolute	 	        OPC $LLHH	 	operand is address $HHLL *
        2: abs,X    ....	absolute, X-indexed	 	OPC $LLHH,X	 	operand is address; effective address is address incremented by X with carry **
        3: abs,Y    ....	absolute, Y-indexed	 	OPC $LLHH,Y	 	operand is address; effective address is address incremented by Y with carry **
        4: #	    ....	immediate	 	        OPC #$BB	 	operand is byte BB
        5: impl	    ....	implied	 	            OPC	 	        operand implied
        6: ind	    ....	indirect	 	        OPC ($LLHH)	 	operand is address; effective address is contents of word at address: C.w($HHLL)
        7: X,ind    ....	X-indexed, indirect	 	OPC ($LL,X)	 	operand is zeropage address; effective address is word in (LL + X, LL + X + 1), inc. without carry: C.w($00LL + X)
        8: ind,Y    ....	indirect, Y-indexed	 	OPC ($LL),Y	 	operand is zeropage address; effective address is word in (LL, LL + 1) incremented by Y with carry: C.w($00LL) + Y
        9: rel      ....	relative	 	        OPC $BB	 	    branch target is PC + signed offset BB ***
        10: zpg     ....	zeropage	 	        OPC $LL	 	    operand is zeropage address (hi-byte is zero, address = $00LL)
        11: zpg,X   ....	zeropage, X-indexed	 	OPC $LL,X	 	operand is zeropage address; effective address is address incremented by X without carry **
        12: zpg,Y   ....	zeropage, Y-indexed	 	OPC $LL,Y	 	operand is zeropage address; effective address is address incremented by Y without carry **
        
        13: internal
        */

        public InstructionCaller(Func<InstructionMode, int> method, InstructionMode mode)
        {
            this.method = method;
            this.mode = mode;
        }

        public int call()
        {
            return this.method(this.mode);
        }

        public string getName()
        {
            return this.method.Method.Name;
        }

    }
}
