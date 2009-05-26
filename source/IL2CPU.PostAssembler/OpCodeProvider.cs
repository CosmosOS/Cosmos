using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
 // http://sandpile.org/aa64/index.htm


    [Flags]
    public enum OperandType { None =0, Label = 1, R8 =2 , R16 = 4 , R32 = 8 , R64 = 16 , M8 =32, };

    struct X86Instructions
    {
        ushort LookupIndex;
        string OpCode;
      //  InstructionType Type;
        int NumberOfOperands; 
        OperandType  ValidOperand1Types;
        OperandType  ValidOperand2Types;
        OperandType ValidOperand3Types;
        OperandType ValidOperand4Types; 
        string Description;
    }

    //TODO replace op code map . 
    internal class OpCodeProvider
    {
//        AAA             37               ASCII adjust AL after addition
//AAD             D5 0A            ASCII adjust AX before division
//(No mnemonic)   D5 ib            Adjust AX before division to number base imm8
//AAM             D4 0A            ASCII adjust AX after multiply
//(No mnemonic)   D4 ib            Adjust AX after multiply to number base imm8
//AAS             3F               ASCII adjust AL after subtraction
//ADC             14 ib            AL,imm8 Add with carry imm8 to AL
//ADC             15 iw            AX,imm16 Add with carry imm16 to AX
//ADC             15 id            EAX,imm32 Add with carry imm32 to EAX
//ADC             80 /2 ib         r/m8,imm8 Add with carry imm8 to r/m8
//ADC             81 /2 iw         r/m16,imm16 Add with carry imm16 to r/m16
//ADC             81 /2 id         r/m32,imm32 Add with CF imm32 to r/m32
//ADC             83 /2 ib         r/m16,imm8 Add with CF sign-extended imm8 to r/m16
//ADC             83 /2 ib         r/m32,imm8 Add with CF sign-extended imm8 into r/m32
//ADC             10 /r            r/m8,r8 Add with carry byte register to r/m8
//ADC             11 /r            r/m16,r16 Add with carry r16 to r/m16
//ADC             11 /r            r/m32,r32 Add with CF r32 to r/m32
//ADC             12 /r            r8,r/m8 Add with carry r/m8 to byte register
//ADC             13 /r            r16,r/m16 Add with carry r/m16 to r16
//ADC             13 /r            r32,r/m32 Add with CF r/m32 to r32
//ADD             04 ib            AL,imm8 Add imm8 to AL
//ADD             05 iw            AX,imm16 Add imm16 to AX
//ADD             05 id            EAX,imm32 Add imm32 to EAX
//ADD             80 /0 ib         r/m8,imm8 Add imm8 to r/m8
//ADD             81 /0 iw         r/m16,imm16 Add imm16 to r/m16
//ADD             81 /0 id         r/m32,imm32 Add imm32 to r/m32
//ADD             83 /0 ib         r/m16,imm8 Add sign-extended imm8 to r/m16
//ADD             83 /0 ib         r/m32,imm8 Add sign-extended imm8 to r/m32
//ADD             00 /r            r/m8,r8 Add r8 to r/m8
//ADD             01 /r            r/m16,r16 Add r16 to r/m16
//ADD             01 /r            r/m32,r32 Add r32 to r/m32
//ADD             02 /r            r8,r/m8 Add r/m8 to r8
//ADD             03 /r            r16,r/m16 Add r/m16 to r16
//ADD             03 /r            r32,r/m32 Add r/m32 to r32
//ADDPD           66 0F 58 /r      xmm1, xmm2/m128 Add packed double-precision floating-point values from xmm2/m128 to xmm1.
//ADDPS           0F 58 /r         xmm1, xmm2/m128 Add packed single-precision floating-point values from xmm2/m128 to xmm1.
//ADDSD           F2 0F 58 /r      xmm1, xmm2/m64 Add the low double-precision floating-point value from xmm2/m64 to xmm1.
//ADDSS           F3 0F 58 /r      xmm1, xmm2/m32 Add the low single-precision floating-point value from xmm2/m32 to xmm1.
//AND             24 ib            AL,imm8 AL  imm8
//AND             25 iw            AX,imm16 AX  imm16
//AND             25 id            EAX,imm32 EAX  imm32
//AND             80 /4 ib         r/m8,imm8 r/m8  imm8
//AND             81 /4 iw         r/m16,imm16 r/m16  imm16
//AND             81 /4 id         r/m32,imm32 r/m32  imm32
//AND             83 /4 ib         r/m16,imm8 r/m16  imm8 (sign-extended)
//AND             83 /4 ib         r/m32,imm8 r/m32  imm8 (sign-extended)
//AND             20 /r            r/m8,r8 r/m8  r8
//AND             21 /r            r/m16,r16 r/m16  r16
//AND             21 /r            r/m32,r32 r/m32  r32
//AND             22 /r            r8,r/m8 r8  r/m8
//AND             23 /r            r16,r/m16 r16  r/m16
//AND             23 /r            r32,r/m32 r32  r/m32
//ANDPD           66 0F 54 /r      xmm1, xmm2/m128 Bitwise logical AND of xmm2/m128 and xmm1.
//ANDPS           0F 54 /r         xmm1, xmm2/m128 Bitwise logical AND of xmm2/m128 and xmm1.
//ANDNPD          66 0F 55 /r      xmm1, xmm2/m128 Bitwise logical AND NOT of xmm2/m128 and xmm1.
//ANDNPS          0F 55 /r         xmm1, xmm2/m128 Bitwise logical AND NOT of xmm2/m128 and xmm1.
//ARPL            63 /r            r/m16,r16 Adjust RPL of r/m16 to not less than RPL of r16
//BOUND           62 /r            r16, m16&16 Check if r16 (array index) is within bounds specified by m16&16
//BOUND           62 /r            r32, m32&32 Check if r32 (array index) is within bounds specified by m32&32
//BSF             0F BC            r16,r/m16 Bit scan forward on r/m16
//BSF             0F BC            r32,r/m32 Bit scan forward on r/m32
//BSR             0F BD            r16,r/m16 Bit scan reverse on r/m16
//BSR             0F BD            r32,r/m32 Bit scan reverse on r/m32
//BSWAP           0F C8+rd         r32 Reverses the byte order of a 32-bit register.
//BT              0F A3            r/m16,r16 Store selected bit in CF flag
//BT              0F A3            r/m32,r32 Store selected bit in CF flag
//BT              0F BA /4 ib      r/m16,imm8 Store selected bit in CF flag
//BT              0F BA /4 ib      r/m32,imm8 Store selected bit in CF flag
//BTC             0F BB            r/m16,r16 Store selected bit in CF flag and complement
//BTC             0F BB            r/m32,r32 Store selected bit in CF flag and complement
//BTC             0F BA /7 ib      r/m16,imm8 Store selected bit in CF flag and complement
//BTC             0F BA /7 ib      r/m32,imm8 Store selected bit in CF flag and complement
//BTR             0F B3            r/m16,r16 Store selected bit in CF flag and clear
//BTR             0F B3            r/m32,r32 Store selected bit in CF flag and clear
//BTR             0F BA /6 ib      r/m16,imm8 Store selected bit in CF flag and clear
//BTR             0F BA /6 ib      r/m32,imm8 Store selected bit in CF flag and clear
//BTS             0F AB            r/m16,r16 Store selected bit in CF flag and set
//BTS             0F AB            r/m32,r32 Store selected bit in CF flag and set
//BTS             0F BA /5 ib      r/m16,imm8 Store selected bit in CF flag and set
//BTS             0F BA /5 ib      r/m32,imm8 Store selected bit in CF flag and set
//CALL            E8 cw            rel16 Call near, relative, displacement relative to next instruction
//CALL            E8 cd            rel32 Call near, relative, displacement relative to next instruction
//CALL            FF /2            r/m16 Call near, absolute indirect, address given in r/m16
//CALL            FF /2            r/m32 Call near, absolute indirect, address given in r/m32
//CALL            9A cd            ptr16:16 Call far, absolute, address given in operand
//CALL            9A cp            ptr16:32 Call far, absolute, address given in operand
//CALL            FF /3            m16:16 Call far, absolute indirect, address given in m16:16
//CALL            FF /3            m16:32 Call far, absolute indirect, address given in m16:32
//CBW             98               AX . sign-extend of AL
//CWDE            98               EAX . sign-extend of AX
//CLC             F8               Clear CF flag
//CLD             FC               Clear DF flag
//CLFLUSH         0F AE /7         m8 Flushes cache line containing m8.
//CLI             FA               Clear interrupt flag; interrupts disabled when interrupt flag cleared
//CLTS            0F 06            Clears TS flag in CR0
//CMC             F5               Complement CF flag
//CMOVA           0F 47 /r         r16, r/m16 Move if above (CF=0 and ZF=0)
//CMOVA           0F 47 /r         r32, r/m32 Move if above (CF=0 and ZF=0)
//CMOVAE          0F 43 /r         r16, r/m16 Move if above or equal (CF=0)
//CMOVAE          0F 43 /r         r32, r/m32 Move if above or equal (CF=0)
//CMOVB           0F 42 /r         r16, r/m16 Move if below (CF=1)
//CMOVB           0F 42 /r         r32, r/m32 Move if below (CF=1)
//CMOVBE          0F 46 /r         r16, r/m16 Move if below or equal (CF=1 or ZF=1)
//CMOVBE          0F 46 /r         r32, r/m32 Move if below or equal (CF=1 or ZF=1)
//CMOVC           0F 42 /r         r16, r/m16 Move if carry (CF=1)
//CMOVC           0F 42 /r         r32, r/m32 Move if carry (CF=1)
//CMOVE           0F 44 /r         r16, r/m16 Move if equal (ZF=1)
//CMOVE           0F 44 /r         r32, r/m32 Move if equal (ZF=1)
//CMOVG           0F 4F /r         r16, r/m16 Move if greater (ZF=0 and SF=OF)
//CMOVG           0F 4F /r         r32, r/m32 Move if greater (ZF=0 and SF=OF)
//CMOVGE          0F 4D /r         r16, r/m16 Move if greater or equal (SF=OF)
//CMOVGE          0F 4D /r         r32, r/m32 Move if greater or equal (SF=OF)
//CMOVL           0F 4C /r         r16, r/m16 Move if less (SF<>OF)
//CMOVL           0F 4C /r         r32, r/m32 Move if less (SF<>OF)
//CMOVLE          0F 4E /r         r16, r/m16 Move if less or equal (ZF=1 or SF<>OF)
//CMOVLE          0F 4E /r         r32, r/m32 Move if less or equal (ZF=1 or SF<>OF)
//CMOVNA          0F 46 /r         r16, r/m16 Move if not above (CF=1 or ZF=1)
//CMOVNA          0F 46 /r         r32, r/m32 Move if not above (CF=1 or ZF=1)
//CMOVNAE         0F 42 /r         r16, r/m16 Move if not above or equal (CF=1)
//CMOVNAE         0F 42 /r         r32, r/m32 Move if not above or equal (CF=1)
//CMOVNB          0F 43 /r         r16, r/m16 Move if not below (CF=0)
//CMOVNB          0F 43 /r         r32, r/m32 Move if not below (CF=0)
//CMOVNBE         0F 47 /r         r16, r/m16 Move if not below or equal (CF=0 and ZF=0)
//CMOVNBE         0F 47 /r         r32, r/m32 Move if not below or equal (CF=0 and ZF=0)
//CMOVNC          0F 43 /r         r16, r/m16 Move if not carry (CF=0)
//CMOVNC          0F 43 /r         r32, r/m32 Move if not carry (CF=0)
//CMOVNE          0F 45 /r         r16, r/m16 Move if not equal (ZF=0)
//CMOVNE          0F 45 /r         r32, r/m32 Move if not equal (ZF=0)
//CMOVNG          0F 4E /r         r16, r/m16 Move if not greater (ZF=1 or SF<>OF)
//CMOVNG          0F 4E /r         r32, r/m32 Move if not greater (ZF=1 or SF<>OF)
//CMOVNGE         0F 4C /r         r16, r/m16 Move if not greater or equal (SF<>OF)
//CMOVNGE         0F 4C /r         r32, r/m32 Move if not greater or equal (SF<>OF)
//CMOVNL          0F 4D /r         r16, r/m16 Move if not less (SF=OF)
//CMOVNL          0F 4D /r         r32, r/m32 Move if not less (SF=OF)
//CMOVNLE         0F 4F /r         r16, r/m16 Move if not less or equal (ZF=0 and SF=OF)
//CMOVNLE         0F 4F /r         r32, r/m32 Move if not less or equal (ZF=0 and SF=OF)
//CMOVNO          0F 41 /r         r16, r/m16 Move if not overflow (OF=0)
//CMOVNO          0F 41 /r         r32, r/m32 Move if not overflow (OF=0)
//CMOVNP          0F 4B /r         r16, r/m16 Move if not parity (PF=0)
//CMOVNP          0F 4B /r         r32, r/m32 Move if not parity (PF=0)
//CMOVNS          0F 49 /r         r16, r/m16 Move if not sign (SF=0)
//CMOVNS          0F 49 /r         r32, r/m32 Move if not sign (SF=0)
//CMOVNZ          0F 45 /r         r16, r/m16 Move if not zero (ZF=0)
//CMOVNZ          0F 45 /r         r32, r/m32 Move if not zero (ZF=0)
//CMOVO           0F 40 /r         r16, r/m16 Move if overflow (OF=0)
//CMOVO           0F 40 /r         r32, r/m32 Move if overflow (OF=0)
//CMOVP           0F 4A /r         r16, r/m16 Move if parity (PF=1)
//CMOVP           0F 4A /r         r32, r/m32 Move if parity (PF=1)
//CMOVPE          0F 4A /r         r16, r/m16 Move if parity even (PF=1)
//CMOVPE          0F 4A /r         r32, r/m32 Move if parity even (PF=1)
//CMOVPO          0F 4B /r         r16, r/m16 Move if parity odd (PF=0)
//CMOVPO          0F 4B /r         r32, r/m32 Move if parity odd (PF=0)
//CMOVS           0F 48 /r         r16, r/m16 Move if sign (SF=1)
//CMOVS           0F 48 /r         r32, r/m32 Move if sign (SF=1)
//CMOVZ           0F 44 /r         r16, r/m16 Move if zero (ZF=1)
//CMOVZ           0F 44 /r         r32, r/m32 Move if zero (ZF=1)
//CMP             3C ib            AL, imm8 Compare imm8 with AL
//CMP             3D iw            AX, imm16 Compare imm16 with AX
//CMP             3D id            EAX, imm32 Compare imm32 with EAX
//CMP             80 /7 ib         r/m8, imm8 Compare imm8 with r/m8
//CMP             81 /7 iw         r/m16, imm16 Compare imm16 with r/m16
//CMP             81 /7 id         r/m32,imm32 Compare imm32 with r/m32
//CMP             83 /7 ib         r/m16,imm8 Compare imm8 with r/m16
//CMP             83 /7 ib         r/m32,imm8 Compare imm8 with r/m32
//CMP             38 /r            r/m8,r8 Compare r8 with r/m8
//CMP             39 /r            r/m16,r16 Compare r16 with r/m16
//CMP             39 /r            r/m32,r32 Compare r32 with r/m32
//CMP             3A /r            r8,r/m8 Compare r/m8 with r8
//CMP             3B /r            r16,r/m16 Compare r/m16 with r16
//CMP             3B /r            r32,r/m32 Compare r/m32 with r32
//CMPPD           66 0F C2 /r ib   xmm1, xmm2/m128, imm8 Compare packed double-precision floatingpoint values in xmm2/m128 and xmm1 using imm8 as comparison predicate.
//CMPPS           0F C2 /r ib      xmm1, xmm2/m128, imm8 Compare packed single-precision floating-point values in xmm2/mem and xmm1 using imm8 as comparison predicate.
//CMPS            A6               m8, m8 Compares byte at address DS:(E)SI with byte at address ES:(E)DI and sets the status flags accordingly
//CMPS            A7               m16, m16 Compares word at address DS:(E)SI with word at address ES:(E)DI and sets the status flags accordingly
//CMPS            A7               m32, m32 Compares doubleword at address DS:(E)SI with doubleword at address ES:(E)DI and sets the status flags accordingly
//CMPSB           A6               Compares byte at address DS:(E)SI with byte at address ES:(E)DI and sets the status flags accordingly
//CMPSW           A7               Compares word at address DS:(E)SI with word at address ES:(E)DI and sets the status flags accordingly
//CMPSD           A7               Compares doubleword at address DS:(E)SI with doubleword at address ES:(E)DI and sets the status flags accordingly
//CMPSD           F2 0F C2 /r ib   xmm1, xmm2/m64, imm8 Compare low double-precision floating-point value in xmm2/m64 and xmm1 using imm8 as comparison predicate.
//CMPSS           F3 0F C2 /r ib   xmm1, xmm2/m32, imm8 Compare low single-precision floating-point value in xmm2/m32 and xmm1 using imm8 as comparison predicate.
//CMPXCHG         0F B0/r          r/m8,r8 Compare AL with r/m8. If equal, ZF is set and r8 is loaded into r/m8. Else, clear ZF and load r/m8 into AL.
//CMPXCHG         0F B1/r          r/m16,r16 Compare AX with r/m16. If equal, ZF is set and r16 is loaded into r/m16. Else, clear ZF and load r/m16 into AX
//CMPXCHG         0F B1/r          r/m32,r32 Compare EAX with r/m32. If equal, ZF is set and r32 is loaded into r/m32. Else, clear ZF and load r/m32 into EAX
//CMPXCHG8B       0F C7 /1 m64     m64 Compare EDX:EAX with m64. If equal, set ZF and load ECX:EBX into m64. Else, clear ZF and load m64 into EDX:EAX.
//COMISD          66 0F 2F /r      xmm1, xmm2/m64 Compare low double-precision floating-point values in xmm1 and xmm2/mem64 and set the EFLAGS flags accordingly.
//COMISS          0F 2F /r         xmm1, xmm2/m32 Compare low single-precision floating-point values in xmm1 and xmm2/mem32 and set the EFLAGS flags accordingly.
//CPUID           0F A2            Returns processor identification and feature information to the EAX, EBX, ECX, and EDX registers, according to the input value entered initially in the EAX register
//CVTDQ2PD        F3 0F E6         xmm1, xmm2/m64 Convert two packed signed doubleword integers from xmm2/m128 to two packed double-precision floatingpoint values in xmm1.
//CVTDQ2PS        0F 5B /r         xmm1, xmm2/m128 Convert four packed signed doubleword integers from xmm2/m128 to four packed single-precision floating-point values in xmm1.
//CVTPD2DQ        F2 0F E6         xmm1, xmm2/m128 Convert two packed double-precision floating-point values from xmm2/m128 to two packed signed doubleword integers in xmm1.
//CVTPD2PI        66 0F 2D /r      mm, xmm/m128 Convert two packed double-precision floating-point values from xmm/m128 to two packed signed doubleword integers in mm.
//CVTPD2PS        66 0F 5A /r      xmm1, xmm2/m128 Convert two packed double-precision floating-point values in xmm2/m128 to two packed singleprecision floating-point values in xmm1.
//CVTPI2PD        66 0F 2A /r      xmm, mm/m64 Convert two packed signed doubleword integers from mm/mem64 to two packed double-precision floating-point values in xmm.
//CVTPI2PS        0F 2A /r         xmm, mm/m64 Convert two signed doubleword integers from mm/m64 to two single-precision floating-point values in xmm..
//CVTPS2DQ        66 0F 5B /r      xmm1, xmm2/m128 Convert four packed single-precision floatingpoint values from xmm2/m128 to four packed signed doubleword integers in xmm1.
//CVTPS2PD        0F 5A /r         xmm1, xmm2/m64 Convert two packed single-precision floating-point values in xmm2/m64 to two packed double-precision floating-point values in xmm1.
//CVTPS2PI        0F 2D /r         mm, xmm/m64 Convert two packed single-precision floating-point values from xmm/m64 to two packed signed doubleword integers in mm.
//CVTSD2SI        F2 0F 2D /r      r32, xmm/m64 Convert one double-precision floating-point value from xmm/m64 to one signed doubleword integer r32.
//CVTSD2SS        F2 0F 5A /r      xmm1, xmm2/m64 Convert one double-precision floating-point value in xmm2/m64 to one single-precision floating-point value in xmm1.
//CVTSI2SD        F2 0F 2A /r      xmm, r/m32 Convert one signed doubleword integer from r/m32 to one double-precision floating-point value in xmm.
//CVTSI2SS        F3 0F 2A /r      xmm, r/m32 Convert one signed doubleword integer from r/m32 to one single-precision floating-point value in xmm.
//CVTSS2SD        F3 0F 5A /r      xmm1, xmm2/m32 Convert one single-precision floating-point value in xmm2/m32 to one double-precision floating-point value in xmm1.
//CVTSS2SI        F3 0F 2D /r      r32, xmm/m32 Convert one single-precision floating-point value from xmm/m32 to one signed doubleword integer in r32.
//CVTTPD2PI       66 0F 2C /r      mm, xmm/m128 Convert two packer double-precision floating-point values from xmm/m128 to two packed signed doubleword integers in mm using truncation.
//CVTTPD2DQ       66 0F E6         xmm1, xmm2/m128 Convert two packed double-precision floating-point values from xmm2/m128 to two packed signed doubleword integers in xmm1 using truncation.
//CVTTPS2DQ       F3 0F 5B /r      xmm1, xmm2/m128 Convert four single-precision floating-point values from xmm2/m128 to four signed doubleword integers in xmm1 using truncation.
//CVTTPS2PI       0F 2C /r         mm, xmm/m64 Convert two single-precision floating-point values from xmm/m64 to two signed doubleword signed integers in mm using truncation.
//CVTTSD2SI       F2 0F 2C /r      r32, xmm/m64 Convert one double-precision floating-point value from xmm/m64 to one signed doubleword integer in r32 using truncation.
//CVTTSS2SI       F3 0F 2C /r      r32, xmm/m32 Convert one single-precision floating-point value from xmm/m32 to one signed doubleword integer in r32 using truncation.
//CWD             99               DX:AX . sign-extend of AX
//CDQ             99               EDX:EAX . sign-extend of EAX
//DAA             27               Decimal adjust AL after addition
//DAS             2F               Decimal adjust AL after subtraction
//DEC             FE /1            r/m8 Decrement r/m8 by 1
//DEC             FF /1            r/m16 Decrement r/m16 by 1
//DEC             FF /1            r/m32 Decrement r/m32 by 1
//DEC             48+rw            r16 Decrement r16 by 1
//DEC             48+rd            r32 Decrement r32 by 1
//DIV             F6 /6            r/m8 Unsigned divide AX by r/m8, with result stored in AL . Quotient, AH . Remainder
//DIV             F7 /6            r/m16 Unsigned divide DX:AX by r/m16, with result stored in AX . Quotient, DX . Remainder
//DIV             F7 /6            r/m32 Unsigned divide EDX:EAX by r/m32, with result stored in EAX . Quotient, EDX . Remainder Operand Size Dividend Divisor Quotient Remainder Maximum Quotient Word/byte AX r/m8 AL AH 255 Doubleword/word DX:AX r/m16 AX DX 65,535 Quadword/doubleword EDX:EAX r/m32 EAX EDX 232 - 1
//DIVPD           66 0F 5E /r      xmm1, xmm2/m128 Divide packed double-precision floating-point values in xmm1 by packed double-precision floating-point values xmm2/m128.
//DIVPS           0F 5E /r         xmm1, xmm2/m128 Divide packed single-precision floating-point values in xmm1 by packed single-precision floating-point values xmm2/m128.
//DIVSD           F2 0F 5E /r      xmm1, xmm2/m64 Divide low double-precision floating-point value n xmm1 by low double-precision floating-point value in xmm2/mem64.
//DIVSS           F3 0F 5E /r      xmm1, xmm2/m32 Divide low single-precision floating-point value in xmm1 by low single-precision floating-point value in xmm2/m32
//EMMS            0F 77            Set the x87 FPU tag word to empty.
//ENTER           C8 iw 00         imm16,0 Create a stack frame for a procedure
//ENTER           C8 iw 01         imm16,1 Create a nested stack frame for a procedure
//ENTER           C8 iw ib         imm16,imm8 Create a nested stack frame for a procedure
//F2XM1           D9 F0            Replace ST(0) with (2ST(0) – 1)
//FADD            D8 /0            m32fp Add m32fp to ST(0) and store result in ST(0)
//FADD            DC /0            m64fp Add m64fp to ST(0) and store result in ST(0)
//FADD            D8 C0+i          ST(0), ST(i) Add ST(0) to ST(i) and store result in ST(0)
//FADD            DC C0+i          ST(i), ST(0) Add ST(i) to ST(0) and store result in ST(i)
//FADDP           DE C0+i          ST(i), ST(0) Add ST(0) to ST(i), store result in ST(i), and pop the register stack
//FADDP           DE C1            Add ST(0) to ST(1), store result in ST(1), and pop the register stack
//FIADD           DA /0            m32int Add m32int to ST(0) and store result in ST(0)
//FIADD           DE /0            m16int Add m16int to ST(0) and store result in ST(0)
//FBLD            DF /4            m80 dec Convert BCD value to floating-point and push onto the FPU stack.
//FBSTP           DF /6            m80bcd Store ST(0) in m80bcd and pop ST(0).
//FCLEX           9B DB E2         Clear floating-point exception flags after checking for pending unmasked floating-point exceptions.
//FNCLEX*         DB E2            Clear floating-point exception flags without checking for pending unmasked floating-point exceptions.
//FCMOVB          DA C0+i          ST(0), ST(i) Move if below (CF=1)
//FCMOVE          DA C8+i          ST(0), ST(i) Move if equal (ZF=1)
//FCMOVBE         DA D0+i          ST(0), ST(i) Move if below or equal (CF=1 or ZF=1)
//FCMOVU          DA D8+i          ST(0), ST(i) Move if unordered (PF=1)
//FCMOVNB         DB C0+i          ST(0), ST(i) Move if not below (CF=0)
//FCMOVNE         DB C8+i          ST(0), ST(i) Move if not equal (ZF=0)
//FCMOVNBE        DB D0+i          ST(0), ST(i) Move if not below or equal (CF=0 and ZF=0)
//FCMOVNU         DB D8+i          ST(0), ST(i) Move if not unordered (PF=0)
//FCOM            D8 /2            m32fp Compare ST(0) with m32fp.
//FCOM            DC /2            m64fp Compare ST(0) with m64fp.
//FCOM            D8 D0+i          ST(i) Compare ST(0) with ST(i).
//FCOM            D8 D1            Compare ST(0) with ST(1).
//FCOMP           D8 /3            m32fp Compare ST(0) with m32fp and pop register stack.
//FCOMP           DC /3            m64fp Compare ST(0) with m64fp and pop register stack.
//FCOMP           D8 D8+i          ST(i) Compare ST(0) with ST(i) and pop register stack.
//FCOMP           D8 D9            Compare ST(0) with ST(1) and pop register stack.
//FCOMPP          DE D9            Compare ST(0) with ST(1) and pop register stack twice.
//FCOMI           DB F0+i          ST, ST(i) Compare ST(0) with ST(i) and set status flags accordingly
//FCOMIP          DF F0+i          ST, ST(i) Compare ST(0) with ST(i), set status flags accordingly, and pop register stack
//FUCOMI          DB E8+i          ST, ST(i) Compare ST(0) with ST(i), check for ordered values, and set status flags accordingly
//FUCOMIP         DF E8+i          ST, ST(i) Compare ST(0) with ST(i), check for ordered values, set status flags accordingly, and pop register stack
//FCOS            D9 FF            Replace ST(0) with its cosine
//FDECSTP         D9 F6            Decrement TOP field in FPU status word.
//FDIV            D8 /6            m32fp Divide ST(0) by m32fp and store result in ST(0)
//FDIV            DC /6            m64fp Divide ST(0) by m64fp and store result in ST(0)
//FDIV            D8 F0+i          ST(0), ST(i) Divide ST(0) by ST(i) and store result in ST(0)
//FDIV            DC F8+i          ST(i), ST(0) Divide ST(i) by ST(0) and store result in ST(i)
//FDIVP           DE F8+i          ST(i), ST(0) Divide ST(i) by ST(0), store result in ST(i), and pop the register stack
//FDIVP           DE F9            Divide ST(1) by ST(0), store result in ST(1), and pop the register stack
//FIDIV           DA /6            m32int Divide ST(0) by m32int and store result in ST(0)
//FIDIV           DE /6            m16int Divide ST(0) by m64int and store result in ST(0)
//FDIVR           D8 /7            m32fp Divide m32fp by ST(0) and store result in ST(0)
//FDIVR           DC /7            m64fp Divide m64fp by ST(0) and store result in ST(0)
//FDIVR           D8 F8+i          ST(0), ST(i) Divide ST(i) by ST(0) and store result in ST(0)
//FDIVR           DC F0+i          ST(i), ST(0) Divide ST(0) by ST(i) and store result in ST(i)
//FDIVRP          DE F0+i          ST(i), ST(0) Divide ST(0) by ST(i), store result in ST(i), and pop the register stack
//FDIVRP          DE F1            Divide ST(0) by ST(1), store result in ST(1), and pop the register stack
//FIDIVR          DA /7            m32int Divide m32int by ST(0) and store result in ST(0)
//FIDIVR          DE /7            m16int Divide m16int by ST(0) and store result in ST(0)
//FFREE           DD C0+i          ST(i) Sets tag for ST(i) to empty
//FICOM           DE /2            m16int Compare ST(0) with m16int
//FICOM           DA /2            m32int Compare ST(0) with m32int
//FICOMP          DE /3            m16int Compare ST(0) with m16int and pop stack register
//FICOMP          DA /3            m32int Compare ST(0) with m32int and pop stack register
//FILD            DF /0            m16int Push m16int onto the FPU register stack.
//FILD            DB /0            m32int Push m32int onto the FPU register stack.
//FILD            DF /5            m64int Push m64int onto the FPU register stack.
//FINCSTP         D9 F7            Increment the TOP field in the FPU status register
//FINIT           9B DB E3         Initialize FPU after checking for pending unmasked floating-point exceptions.
//FNINIT*         DB E3            Initialize FPU without checking for pending unmasked floating-point exceptions.
//FIST            DF /2            m16int Store ST(0) in m16int
//FIST            DB /2            m32int Store ST(0) in m32int
//FISTP           DF /3            m16int Store ST(0) in m16int and pop register stack
//FISTP           DB /3            m32int Store ST(0) in m32int and pop register stack
//FISTP           DF /7            m64int Store ST(0) in m64int and pop register stack
//FLD             D9 /0            m32fp Push m32fp onto the FPU register stack.
//FLD             DD /0            m64fp Push m64fp onto the FPU register stack.
//FLD             DB /5            m80fp Push m80fp onto the FPU register stack.
//FLD             D9 C0+i          ST(i) Push ST(i) onto the FPU register stack.
//FLD1            D9 E8            Push +1.0 onto the FPU register stack.
//FLDL2T          D9 E9            Push log210 onto the FPU register stack.
//FLDL2E          D9 EA            Push log2e onto the FPU register stack.
//FLDPI           D9 EB            Push p onto the FPU register stack.
//FLDLG2          D9 EC            Push log102 onto the FPU register stack.
//FLDLN2          D9 ED            Push loge2 onto the FPU register stack.
//FLDZ            D9 EE            Push +0.0 onto the FPU register stack.
//FLDCW           D9 /5            m2byte Load FPU control word from m2byte.
//FLDENV          D9 /4            m14/28byte Load FPU environment from m14byte or m28byte.
//FMUL            D8 /1            m32fp Multiply ST(0) by m32fp and store result in ST(0)
//FMUL            DC /1            m64fp Multiply ST(0) by m64fp and store result in ST(0)
//FMUL            D8 C8+i          ST(0), ST(i) Multiply ST(0) by ST(i) and store result in ST(0)
//FMUL            DC C8+i          ST(i), ST(0) Multiply ST(i) by ST(0) and store result in ST(i)
//FMULP           DE C8+i          ST(i), ST(0) Multiply ST(i) by ST(0), store result in ST(i), and pop the register stack
//FMULP           DE C9            Multiply ST(1) by ST(0), store result in ST(1), and pop the register stack
//FIMUL           DA /1            m32int Multiply ST(0) by m32int and store result in ST(0)
//FIMUL           DE /1            m16int Multiply ST(0) by m16int and store result in ST(0)
//FNOP            D9 D0            No operation is performed.
//FPATAN          D9 F3            Replace ST(1) with arctan(ST(1)/ST(0)) and pop the register stack
//FPTAN           D9 F2            Replace ST(0) with its tangent and push 1 onto the FPU stack.
//FRNDINT         D9 FC            Round ST(0) to an integer.
//FRSTOR          DD /4            m94/108byte Load FPU state from m94byte or m108byte.
//FSAVE           9B DD /6         m94/108byte Store FPU state to m94byte or m108byte after checking for pending unmasked floating-point exceptions. Then reinitialize the FPU.
//FNSAVE*         DD /6            m94/108byte Store FPU environment to m94byte or m108byte without checking for pending unmasked floating-point exceptions. Then re-initialize the FPU.
//FSCALE          D9 FD            Scale ST(0) by ST(1).
//FSIN            D9 FE            Replace ST(0) with its sine.
//FSINCOS         D9 FB            Compute the sine and cosine of ST(0); replace ST(0) with the sine, and push the cosine onto the register stack.
//FSQRT           D9 FA            Computes square root of ST(0) and stores the result in
//FST             D9 /2            m32fp Copy ST(0) to m32fp
//FST             DD /2            m64fp Copy ST(0) to m64fp
//FST             DD D0+i          ST(i) Copy ST(0) to ST(i)
//FSTP            D9 /3            m32fp Copy ST(0) to m32fp and pop register stack
//FSTP            DD /3            m64fp Copy ST(0) to m64fp and pop register stack
//FSTP            DB /7            m80fp Copy ST(0) to m80fp and pop register stack
//FSTP            DD D8+i          ST(i) Copy ST(0) to ST(i) and pop register stack
//FSTCW           9B D9 /7         m2byte Store FPU control word to m2byte after checking for pending unmasked floating-point exceptions.
//FNSTCW*         D9 /7            m2byte Store FPU control word to m2byte without checking for pending unmasked floating-point exceptions.
//FSTENV          9B D9 /6         m14/28byte Store FPU environment to m14byte or m28byte after checking for pending unmasked floating-point exceptions. Then mask all floating-point exceptions.
//FNSTENV*        D9 /6            m14/28byte Store FPU environment to m14byte or m28byte without checking for pending unmasked floating-point exceptions. Then mask all floating-point exceptions.
//FSTSW           9B DD /7         m2byte Store FPU status word at m2byte after checking for pending unmasked floating-point exceptions.
//FSTSW           9B DF E0         AX Store FPU status word in AX register after checking for pending unmasked floating-point exceptions.
//FNSTSW*         DD /7            m2byte Store FPU status word at m2byte without checking for pending unmasked floating-point exceptions.
//FNSTSW*         DF E0            AX Store FPU status word in AX register without checking for pending unmasked floating-point exceptions.
//FSUB            D8 /4            m32fp Subtract m32fp from ST(0) and store result in ST(0)
//FSUB            DC /4            m64fp Subtract m64fp from ST(0) and store result in ST(0)
//FSUB            D8 E0+i          ST(0), ST(i) Subtract ST(i) from ST(0) and store result in ST(0)
//FSUB            DC E8+i          ST(i), ST(0) Subtract ST(0) from ST(i) and store result in ST(i)
//FSUBP           DE E8+i          ST(i), ST(0) Subtract ST(0) from ST(i), store result in ST(i), and pop register stack
//FSUBP           DE E9            Subtract ST(0) from ST(1), store result in ST(1), and pop register stack
//FISUB           DA /4            m32int Subtract m32int from ST(0) and store result in ST(0)
//FISUB           DE /4            m16int Subtract m16int from ST(0) and store result in ST(0)
//FSUBR           D8 /5            m32fp Subtract ST(0) from m32fp and store result in ST(0)
//FSUBR           DC /5            m64fp Subtract ST(0) from m64fp and store result in ST(0)
//FSUBR           D8 E8+i          ST(0), ST(i) Subtract ST(0) from ST(i) and store result in ST(0)
//FSUBR           DC E0+i          ST(i), ST(0) Subtract ST(i) from ST(0) and store result in ST(i)
//FSUBRP          DE E0+i          ST(i), ST(0) Subtract ST(i) from ST(0), store result in ST(i), and pop register stack
//FSUBRP          DE E1            Subtract ST(1) from ST(0), store result in ST(1), and pop register stack
//FISUBR          DA /5            m32int Subtract ST(0) from m32int and store result in ST(0)
//FISUBR          DE /5            m16int Subtract ST(0) from m16int and store result in ST(0)
//FTST            D9 E4            Compare ST(0) with 0.0.
//FUCOM           DD E0+i          ST(i) Compare ST(0) with ST(i)
//FUCOM           DD E1            Compare ST(0) with ST(1)
//FUCOMP          DD E8+i          ST(i) Compare ST(0) with ST(i) and pop register stack
//FUCOMP          DD E9            Compare ST(0) with ST(1) and pop register stack
//FUCOMPP         DA E9            Compare ST(0) with ST(1) and pop register stack twice
//FXAM            D9 E5            Classify value or number in ST(0)
//FXCH            D9 C8+i          ST(i) Exchange the contents of ST(0) and ST(i)
//FXCH            D9 C9            Exchange the contents of ST(0) and ST(1)
//FXRSTOR         0F AE /1         m512byte Restore the x87 FPU, MMX, XMM, and MXCSR register state from m512byte.
//FXSAVE          0F AE /0         m512byte Save the x87 FPU, MMX, XMM, and MXCSR register state to m512byte.
//FXTRACT         D9 F4            Separate value in ST(0) into exponent and significand, store exponent in ST(0), and push the significand onto the register stack.
//FYL2X           D9 F1            Replace ST(1) with (ST(1) * log2ST(0)) and pop the register stack
//FYL2XP1         D9 F9            Replace ST(1) with ST(1) * log2(ST(0) + 1.0) and pop the register stack
//HLT             F4               Halt
//IDIV            F6 /7            r/m8 Signed divide AX by r/m8, with result stored in AL . Quotient, AH . Remainder
//IDIV            F7 /7            r/m16 Signed divide DX:AX by r/m16, with result stored in AX . Quotient, DX . Remainder
//IDIV            F7 /7            r/m32 Signed divide EDX:EAX by r/m32, with result stored in EAX . Quotient, EDX . Remainder Operand Size Dividend Divisor Quotient Remainder Quotient Range Word/byte AX r/m8 AL AH -128 to +127 Doubleword/word DX:AX r/m16 AX DX -32,768 to +32,767 Quadword/doubleword EDX:EAX r/m32 EAX EDX -231 to 232 - 1
//IMUL            F6 /5            r/m8 AX. AL * r/m byte
//IMUL            F7 /5            r/m16 DX:AX . AX * r/m word
//IMUL            F7 /5            r/m32 EDX:EAX . EAX * r/m doubleword
//IMUL            0F AF /r         r16,r/m16 word register . word register * r/m word
//IMUL            0F AF /r         r32,r/m32 doubleword register . doubleword register * r/m doubleword
//IMUL            6B /r ib         r16,r/m16,imm8 word register . r/m16 * sign-extended immediate byte
//IMUL            6B /r ib         r32,r/m32,imm8 doubleword register . r/m32 * sign-extended immediate byte
//IMUL            6B /r ib         r16,imm8 word register . word register * sign-extended immediate byte
//IMUL            6B /r ib         r32,imm8 doubleword register . doubleword register * signextended immediate byte
//IMUL            69 /r iw         r16,r/ m16,imm16 word register . r/m16 * immediate word
//IMUL            69 /r id         r32,r/ m32,imm32 doubleword register . r/m32 * immediate doubleword
//IMUL            69 /r iw         r16,imm16 word register . r/m16 * immediate word
//IMUL            69 /r id         r32,imm32 doubleword register . r/m32 * immediate doubleword
//IN              E4 ib            AL,imm8 Input byte from imm8 I/O port address into AL
//IN              E5 ib            AX,imm8 Input byte from imm8 I/O port address into AX
//IN              E5 ib            EAX,imm8 Input byte from imm8 I/O port address into EAX
//IN              EC               AL,DX Input byte from I/O port in DX into AL
//IN              ED               AX,DX Input word from I/O port in DX into AX
//IN              ED               EAX,DX Input doubleword from I/O port in DX into EAX
//INC             FE /0            r/m8 Increment r/m byte by 1
//INC             FF /0            r/m16 Increment r/m word by 1
//INC             FF /0            r/m32 Increment r/m doubleword by 1
//INC             40+ rw           r16 Increment word register by 1
//INC             40+ rd           r32 Increment doubleword register by 1
//INS             6C               m8, DX Input byte from I/O port specified in DX into memory location specified in ES:(E)DI
//INS             6D               m16, DX Input word from I/O port specified in DX into memory location specified in ES:(E)DI
//INS             6D               m32, DX Input doubleword from I/O port specified in DX into memory location specified in ES:(E)DI
//INSB            6C               Input byte from I/O port specified in DX into memory location specified with ES:(E)DI
//INSW            6D               Input word from I/O port specified in DX into memory location specified in ES:(E)DI
//INSD            6D               Input doubleword from I/O port specified in DX into memory location specified in ES:(E)DI
//INT             CC               3 Interrupt 3—trap to debugger
//INT             CD ib            imm8 Interrupt vector number specified by immediate byte
//INTO            CE               Interrupt 4—if overflow flag is 1
//INVD            0F 08            Flush internal caches; initiate flushing of external caches.
//INVLPG          0F 01/7          m Invalidate TLB Entry for page that contains m
//IRET            CF               Interrupt return (16-bit operand size)
//IRETD           CF               Interrupt return (32-bit operand size)
                
//JA              77 cb            rel8 Jump short if above (CF=0 and ZF=0)
//JAE             73 cb            rel8 Jump short if above or equal (CF=0)
//JB              72 cb            rel8 Jump short if below (CF=1)
//JBE             76 cb            rel8 Jump short if below or equal (CF=1 or ZF=1)
//JC              72 cb            rel8 Jump short if carry (CF=1)
//JCXZ            E3 cb            rel8 Jump short if CX register is 0
//JECXZ           E3 cb            rel8 Jump short if ECX register is 0
//JE              74 cb            rel8 Jump short if equal (ZF=1)
//JG              7F cb            rel8 Jump short if greater (ZF=0 and SF=OF)
//JGE             7D cb            rel8 Jump short if greater or equal (SF=OF)
//JL              7C cb            rel8 Jump short if less (SF<>OF)
//JLE             7E cb            rel8 Jump short if less or equal (ZF=1 or SF<>OF)
//JNA             76 cb            rel8 Jump short if not above (CF=1 or ZF=1)
//JNAE            72 cb            rel8 Jump short if not above or equal (CF=1)
//JNB             73 cb            rel8 Jump short if not below (CF=0)
//JNBE            77 cb            rel8 Jump short if not below or equal (CF=0 and ZF=0)
//JNC             73 cb            rel8 Jump short if not carry (CF=0)
//JNE             75 cb            rel8 Jump short if not equal (ZF=0)
//JNG             7E cb            rel8 Jump short if not greater (ZF=1 or SF<>OF)
//JNGE            7C cb            rel8 Jump short if not greater or equal (SF<>OF)
//JNL             7D cb            rel8 Jump short if not less (SF=OF)
//JNLE            7F cb            rel8 Jump short if not less or equal (ZF=0 and SF=OF)
//JNO             71 cb            rel8 Jump short if not overflow (OF=0)
//JNP             7B cb            rel8 Jump short if not parity (PF=0)
//JNS             79 cb            rel8 Jump short if not sign (SF=0)
//JNZ             75 cb            rel8 Jump short if not zero (ZF=0)
//JO              70 cb            rel8 Jump short if overflow (OF=1)
//JP              7A cb            rel8 Jump short if parity (PF=1)
//JPE             7A cb            rel8 Jump short if parity even (PF=1)
//JPO             7B cb            rel8 Jump short if parity odd (PF=0)
//JS              78 cb            rel8 Jump short if sign (SF=1)
//JZ              74 cb            rel8 Jump short if zero (ZF = 1)
                
//JA              0F 87 cw/cd      rel16/32 Jump near if above (CF=0 and ZF=0)
//JAE             0F 83 cw/cd      rel16/32 Jump near if above or equal (CF=0)
//JB              0F 82 cw/cd      rel16/32 Jump near if below (CF=1)
//JBE             0F 86 cw/cd      rel16/32 Jump near if below or equal (CF=1 or ZF=1)
//JC              0F 82 cw/cd      rel16/32 Jump near if carry (CF=1)
//JE              0F 84 cw/cd      rel16/32 Jump near if equal (ZF=1)
//JZ              0F 84 cw/cd      rel16/32 Jump near if 0 (ZF=1)
//JG              0F 8F cw/cd      rel16/32 Jump near if greater (ZF=0 and SF=OF)
//JGE             0F 8D cw/cd      rel16/32 Jump near if greater or equal (SF=OF)
//JL              0F 8C cw/cd      rel16/32 Jump near if less (SF<>OF)
//JLE             0F 8E cw/cd      rel16/32 Jump near if less or equal (ZF=1 or SF<>OF)
//JNA             0F 86 cw/cd      rel16/32 Jump near if not above (CF=1 or ZF=1)
//JNAE            0F 82 cw/cd      rel16/32 Jump near if not above or equal (CF=1)
//JNB             0F 83 cw/cd      rel16/32 Jump near if not below (CF=0)
//JNBE            0F 87 cw/cd      rel16/32 Jump near if not below or equal (CF=0 and ZF=0)
//JNC             0F 83 cw/cd      rel16/32 Jump near if not carry (CF=0)
//JNE             0F 85 cw/cd      rel16/32 Jump near if not equal (ZF=0)
//JNG             0F 8E cw/cd      rel16/32 Jump near if not greater (ZF=1 or SF<>OF)
//JNGE            0F 8C cw/cd      rel16/32 Jump near if not greater or equal (SF<>OF)
//JNL             0F 8D cw/cd      rel16/32 Jump near if not less (SF=OF)
//JNLE            0F 8F cw/cd      rel16/32 Jump near if not less or equal (ZF=0 and SF=OF)
//JNO             0F 81 cw/cd      rel16/32 Jump near if not overflow (OF=0)
//JNP             0F 8B cw/cd      rel16/32 Jump near if not parity (PF=0)
//JNS             0F 89 cw/cd      rel16/32 Jump near if not sign (SF=0)
//JNZ             0F 85 cw/cd      rel16/32 Jump near if not zero (ZF=0)
//JO              0F 80 cw/cd      rel16/32 Jump near if overflow (OF=1)
//JP              0F 8A cw/cd      rel16/32 Jump near if parity (PF=1)
//JPE             0F 8A cw/cd      rel16/32 Jump near if parity even (PF=1)
//JPO             0F 8B cw/cd      rel16/32 Jump near if parity odd (PF=0)
//JS              0F 88 cw/cd      rel16/32 Jump near if sign (SF=1)
//JZ              0F 84 cw/cd      rel16/32 Jump near if 0 (ZF=1)
                
//JMP             EB cb            rel8 Jump short, relative, displacement relative to next instruction
//JMP             E9 cw            rel16 Jump near, relative, displacement relative to next instruction
//JMP             E9 cd            rel32 Jump near, relative, displacement relative to next instruction
//JMP             FF /4            r/m16 Jump near, absolute indirect, address given in r/m16
//JMP             FF /4            r/m32 Jump near, absolute indirect, address given in r/m32
//JMP             EA cd            ptr16:16 Jump far, absolute, address given in operand
//JMP             EA cp            ptr16:32 Jump far, absolute, address given in operand
//JMP             FF /5            m16:16 Jump far, absolute indirect, address given in m16:16
//JMP             FF /5            m16:32 Jump far, absolute indirect, address given in m16:32
//LAHF            9F               Load: AH . EFLAGS(SF:ZF:0:AF:0:PF:1:CF)
//LAR             0F 02 /r         r16,r/m16 r16 . r/m16 masked by FF00H
//LAR             0F 02 /r         r32,r/m32 r32 . r/m32 masked by 00FxFF00H
//LDMXCSR         0F AE /2         m32 Load MXCSR register from m32.
//LDS             C5 /r            r16,m16:16 Load DS:r16 with far pointer from memory
//LDS             C5 /r            r32,m16:32 Load DS:r32 with far pointer from memory
//LSS             0F B2 /r         r16,m16:16 Load SS:r16 with far pointer from memory
//LSS             0F B2 /r         r32,m16:32 Load SS:r32 with far pointer from memory
//LES             C4 /r            r16,m16:16 Load ES:r16 with far pointer from memory
//LES             C4 /r            r32,m16:32 Load ES:r32 with far pointer from memory
//LFS             0F B4 /r         r16,m16:16 Load FS:r16 with far pointer from memory
//LFS             0F B4 /r         r32,m16:32 Load FS:r32 with far pointer from memory
//LGS             0F B5 /r         r16,m16:16 Load GS:r16 with far pointer from memory
//LGS             0F B5 /r         r32,m16:32 Load GS:r32 with far pointer from memory
//LEA             8D /r            r16,m Store effective address for m in register r16
//LEA             8D /r            r32,m Store effective address for m in register r32 Operand Size Address Size Action Performed 16 16 16-bit effective address is calculated and stored in requested 16-bit register destination. 16 32 32-bit effective address is calculated. The lower 16 bits of the address are stored in the requested 16-bit register destination. 32 16 16-bit effective address is calculated. The 16-bit address is zeroextended and stored in the requested 32-bit register destination. 32 32 32-bit effective address is calculated and stored in the requested 32-bit register destination.
//LEAVE           C9               Set SP to BP, then pop BP
//LEAVE           C9               Set ESP to EBP, then pop EBP
//LFENCE          0F AE /5         Serializes load operations.
//LGDT            0F 01 /2         m16&32 Load m into GDTR
//LIDT            0F 01 /3         m16&32 Load m into IDTR
//LLDT            0F 00 /2         r/m16 Load segment selector r/m16 into LDTR
//LMSW            0F 01 /6         r/m16 Loads r/m16 in machine status word of CR0
//LOCK            F0               Asserts # signal for duration of the accompanying instruction
//LODS            AC               m8 Load byte at address DS:(E)SI into AL
//LODS            AD               m16 Load word at address DS:(E)SI into AX
//LODS            AD               m32 Load doubleword at address DS:(E)SI into EAX
//LODSB           AC               Load byte at address DS:(E)SI into AL
//LODSW           AD               Load word at address DS:(E)SI into AX
//LODSD           AD               Load doubleword at address DS:(E)SI into EAX
//LOOP            E2 cb            rel8 Decrement count; jump short if count . 0
//LOOPE           E1 cb            rel8 Decrement count; jump short if count . 0 and ZF=1
//LOOPZ           E1 cb            rel8 Decrement count; jump short if count . 0 and ZF=1
//LOOPNE          E0 cb            rel8 Decrement count; jump short if count . 0 and ZF=0
//LOOPNZ          E0 cb            rel8 Decrement count; jump short if count . 0 and ZF=0
//LSL             0F 03 /r         r16,r/m16 Load: r16 . segment limit, selector r/m16
//LSL             0F 03 /r         r32,r/m32 Load: r32 . segment limit, selector r/m32)
//LTR             0F 00 /3         r/m16 Load r/m16 into task register
//MASKMOVDQU      66 0F F7 /r      xmm1, xmm2 Selectively write bytes from xmm1 to memory location using the byte mask in xmm2.
//MASKMOVQ        0F F7 /r         mm1, mm2 Selectively write bytes from mm1 to memory location using the byte mask in mm2
//MAXPD           66 0F 5F /r      xmm1, xmm2/m128 Return the maximum double-precision floating-point values between xmm2/m128 and xmm1.
//MAXPS           0F 5F /r         xmm1, xmm2/m128 Return the maximum single-precision floating-point values between xmm2/m128 and xmm1.
//MAXSD           F2 0F 5F /r      xmm1, xmm2/m64 Return the maximum scalar double-precision floatingpoint value between xmm2/mem64 and xmm1.
//MAXSS           F3 0F 5F /r      xmm1, xmm2/m32 Return the maximum scalar single-precision floatingpoint value between xmm2/mem32 and xmm1.
//MFENCE          0F AE /6         Serializes load and store operations.
//MINPD           66 0F 5D /r      xmm1, xmm2/m128 Return the minimum double-precision floating-point values between xmm2/m128 and xmm1.
//MINPS           0F 5D /r         xmm1, xmm2/m128 Return the minimum single-precision floating-point values between xmm2/m128 and xmm1.
//MINSD           F2 0F 5D /r      xmm1, xmm2/m64 Return the minimum scalar double-precision floating-point value between xmm2/mem64 and xmm1.
//MINSS           F3 0F 5D /r      xmm1, xmm2/m32 Return the minimum scalar single-precision floatingpoint value between xmm2/mem32 and xmm1.
//MOV             88 /r            r/m8,r8 Move r8 to r/m8
//MOV             89 /r            r/m16,r16 Move r16 to r/m16
//MOV             89 /r            r/m32,r32 Move r32 to r/m32
//MOV             8A /r            r8,r/m8 Move r/m8 to r8
//MOV             8B /r            r16,r/m16 Move r/m16 to r16
//MOV             8B /r            r32,r/m32 Move r/m32 to r32
//MOV             8C /r            r/m16,Sreg** Move segment register to r/m16
//MOV             8E /r            Sreg,r/m16** Move r/m16 to segment register
//MOV             A0               AL,moffs8* Move byte at (seg:offset) to AL
//MOV             A1               AX,moffs16* Move word at (seg:offset) to AX
//MOV             A1               EAX,moffs32* Move doubleword at (seg:offset) to EAX
//MOV             A2               moffs8*,AL Move AL to (seg:offset)
//MOV             A3               moffs16*,AX Move AX to (seg:offset)
//MOV             A3               moffs32*,EAX Move EAX to (seg:offset)
//MOV             B0+ rb           r8,imm8 Move imm8 to r8
//MOV             B8+ rw           r16,imm16 Move imm16 to r16
//MOV             B8+ rd           r32,imm32 Move imm32 to r32
//MOV             C6 /0            r/m8,imm8 Move imm8 to r/m8
//MOV             C7 /0            r/m16,imm16 Move imm16 to r/m16
//MOV             C7 /0            r/m32,imm32 Move imm32 to r/m32
//MOV             0F 22 /r         CR0,r32 Move r32 to CR0
//MOV             0F 22 /r         CR2,r32 Move r32 to CR2
//MOV             0F 22 /r         CR3,r32 Move r32 to CR3
//MOV             0F 22 /r         CR4,r32 Move r32 to CR4
//MOV             0F 20 /r         r32,CR0 Move CR0 to r32
//MOV             0F 20 /r         r32,CR2 Move CR2 to r32
//MOV             0F 20 /r         r32,CR3 Move CR3 to r32
//MOV             0F 20 /r         r32,CR4 Move CR4 to r32
//MOV             0F 21/r          r32, DR0-DR7 Move debug register to r32
//MOV             0F 23 /r         DR0-DR7,r32 Move r32 to debug register
//MOVAPD          66 0F 28 /r      xmm1, xmm2/m128 Move packed double-precision floating-point values from xmm2/m128 to xmm1.
//MOVAPD          66 0F 29 /r      xmm2/m128, xmm1 Move packed double-precision floating-point values from xmm1 to xmm2/m128.
//MOVAPS          0F 28 /r         xmm1, xmm2/m128 Move packed single-precision floating-point values from xmm2/m128 to xmm1.
//MOVAPS          0F 29 /r         xmm2/m128, xmm1 Move packed single-precision floating-point values from xmm1 to xmm2/m128.
//MOVD            0F 6E /r         mm, r/m32 Move doubleword from r/m32 to mm.
//MOVD            0F 7E /r         r/m32, mm Move doubleword from mm to r/m32.
//MOVD            66 0F 6E /r      xmm, r/m32 Move doubleword from r/m32 to xmm.
//MOVD            66 0F 7E /r      r/m32, xmm Move doubleword from xmm register to r/m32.
//MOVDQA          66 0F 6F /r      xmm1, xmm2/m128 Move aligned double quadword from xmm2/m128 to xmm1.
//MOVDQA          66 0F 7F /r      xmm2/m128, xmm1 Move aligned double quadword from xmm1 to xmm2/m128.
//MOVDQU          F3 0F 6F /r      xmm1, xmm2/m128 Move unaligned double quadword from xmm2/m128 to xmm1.
//MOVDQU          F3 0F 7F /r      xmm2/m128, xmm1 Move unaligned double quadword from xmm1 to xmm2/m128.
//MOVDQ2Q         F2 0F D6         mm, xmm Move low quadword from xmm to mmx register .
//MOVHLPS         OF 12 /r         xmm1, xmm2 Move two packed single-precision floating-point values from high quadword of xmm2 to low quadword of xmm1.
//MOVHPD          66 0F 16 /r      xmm, m64 Move double-precision floating-point value from m64 to high quadword of xmm.
//MOVHPD          66 0F 17 /r      m64, xmm Move double-precision floating-point value from high quadword of xmm to m64.
//MOVHPS          0F 16 /r         xmm, m64 Move two packed single-precision floating-point values from m64 to high quadword of xmm.
//MOVHPS          0F 17 /r         m64, xmm Move two packed single-precision floating-point values from high quadword of xmm to m64.
//MOVLHPS         OF 16 /r         xmm1, xmm2 Move two packed single-precision floating-point values from low quadword of xmm2 to high quadword of xmm1.
//MOVLPD          66 0F 12 /r      xmm, m64 Move double-precision floating-point value from m64 to low quadword of xmm register.
//MOVLPD          66 0F 13 /r      m64, xmm Move double-precision floating-point nvalue from low quadword of xmm register to m64.
//MOVLPS          0F 12 /r         xmm, m64 Move two packed single-precision floating-point values from m64 to low quadword of xmm.
//MOVLPS          0F 13 /r         m64, xmm Move two packed single-precision floating-point values from low quadword of xmm to m64.
//MOVMSKPD        66 0F 50 /r      r32, xmm Extract 2-bit sign mask of from xmm and store in r32.
//MOVMSKPS        0F 50 /r         r32, xmm Extract 4-bit sign mask of from xmm and store in r32.
//MOVNTDQ         66 0F E7 /r      m128, xmm Move double quadword from xmm to m128 using nontemporal hint.
//MOVNI           0F C3 /r         m32, r32 Move doubleword from r32 to m32 using non-temporal hint.
//MOVNTPD         66 0F 2B /r      m128, xmm Move packed double-precision floating-point values from xmm to m128 using non-temporal hint.
//MOVNTPS         0F 2B /r         m128, xmm Move packed single-precision floating-point values from xmm to m128 using non-temporal hint.
//MOVNTQ          0F E7 /r         m64, mm Move quadword from mm to m64 using non-temporal hint.
//MOVQ            0F 6F /r         mm, mm/m64 Move quadword from mm/m64 to mm.
//MOVQ            0F 7F /r         mm/m64, mm Move quadword from mm to mm/m64.
//MOVQ            F3 0F 7E         xmm1, xmm2/m64 Move quadword from xmm2/mem64 to xmm1.
//MOVQ            66 0F D6         xmm2/m64, xmm1 Move quadword from xmm1 to xmm2/mem64.
//MOVQ2DQ         F3 0F D6         xmm, mm Move quadword from mmx to low quadword of xmm.
//MOVS            A4               m8, m8 Move byte at address DS:(E)SI to address ES:(E)DI
//MOVS            A5               m16, m16 Move word at address DS:(E)SI to address ES:(E)DI
//MOVS            A5               m32, m32 Move doubleword at address DS:(E)SI to address ES:(E)DI
//MOVSB           A4               Move byte at address DS:(E)SI to address ES:(E)DI
//MOVSW           A5               Move word at address DS:(E)SI to address ES:(E)DI
//MOVSD           A5               Move doubleword at address DS:(E)SI to address ES:(E)DI
//MOVSD           F2 0F 10 /r      xmm1, xmm2/m64 Move scalar double-precision floating-point value from xmm2/m64 to xmm1 register.
//MOVSD           F2 0F 11 /r      xmm2/m64, xmm Move scalar double-precision floating-point value from xmm1 register to xmm2/m64.
//MOVSS           F3 0F 10 /r      xmm1, xmm2/m32 Move scalar single-precision floating-point value from xmm2/m64 to xmm1 register.
//MOVSS           F3 0F 11 /r      xmm2/m32, xmm Move scalar single-precision floating-point value from xmm1 register to xmm2/m64.
//MOVSX           0F BE /r         r16,r/m8 Move byte to word with sign-extension
//MOVSX           0F BE /r         r32,r/m8 Move byte to doubleword, sign-extension
//MOVSX           0F BF /r         r32,r/m16 Move word to doubleword, sign-extension
//MOVUPD          66 0F 10 /r      xmm1, xmm2/m128 Move packed double-precision floating-point values from xmm2/m128 to xmm1.
//MOVUPD          66 0F 11 /r      xmm2/m128, xmm Move packed double-precision floating-point values from xmm1 to xmm2/m128.
//MOVUPS          0F 10 /r         xmm1, xmm2/m128 Move packed single-precision floating-point values from xmm2/m128 to xmm1.
//MOVUPS          0F 11 /r         xmm2/m128, xmm1 Move packed single-precision floating-point values from xmm1 to xmm2/m128.
//MOVZX           0F B6 /r         r16,r/m8 Move byte to word with zero-extension
//MOVZX           0F B6 /r         r32,r/m8 Move byte to doubleword, zero-extension
//MOVZX           0F B7 /r         r32,r/m16 Move word to doubleword, zero-extension
//MUL             F6 /4            r/m8 Unsigned multiply (AX . AL * r/m8)
//MUL             F7 /4            r/m16 Unsigned multiply (DX:AX . AX * r/m16)
//MUL             F7 /4            r/m32 Unsigned multiply (EDX:EAX . EAX * r/m32) Operand Size Source 1 Source 2 Destination Byte AL r/m8 AX Word AX r/m16 DX:AX Doubleword EAX r/m32 EDX:EAX
//MULPD           66 0F 59 /r      xmm1, xmm2/m128 Multiply packed double-precision floating-point values in xmm2/m128 by xmm1.
//MULPS           0F 59 /r         xmm1, xmm2/m128 Multiply packed single-precision floating-point values in xmm2/mem by xmm1.
//MULSD           F2 0F 59 /r      xmm1, xmm2/m64 Multiply the low double-precision floating-point value in xmm2/mem64 by low double-precision floating-point value in xmm1.
//MULSS           F3 0F 59 /r      xmm1, xmm2/m32 Multiply the low single-precision floating-point value in xmm2/mem by the low single-precision floating-point value in xmm1.
//NEG             F6 /3            r/m8 Two’s complement negate r/m8
//NEG             F7 /3            r/m16 Two’s complement negate r/m16
//NEG             F7 /3            r/m32 Two’s complement negate r/m32
//NOP             90               No operation
//NOT             F6 /2            r/m8 Reverse each bit of r/m8
//NOT             F7 /2            r/m16 Reverse each bit of r/m16
//NOT             F7 /2            r/m32 Reverse each bit of r/m32
//OR              0C ib            AL,imm8 AL  imm8
//OR              0D iw            AX,imm16 AX  imm16
//OR              0D id            EAX,imm32 EAX  imm32
//OR              80 /1 ib         r/m8,imm8 r/m8  imm8
//OR              81 /1 iw         r/m16,imm16 r/m16  imm16
//OR              81 /1 id         r/m32,imm32 r/m32  imm32
//OR              83 /1 ib         r/m16,imm8 r/m16  imm8 (sign-extended)
//OR              83 /1 ib         r/m32,imm8 r/m32  imm8 (sign-extended)
//OR              08 /r            r/m8,r8 r/m8  r8
//OR              09 /r            r/m16,r16 r/m16  r16
//OR              09 /r            r/m32,r32 r/m32  r32
//OR              0A /r            r8,r/m8 r8  r/m8
//OR              0B /r            r16,r/m16 r16  r/m16
//OR              0B /r            r32,r/m32 r32  r/m32
//ORPD            66 0F 56 /r      xmm1, xmm2/m128 Bitwise OR of xmm2/m128 and xmm1.
//ORPS            0F 56 /r         xmm1, xmm2/m128 Bitwise OR of xmm2/m128 and xmm1
//OUT             E6 ib            imm8, AL Output byte in AL to I/O port address imm8
//OUT             E7 ib            imm8, AX Output word in AX to I/O port address imm8
//OUT             E7 ib            imm8, EAX Output doubleword in EAX to I/O port address imm8
//OUT             EE               DX, AL Output byte in AL to I/O port address in DX
//OUT             EF               DX, AX Output word in AX to I/O port address in DX
//OUT             EF               DX, EAX Output doubleword in EAX to I/O port address in DX
//OUTS            6E               DX, m8 Output byte from memory location specified in DS:(E)SI to I/O port specified in DX
//OUTS            6F               DX, m16 Output word from memory location specified in DS:(E)SI to I/O port specified in DX
//OUTS            6F               DX, m32 Output doubleword from memory location specified in DS:(E)SI to I/O port specified in DX
//OUTSB           6E               Output byte from memory location specified in DS:(E)SI to I/O port specified in DX
//OUTSW           6F               Output word from memory location specified in DS:(E)SI to I/O port specified in DX
//OUTSD           6F               Output doubleword from memory location specified in DS:(E)SI to I/O port specified in DX
//PACKSSWB        0F 63 /r         mm1, mm2/m64 Converts 4 packed signed word integers from mm1 and from mm2/m64 into 8 packed signed byte integers in mm1 using signed saturation.
//PACKSSWB        66 0F 63 /r      xmm1, xmm2/m128 Converts 8 packed signed word integers from xmm1 and from xmm2/m128 into 16 packed signed byte integers in xmm1 using signed saturation.
//PACKSSDW        0F 6B /r         mm1, mm2/m64 Converts 2 packed signed doubleword integers from mm1 and from mm2/m64 into 4 packed signed word integers in mm1 using signed saturation.
//PACKSSDW        66 0F 6B /r      xmm1, xmm2/m128 Converts 4 packed signed doubleword integers from xmm1 and from xmm2/m128 into 8 packed signed word integers in xmm1 using signed saturation.
//PACKUSWB        0F 67 /r         mm, mm/m64 Converts 4 signed word integers from mm and 4 signed word integers from mm/m64 into 8 unsigned byte integers in mm using unsigned saturation.
//PACKUSWB        66 0F 67 /r      xmm1, xmm2/m128 Converts 8 signed word integers from xmm1 and 8 signed word integers from xmm2/m128 into 16 unsigned byte integers in xmm1 using unsigned saturation.
//PADDB           0F FC /r         mm, mm/m64 Add packed byte integers from mm/m64 and mm.
//PADDB           66 0F FC /r      xmm1,xmm2/m128 Add packed byte integers from xmm2/m128 and xmm1.
//PADDW           0F FD /r         mm, mm/m64 Add packed word integers from mm/m64 and mm.
//PADDW           66 0F FD /r      xmm1, xmm2/m128 Add packed word integers from xmm2/m128 and xmm1.
//PADDD           0F FE /r         mm, mm/m64 Add packed doubleword integers from mm/m64 and mm.
//PADDD           66 0F FE /r      xmm1, xmm2/m128 Add packed doubleword integers from xmm2/m128 and xmm1.
//PADDQ           0F D4 /r         mm1,mm2/m64 Add quadword integer mm2/m64 to mm1
//PADDQ           66 0F D4 /r      xmm1,xmm2/m128 Add packed quadword integers xmm2/m128 to xmm1
//PADDSB          0F EC /r         mm, mm/m64 Add packed signed byte integers from mm/m64 and mm and saturate the results.
//PADDSB          66 0F EC /r      xmm1, Add packed signed byte integers from xmm2/m128 and xmm1 saturate the results.
//PADDSW          0F ED /r         mm, mm/m64 Add packed signed word integers from mm/m64 and mm and saturate the results.
//PADDSW          66 0F ED /r      xmm1, xmm2/m128 Add packed signed word integers from xmm2/m128 and xmm1 and saturate the results.
//PADDUSB         0F DC /r         mm, mm/m64 Add packed unsigned byte integers from mm/m64 and mm and saturate the results.
//PADDUSB         66 0F DC /r      xmm1, xmm2/m128 Add packed unsigned byte integers from xmm2/m128 and xmm1 saturate the results.
//PADDUSW         0F DD /r         mm, mm/m64 Add packed unsigned word integers from mm/m64 and mm and saturate the results.
//PADDUSW         66 0F DD /r      xmm1, xmm2/m128 Add packed unsigned word integers from xmm2/m128 to xmm1 and saturate the results.
//PAND            0F DB /r         mm, mm/m64 Bitwise AND mm/m64 and mm.
//PAND            66 0F DB /r      xmm1, xmm2/m128 Bitwise AND of xmm2/m128 and xmm1.
//PANDN           0F DF /r         mm, mm/m64 Bitwise AND NOT of mm/m64 and mm.
//PANDN           66 0F DF /r      xmm1, xmm2/m128 Bitwise AND NOT of xmm2/m128 and xmm1.
//PAUSE           F3 90            Gives hint to processor that improves performance of spin-wait loops.
//PAVGB           0F E0 /r         mm1, mm2/m64 Average packed unsigned byte integers from mm2/m64 and mm1 with rounding.
//PAVGB           66 0F E0, /r     xmm1, xmm2/m128 Average packed unsigned byte integers from xmm2/m128 and xmm1 with rounding.
//PAVGW           0F E3 /r         mm1, mm2/m64 Average packed unsigned word integers from mm2/m64 and mm1 with rounding.
//PAVGW           66 0F E3 /r      xmm1, xmm2/m128 Average packed unsigned word integers from xmm2/m128 and xmm1 with rounding.
//PCMPEQB         0F 74 /r         mm, mm/m64 Compare packed bytes in mm/m64 and mm for equality.
//PCMPEQB         66 0F 74 /r      xmm1, xmm2/m128 Compare packed bytes in xmm2/m128 and xmm1 for equality.
//PCMPEQW         0F 75 /r         mm, mm/m64 Compare packed words in mm/m64 and mm for equality.
//PCMPEQW         66 0F 75 /r      xmm1, xmm2/m128 Compare packed words in xmm2/m128 and xmm1 for equality.
//PCMPEQD         0F 76 /r         mm, mm/m64 Compare packed doublewords in mm/m64 and mm for equality.
//PCMPEQD         66 0F 76 /r      xmm1, xmm2/m128 Compare packed doublewords in xmm2/m128 and xmm1 for equality.
//PCMPGTB         0F 64 /r         mm, mm/m64 Compare packed signed byte integers in mm and mm/m64 for greater than.
//PCMPGTB         66 0F 64 /r      xmm1, xmm2/m128 Compare packed signed byte integers in xmm1 and xmm2/m128 for greater than.
//PCMPGTW         0F 65 /r         mm, mm/m64 Compare packed signed word integers in mm and mm/m64 for greater than.
//PCMPGTW         66 0F 65 /r      xmm1, xmm2/m128 Compare packed signed word integers in xmm1 and xmm2/m128 for greater than.
//PCMPGTD         0F 66 /r         mm, mm/m64 Compare packed signed doubleword integers in mm and mm/m64 for greater than.
//PCMPGTD         66 0F 66 /r      xmm1, xmm2/m128 Compare packed signed doubleword integers in xmm1 and xmm2/m128 for greater than.
//PEXTRW          0F C5 /r ib      r32, mm, imm8 Extract the word specified by imm8 from mm and move it to r32.
//PEXTRW          66 0F C5 /r ib   r32, xmm, imm8 Extract the word specified by imm8 from xmm and move it to a r32.
//PINSRW          0F C4 /r ib      mm, r32/m16, imm8 Insert the low word from r32 or from m16 into mm at the word position specified by imm8
//PINSRW          66 0F C4 /r ib   xmm, r32/m16, imm8 Move the low word of r32 or from m16 into xmm at the word position specified by imm8.
//PMADDWD         0F F5 /r         mm, mm/m64 Multiply the packed words in mm by the packed words in mm/m64, add adjacent doubleword results, and store in mm.
//PMADDWD         66 0F F5 /r      xmm1, xmm2/m128 Multiply the packed word integers in xmm1 by the packed word integers in xmm2/m128, add adjacent doubleword results, and store in xmm1.
//PMAXSW          0F EE /r         mm1, mm2/m64 Compare signed word integers in mm2/m64 and mm1 and return maximum values.
//PMAXSW          66 0F EE /r      xmm1, xmm2/m128 Compare signed word integers in xmm2/m128 and xmm1 and return maximum values.
//PMAXUB          0F DE /r         mm1, mm2/m64 Compare unsigned byte integers in mm2/m64 and mm1 and returns maximum values.
//PMAXUB          66 0F DE /r      xmm1, xmm2/m128 Compare unsigned byte integers in xmm2/m128 and xmm1 and returns maximum values.
//PMINSW          0F EA /r         mm1, mm2/m64 Compare signed word integers in mm2/m64 and mm1 and return minimum values.
//PMINSW          66 0F EA /r      xmm1, xmm2/m128 Compare signed word integers in xmm2/m128 and xmm1 and return minimum values.
//PMINUB          0F DA /r         mm1, mm2/m64 Compare unsigned byte integers in mm2/m64 and mm1 and returns minimum values.
//PMINUB          66 0F DA /r      xmm1, xmm2/m128 Compare unsigned byte integers in xmm2/m128 and xmm1 and returns minimum values.
//PMOVMSKB        0F D7 /r         r32, mm Move a byte mask of mm to r32.
//PMOVMSKB        66 0F D7 /r      r32, xmm Move a byte mask of xmm to r32.
//PMULHUW         0F E4 /r         mm1, mm2/m64 Multiply the packed unsigned word integers in mm1 register and mm2/m64, and store the high 16 bits of the results in mm1.
//PMULHUW         66 0F E4 /r      xmm1, xmm2/m128 Multiply the packed unsigned word integers in xmm1 and xmm2/m128, and store the high 16 bits of the results in xmm1.
//PMULHW          0F E5 /r         mm, mm/m64 Multiply the packed signed word integers in mm1 register and mm2/m64, and store the high 16 bits of the results in mm1.
//PMULHW          66 0F E5 /r      xmm1, xmm2/m128 Multiply the packed signed word integers in xmm1 and xmm2/m128, and store the high 16 bits of the results in xmm1.
//PMULLW          0F D5 /r         mm, mm/m64 Multiply the packed signed word integers in mm1 register and mm2/m64, and store the low 16 bits of the results in mm1.
//PMULLW          66 0F D5 /r      xmm1, xmm2/m128 Multiply the packed signed word integers in xmm1 and xmm2/m128, and store the low 16 bits of the results in xmm1.
//PMULUDQ         0F F4 /r         mm1, mm2/m64 Multiply unsigned doubleword integer in mm1 by unsigned doubleword integer in mm2/m64, and store the quadword result in mm1.
//PMULUDQ         66 OF F4 /r      xmm1, xmm2/m128 Multiply packed unsigned doubleword integers in xmm1 by packed unsigned doubleword integers in xmm2/m128, and store the quadword results in xmm1.
//POP             8F /0            r/m16 Pop top of stack into m16; increment stack pointer
//POP             8F /0            r/m32 Pop top of stack into m32; increment stack pointer
//POP             58+ rw           r16 Pop top of stack into r16; increment stack pointer
//POP             58+ rd           r32 Pop top of stack into r32; increment stack pointer
//POP             1F               DS Pop top of stack into DS; increment stack pointer
//POP             07               ES Pop top of stack into ES; increment stack pointer
//POP             17               SS Pop top of stack into SS; increment stack pointer
//POP             0F A1            FS Pop top of stack into FS; increment stack pointer
//POP             0F A9            GS Pop top of stack into GS; increment stack pointer
//POPA            61               Pop DI, SI, BP, BX, DX, CX, and AX
//POPAD           61               Pop EDI, ESI, EBP, EBX, EDX, ECX, and EAX
//POPF            9D               Pop top of stack into lower 16 bits of EFLAGS
//POPFD           9D               Pop top of stack into EFLAGS
//POR             0F EB /r         mm, mm/m64 Bitwise OR of mm/m64 and mm.
//POR             66 0F EB /r      xmm1, xmm2/m128 Bitwise OR of xmm2/m128 and xmm1.
//PREFETCHT0      0F 18 /1         m8 Move data from m8 closer to the processor using T0 hint.
//PREFETCHT1      0F 18 /2         m8 Move data from m8 closer to the processor using T1 hint.
//PREFETCHT2      0F 18 /3         m8 Move data from m8 closer to the processor using T2 hint.
//PREFETCHNTA     0F 18 /0         m8 Move data from m8 closer to the processor using NTA hint.
//PSADBW          0F F6 /r         mm1, mm2/m64 Computes the absolute differences of the packed unsigned byte integers from mm2 /m64 and mm1; differences are then summed to produce an unsigned word integer result.
//PSADBW          66 0F F6 /r      xmm1, xmm2/m128 Computes the absolute differences of the packed unsigned byte integers from xmm2 /m128 and xmm1; the 8 low differences and 8 high differences are then summed separately to produce two unsigned word integer results.
//PSHUFD          66 0F 70 /r ib   xmm1, xmm2/m128, imm8 Shuffle the doublewords in xmm2/m128 based on the encoding in imm8 and store the result in xmm1.
//PSHUFHW         F3 0F 70 /r ib   xmm1, xmm2/m128, imm8 Shuffle the high words in xmm2/m128 based on the encoding in imm8 and store the result in xmm1.
//PSHUFLW         F2 0F 70 /r ib   xmm1, xmm2/m128, imm8 Shuffle the low words in xmm2/m128 based on the encoding in imm8 and store the result in xmm1.
//PSHUFW          0F 70 /r ib      mm1, mm2/m64, imm8 Shuffle the words in mm2/m64 based on the encoding in imm8 and store the result in mm1.
//PSLLDQ          66 0F 73 /7 ib   xmm1, imm8 Shift xmm1 left by imm8 bytes while shifting in 0s.
//PSLLW           0F F1 /r         mm, mm/m64 Shift words in mm left mm/m64 while shifting in 0s.
//PSLLW           66 0F F1 /r      xmm1, xmm2/m128 Shift words in xmm1 left by xmm2/m128 while shifting in 0s.
//PSLLW           0F 71 /6 ib      mm, imm8 Shift words in mm left by imm8 while shifting in 0s.
//PSLLW           66 0F 71 /6 ib   xmm1, imm8 Shift words in xmm1 left by imm8 while shifting in 0s.
//PSLLD           0F F2 /r         mm, mm/m64 Shift doublewords in mm left by mm/m64 while shifting in 0s.
//PSLLD           66 0F F2 /r      xmm1, xmm2/m128 Shift doublewords in xmm1 left by xmm2/m128 while shifting in 0s.
//PSLLD           0F 72 /6 ib      mm, imm8 Shift doublewords in mm left by imm8 while shifting in 0s.
//PSLLD           66 0F 72 /6 ib   xmm1, imm8 Shift doublewords in xmm1 left by imm8 while shifting in 0s.
//PSLLQ           0F F3 /r         mm, mm/m64 Shift quadword in mm left by mm/m64 while shifting in 0s.
//PSLLQ           66 0F F3 /r      xmm1, xmm2/m128 Shift quadwords in xmm1 left by xmm2/m128 while shifting in 0s.
//PSLLQ           0F 73 /6 ib      mm, imm8 Shift quadword in mm left by imm8 while shifting in 0s.
//PSLLQ           66 0F 73 /6 ib   xmm1, imm8 Shift quadwords in xmm1 left by imm8 while shifting in 0s.
//PSRAW           0F E1 /r         mm, mm/m64 Shift words in mm right by mm/m64 while shifting in sign bits.
//PSRAW           66 0F E1 /r      xmm1, xmm2/m128 Shift words in xmm1 right by xmm2/m128 while shifting in sign bits.
//PSRAW           0F 71 /4 ib      mm, imm8 Shift words in mm right by imm8 while shifting in sign bits
//PSRAW           66 0F 71 /4 ib   xmm1, imm8 Shift words in xmm1 right by imm8 while shifting in sign bits
//PSRAD           0F E2 /r         mm, mm/m64 Shift doublewords in mm right by mm/m64 while shifting in sign bits.
//PSRAD           66 0F E2 /r      xmm1, xmm2/m128 Shift doubleword in xmm1 right by xmm2 /m128 while shifting in sign bits.
//PSRAD           0F 72 /4 ib      mm, imm8 Shift doublewords in mm right by imm8 while shifting in sign bits.
//PSRAD           66 0F 72 /4 ib   xmm1, imm8 Shift doublewords in xmm1 right by imm8 while shifting in sign bits.
//PSRLDQ          66 0F 73 /3 ib   xmm1, imm8 Shift xmm1 right by imm8 while shifting in 0s.
//PSRLW           0F D1 /r         mm, mm/m64 Shift words in mm right by amount specified in mm/m64 while shifting in 0s.
//PSRLW           66 0F D1 /r      xmm1, xmm2/m128 Shift words in xmm1 right by amount specified in xmm2/m128 while shifting in 0s.
//PSRLW           0F 71 /2 ib      mm, imm8 Shift words in mm right by imm8 while shifting in 0s.
//PSRLW           66 0F 71 /2 ib   xmm1, imm8 Shift words in xmm1 right by imm8 while shifting in 0s.
//PSRLD           0F D2 /r         mm, mm/m64 Shift doublewords in mm right by amount specified in mm/m64 while shifting in 0s.
//PSRLD           66 0F D2 /r      xmm1, xmm2/m128 Shift doublewords in xmm1 right by amount specified in xmm2 /m128 while shifting in 0s.
//PSRLD           0F 72 /2 ib      mm, imm8 Shift doublewords in mm right by imm8 while shifting in 0s.
//PSRLD           66 0F 72 /2 ib   xmm1, imm8 Shift doublewords in xmm1 right by imm8 while shifting in 0s.
//PSRLQ           0F D3 /r         mm, mm/m64 Shift mm right by amount specified in mm/m64 while shifting in 0s.
//PSRLQ           66 0F D3 /r      xmm1, xmm2/m128 Shift quadwords in xmm1 right by amount specified in xmm2/m128 while shifting in 0s.
//PSRLQ           0F 73 /2 ib      mm, imm8 Shift mm right by imm8 while shifting in 0s.
//PSRLQ           66 0F 73 /2 ib   xmm1, imm8 Shift quadwords in xmm1 right by imm8 while shifting in 0s.
//PSUBB           0F F8 /r         mm, mm/m64 Subtract packed byte integers in mm/m64 from packed byte integers in mm.
//PSUBB           66 0F F8 /r      xmm1, xmm2/m128 Subtract packed byte integers in xmm2/m128 from packed byte integers in xmm1.
//PSUBW           0F F9 /r         mm, mm/m64 Subtract packed word integers in mm/m64 from packed word integers in mm.
//PSUBW           66 0F F9 /r      xmm1, xmm2/m128 Subtract packed word integers in xmm2/m128 from packed word integers in xmm1.
//PSUBD           0F FA /r         mm, mm/m64 Subtract packed doubleword integers in mm/m64 from packed doubleword integers in mm.
//PSUBD           66 0F FA /r      xmm1, xmm2/m128 Subtract packed doubleword integers in xmm2/mem128 from packed doubleword integers in xmm1.
//PSUBQ           0F FB /r         mm1, mm2/m64 Subtract quadword integer in mm1 from mm2 /m64.
//PSUBQ           66 0F FB /r      xmm1, xmm2/m128 Subtract packed quadword integers in xmm1 from xmm2 /m128.
//PSUBSB          0F E8 /r         mm, mm/m64 Subtract signed packed bytes in mm/m64 from signed packed bytes in mm and saturate results.
//PSUBSB          66 0F E8 /r      xmm1, xmm2/m128 Subtract packed signed byte integers in xmm2/m128 from packed signed byte integers in xmm1 and saturate results.
//PSUBSW          0F E9 /r         mm, mm/m64 Subtract signed packed words in mm/m64 from signed packed words in mm and saturate results.
//PSUBSW          66 0F E9 /r      xmm1, xmm2/m128 Subtract packed signed word integers in xmm2/m128 from packed signed word integers in xmm1 and saturate results.
//PSUBUSB         0F D8 /r         mm, mm/m64 Subtract unsigned packed bytes in mm/m64 from unsigned packed bytes in mm and saturate result.
//PSUBUSB         66 0F D8 /r      xmm1, xmm2/m128 Subtract packed unsigned byte integers in xmm2/m128 from packed unsigned byte integers in xmm1 and saturate result.
//PSUBUSW         0F D9 /r         mm, mm/m64 Subtract unsigned packed words in mm/m64 from unsigned packed words in mm and saturate result.
//PSUBUSW         66 0F D9 /r      xmm1, xmm2/m128 Subtract packed unsigned word integers in xmm2/m128 from packed unsigned word integers in xmm1 and saturate result.
//PUNPCKHBW       0F 68 /r         mm, mm/m64 Unpack and interleave high-order bytes from mm and mm/m64 into mm.
//PUNPCKHBW       66 0F 68 /r      xmm1, xmm2/m128 Unpack and interleave high-order bytes from xmm1 and xmm2/m128 into xmm1.
//PUNPCKHWD       0F 69 /r         mm, mm/m64 Unpack and interleave high-order words from mm and mm/m64 into mm.
//PUNPCKHWD       66 0F 69 /r      xmm1, xmm2/m128 Unpack and interleave high-order words from xmm1 and xmm2/m128 into xmm1.
//PUNPCKHDQ       0F 6A /r         mm, mm/m64 Unpack and interleave high-order doublewords from mm and mm/m64 into mm.
//PUNPCKHDQ       66 0F 6A /r      xmm1, xmm2/m128 Unpack and interleave high-order doublewords from xmm1 and xmm2/m128 into xmm1.
//PUNPCKHQDQ      66 0F 6D /r      xmm1, xmm2/m128 Unpack and interleave high-order quadwords from xmm1 and xmm2/m128 into xmm1
//PUNPCKLBW       0F 60 /r         mm, mm/m32 Interleave low-order bytes from mm and mm/m32 into mm.
//PUNPCKLBW       66 0F 60 /r      xmm1, xmm2/m128 Interleave low-order bytes from xmm1 and xmm2/m128 into xmm1.
//PUNPCKLWD       0F 61 /r         mm, mm/m32 Interleave low-order words from mm and mm/m32 into mm.
//PUNPCKLWD       66 0F 61 /r      xmm1, xmm2/m128 Interleave low-order words from xmm1 and xmm2/m128 into xmm1.
//PUNPCKLDQ       0F 62 /r         mm, mm/m32 Interleave low-order doublewords from mm and mm/m32 into mm.
//PUNPCKLDQ       66 0F 62 /r      xmm1, xmm2/m128 Interleave low-order doublewords from xmm1 and xmm2/m128 into xmm1.
//PUNPCKLQDQ      66 0F 6C /r      xmm1, xmm2/m128 Interleave low-order quadwords from xmm1 and xmm2/m128 into xmm1 register
//PUSH            FF /6            r/m16 Push r/m16
//PUSH            FF /6            r/m32 Push r/m32
//PUSH            50+rw            r16 Push r16
//PUSH            50+rd            r32 Push r32
//PUSH            6A               imm8 Push imm8
//PUSH            68               imm16 Push imm16
//PUSH            68               imm32 Push imm32
//PUSH            0E               CS Push CS
//PUSH            16               SS Push SS
//PUSH            1E               DS Push DS
//PUSH            06               ES Push ES
//PUSH            0F A0            FS Push FS
//PUSH            0F A8            GS Push GS
//PUSHA           60               Push AX, CX, DX, BX, original SP, BP, SI, and DI
//PUSHAD          60               Push EAX, ECX, EDX, EBX, original ESP, EBP, ESI, and EDI
//PUSHF           9C               Push lower 16 bits of EFLAGS
//PUSHFD          9C               Push EFLAGS
//PXOR            0F EF /r         mm, mm/m64 Bitwise XOR of mm/m64 and mm.
//PXOR            66 0F EF /r      xmm1, xmm2/m128 Bitwise XOR of xmm2/m128 and xmm1.
//RCL             D0 /2            r/m8, 1 Rotate 9 bits (CF, r/m8) left once
//RCL             D2 /2            r/m8, CL Rotate 9 bits (CF, r/m8) left CL times
//RCL             C0 /2 ib         r/m8, imm8 Rotate 9 bits (CF, r/m8) left imm8 times
//RCL             D1 /2            r/m16, 1 Rotate 17 bits (CF, r/m16) left once
//RCL             D3 /2            r/m16, CL Rotate 17 bits (CF, r/m16) left CL times
//RCL             C1 /2 ib         r/m16, imm8 Rotate 17 bits (CF, r/m16) left imm8 times
//RCL             D1 /2            r/m32, 1 Rotate 33 bits (CF, r/m32) left once
//RCL             D3 /2            r/m32, CL Rotate 33 bits (CF, r/m32) left CL times
//RCL             C1 /2 ib         r/m32,i mm8 Rotate 33 bits (CF, r/m32) left imm8 times
//RCR             D0 /3            r/m8, 1 Rotate 9 bits (CF, r/m8) right once
//RCR             D2 /3            r/m8, CL Rotate 9 bits (CF, r/m8) right CL times
//RCR             C0 /3 ib         r/m8, imm8 Rotate 9 bits (CF, r/m8) right imm8 times
//RCR             D1 /3            r/m16, 1 Rotate 17 bits (CF, r/m16) right once
//RCR             D3 /3            r/m16, CL Rotate 17 bits (CF, r/m16) right CL times
//RCR             C1 /3 ib         r/m16, imm8 Rotate 17 bits (CF, r/m16) right imm8 times
//RCR             D1 /3            r/m32, 1 Rotate 33 bits (CF, r/m32) right once
//RCR             D3 /3            r/m32, CL Rotate 33 bits (CF, r/m32) right CL times
//RCR             C1 /3 ib         r/m32, imm8 Rotate 33 bits (CF, r/m32) right imm8 times
//ROL             D0 /0            r/m8, 1 Rotate 8 bits r/m8 left once
//ROL             D2 /0            r/m8, CL Rotate 8 bits r/m8 left CL times
//ROL             C0 /0 ib         r/m8, imm8 Rotate 8 bits r/m8 left imm8 times
//ROL             D1 /0            r/m16, 1 Rotate 16 bits r/m16 left once
//ROL             D3 /0            r/m16, CL Rotate 16 bits r/m16 left CL times
//ROL             C1 /0  ib        r/m16, imm8 Rotate 16 bits r/m16 left imm8 times
//ROL             D1 /0            r/m32, 1 Rotate 32 bits r/m32 left once
//ROL             D3 /0            r/m32, CL Rotate 32 bits r/m32 left CL times
//ROL             C1 /0 ib         r/m32, imm8 Rotate 32 bits r/m32 left imm8 times
//ROR             D0 /1            r/m8, 1 Rotate 8 bits r/m8 right once
//ROR             D2 /1            r/m8, CL Rotate 8 bits r/m8 right CL times
//ROR             C0 /1 ib         r/m8, imm8 Rotate 8 bits r/m16 right imm8 times
//ROR             D1 /1            r/m16, 1 Rotate 16 bits r/m16 right once
//ROR             D3 /1            r/m16, CL Rotate 16 bits r/m16 right CL times
//ROR             C1 /1 ib         r/m16, imm8 Rotate 16 bits r/m16 right imm8 times
//ROR             D1 /1            r/m32, 1 Rotate 32 bits r/m32 right once
//ROR             D3 /1            r/m32, CL Rotate 32 bits r/m32 right CL times
//ROR             C1 /1 ib         r/m32, imm8 Rotate 32 bits r/m32 right imm8 times
//RCPPS           0F 53 /r         xmm1, xmm2/m128 Computes the approximate reciprocals of the packed single-precision floating-point values in xmm2/m128 and stores the results in xmm1.
//RCPSS           F3 0F 53 /r      xmm1, xmm2/m32 Computes the approximate reciprocal of the scalar single-precision floating-point value in xmm2/m32 and stores the result in xmm1.
//RDMSR           0F 32            Load MSR specified by ECX into EDX:EAX
//RDPMC           0F 33            Read performance-monitoring counter specified by ECX into EDX:EAX
//RDTSC           0F 31            Read time-stamp counter into EDX:EAX
//REP INS         F3 6C            r/m8, DX Input (E)CX bytes from port DX into ES:[(E)DI]
//REP INS         F3 6D            r/m16, DX Input (E)CX words from port DX into ES:[(E)DI]
//REP INS         F3 6D            r/m32, DX Input (E)CX doublewords from port DX into ES:[(E)DI]
//REP MOVS        F3 A4            m8, m8 Move (E)CX bytes from DS:[(E)SI] to ES:[(E)DI]
//REP MOVS        F3 A5            m16, m16 Move (E)CX words from DS:[(E)SI] to ES:[(E)DI]
//REP MOVS        F3 A5            m32, m32 Move (E)CX doublewords from DS:[(E)SI] to ES:[(E)DI]
//REP OUTS        F3 6E            DX, r/m8 Output (E)CX bytes from DS:[(E)SI] to port DX
//REP OUTS        F3 6F            DX, r/m16 Output (E)CX words from DS:[(E)SI] to port DX
//REP OUTS        F3 6F            DX, r/m32 Output (E)CX doublewords from DS:[(E)SI] to port DX
//REP LODS        F3 AC            AL Load (E)CX bytes from DS:[(E)SI] to AL
//REP LODS        F3 AD            AX Load (E)CX words from DS:[(E)SI] to AX
//REP LODS        F3 AD            EAX Load (E)CX doublewords from DS:[(E)SI] to EAX
//REP STOS        F3 AA            m8 Fill (E)CX bytes at ES:[(E)DI] with AL
//REP STOS        F3 AB            m16 Fill (E)CX words at ES:[(E)DI] with AX
//REP STOS        F3 AB            m32 Fill (E)CX doublewords at ES:[(E)DI] with EAX
//REPE CMPS       F3 A6            m8, m8 Find nonmatching bytes in ES:[(E)DI] and DS:[(E)SI]
//REPE CMPS       F3 A7            m16, m16 Find nonmatching words in ES:[(E)DI] and DS:[(E)SI]
//REPE CMPS       F3 A7            m32, m32 Find nonmatching doublewords in ES:[(E)DI] and DS:[(E)SI]
//REPE SCAS       F3 AE            m8 Find non-AL byte starting at ES:[(E)DI]
//REPE SCAS       F3 AF            m16 Find non-AX word starting at ES:[(E)DI]
//REPE SCAS       F3 AF            m32 Find non-EAX doubleword starting at ES:[(E)DI]
//REPNE CMPS      F2 A6            m8, m8 Find matching bytes in ES:[(E)DI] and DS:[(E)SI]
//REPNE CMPS      F2 A7            m16, m16 Find matching words in ES:[(E)DI] and DS:[(E)SI]
//REPNE CMPS      F2 A7            m32, m32 Find matching doublewords in ES:[(E)DI] and DS:[(E)SI]
//REPNE SCAS      F2 AE            m8 Find AL, starting at ES:[(E)DI]
//REPNE SCAS      F2 AF            m16 Find AX, starting at ES:[(E)DI]
//REPNE SCAS      F2 AF            m32 Find EAX, starting at ES:[(E)DI]
//RET             C3               Near return to calling procedure
//RET             CB               Far return to calling procedure
//RET             C2 iw            imm16 Near return to calling procedure and pop imm16 bytes from stack
//RET             CA iw            imm16 Far return to calling procedure and pop imm16 bytes from stack
//RSM             0F AA            Resume operation of interrupted program
//RSQRTPS         0F 52 /r         xmm1, xmm2/m128 Computes the approximate reciprocals of the square roots of the packed single-precision floating-point values in xmm2/m128 and stores the results in xmm1. F3 0F 52 /r RSQRTSS xmm1, xmm2/m32 Computes the approximate reciprocal of the square root of the low single-precision floating-point value in xmm2/m32 and stores the results in xmm1.
//SAHF            9E               Loads SF, ZF, AF, PF, and CF from AH into EFLAGS register
//SAL             D0 /4            r/m8 Multiply r/m8 by 2, 1 time
//SAL             D2 /4            r/m8,CL Multiply r/m8 by 2, CL times
//SAL             C0 /4 ib         r/m8,imm8 Multiply r/m8 by 2, imm8 times
//SAL             D1 /4            r/m16 Multiply r/m16 by 2, 1 time
//SAL             D3 /4            r/m16,CL Multiply r/m16 by 2, CL times
//SAL             C1 /4 ib         r/m16,imm8 Multiply r/m16 by 2, imm8 times
//SAL             D1 /4            r/m32 Multiply r/m32 by 2, 1 time
//SAL             D3 /4            r/m32,CL Multiply r/m32 by 2, CL times
//SAL             C1 /4 ib         r/m32,imm8 Multiply r/m32 by 2, imm8 times
//SAR             D0 /7            r/m8 Signed divide* r/m8 by 2, 1 time
//SAR             D2 /7            r/m8,CL Signed divide* r/m8 by 2, CL times
//SAR             C0 /7 ib         r/m8,imm8 Signed divide* r/m8 by 2, imm8 times
//SAR             D1 /7            r/m16 Signed divide* r/m16 by 2, 1 time
//SAR             D3 /7            r/m16,CL Signed divide* r/m16 by 2, CL times
//SAR             C1 /7 ib         r/m16,imm8 Signed divide* r/m16 by 2, imm8 times
//SAR             D1 /7            r/m32 Signed divide* r/m32 by 2, 1 time
//SAR             D3 /7            r/m32,CL Signed divide* r/m32 by 2, CL times
//SAR             C1 /7 ib         r/m32,imm8 Signed divide* r/m32 by 2, imm8 times
//SHL             D0 /4            r/m8 Multiply r/m8 by 2, 1 time
//SHL             D2 /4            r/m8,CL Multiply r/m8 by 2, CL times
//SHL             C0 /4 ib         r/m8,imm8 Multiply r/m8 by 2, imm8 times
//SHL             D1 /4            r/m16 Multiply r/m16 by 2, 1 time
//SHL             D3 /4            r/m16,CL Multiply r/m16 by 2, CL times
//SHL             C1 /4 ib         r/m16,imm8 Multiply r/m16 by 2, imm8 times
//SHL             D1 /4            r/m32 Multiply r/m32 by 2, 1 time
//SHL             D3 /4            r/m32,CL Multiply r/m32 by 2, CL times
//SHL             C1 /4 ib         r/m32,imm8 Multiply r/m32 by 2, imm8 times
//SHR             D0 /5            r/m8 Unsigned divide r/m8 by 2, 1 time
//SHR             D2 /5            r/m8,CL Unsigned divide r/m8 by 2, CL times
//SHR             C0 /5 ib         r/m8,imm8 Unsigned divide r/m8 by 2, imm8 times
//SHR             D1 /5            r/m16 Unsigned divide r/m16 by 2, 1 time
//SHR             D3 /5            r/m16,CL Unsigned divide r/m16 by 2, CL times
//SHR             C1 /5 ib         r/m16,imm8 Unsigned divide r/m16 by 2, imm8 times
//SHR             D1 /5            r/m32 Unsigned divide r/m32 by 2, 1 time
//SHR             D3 /5            r/m32,CL Unsigned divide r/m32 by 2, CL times
//SHR             C1 /5 ib         r/m32,imm8 Unsigned divide r/m32 by 2, imm8 times
//SBB             1C ib            AL,imm8 Subtract with borrow imm8 from AL
//SBB             1D iw            AX,imm16 Subtract with borrow imm16 from AX
//SBB             1D id            EAX,imm32 Subtract with borrow imm32 from EAX
//SBB             80 /3 ib         r/m8,imm8 Subtract with borrow imm8 from r/m8
//SBB             81 /3 iw         r/m16,imm16 Subtract with borrow imm16 from r/m16
//SBB             81 /3 id         r/m32,imm32 Subtract with borrow imm32 from r/m32
//SBB             83 /3 ib         r/m16,imm8 Subtract with borrow sign-extended imm8 from r/m16
//SBB             83 /3 ib         r/m32,imm8 Subtract with borrow sign-extended imm8 from r/m32
//SBB             18 /r            r/m8,r8 Subtract with borrow r8 from r/m8
//SBB             19 /r            r/m16,r16 Subtract with borrow r16 from r/m16
//SBB             19 /r            r/m32,r32 Subtract with borrow r32 from r/m32
//SBB             1A /r            r8,r/m8 Subtract with borrow r/m8 from r8
//SBB             1B /r            r16,r/m16 Subtract with borrow r/m16 from r16
//SBB             1B /r            r32,r/m32 Subtract with borrow r/m32 from r32
//SCAS            AE               m8 Compare AL with byte at ES:(E)DI and set status flags
//SCAS            AF               m16 Compare AX with word at ES:(E)DI and set status flags
//SCAS            AF               m32 Compare EAX with doubleword at ES(E)DI and set status flags
//SCASB           AE               Compare AL with byte at ES:(E)DI and set status flags
//SCASW           AF               Compare AX with word at ES:(E)DI and set status flags
//SCASD           AF               Compare EAX with doubleword at ES:(E)DI and set status flags
//SETA            0F 97            r/m8 Set byte if above (CF=0 and ZF=0)
//SETAE           0F 93            r/m8 Set byte if above or equal (CF=0)
//SETB            0F 92            r/m8 Set byte if below (CF=1)
//SETBE           0F 96            r/m8 Set byte if below or equal (CF=1 or ZF=1)
//SETC            0F 92            r/m8 Set if carry (CF=1)
//SETE            0F 94            r/m8 Set byte if equal (ZF=1)
//SETG            0F 9F            r/m8 Set byte if greater (ZF=0 and SF=OF)
//SETGE           0F 9D            r/m8 Set byte if greater or equal (SF=OF)
//SETL            0F 9C            r/m8 Set byte if less (SF<>OF)
//SETLE           0F 9E            r/m8 Set byte if less or equal (ZF=1 or SF<>OF)
//SETNA           0F 96            r/m8 Set byte if not above (CF=1 or ZF=1)
//SETNAE          0F 92            r/m8 Set byte if not above or equal (CF=1)
//SETNB           0F 93            r/m8 Set byte if not below (CF=0)
//SETNBE          0F 97            r/m8 Set byte if not below or equal (CF=0 and ZF=0)
//SETNC           0F 93            r/m8 Set byte if not carry (CF=0)
//SETNE           0F 95            r/m8 Set byte if not equal (ZF=0)
//SETNG           0F 9E            r/m8 Set byte if not greater (ZF=1 or SF<>OF)
//SETNGE          0F 9C            r/m8 Set if not greater or equal (SF<>OF)
//SETNL           0F 9D            r/m8 Set byte if not less (SF=OF)
//SETNLE          0F 9F            r/m8 Set byte if not less or equal (ZF=0 and SF=OF)
//SETNO           0F 91            r/m8 Set byte if not overflow (OF=0)
//SETNP           0F 9B            r/m8 Set byte if not parity (PF=0)
//SETNS           0F 99            r/m8 Set byte if not sign (SF=0)
//SETNZ           0F 95            r/m8 Set byte if not zero (ZF=0)
//SETO            0F 90            r/m8 Set byte if overflow (OF=1)
//SETP            0F 9A            r/m8 Set byte if parity (PF=1)
//SETPE           0F 9A            r/m8 Set byte if parity even (PF=1)
//SETPO           0F 9B            r/m8 Set byte if parity odd (PF=0)
//SETS            0F 98            r/m8 Set byte if sign (SF=1)
//SETZ            0F 94            r/m8 Set byte if zero (ZF=1)
//SFENCE          0F AE /7         Serializes store operations.
//SGDT            0F 01 /0         m Store GDTR to m
//SIDT            0F 01 /1         m Store IDTR to m
//SHLD            0F A4            r/m16, r16, imm8 Shift r/m16 to left imm8 places while shifting bits from r16 in from the right
//SHLD            0F A5            r/m16, r16, CL Shift r/m16 to left CL places while shifting bits from r16 in from the right
//SHLD            0F A4            r/m32, r32, imm8 Shift r/m32 to left imm8 places while shifting bits from r32 in from the right
//SHLD            0F A5            r/m32, r32, CL Shift r/m32 to left CL places while shifting bits from r32 in from the right
//SHRD            0F AC            r/m16, r16, imm8 Shift r/m16 to right imm8 places while shifting bits from r16 in from the left
//SHRD            0F AD            r/m16, r16, CL Shift r/m16 to right CL places while shifting bits from r16 in from the left
//SHRD            0F AC            r/m32, r32, mm8 Shift r/m32 to right imm8 places while shifting bits from r32 in from the left
//SHRD            0F AD            r/m32, r32, CL Shift r/m32 to right CL places while shifting bits from r32 in from the left
//SHUFPD          66 0F C6 /r ib   xmm1, xmm2/m128, imm8 Shuffle packed double-precision floating-point values selected by imm8 from xmm1 and xmm1/m128 to xmm1.
//SHUFPS          0F C6 /r ib      xmm1, xmm2/m128, imm8 Shuffle packed single-precision floating-point values selected by imm8 from xmm1 and xmm1/m128 to xmm1.
//SLDT            0F 00 /0         r/m16 Stores segment selector from LDTR in r/m16
//SLDT            0F 00 /0         r/m32 Store segment selector from LDTR in low-order 16 bits of r/m32
//SMSW            0F 01 /4         r/m16 Store machine status word to r/m16
//SMSW            0F 01 /4         r32/m16 Store machine status word in low-order 16 bits of r32/m16; high-order 16 bits of r32 are undefined
//SQRTPD          66 0F 51 /r      xmm1, xmm2/m128 Computes square roots of the packed double-precision floating-point values in xmm2/m128 and stores the results in xmm1.
//SQRTPS          0F 51 /r         xmm1, xmm2/m128 Computes square roots of the packed single-precision floating-point values in xmm2/m128 and stores the results in xmm1.
//SQRTSD          F2 0F 51 /r      xmm1, xmm2/m64 Computes square root of the low double-precision floating-point value in xmm2/m64 and stores the results in xmm1.
//SQRTSS          F3 0F 51 /r      xmm1, xmm2/m32 Computes square root of the low single-precision floating-point value in xmm2/m32 and stores the results in xmm1.
//STC             F9               Set CF flag
//STD             FD               Set DF flag
//STI             FB               Set interrupt flag; external, maskable interrupts enabled at the end of the next instruction
//STMXCSR         0F AE /3         m32 Store contents of MXCSR register to m32.
//STOS            AA               m8 Store AL at address ES:(E)DI
//STOS            AB               m16 Store AX at address ES:(E)DI
//STOS            AB               m32 Store EAX at address ES:(E)DI
//STOSB           AA               Store AL at address ES:(E)DI
//STOSW           AB               Store AX at address ES:(E)DI
//STOSD           AB               Store EAX at address ES:(E)DI
//STR             0F 00 /1         r/m16 Stores segment selector from TR in r/m16
//SUB             2C ib            AL,imm8 Subtract imm8 from AL
//SUB             2D iw            AX,imm16 Subtract imm16 from AX
//SUB             2D id            EAX,imm32 Subtract imm32 from EAX
//SUB             80 /5 ib         r/m8,imm8 Subtract imm8 from r/m8
//SUB             81 /5 iw         r/m16,imm16 Subtract imm16 from r/m16
//SUB             81 /5 id         r/m32,imm32 Subtract imm32 from r/m32
//SUB             83 /5 ib         r/m16,imm8 Subtract sign-extended imm8 from r/m16
//SUB             83 /5 ib         r/m32,imm8 Subtract sign-extended imm8 from r/m32
//SUB             28 /r            r/m8,r8 Subtract r8 from r/m8
//SUB             29 /r            r/m16,r16 Subtract r16 from r/m16
//SUB             29 /r            r/m32,r32 Subtract r32 from r/m32
//SUB             2A /r            r8,r/m8 Subtract r/m8 from r8
//SUB             2B /r            r16,r/m16 Subtract r/m16 from r16
//SUB             2B /r            r32,r/m32 Subtract r/m32 from r32
//SUBPD           66 0F 5C /r      xmm1, xmm2/m128 Subtract packed double-precision floating-point values in xmm2/m128 from xmm1.
//SUBPS           0F 5C /r         xmm1 xmm2/m128 Subtract packed single-precision floating-point values in xmm2/mem from xmm1.
//SUBSD           F2 0F 5C /r      xmm1, xmm2/m64 Subtracts the low double-precision floating-point values in xmm2/mem64 from xmm1.
//SUBSS           F3 0F 5C /r      xmm1, xmm2/m32 Subtract the lower single-precision floating-point values in xmm2/m32 from xmm1.
//SYSENTER        0F 34            Fast call to privilege level 0 system procedures
//SYSEXIT         0F 35            Fast return to privilege level 3 user code.
//TEST            A8 ib            AL,imm8 AND imm8 with AL; set SF, ZF, PF according to result
//TEST            A9 iw            AX,imm16 AND imm16 with AX; set SF, ZF, PF according to result
//TEST            A9 id            EAX,imm32 AND imm32 with EAX; set SF, ZF, PF according to result
//TEST            F6 /0 ib         r/m8,imm8 AND imm8 with r/m8; set SF, ZF, PF according to result
//TEST            F7 /0 iw         r/m16,imm16 AND imm16 with r/m16; set SF, ZF, PF according to result
//TEST            F7 /0 id         r/m32,imm32 AND imm32 with r/m32; set SF, ZF, PF according to result
//TEST            84 /r            r/m8,r8 AND r8 with r/m8; set SF, ZF, PF according to result
//TEST            85 /r            r/m16,r16 AND r16 with r/m16; set SF, ZF, PF according to result
//TEST            85 /r            r/m32,r32 AND r32 with r/m32; set SF, ZF, PF according to result
//UCOMISD         66 0F 2E /r      xmm1, xmm2/m64 Compares (unordered) the low double-precision floating-point values in xmm1 and xmm2/m64 and set the EFLAGS accordingly.
//UCOMISS         0F 2E /r         xmm1, xmm2/m32 Compare lower single-precision floating-point value in xmm1 register with lower single-precision floating-point value in xmm2/mem and set the status flags accordingly.
//UD2             0F 0B            Raise invalid opcode exception
//UNPCKHPD        66 0F 15 /r      xmm1, xmm2/m128 Unpacks and Interleaves double-precision floating-point values from high quadwords of xmm1 and xmm2/m128.
//UNPCKHPS        0F 15 /r         xmm1, xmm2/m128 Unpacks and Interleaves single-precision floating-point values from high quadwords of xmm1 and xmm2/mem into xmm1.
//UNPCKLPD        66 0F 14 /r      xmm1, xmm2/m128 Unpacks and Interleaves double-precision floatingpoint values from low quadwords of xmm1 and xmm2/m128.
//UNPCKLPS        0F 14 /r         xmm1, xmm2/m128 Unpacks and Interleaves single-precision floatingpoint values from low quadwords of xmm1 and xmm2/mem into xmm1.
//VERR            0F 00 /4         r/m16 Set ZF=1 if segment specified with r/m16 can be read
//VERW            0F 00 /5         r/m16 Set ZF=1 if segment specified with r/m16 can be written
//WAIT            9B               Check pending unmasked floating-point exceptions.
//FWAIT           9B               Check pending unmasked floating-point exceptions.
//WBINVD          0F 09            Write back and flush Internal caches; initiate writing-back and flushing of external caches.
//WRMSR           0F 30            Write the value in EDX:EAX to MSR specified by ECX
//XADD            0F C0 /r         r/m8, r8 Exchange r8 and r/m8; load sum into r/m8.
//XADD            0F C1 /r         r/m16, r16 Exchange r16 and r/m16; load sum into r/m16.
//XADD            0F C1 /r         r/m32, r32 Exchange r32 and r/m32; load sum into r/m32.
//XCHG            90+rw            AX, 16 Exchange r16 with AX
//XCHG            90+rw            r16, X Exchange AX with r16
//XCHG            90+rd            EAX, r32 Exchange r32 with EAX
//XCHG            90+rd            r32, EAX Exchange EAX with r32
//XCHG            86 /r            r/m8, r8 Exchange r8 (byte register) with byte from r/m8
//XCHG            86 /r            r8, r/m8 Exchange byte from r/m8 with r8 (byte register)
//XCHG            87 /r            r/m16, r16 Exchange r16 with word from r/m16
//XCHG            87 /r            r16, r/m16 Exchange word from r/m16 with r16
//XCHG            87 /r            r/m32, r32 Exchange r32 with doubleword from r/m32
//XCHG            87 /r            r32, r/m32 Exchange doubleword from r/m32 with r32
//XLAT            D7               m8 Set AL to memory byte DS:[(E)BX + unsigned AL]
//XLATB           D7               Set AL to memory byte DS:[(E)BX + unsigned AL]
//XOR             34 ib            AL,imm8 AL  imm8
//XOR             35 iw            AX,imm16 AX  imm16
//XOR             35 id            EAX,imm32 EAX  imm32
//XOR             80 /6 ib         r/m8,imm8 r/m8  imm8
//XOR             81 /6 iw         r/m16,imm16 r/m16  imm16
//XOR             81 /6 id         r/m32,imm32 r/m32  imm32
//XOR             83 /6 ib         r/m16,imm8 r/m16  imm8 (sign-extended)
//XOR             83 /6 ib         r/m32,imm8 r/m32  imm8 (sign-extended)
//XOR             30 /r            r/m8,r8 r/m8  r8
//XOR             31 /r            r/m16,r16 r/m16  r16
//XOR             31 /r            r/m32,r32 r/m32  r32
//XOR             32 /r            r8,r/m8 r8  r/m8
//XOR             33 /r            r16,r/m16 r8  r/m8
//XOR             33 /r            r32,r/m32 r8  r/m8
//XORPD           66 0F 57 /r      xmm1, xmm2/m128 Bitwise exclusive-OR of xmm2/m128 and xmm1
//XORPS           0F 57 /r         xmm1, xmm2/m128 Bitwise exclusive-OR of xmm2/m128 and xmm1

    }
}
