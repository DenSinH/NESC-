table = """
SLO	 	 	$07	$17	 	$03	$13	$0F	$1F	$1B	 	 	{adr}:={adr}*2 A:=A or {adr}	*	 	 	 	 	*	*
RLA	 	 	$27	$37	 	$23	$33	$2F	$3F	$3B	 	 	{adr}:={adr}rol A:=A and {adr}	*	 	 	 	 	*	*
SRE	 	 	$47	$57	 	$43	$53	$4F	$5F	$5B	 	 	{adr}:={adr}/2 A:=A exor {adr}	*	 	 	 	 	*	*
RRA	 	 	$67	$77	 	$63	$73	$6F	$7F	$7B	 	 	{adr}:={adr}ror A:=A adc {adr}	*	*	 	 	 	*	*
SAX	 	 	$87	 	$97	$83	 	$8F	 	 	 	 	{adr}:=A&X	 	 	 	 	 	 	 
LAX	 	 	$A7	 	$B7	$A3	$B3	$AF	 	$BF	 	 	A,X:={adr}	*	 	 	 	 	*	 
DCP	 	 	$C7	$D7	 	$C3	$D3	$CF	$DF	$DB	 	 	{adr}:={adr}-1 A-{adr}	*	 	 	 	 	*	*
ISC	 	 	$E7	$F7	 	$E3	$F3	$EF	$FF	$FB	 	 	{adr}:={adr}+1 A:=A-{adr}	*	*	 	 	 	*	*
ANC	 	$0B	 	 	 	 	 	 	 	 	 	 	A:=A&#{imm}	*	 	 	 	 	*	*
ANC	 	$2B	 	 	 	 	 	 	 	 	 	 	A:=A&#{imm}	*	 	 	 	 	*	*
ALR	 	$4B	 	 	 	 	 	 	 	 	 	 	A:=(A&#{imm})/2	*	 	 	 	 	*	*
ARR	 	$6B	 	 	 	 	 	 	 	 	 	 	A:=(A&#{imm})/2	*	*	 	 	 	*	*
XAA²	 	$8B	 	 	 	 	 	 	 	 	 	 	A:=X&#{imm}	*	 	 	 	 	*	 
LAX²	 	$AB	 	 	 	 	 	 	 	 	 	 	A,X:=#{imm}	*	 	 	 	 	*	 
AXS	 	$CB	 	 	 	 	 	 	 	 	 	 	X:=A&X-#{imm}	*	 	 	 	 	*	*
SBC	 	$EB	 	 	 	 	 	 	 	 	 	 	A:=A-#{imm}	*	*	 	 	 	*	*
AHX¹	 	 	 	 	 	 	$93	 	 	$9F	 	 	{adr}:=A&X&H	 	 	 	 	 	 	 
SHY¹	 	 	 	 	 	 	 	 	$9C	 	 	 	{adr}:=Y&H	 	 	 	 	 	 	 
SHX¹	 	 	 	 	 	 	 	 	 	$9E	 	 	{adr}:=X&H	 	 	 	 	 	 	 
TAS¹	 	 	 	 	 	 	 	 	 	$9B	 	 	S:=A&X {adr}:=S&H	 	 	 	 	 	 	 
LAS	 	 	 	 	 	 	 	 	 	$BB	 	 	A,X,S:={adr}&S	*	 	 	 	 	*	 
"""

modes = "imp # zpg zpg,X zpg,Y X,ind ind,Y abs abs,X abs,Y ind rel".split(" ")

for line in table.split("\n")[1:-1]:
    instruction, *opcodes = line.split("\t")[:len(modes) + 1]

    for i in range(len(opcodes)):
        opcode = opcodes[i]
        if not opcode.strip():
            continue
        mode = modes[i]
        print(f'    "{"0x" + opcode[1:].lower().lstrip("0")}": "{instruction} {mode}",')
    print()
