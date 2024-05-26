//using Microsoft.VisualBasic;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp81
{
    public class Z80
    {


        // /*******************************************************************************
        // modZ80.bas within vb81.vbp
        // 
        // Complete Z80 emulation, including (as far as I know) the
        // correct emulation of bits 3 and 5 of F, and undocumented ops.
        // Please mail me if you find any bugs in the emulation!
        // 
        // Author: Chris Cowley <ccowley@grok.co.uk>
        // 
        // Copyright (C)1999-2002  Grok Developments Ltd.
        // http://www.grok.co.uk/
        // 
        // This program is free software; you can redistribute it and/or
        // modify it under the terms of the GNU General Public License
        // as published by the Free Software Foundation; either version 2
        // of the License, or (at your option) any later version.
        // This program is distributed in the hope that it will be useful,
        // but WITHOUT ANY WARRANTY; without even the implied warranty of
        // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        // GNU General Public License for more details.
        // 
        // You should have received a copy of the GNU General Public License
        // along with this program; if not, write to the Free Software
        // Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
        // 
        // *******************************************************************************/

        // Ported to C# .NET 6.0  by Allan Macpherson Feb 2024 
        // GOTOs and labels replaced by CASE statements as the speed improvement not needed
        // on a 2024 PC :-)

        private int regA;
        private int regHL;
        private int regB;
        private int regC;
        private int regDE;

        private bool fS;
        private bool fZ;
        private bool f5;
        private bool fH;
        private bool f3;
        private bool fPV;
        private bool fN;
        private bool fC;


        // // Flag positions
        private const int F_C = 1;
        private const int F_N = 2;
        private const int F_PV = 4;
        private const int F_3 = 8;
        private const int F_H = 16;
        private const int F_5 = 32;
        private const int F_Z = 64;
        private const int F_S = 128;

        // // Alternate registers //
        private int regAF_;
        private int regHL_;
        private int regBC_;
        private int regDE_;

        // // Index registers  - ID used as temp for ix/iy //
        private int regIX;
        private int regIY;
        private int regID;

        // // Stack pointer and program counter
        private int regSP;
        private int regPC;

        // // Interrupt registers and flip-flops and refresh registers //
        private int intI;
        private int intR;
        private int intRTemp;
        private bool intIFF1;
        private bool intIFF2;
        private byte intIM;

        private Boolean[] Parity = new Boolean[257];

        private Memory _mem; //new Memory();
        private ZX81 _zx81;


        public Z80(Memory mem, ZX81 z)
        {
            _mem = mem;
            _zx81 = z;
            intIFF1 = true;
            intIFF2 = true;
            intIM = 2;
            InitParity();
        }

        public void InitParity()
        {
            int iCounter;
            byte j;
            Boolean p;

            for (iCounter = 0; iCounter <= 255; iCounter++)
            {
                p = true;
                for (j = 0; j <= 7; j++)
                {
                    if ((iCounter & (long)Math.Round(Math.Pow(2d, j))) != 0L)
                        p = !p;
                }
                Parity[iCounter] = p;
            }
        }

        public void SetintIFF1(bool a)
        {
            intIFF1 = a;
        }

        public void SetintIFF2(bool a)
        {
            intIFF2 = a;
        }

        public void SetintIM(byte a)
        {
            intIM = a;
        }


        public int GetintI()
        {
            return intI;
        }


        public void SetPC(int Address)
        {
            regPC = (int)Address;
        }

        private void Adc_a(int b)
        {
            int wans;
            int ans;
            int c = 0;

            if (fC) c = 1;

            wans = (int)(regA + b + c);
            ans = (int)(wans & 0xFFL);

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fC = (wans & 0x100L) != 0L;
            fPV = ((regA ^ ~b & 0xFFFF) & (regA ^ ans) & 0x80L) != 0L;

            fH = ((regA & 0xFL) + (b & 0xFL) + c & F_H) != 0L;
            fN = false;

            regA = (int)ans;
        }

        private void Add_a(int b)
        {
            int wans;
            int ans;

            wans = regA + b;
            ans = (wans & 0xFF);

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fC = (wans & 0x100L) != 0L;
            fPV = (((long)regA ^ ~b & 0xFFFF) & (regA ^ ans) & 0x80L) != 0L;
            fH = ((regA & 0xFL) + (b & 0xFL) & F_H) != 0L;
            fN = false;

            regA = (int)ans;
        }
        private int Adc16(int a, int b)
        {
            int c = 0;
            int lans;
            int ans;

            if (fC)
                c = 1;

            lans = a + b + c;
            ans = lans & 0xFFFF;

            fS = (ans & F_S * 256L) != 0L;
            f3 = (ans & F_3 * 256L) != 0L;
            f5 = (ans & F_5 * 256L) != 0L;
            fZ = ans == 0L;
            fC = (lans & 0x10000L) != 0L;
            fPV = ((a ^ ~b & 0xFFFF) & (a ^ ans) & 0x8000L) != 0L;
            fH = ((a & 0xFFFL) + (b & 0xFFFL) + c & 0x1000L) != 0L;
            fN = false;

            return ans;
        }
        private int Add16(int a, int b)
        {
        
            int lans;
            int ans;

            lans = a + b;
            ans = lans & 0xFFFF;

            f3 = (ans & F_3 * 256L) != 0L;
            f5 = (ans & F_5 * 256L) != 0L;
            fC = (lans & 0x10000L) != 0L;
            fH = ((a & 0xFFFL) + (b & 0xFFFL) & 0x1000L) != 0L;
            fN = false;

            return ans;
 
        }
        private void And_a(int b)
        {
            regA = (int)(regA & b);

            fS = (regA & F_S) != 0;
            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fH = true;
            fPV = Parity[regA];
            fZ = regA == 0;
            fN = false;
            fC = false;
        }
        private void Bit(int b, int r)
        {
            Boolean IsbitSet;

            IsbitSet = ((r & b) != 0L);
            fN = false;
            fH = true;
            f3 = (r & F_3) != 0L;
            f5 = (r & F_5) != 0L;

            if (b == F_S)
                fS = IsbitSet;
            else
                fS = false;

            fZ = !IsbitSet;
            fPV = fZ;
        }

        private static int BitRes(int bit, int val)
        {
           return val & (~bit & 0xFFFF);
         
        }
        public static int BitSet(int bit, int val)
        {
            return  val | bit;
        }

        private void Ccf()
        {
            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fH = fC;
            fN = false;
            fC = !fC;
        }


     


        public void Cp_a(int b)
        {
            int a;
            int wans;
            int ans;

            a = regA;
            wans = a - b;
            ans = wans & 0xFF;

            fS = (ans & F_S) != 0L;
            f3 = (b & F_3) != 0L;
            f5 = (b & F_5) != 0L;
            fN = true;
            fZ = ans == 0L;
            fC = (wans & 0x100L) != 0L;
            fH = ((a & 0xFL) - (b & 0xFL) & F_H) != 0L;
            fPV = ((a ^ b) & (a ^ ans) & 0x80L) != 0L;
        }

        private void Cpl_a()
        {
            regA = (int)((regA ^ 0xFFL) & 0xFFL);

            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fH = true;
            fN = true;
        }
        private void Daa_a()
        {
            int ans;
            int incr = 0;
            Boolean carry;

            ans = regA;
            carry = fC;

            if (fH == true | (ans & 0xFL) > 0x9L)
            {
                incr = incr | 0x6;
            }

            if (carry == true | ans > 0x9FL)
            {
                incr = incr | 0x60;
            }

            if (ans > 0x8FL & (ans & 0xF) > 9)
            {
                incr = incr | 0x60;
            }

            if (ans > 0x99L)
            {
                carry = true;
            }
            if (fN == true)
            {
                Sub_a(incr);
            }
            else
            {
                Add_a(incr);
            }

            ans = regA;
            fC = carry;
            fPV = Parity[ans];
        }
        private static int Dec16(int a)
        {
        
            return a - 1 & 0xFFFF;

        }
        private void Ex_af_af()
        {
            int t;

            t = GetAF();
            SetAF(GetAF_());
            SetAF_(t);
        }
        private int Execute_cb()
        {

            int xxx;


            intRTemp = intRTemp + 1;

            xxx = Nxtpcb();

            switch (xxx)
            {
                case 0:
                    {
                        // 000 RLC B
                        regB = (int)Rlc(regB);
                        return 8;

                    }
                case 1:
                    {
                        // 001 RLC C
                        regC = (int)Rlc(regC);
                        return 8;

                    }
                case 2:
                    {
                        // 002 RLC D
                        SetD(Rlc(GetD()));
                        return 8;

                    }
                case 3:
                    {
                        // 003 RLC E
                        SetE(Rlc(GetE()));
                        return 8;

                    }
                case 4:
                    {
                        // 004 RLC H
                        SetH(Rlc(GetH()));
                        return 8;

                    }
                case 5:
                    {
                        // 005 RLC L
                        SetL(Rlc(GetL()));
                        return 8;

                    }
                case 6:
                    {
                        // 006 RLC (HL)
                        _mem.Pokeb(regHL, Rlc(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 7:
                    {
                        // 007 RLC A
                        regA = (int)Rlc(regA);
                        return 8;

                    }
                case 8:
                    {
                        // 008 RRC B
                        regB = (int)Rrc(regB);
                        return 8;

                    }
                case 9:
                    {
                        // 009 RRC C
                        regC = (int)Rrc(regC);
                        return 8;

                    }
                case 10:
                    {
                        // 010 RRC D
                        SetD(Rrc(GetD()));
                        return 8;

                    }
                case 11:
                    {
                        // 011 RRC E
                        SetE(Rrc(GetE()));
                        return 8;

                    }
                case 12:
                    {
                        // 012 RRC H
                        SetH(Rrc(GetH()));
                        return 8;

                    }
                case 13:
                    {
                        // 013 RRC L
                        SetL(Rrc(GetL()));
                        return 8;

                    }
                case 14:
                    {
                        // 014 RRC (HL)
                        _mem.Pokeb(regHL, Rrc(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 15:
                    {
                        // 015 RRC A
                        regA = (int)Rrc(regA);
                        return 8;

                    }
                case 16:
                    {
                        // 016 RL B
                        regB = (int)Rl(regB);
                        return 8;

                    }
                case 17:
                    {
                        // 017 RL C
                        regC = (int)Rl(regC);
                        return 8;

                    }
                case 18:
                    {
                        // 018 RL D
                        SetD(Rl(GetD()));
                        return 8;

                    }
                case 19:
                    {
                        // 019 RL E
                        SetE(Rl(GetE()));
                        return 8;

                    }
                case 20:
                    {
                        // 020 RL H
                        SetH(Rl(GetH()));
                        return 8;

                    }
                case 21:
                    {
                        // 021 RL L
                        SetL(Rl(GetL()));
                        return 8;

                    }
                case 22:
                    {
                        // 022 RL (HL)
                        _mem.Pokeb(regHL, Rl(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 23:
                    {
                        // 023 RL A
                        regA = (int)Rl(regA);
                        return 8;

                    }
                case 24:
                    {
                        // 024 RR B
                        regB = (int)Rr(regB);
                        return 8;

                    }
                case 25:
                    {
                        // 025 RR C
                        regC = (int)Rr(regC);
                        return 8;

                    }
                case 26:
                    {
                        // 026 RR D
                        SetD(Rr(GetD()));
                        return 8;

                    }
                case 27:
                    {
                        // 027 RR E
                        SetE(Rr(GetE()));
                        return 8;

                    }
                case 28:
                    {
                        // 028 RR H
                        SetH(Rr(GetH()));
                        return 8;

                    }
                case 29:
                    {
                        // 029 RR L
                        SetL(Rr(GetL()));
                        return 8;

                    }
                case 30:
                    {
                        // 030 RR (HL)
                        _mem.Pokeb(regHL, Rr(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 31:
                    {
                        // 031 RR A
                        regA = (int)Rr(regA);
                        return 8;

                    }
                case 32: // SLA B
                    {
                        regB = (int)sla(regB);
                        return 8;

                    }
                case 33: // SLA C
                    {
                        regC = (int)sla(regC);
                        return 8;

                    }
                case 34: // SLA D
                    {
                        SetD(sla(GetD()));
                        return 8;

                    }
                case 35: // SLA E
                    {
                        SetE(sla(GetE()));
                        return 8;

                    }
                case 36: // SLA H
                    {
                        SetH(sla(GetH()));
                        return 8;

                    }
                case 37: // SLA L
                    {
                        SetL(sla(GetL()));
                        return 8;

                    }
                case 38: // SLA (HL)
                    {
                        _mem.Pokeb(regHL, sla(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 39: // SLA A
                    {
                        regA = (int)sla(regA);
                        return 8;

                    }
                case 40: // SRA B
                    {
                        regB = (int)Sra(regB);
                        return 8;

                    }
                case 41: // SRA C
                    {
                        regC = (int)Sra(regC);
                        return 8;

                    }
                case 42: // SRA D
                    {
                        SetD(Sra(GetD()));
                        return 8;

                    }
                case 43: // SRA E
                    {
                        SetE(Sra(GetE()));
                        return 8;

                    }
                case 44: // SRA H
                    {
                        SetH(Sra(GetH()));
                        return 8;

                    }
                case 45:  // SRA L
                    {
                        SetL(Sra(GetL()));
                        return 8;

                    }
                case 46: // SRA (HL)
                    {
                        _mem.Pokeb(regHL, Sra(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 47: // SRA A
                    {
                        regA = (int)Sra(regA);
                        return 8;

                    }
                case 48: // SLS B
                    {
                        regB = (int)Sls(regB);
                        return 8;

                    }
                case 49: // SLS C
                    {
                        regC = (int)Sls(regC);
                        return 8;

                    }
                case 50: // SLS D
                    {
                        SetD(Sls(GetD()));
                        return 8;

                    }
                case 51: // SLS E
                    {
                        SetE(Sls(GetE()));
                        return 8;

                    }
                case 52: // SLS H
                    {
                        SetH(Sls(GetH()));
                        return 8;

                    }
                case 53: // SLS L
                    {
                        SetL(Sls(GetL()));
                        return 8;

                    }
                case 54: // SLS (HL)
                    {
                        _mem.Pokeb(regHL, Sls(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 55: // SLS A
                    {
                        regA = (int)Sls(regA);
                        return 8;

                    }
                case 56: // SRL B
                    {
                        regB = (int)Srl(regB);
                        return 8;

                    }
                case 57: // SRL C
                    {
                        regC = (int)Srl(regC);
                        return 8;

                    }
                case 58: // SRL D
                    {
                        SetD(Srl(GetD()));
                        return 8;

                    }
                case 59: // SRL E
                    {
                        SetE(Srl(GetE()));
                        return 8;

                    }
                case 60: // SRL H
                    {
                        SetH(Srl(GetH()));
                        return 8;

                    }
                case 61: // SRL L
                    {
                        SetL(Srl(GetL()));
                        return 8;

                    }
                case 62: // SRL (HL)
                    {
                        _mem.Pokeb(regHL, Srl(_mem.Peekb(regHL)));
                        return 15;

                    }
                case 63: // SRL A
                    {
                        regA = (int)Srl(regA);
                        return 8;

                    }
                case 64:
                    {
                        // 064 BIT 0,B
                        Bit(0x1, regB);
                        return 8;

                    }
                case 65:
                    {
                        // 065 ' BIT 0,C
                        Bit(1, regC);
                        return 8;

                    }
                case 66:
                    {
                        // 066 BIT 0,D
                        Bit(1, GetD());
                        return 8;

                    }
                case 67:
                    {
                        // 067 BIT 0,E
                        Bit(1, GetE());
                        return 8;

                    }
                case 68:
                    {
                        // 068 BIT 0,H
                        Bit(1, GetH());
                        return 8;

                    }
                case 69:
                    {
                        // 069 BIT 0,L
                        Bit(1, GetL());
                        return 8;

                    }
                case 70:
                    {
                        // 070 BIT 0,(HL)
                        Bit(1, _mem.Peekb(regHL));
                        return 12;

                    }
                case 71:
                    {
                        // 071 BIT 0,A
                        Bit(1, regA);
                        return 8;

                    }
                case 72: // BIT 1,B
                    {
                        Bit(2, regB);
                        return 8;

                    }
                case 73: // BIT 1,C
                    {
                        Bit(2, regC);
                        return 8;

                    }
                case 74: // BIT 1,D
                    {
                        Bit(2, GetD());
                        return 8;

                    }
                case 75: // BIT 1,E
                    {
                        Bit(2, GetE());
                        return 8;

                    }
                case 76: // BIT 1,H
                    {
                        Bit(2, GetH());
                        return 8;

                    }
                case 77: // BIT 1,L
                    {
                        Bit(2, GetL());
                        return 8;

                    }
                case 78: // BIT 1,(HL)
                    {
                        Bit(2, _mem.Peekb(regHL));
                        return 12;

                    }
                case 79: // BIT 1,A
                    {
                        Bit(2, regA);
                        return 8;

                    }
                case 80: // BIT 2,B
                    {
                        Bit(4, regB);
                        return 8;

                    }
                case 81: // BIT 2,C
                    {
                        Bit(4, regC);
                        return 8;

                    }
                case 82: // BIT 2,D
                    {
                        Bit(4, GetD());
                        return 8;

                    }
                case 83: // BIT 2,E
                    {
                        Bit(4, GetE());
                        return 8;

                    }
                case 84: // BIT 2,H
                    {
                        Bit(4, GetH());
                        return 8;

                    }
                case 85: // BIT 2,L
                    {
                        Bit(4, GetL());
                        return 8;

                    }
                case 86: // BIT 2,(HL)
                    {
                        Bit(4, _mem.Peekb(regHL));
                        return 12;

                    }
                case 87: // BIT 2,A
                    {
                        Bit(4, regA);
                        return 8;

                    }
                case 88: // BIT 3,B
                    {
                        Bit(8, regB);
                        return 8;

                    }
                case 89: // BIT 3,C
                    {
                        Bit(8, regC);
                        return 8;

                    }
                case 90: // BIT 3,D
                    {
                        Bit(8, GetD());
                        return 8;

                    }
                case 91: // BIT 3,E
                    {
                        Bit(8, GetE());
                        return 8;

                    }
                case 92: // BIT 3,H
                    {
                        Bit(8, GetH());
                        return 8;

                    }
                case 93: // BIT 3,L
                    {
                        Bit(8, GetL());
                        return 8;

                    }
                case 94: // BIT 3,(HL)
                    {
                        Bit(8, _mem.Peekb(regHL));
                        return 12;

                    }
                case 95: // BIT 3,A
                    {
                        Bit(8, regA);
                        return 8;

                    }
                case 96: // BIT 4,B
                    {
                        Bit(0x10, regB);
                        return 8;

                    }
                case 97: // BIT 4,C
                    {
                        Bit(0x10, regC);
                        return 8;

                    }
                case 98: // BIT 4,D
                    {
                        Bit(0x10, GetD());
                        return 8;

                    }
                case 99: // BIT 4,E
                    {
                        Bit(0x10, GetE());
                        return 8;

                    }
                case 100: // BIT 4,H
                    {
                        Bit(0x10, GetH());
                        return 8;

                    }
                case 101: // BIT 4,L
                    {
                        Bit(0x10, GetL());
                        return 8;

                    }
                case 102: // BIT 4,(HL)
                    {
                        Bit(0x10, _mem.Peekb(regHL));
                        return 12;

                    }
                case 103: // BIT 4,A
                    {
                        Bit(0x10, regA);
                        return 8;

                    }
                case 104: // BIT 5,B
                    {
                        Bit(0x20, regB);
                        return 8;

                    }
                case 105: // BIT 5,C
                    {
                        Bit(0x20, regC);
                        return 8;

                    }
                case 106: // BIT 5,D
                    {
                        Bit(0x20, GetD());
                        return 8;

                    }
                case 107: // BIT 5,E
                    {
                        Bit(0x20, GetE());
                        return 8;

                    }
                case 108: // BIT 5,H
                    {
                        Bit(0x20, GetH());
                        return 8;

                    }
                case 109: // BIT 5,L
                    {
                        Bit(0x20, GetL());
                        return 8;

                    }
                case 110: // BIT 5,(HL)
                    {
                        Bit(0x20, _mem.Peekb(regHL));
                        return 12;

                    }
                case 111: // BIT 5,A
                    {
                        Bit(0x20, regA);
                        return 8;

                    }
                case 112:
                    {
                        // 112 BIT 6,B
                        Bit(0x40, regB);
                        return 8;

                    }
                case 113:
                    {
                        // 113 BIT 6,C
                        Bit(0x40, regC);
                        return 8;

                    }
                case 114:
                    {
                        // 114 BIT 6,D
                        Bit(0x40, GetD());
                        return 8;

                    }
                case 115:
                    {
                        // 115 BIT 6,E
                        Bit(0x40, GetE());
                        return 8;

                    }
                case 116:
                    {
                        // 116 BIT 6,H
                        Bit(0x40, GetH());
                        return 8;

                    }
                case 117:
                    {
                        // 117 BIT 6,L
                        Bit(0x40, GetL());
                        return 8;

                    }
                case 118:
                    {
                        // 118 BIT 6,(HL)
                        Bit(0x40, _mem.Peekb(regHL));
                        return 12;

                    }
                case 119:
                    {
                        // 119 ' BIT 6,A
                        Bit(0x40, regA);
                        return 8;

                    }
                case 120:
                    {
                        // 120 BIT 7,B
                        Bit(0x80, regB);
                        return 8;

                    }
                case 121:
                    {
                        // 121 BIT 7,C
                        Bit(0x80, regC);
                        return 8;

                    }
                case 122:
                    {
                        // 122 BIT 7,D
                        Bit(0x80, GetD());
                        return 8;

                    }
                case 123:
                    {
                        // 123 BIT 7,E
                        Bit(0x80, GetE());
                        return 8;

                    }
                case 124:
                    {
                        // 124 BIT 7,H
                        Bit(0x80, GetH());
                        return 8;

                    }
                case 125:
                    {
                        // 125 BIT 7,L
                        Bit(0x80, GetL());
                        return 8;

                    }
                case 126:
                    {
                        // 126 BIT 7,(HL)
                        Bit(0x80, _mem.Peekb(regHL));
                        return 12;

                    }
                case 127:
                    {
                        // 127 BIT 7,A
                        Bit(0x80, regA);
                        return 8;

                    }
                case 128: // RES 0,B
                    {
                        regB = (int)BitRes(1, regB);
                        return 8;

                    }
                case 129: // RES 0,C
                    {
                        regC = (int)BitRes(1, regC);
                        return 8;

                    }
                case 130: // RES 0,D
                    {
                        SetD(BitRes(1, GetD()));
                        return 8;

                    }
                case 131: // RES 0,E
                    {
                        SetE(BitRes(1, GetE()));
                        return 8;

                    }
                case 132: // RES 0,H
                    {
                        SetH(BitRes(1, GetH()));
                        return 8;

                    }
                case 133: // RES 0,L
                    {
                        SetL(BitRes(1, GetL()));
                        return 8;

                    }
                case 134: // RES 0,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(0x1, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 135: // RES 0,A
                    {
                        regA = (int)BitRes(1, regA);
                        return 8;

                    }
                case 136: // RES 1,B
                    {
                        regB = (int)BitRes(2, regB);
                        return 8;

                    }
                case 137: // RES 1,C
                    {
                        regC = (int)BitRes(2, regC);
                        return 8;

                    }
                case 138: // RES 1,D
                    {
                        SetD(BitRes(2, GetD()));
                        return 8;

                    }
                case 139: // RES 1,E
                    {
                        SetE(BitRes(2, GetE()));
                        return 8;

                    }
                case 140: // RES 1,H
                    {
                        SetH(BitRes(2, GetH()));
                        return 8;

                    }
                case 141: // RES 1,L
                    {
                        SetL(BitRes(2, GetL()));
                        return 8;

                    }
                case 142: // RES 1,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(2, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 143: // RES 1,A
                    {
                        regA = (int)BitRes(2, regA);
                        return 8;

                    }
                case 144: // RES 2,B
                    {
                        regB = (int)BitRes(4, regB);
                        return 8;

                    }
                case 145: // RES 2,C
                    {
                        regC = (int)BitRes(4, regC);
                        return 8;

                    }
                case 146: // RES 2,D
                    {
                        SetD(BitRes(4, GetD()));
                        return 8;

                    }
                case 147: // RES 2,E
                    {
                        SetE(BitRes(4, GetE()));
                        return 8;

                    }
                case 148: // RES 2,H
                    {
                        SetH(BitRes(4, GetH()));
                        return 8;

                    }
                case 149: // RES 2,L
                    {
                        SetL(BitRes(4, GetL()));
                        return 8;

                    }
                case 150: // RES 2,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(4, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 151: // RES 2,A
                    {
                        regA = (int)BitRes(4, regA);
                        return 8;

                    }
                case 152: // RES 3,B
                    {
                        regB = (int)BitRes(8, regB);
                        return 8;

                    }
                case 153: // RES 3,C
                    {
                        regC = (int)BitRes(8, regC);
                        return 8;

                    }
                case 154: // RES 3,D
                    {
                        SetD(BitRes(8, GetD()));
                        return 8;

                    }
                case 155: // RES 3,E
                    {
                        SetE(BitRes(8, GetE()));
                        return 8;

                    }
                case 156: // RES 3,H
                    {
                        SetH(BitRes(8, GetH()));
                        return 8;

                    }
                case 157: // RES 3,L
                    {
                        SetL(BitRes(8, GetL()));
                        return 8;

                    }
                case 158: // RES 3,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(8, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 159: // RES 3,A
                    {
                        regA = (int)BitRes(8, regA);
                        return 8;

                    }
                case 160: // RES 4,B
                    {
                        regB = (int)BitRes(0x10, regB);
                        return 8;

                    }
                case 161: // RES 4,C
                    {
                        regC = (int)BitRes(0x10, regC);
                        return 8;

                    }
                case 162: // RES 4,D
                    {
                        SetD(BitRes(0x10, GetD()));
                        return 8;

                    }
                case 163: // RES 4,E
                    {
                        SetE(BitRes(0x10, GetE()));
                        return 8;

                    }
                case 164: // RES 4,H
                    {
                        SetH(BitRes(0x10, GetH()));
                        return 8;

                    }
                case 165: // RES 4,L
                    {
                        SetL(BitRes(0x10, GetL()));
                        return 8;

                    }
                case 166: // RES 4,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(0x10, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 167: // RES 4,A
                    {
                        regA = (int)BitRes(0x10, regA);
                        return 8;

                    }
                case 168:
                    {
                        // 168 RES 5,B
                        regB = (int)BitRes(0x20, regB);
                        return 8;

                    }
                case 169:
                    {
                        // 169 RES 5,C
                        regC = (int)BitRes(0x20, regC);
                        return 8;

                    }
                case 170:
                    {
                        // 170 RES 5,D
                        SetD(BitRes(0x20, GetD()));
                        return 8;

                    }
                case 171:
                    {
                        // 171 RES 5,E
                        SetE(BitRes(0x20, GetE()));
                        return 8;

                    }
                case 172: // RES 5,H
                    {
                        SetH(BitRes(0x20, GetH()));
                        return 8;

                    }
                case 173: // RES 5,L
                    {
                        SetL(BitRes(0x20, GetL()));
                        return 8;

                    }
                case 174: // RES 5,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(0x20, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 175: // RES 5,A
                    {
                        regA = (int)BitRes(0x20, regA);
                        return 8;

                    }
                case 176: // RES 6,B
                    {
                        regB = (int)BitRes(0x40, regB);
                        return 8;

                    }
                case 177: // RES 6,C
                    {
                        regC = (int)BitRes(0x40, regC);
                        return 8;

                    }
                case 178: // RES 6,D
                    {
                        SetD(BitRes(0x40, GetD()));
                        return 8;

                    }
                case 179: // RES 6,E
                    {
                        SetE(BitRes(0x40, GetE()));
                        return 8;

                    }
                case 180: // RES 6,H
                    {
                        SetH(BitRes(0x40, GetH()));
                        return 8;

                    }
                case 181: // RES 6,L
                    {
                        SetL(BitRes(0x40, GetL()));
                        return 8;

                    }
                case 182: // RES 6,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(0x40, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 183: // RES 6,A
                    {
                        regA = (int)BitRes(0x40, regA);
                        return 8;

                    }
                case 184: // RES 7,B
                    {
                        regB = (int)BitRes(0x80, regB);
                        return 8;

                    }
                case 185: // RES 7,C
                    {
                        regC = (int)BitRes(0x80, regC);
                        return 8;

                    }
                case 186: // RES 7,D
                    {
                        SetD(BitRes(0x80, GetD()));
                        return 8;

                    }
                case 187: // RES 7,E
                    {
                        SetE(BitRes(0x80, GetE()));
                        return 8;

                    }
                case 188: // RES 7,H
                    {
                        SetH(BitRes(0x80, GetH()));
                        return 8;

                    }
                case 189: // RES 7,L
                    {
                        SetL(BitRes(0x80, GetL()));
                        return 8;

                    }
                case 190: // RES 7,(HL)
                    {
                        _mem.Pokeb(regHL, BitRes(0x80, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 191: // RES 7,A
                    {
                        regA = (int)BitRes(0x80, regA);
                        return 8;

                    }
                case 192: // SET 0,B
                    {
                        regB = (int)BitSet(1, regB);
                        return 8;

                    }
                case 193: // SET 0,C
                    {
                        regC = (int)BitSet(1, regC);
                        return 8;

                    }
                case 194: // SET 0,D
                    {
                        SetD(BitSet(1, GetD()));
                        return 8;

                    }
                case 195: // SET 0,E
                    {
                        SetE(BitSet(1, GetE()));
                        return 8;

                    }
                case 196: // SET 0,H
                    {
                        SetH(BitSet(1, GetH()));
                        return 8;

                    }
                case 197: // SET 0,L
                    {
                        SetL(BitSet(1, GetL()));
                        return 8;

                    }
                case 198: // SET 0,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(1, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 199: // SET 0,A
                    {
                        regA = (int)BitSet(1, regA);
                        return 8;

                    }
                case 200: // SET 1,B
                    {
                        regB = (int)BitSet(2, regB);
                        return 8;

                    }
                case 201: // SET 1,C
                    {
                        regC = (int)BitSet(2, regC);
                        return 8;

                    }
                case 202: // SET 1,D
                    {
                        SetD(BitSet(2, GetD()));
                        return 8;

                    }
                case 203: // SET 1,E
                    {
                        SetE(BitSet(2, GetE()));
                        return 8;

                    }
                case 204: // SET 1,H
                    {
                        SetH(BitSet(2, GetH()));
                        return 8;

                    }
                case 205: // SET 1,L
                    {
                        SetL(BitSet(2, GetL()));
                        return 8;

                    }
                case 206: // SET 1,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(2, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 207: // SET 1,A
                    {
                        regA = (int)BitSet(2, regA);
                        return 8;

                    }
                case 208: // SET 2,B
                    {
                        regB = (int)BitSet(4, regB);
                        return 8;

                    }
                case 209: // SET 2,C
                    {
                        regC = (int)BitSet(4, regC);
                        return 8;

                    }
                case 210: // SET 2,D
                    {
                        SetD(BitSet(4, GetD()));
                        return 8;

                    }
                case 211: // SET 2,E
                    {
                        SetE(BitSet(4, GetE()));
                        return 8;

                    }
                case 212: // SET 2,H
                    {
                        SetH(BitSet(4, GetH()));
                        return 8;

                    }
                case 213: // SET 2,L
                    {
                        SetL(BitSet(4, GetL()));
                        return 8;

                    }
                case 214: // SET 2,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(0x4, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 215: // SET 2,A
                    {
                        regA = (int)BitSet(4, regA);
                        return 8;

                    }
                case 216: // SET 3,B
                    {
                        regB = (int)BitSet(8, regB);
                        return 8;

                    }
                case 217: // SET 3,C
                    {
                        regC = (int)BitSet(8, regC);
                        return 8;

                    }
                case 218: // SET 3,D
                    {
                        SetD(BitSet(8, GetD()));
                        return 8;

                    }
                case 219: // SET 3,E
                    {
                        SetE(BitSet(8, GetE()));
                        return 8;

                    }
                case 220: // SET 3,H
                    {
                        SetH(BitSet(8, GetH()));
                        return 8;

                    }
                case 221: // SET 3,L
                    {
                        SetL(BitSet(8, GetL()));
                        return 8;

                    }
                case 222: // SET 3,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(0x8, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 223: // SET 3,A
                    {
                        regA = (int)BitSet(8, regA);
                        return 8;

                    }
                case 224: // SET 4,B
                    {
                        regB = (int)BitSet(0x10, regB);
                        return 8;

                    }
                case 225: // SET 4,C
                    {
                        regC = (int)BitSet(0x10, regC);
                        return 8;

                    }
                case 226: // SET 4,D
                    {
                        SetD(BitSet(0x10, GetD()));
                        return 8;

                    }
                case 227: // SET 4,E
                    {
                        SetE(BitSet(0x10, GetE()));
                        return 8;

                    }
                case 228: // SET 4,H
                    {
                        SetH(BitSet(0x10, GetH()));
                        return 8;

                    }
                case 229: // SET 4,L
                    {
                        SetL(BitSet(0x10, GetL()));
                        return 8;

                    }
                case 230: // SET 4,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(0x10, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 231: // SET 4,A
                    {
                        regA = (int)BitSet(0x10, regA);
                        return 8;

                    }
                case 232: // SET 5,B
                    {
                        regB = (int)BitSet(0x20, regB);
                        return 8;

                    }
                case 233: // SET 5,C
                    {
                        regC = (int)BitSet(0x20, regC);
                        return 8;

                    }
                case 234: // SET 5,D
                    {
                        SetD(BitSet(0x20, GetD()));
                        return 8;

                    }
                case 235: // SET 5,E
                    {
                        SetE(BitSet(0x20, GetE()));
                        return 8;

                    }
                case 236: // SET 5,H
                    {
                        SetH(BitSet(0x20, GetH()));
                        return 8;

                    }
                case 237: // SET 5,L
                    {
                        SetL(BitSet(0x20, GetL()));
                        return 8;

                    }
                case 238: // SET 5,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(0x20, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 239: // SET 5,A
                    {
                        regA = (int)BitSet(0x20, regA);
                        return 8;

                    }
                case 240: // SET 6,B
                    {
                        regB = (int)BitSet(0x40, regB);
                        return 8;

                    }
                case 241: // SET 6,C
                    {
                        regC = (int)BitSet(0x40, regC);
                        return 8;

                    }
                case 242: // SET 6,D
                    {
                        SetD(BitSet(0x40, GetD()));
                        return 8;

                    }
                case 243: // SET 6,E
                    {
                        SetE(BitSet(0x40, GetE()));
                        return 8;

                    }
                case 244: // SET 6,H
                    {
                        SetH(BitSet(0x40, GetH()));
                        return 8;

                    }
                case 245: // SET 6,L
                    {
                        SetL(BitSet(0x40, GetL()));
                        return 8;

                    }
                case 246: // SET 6,(HL)
                    {
                        _mem.Pokeb(regHL, BitSet(0x40, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 247: // SET 6,A
                    {
                        regA = (int)BitSet(0x40, regA);
                        return 8;

                    }
                case 248: // SET 7,B
                    {
                        regB = (int)BitSet(0x80, regB);
                        return 8;

                    }
                case 249: // SET 7,C
                    {
                        regC = (int)BitSet(0x80, regC);
                        return 8;

                    }
                case 250: // SET 7,D
                    {
                        SetD(BitSet(0x80, GetD()));
                        return 8;

                    }
                case 251: // SET 7,E
                    {
                        SetE(BitSet(0x80, GetE()));
                        return 8;

                    }
                case 252:
                    {
                        // 252 SET 7,H
                        SetH(BitSet(0x80, GetH()));
                        return 8;

                    }
                case 253:
                    {
                        // 253 SET 7,L
                        SetL(BitSet(0x80, GetL()));
                        return 8;

                    }
                case 254:
                    {
                        // 254 SET 7,(HL)
                        _mem.Pokeb(regHL, BitSet(0x80, _mem.Peekb(regHL)));
                        return 15;

                    }
                case 255:
                    {
                        // 255 SET 7,A
                        regA = (int)BitSet(0x80, regA);
                        return 8;

                    }
            }
            return 0;
        }



        public int sla(int ans)
        {
        
            Boolean c;

            c = (ans & 0x80L) != 0;
            ans = ans * 2 & 0xFF;

            fS = (ans & F_S) != 0;
            f3 = (ans & F_3) != 0;
            f5 = (ans & F_5) != 0;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;

           return ans;
         
        }

        private int Execute_ed(int local_tstates)
        {

            int xxx;
            int count;
            int dest;
            int @from;
            int TempLocal_tstates;
            Boolean c;
            int b;


            intRTemp = intRTemp + 1;

            xxx = Nxtpcb();

            switch (xxx)
            {

                case >= 0 and <= 63:
                    {
                        // 000 to 063 = NOP
                        return 8;

                    }
                case 64:
                    {
                        // 064 IN B,(c)
                        regB = (int)In_bc();
                        return 12;

                    }
                case 65:
                    {
                        // 065 OUT (c),B
                        Outb(GetBC(), regB);
                        return 12;

                    }
                case 66:
                    {
                        // 066 SBC HL,BC
                        regHL = (int)Sbc16(regHL, GetBC());
                        return 15;

                    }
                case 67:
                    {
                        // 067 LD (nn),BC
                        _mem.Pokew(Nxtpcw(), GetBC());
                        return 20;

                    }
                case 68:
                    {
                        // 068 NEG
                        Neg_a();
                        return 8;

                    }
                case 69:
                    {
                        // 069 RETn
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 70:
                    {
                        // 070 IM 0
                        intIM = 0;
                        return 8;

                    }
                case 71:
                    {
                        // 071 LD I,A
                        intI = regA;
                        return 9;

                    }
                case 72:
                    {
                        // 072 IN C,(c)
                        regC = (int)In_bc();
                        return 12;

                    }
                case 73:
                    {
                        // 073 OUT (c),C
                        Outb(GetBC(), regC);
                        return 12;

                    }
                case 74:
                    {
                        // 074 ADC HL,BC
                        regHL = (int)Adc16(regHL, GetBC());
                        return 15;

                    }
                case 75:
                    {
                        // 075 LD BC,(nn)
                        SetBC(_mem.Peekw(Nxtpcw()));
                        return 20;

                    }
                case 76:
                    {
                        // 076 NEG
                        Neg_a();
                        return 8;

                    }
                case 77:
                    {
                        // 077 RETI
                        // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                        // //          copied to IFF1 for RETI - but in a real Z80 it is
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 78:
                    {
                        // 078 IM 0
                        intIM = 0;
                        return 8;

                    }
                case 79:
                    {
                        // 079 LD R,A
                        intR = regA & 128;
                        intRTemp = intR;
                        return 9;

                    }
                case 80:
                    {
                        // 080 IN D,(c)
                        SetD(In_bc());
                        return 12;

                    }
                case 81:
                    {
                        // 081 OUT (c),D
                        Outb(GetBC(), GetD());
                        return 12;

                    }
                case 82:
                    {
                        // 082 SBC HL,DE
                        regHL = (int)Sbc16(regHL, regDE);
                        return 15;

                    }
                case 83:
                    {
                        // 083 LD (nn),DE
                        _mem.Pokew(Nxtpcw(), regDE);
                        return 20;

                    }
                case 84:
                    {
                        // NEG
                        Neg_a();
                        return 8;

                    }
                case 85:
                    {
                        // 85 RETn
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 86:
                    {
                        // 86 ' IM 1
                        intIM = 1;
                        return 8;

                    }
                case 87:
                    {
                        // 87 ' LD A,I
                        Ld_a_i();
                        return 9;

                    }
                case 88:
                    {
                        // 088 IN E,(c)
                        SetE(In_bc());
                        return 12;

                    }
                case 89:
                    {
                        // 089 OUT (c),E
                        Outb(GetBC(), GetE());
                        return 12;

                    }
                case 90:
                    {
                        // 090 ADC HL,DE
                        regHL = (int)Adc16(regHL, regDE);
                        return 15;

                    }
                case 91:
                    {
                        // 091 LD DE,(nn)
                        regDE = (int)_mem.Peekw(Nxtpcw());
                        return 20;

                    }
                case 92:
                    {
                        // NEG
                        Neg_a();
                        return 8;

                    }
                case 93:
                    {
                        // 93 RETI
                        // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                        // //          copied to IFF1 for RETI - but in a real Z80 it is
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 94:
                    {
                        // IM 2
                        intIM = 2;
                        return 8;

                    }
                case 95:
                    {
                        // 95 LD A,R
                        Ld_a_r();
                        return 9;

                    }
                case 96: // IN H,(c)
                    {
                        SetH(In_bc());
                        return 12;

                    }
                case 97: // OUT (c),H
                    {
                        Outb(GetBC(), GetH());
                        return 12;

                    }
                case 98: // SBC HL,HL
                    {
                        regHL = (int)Sbc16(regHL, regHL);
                        return 15;

                    }
                case 99: // LD (nn),HL
                    {
                        _mem.Pokew(Nxtpcw(), regHL);
                        return 20;

                    }
                case 100: // NEG
                    {
                        Neg_a();
                        return 8;

                    }
                case 101: // RETn
                    {
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 102: // IM 0
                    {
                        intIM = 0;
                        return 8;

                    }
                case 103: // RRD
                    {
                        rrd_a();
                        return 18;

                    }
                case 104: // IN L,(c)
                    {
                        SetL(In_bc());
                        return 12;

                    }
                case 105: // OUT (c),L
                    {
                        Outb(GetBC(), GetL());
                        return 12;

                    }
                case 106: // ADC HL,HL
                    {
                        regHL = (int)Adc16(regHL, regHL);
                        return 15;

                    }
                case 107: // LD HL,(nn)
                    {
                        regHL = (int)_mem.Peekw(Nxtpcw());
                        return 20;

                    }
                case 108: // NEG
                    {
                        Neg_a();
                        return 8;

                    }
                case 109: // RETI
                    {
                        // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                        // //          copied to IFF1 for RETI - but in a real Z80 it is
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 110: // IM 0
                    {
                        intIM = 0;
                        return 8;

                    }
                case 111:  // RLD
                    {
                        rld_a();
                        return 18;

                    }
                case 112: // IN (c)
                    {
                        In_bc();
                        return 12;

                    }
                case 113: // OUT (c),0
                    {
                        Outb(GetBC(), 0);
                        return 12;

                    }
                case 114: // SBC HL,SP
                    {
                        regHL = (int)Sbc16(regHL, regSP);
                        return 15;

                    }
                case 115: // LD (nn),SP
                    {
                        _mem.Pokew(Nxtpcw(), regSP);
                        return 20;

                    }
                case 116: // NEG
                    {
                        Neg_a();
                        return 8;

                    }
                case 117: // RETn
                    {
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 118: // IM 1
                    {
                        intIM = 1;
                        return 8;

                    }
                case 119:
                    {
                        Interaction.MsgBox("Unknown opcode 0xED 119");
                        return 0;
                    }
                case 120: // IN A,(c)
                    {
                        regA = (int)In_bc();
                        return 12;

                    }
                case 121: // OUT (c),A
                    {
                        Outb(GetBC(), regA);
                        return 12;

                    }
                case 122: // ADC HL,SP
                    {
                        regHL = (int)Adc16(regHL, regSP);
                        return 15;

                    }
                case 123: // LD SP,(nn)
                    {
                        regSP = (int)_mem.Peekw(Nxtpcw());
                        return 20;

                    }
                case 124: // NEG
                    {
                        Neg_a();
                        return 8;

                    }
                case 125: // RETI
                    {
                        // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                        // //          copied to IFF1 for RETI - but in a real Z80 it is
                        intIFF1 = intIFF2;
                        Poppc();
                        return 14;

                    }
                case 126: // IM 2
                    {
                        intIM = 2;
                        return 8;

                    }
                case 127: // NOP
                    {
                        return 8;

                    }
                case var case1 when 128L <= case1 && case1 <= 159:
                    {
                        // NOP
                        return 8;

                    }
                case 160: // LDI
                    {
                        _mem.Pokeb(regDE, _mem.Peekb(regHL));

                        //f3 = Conversions.ToBoolean(F_3 & _mem.peekb(regHL) + regA); // // TOCHECK: Is this correct?
                        //f5 = Conversions.ToBoolean(2L & _mem.peekb(regHL) + regA);   // // TOCHECK: Is this correct?

                        regDE = (int)Inc16(regDE);
                        regHL = (int)Inc16(regHL);
                        SetBC(Dec16(GetBC()));

                        fPV = GetBC() != 0L;
                        fH = false;
                        fN = false;

                        return 16;

                    }
                case 161: // CPI
                    {
                        c = fC;

                        Cp_a(_mem.Peekb(regHL));
                        regHL = (int)Inc16(regHL);
                        SetBC(Dec16(GetBC()));

                        fPV = GetBC() != 0L;
                        fC = c;

                        return 16;

                    }
                case 162: // INI
                    {
                        _mem.Pokeb(regHL, Inb(GetBC()));
                        b = Qdec8(regB);
                        regB = (int)b;
                        regHL = (int)Inc16(regHL);

                        fZ = b == 0L;
                        fN = true;

                        return 16;

                    }
                case 163: // OUTI
                    {
                        b = Qdec8(regB);
                        regB = (int)b;
                        Outb(GetBC(), _mem.Peekb(regHL));
                        regHL = (int)Inc16(regHL);

                        fZ = b == 0L;
                        fN = true;

                        return 16;

                    }

                // /* xxD */
                case 168: // LDD
                    {
                        _mem.Pokeb(regDE, _mem.Peekb(regHL));

                        //f3 = Conversions.ToBoolean(F_3 & _mem.peekb(regHL) + regA); // // TOCHECK: Is this correct?
                        //f5 = Conversions.ToBoolean(2L & _mem.peekb(regHL) + regA);   // // TOCHECK: Is this correct?

                        regDE = (int)Dec16(regDE);
                        regHL = (int)Dec16(regHL);
                        SetBC(Dec16(GetBC()));

                        fPV = GetBC() != 0L;
                        fH = false;
                        fN = false;

                        return 16;

                    }
                case 169: // CPD
                    {
                        c = fC;

                        Cp_a(_mem.Peekb(regHL));
                        regHL = (int)Dec16(regHL);
                        SetBC(Dec16(GetBC()));

                        fPV = GetBC() != 0L;
                        fC = c;

                        return 16;

                    }
                case 170: // IND
                    {
                        _mem.Pokeb(regHL, Inb(GetBC()));
                        b = Qdec8(regB);
                        regB = (int)b;
                        regHL = (int)Dec16(regHL);

                        fZ = b == 0;
                        fN = true;

                        return 16;

                    }
                case 171: // OUTD
                    {
                        count = Qdec8(regB);
                        regB = (int)count;
                        Outb(GetBC(), _mem.Peekb(regHL));
                        regHL = (int)Dec16(regHL);

                        fZ = count == 0L;
                        fN = true;

                        return 16;

                    }

                // // xxIR
                case 176: // LDIR
                    {
                        TempLocal_tstates = 0;
                        count = GetBC();
                        dest = regDE;
                        from = regHL;

                        // // REFRESH -2
                        intRTemp = intRTemp - 2;
                        do
                        {
                            _mem.Pokeb(dest, _mem.Peekb(from));
                            from = from + 1 & 65535;
                            dest = dest + 1 & 65535;
                            count = count - 1;

                            TempLocal_tstates = TempLocal_tstates + 21;
                            // // REFRESH (2)
                            intRTemp = intRTemp + 2;
                            if (TempLocal_tstates >= 0L)
                            {
                                // // interruptTriggered

                            }
                        }
                        while (count != 0L);

                        regPC = regPC - 2;
                        fH = false;
                        fN = false;
                        fPV = true;
                        // f3 = Conversions.ToBoolean(F_3 & _mem.peekb(from - 1) + regA); // // TOCHECK: Is this correct?
                        // f5 = Conversions.ToBoolean(2L & _mem.peekb(from - 1) + regA);   // // TOCHECK: Is this correct?

                        if (count == 0L)
                        {
                            regPC = regPC + 2;
                            TempLocal_tstates = TempLocal_tstates - 5;
                            fPV = false;
                        }
                        regDE = (int)dest;
                        regHL = (int)from;
                        SetBC(count);

                        return TempLocal_tstates;

                    }
                case 177: // CPIR
                    {
                        c = fC;

                        Cp_a(_mem.Peekb(regHL));
                        regHL = (int)Inc16(regHL);
                        SetBC(Dec16(GetBC()));

                        fC = c;
                        c = GetBC() != 0;
                        fPV = c;
                        if (fPV & fZ == false)
                        {
                            regPC = regPC - 2;
                            return 21;
                        }
                        else
                        {
                            return 16;
                        }


                    }
                case 178: // INIR
                    {
                        _mem.Pokeb(regHL, Inb(GetBC()));
                        b = Qdec8(regB);
                        regB = (int)b;
                        regHL = (int)Inc16(regHL);

                        fZ = true;
                        fN = true;
                        if (b != 0L)
                        {
                            regPC = regPC - 2;
                            return 21;
                        }
                        else
                        {
                            return 16;
                        }


                    }
                case 179: // OTIR
                    {
                        b = Qdec8(regB);
                        regB = (int)b;
                        Outb(GetBC(), _mem.Peekb(regHL));
                        regHL = (int)Inc16(regHL);

                        fZ = true;
                        fN = true;
                        if (b != 0L)
                        {
                            regPC = regPC - 2;
                            return 21;
                        }
                        else
                        {
                            return 16;
                        }


                    }

                // // xxDR
                case 184: // LDDR
                    {
                        TempLocal_tstates = 0;
                        count = GetBC();
                        dest = regDE;
                        from = regHL;

                        // // REFRESH -2
                        intRTemp = intRTemp - 2;
                        do
                        {
                            _mem.Pokeb(dest, _mem.Peekb(from));
                            from = from - 1 & 65535;
                            dest = dest - 1 & 65535;
                            count = count - 1;

                            TempLocal_tstates = TempLocal_tstates + 21;

                            // // REFRESH (2)
                            intRTemp = intRTemp + 2;

                            if (TempLocal_tstates >= 0L)
                            {
                                // // interruptTriggered

                            }
                        }
                        while (count != 0L);
                        regPC = regPC - 2;
                        fH = false;
                        fN = false;
                        fPV = true;

                        //f3 = Conversions.ToBoolean(F_3 & _mem.peekb(from - 1L) + regA); // // TOCHECK: Is this correct?
                        //f5 = Conversions.ToBoolean(2L & _mem.peekb(from - 1L) + regA);   // // TOCHECK: Is this correct?

                        if (count == 0L)
                        {
                            regPC = regPC + 2;
                            TempLocal_tstates = TempLocal_tstates - 5;
                            fPV = false;
                        }

                        regDE = (int)dest;
                        regHL = (int)from;
                        SetBC(count);

                        return TempLocal_tstates;

                    }
                case 185: // CPDR
                    {
                        c = fC;

                        Cp_a(_mem.Peekb(regHL));
                        regHL = (int)Dec16(regHL);
                        SetBC(Dec16(GetBC()));

                        fPV = GetBC() != 0L;
                        fC = c;
                        if (fPV & fZ == false)
                        {
                            regPC = regPC - 2;
                            return 21;
                        }
                        else
                        {
                            return 16;
                        }


                    }
                case 186: // INDR
                    {
                        _mem.Pokeb(regHL, Inb(GetBC()));
                        b = Qdec8(regB);
                        regB = (int)b;
                        regHL = (int)Dec16(regHL);

                        fZ = true;
                        fN = true;
                        if (b != 0L)
                        {
                            regPC = regPC - 2;
                            return 21;
                        }
                        else
                        {
                            return 16;
                        }


                    }
                case 187: // OTDR
                    {
                        b = Qdec8(regB);
                        regB = (int)b;
                        Outb(GetBC(), _mem.Peekb(regHL));
                        regHL = (int)Dec16(regHL);

                        fZ = true;
                        fN = true;
                        if (b != 0L)
                        {
                            regPC = regPC - 2;
                            return 21;
                        }
                        else
                        {
                            return 16;
                        }


                    }

                case var case2 when 164L <= case2 && case2 <= 167:
                    {
                        // NOP
                        return 8;

                    }
                case var case3 when 172L <= case3 && case3 <= 175:
                    {
                        // NOP
                        return 8;

                    }

                case var case4 when 180L <= case4 && case4 <= 183:
                    {
                        // NOP
                        return 8;

                    }
                case 252:
                    {
                        // // Patched tape LOAD routine
                        _zx81.TapeLoad(regHL);
                        return 0;

                    }
                case 253:
                    {
                        // // Patched tape SAVE routine
                        _zx81.TapeSave(regHL);
                        return 0;
                    }

                default:
                    {
                        Interaction.MsgBox("Unknown ED instruction " + xxx + " at " + regPC);
                        return 8;

                    }
            }

        }


        private void rld_a()
        {
            int ans;
            int t;
            int q;

            ans = regA;
            t = _mem.Peekb(regHL);
            q = t;

            t = (t * 16) | (ans & 0xF);
            ans = (ans & 0xF0) | (q / 16);
            _mem.Pokeb(regHL, t & 0xFF);

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = intIFF2;
            fH = false;
            fN = false;

            regA = (int)ans;
        }
        private void rrd_a()
        {
            int ans;
            int t;
            int q;

            ans = regA;
            t = _mem.Peekb(regHL);
            q = t;

            t = (t / 16) | (ans * 16);
            ans = (ans & 0xF0) | (q & 0xF);
            _mem.Pokeb(regHL, t);

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = intIFF2;
            fH = false;
            fN = false;

            regA = (int)ans;
        }
        private void Neg_a()
        {
            int t;

            t = regA;
            regA = 0;
            Sub_a(t);
        }
        private void Execute_id_cb(int op, int z)
        {
            switch (op)
            {
                case 0: // RLC B
                    {
                        op = Rlc(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 1: // RLC C
                    {
                        op = Rlc(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 2: // RLC D
                    {
                        op = Rlc(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 3: // RLC E
                    {
                        op = Rlc(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 4: // RLC H
                    {
                        op = Rlc(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 5: // RLC L
                    {
                        op = Rlc(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 6: // RLC (HL)
                    {
                        _mem.Pokeb(z, Rlc(_mem.Peekb(z)));
                        break;
                    }
                case 7: // RLC A
                    {
                        op = Rlc(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }

                case 8: // RRC B
                    {
                        op = Rrc(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 9: // RRC C
                    {
                        op = Rrc(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 10: // RRC D
                    {
                        op = Rrc(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 11: // RRC E
                    {
                        op = Rrc(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 12: // RRC H
                    {
                        op = Rrc(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 13: // RRC L
                    {
                        op = Rrc(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 14: // RRC (HL)
                    {
                        _mem.Pokeb(z, Rrc(_mem.Peekb(z)));
                        break;
                    }
                case 15: // RRC A
                    {
                        op = Rrc(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 16: // RL B
                    {
                        op = Rl(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 17: // RL C
                    {
                        op = Rl(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 18: // RL D
                    {
                        op = Rl(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 19: // RL E
                    {
                        op = Rl(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 20: // RL H
                    {
                        op = Rl(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 21: // RL L
                    {
                        op = Rl(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 22: // RL (HL)
                    {
                        _mem.Pokeb(z, Rl(_mem.Peekb(z)));
                        break;
                    }
                case 23: // RL A
                    {
                        op = Rl(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 24: // RR B
                    {
                        op = Rr(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 25: // RR C
                    {
                        op = Rr(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 26: // RR D
                    {
                        op = Rr(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 27: // RR E
                    {
                        op = Rr(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 28: // RR H
                    {
                        op = Rr(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 29: // RR L
                    {
                        op = Rr(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 30: // RR (HL)
                    {
                        _mem.Pokeb(z, Rl(_mem.Peekb(z)));
                        break;
                    }
                case 31: // RR A
                    {
                        op = Rr(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 32: // SLA B
                    {
                        op = sla(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 33: // SLA C
                    {
                        op = sla(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 34: // SLA D
                    {
                        op = sla(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 35: // SLA E
                    {
                        op = sla(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 36: // SLA H
                    {
                        op = sla(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 37: // SLA L
                    {
                        op = sla(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 38: // SLA (HL)
                    {
                        _mem.Pokeb(z, sla(_mem.Peekb(z)));
                        break;
                    }
                case 39: // SLA A
                    {
                        op = sla(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 40: // SRA B
                    {
                        op = Sra(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 41: // SRA C
                    {
                        op = Sra(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 42: // SRA D
                    {
                        op = Sra(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 43: // SRA E
                    {
                        op = Sra(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 44: // SRA H
                    {
                        op = Sra(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 45: // SRA L
                    {
                        op = Sra(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 46: // SRA (HL)
                    {
                        _mem.Pokeb(z, Sra(_mem.Peekb(z)));
                        break;
                    }
                case 47: // SRA A
                    {
                        op = Sra(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 48: // SLS B
                    {
                        op = Sls(_mem.Peekb(z));
                        regB = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 49: // SLS C
                    {
                        op = Sls(_mem.Peekb(z));
                        regC = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 50: // SLS D
                    {
                        op = Sls(_mem.Peekb(z));
                        SetD(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 51: // SLS E
                    {
                        op = Sls(_mem.Peekb(z));
                        SetE(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 52: // SLS H
                    {
                        op = Sls(_mem.Peekb(z));
                        SetH(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 53: // SLS L
                    {
                        op = Sls(_mem.Peekb(z));
                        SetL(op);
                        _mem.Pokeb(z, op);
                        break;
                    }
                case 54: // SLS (HL)
                    {
                        _mem.Pokeb(z, Sls(_mem.Peekb(z)));
                        break;
                    }
                case 55: // SLS A
                    {
                        op = Sls(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }


                case 62: // SRL (HL)
                    {
                        _mem.Pokeb(z, Srl(_mem.Peekb(z)));
                        break;
                    }
                case 63: // SRL A
                    {
                        op = Srl(_mem.Peekb(z));
                        regA = (int)op;
                        _mem.Pokeb(z, op);
                        break;
                    }
                case var @case when 64L <= @case && @case <= 71: // BIT 0,B
                    {
                        Bit(0x1, _mem.Peekb(z));
                        break;
                    }
                case var case1 when 72L <= case1 && case1 <= 79: // BIT 1,B
                    {
                        Bit(0x2, _mem.Peekb(z));
                        break;
                    }
                case var case2 when 80L <= case2 && case2 <= 87: // BIT 2,B
                    {
                        Bit(0x4, _mem.Peekb(z));
                        break;
                    }
                case var case3 when 88L <= case3 && case3 <= 95: // BIT 3,B
                    {
                        Bit(0x8, _mem.Peekb(z));
                        break;
                    }

                case var case4 when 96L <= case4 && case4 <= 103: // BIT 4,B
                    {
                        Bit(0x10, _mem.Peekb(z));
                        break;
                    }
                case var case5 when 104L <= case5 && case5 <= 111: // BIT 5,B
                    {
                        Bit(0x20, _mem.Peekb(z));
                        break;
                    }
                case var case6 when 112L <= case6 && case6 <= 119: // BIT 6,B
                    {
                        Bit(0x40, _mem.Peekb(z));
                        break;
                    }
                case var case7 when 120L <= case7 && case7 <= 127: // BIT 7,B
                    {
                        Bit(0x80, _mem.Peekb(z));
                        break;
                    }
                case 134: // RES 0,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x1, _mem.Peekb(z)));
                        break;
                    }
                case 142: // RES 1,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x2, _mem.Peekb(z)));
                        break;
                    }
                case 150: // RES 2,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x4, _mem.Peekb(z)));
                        break;
                    }
                case 158: // RES 3,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x8, _mem.Peekb(z)));
                        break;
                    }
                case 166: // RES 4,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x10, _mem.Peekb(z)));
                        break;
                    }
                case 172: // RES 5,H
                    {
                        SetH(BitRes(0x20, _mem.Peekb(z)));
                        _mem.Pokeb(z, GetH());
                        break;
                    }
                case 174: // RES 5,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x20, _mem.Peekb(z)));
                        break;
                    }
                case 175: // RES 5,A
                    {
                        regA = (int)BitRes(0x20, _mem.Peekb(z));
                        _mem.Pokeb(z, regA);
                        break;
                    }
                case 182: // RES 6,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x40, _mem.Peekb(z)));
                        break;
                    }
                case 190: // RES 7,(HL)
                    {
                        _mem.Pokeb(z, BitRes(0x80, _mem.Peekb(z)));
                        break;
                    }
                case 198: // SET 0,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x1, _mem.Peekb(z)));
                        break;
                    }
                case 206: // SET 1,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x2, _mem.Peekb(z)));
                        break;
                    }
                case 214: // SET 2,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x4, _mem.Peekb(z)));
                        break;
                    }
                case 222: // SET 3,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x8, _mem.Peekb(z)));
                        break;
                    }
                case 230: // SET 4,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x10, _mem.Peekb(z)));
                        break;
                    }
                case 238: // SET 5,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x20, _mem.Peekb(z)));
                        break;
                    }
                case 246: // SET 6,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x40, _mem.Peekb(z)));
                        break;
                    }
                case 254: // SET 7,(HL)
                    {
                        _mem.Pokeb(z, BitSet(0x80, _mem.Peekb(z)));
                        break;
                    }
                case 255: // SET 7,A
                    {
                        regA = (int)BitSet(0x80, _mem.Peekb(z));
                        _mem.Pokeb(z, regA);
                        break;
                    }

                default:
                    {
                        Interaction.MsgBox("Invalid ID CB op=" + op + " z=" + z);
                        break;
                    }
            }
        }

        private void Exx()
        {
            int t;

            t = regHL;
            regHL = regHL_;
            regHL_ = (int)t;

            t = regDE;
            regDE = regDE_;
            regDE_ = (int)t;

            t = GetBC();
            SetBC(regBC_);
            regBC_ = (int)t;
        }
        private int GetAF()
        {
            return (regA * 256) | GetF();
        }

        public int GetAF_()
        {
         
            return regAF_;

        }

        public void SetAF_(int a)
        {
            regAF_ = a;
        }


        private int GetBC()
        {
            int getBCRet = default;
            getBCRet = (regB * 256) | regC;
            return getBCRet;
        }
        private int GetD()
        {
            return regDE >> 8;
        }
        private int GetE()
        {
            return regDE & 0xFF;
        }

        private int GetF()
        {
            int getFRet = default;
            if (fS)
                getFRet = getFRet | F_S;
            if (fZ)
                getFRet = getFRet | F_Z;
            if (f5)
                getFRet = getFRet | F_5;
            if (fH)
                getFRet = getFRet | F_H;
            if (f3)
                getFRet = getFRet | F_3;
            if (fPV)
                getFRet = getFRet | F_PV;
            if (fN)
                getFRet = getFRet | F_N;
            if (fC)
                getFRet = getFRet | F_C;
            return getFRet;
        }


        private int GetH()
        {
            // getH = glMemAddrDiv256(regHL)
            return regHL >> 8;
        }
        private int GetL()
        {
            return regHL & 0xFF;
        }

        private int GetR()
        {
            return intR;
        }
        private int Id_d()
        {

            int d;

            d = Nxtpcb();
            if ((d & 128) == 128)
                d = -(256 - d);
            return (regID + d) & 0xFFFF;

        }
        private void Ld_a_i()
        {
            fS = (intI & F_S) != 0;
            f3 = (intI & F_3) != 0;
            f5 = (intI & F_5) != 0;
            fZ = intI == 0;
            fPV = intIFF2;
            fH = false;
            fN = false;

            regA = intI;
        }
        private void Ld_a_r()
        {
            // If ((intRTemp \ 128) And 1) = 1 Then
            // intR = 0
            // Else
            // intR = 128
            // End If

            intRTemp = (int)(intRTemp & 0x7FL);
            regA = (int)(intR & 0x80L | (long)intRTemp);
            fS = (regA & F_S) != 0;
            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fZ = regA == 0;
            fPV = intIFF2;
            fH = false;
            fN = false;
        }
        private int In_bc()
        {
           
            int ans;

            ans = Inb(GetBC());

            fZ = ans == 0L;
            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fPV = Parity[ans];
            fN = false;
            fH = false;

            return ans;
           
        }
        private int Inc8(int ans)
        {

            fPV = (ans == 0x7FL);
            fH = ((ans & 0xFL) + 1L & F_H) != 0L;

            ans = ans + 1 & 0xFF;

            fS = (ans & F_S) != 0L;
            //f3 = (ans & F_3) != 0L;
            //f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fN = false;

            return ans;

        }
        private int Dec8(int ans)
        {

            fPV = ans == 0x80L;
            fH = ((ans & 0xFL) - 1L & F_H) != 0L;

            ans = (ans - 1) & 0xFF;

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;

            fN = true;

            return ans;

        }
        public void Interrupt()
        {
            _zx81.HandleInterrupt(intI);
        }

        private Boolean InterruptTriggered(int tstates)
        {
            return (tstates >= 0);
        }

        private void Or_a(int b)
        {
            regA = (regA | b);

            fS = (regA & F_S) != 0;
            //f3 = (regA & F_3) != 0;
            //f5 = (regA & F_5) != 0;
            fH = false;
            fPV = Parity[regA];
            fZ = regA == 0;
            fN = false;
            fC = false;
        }




        public void Poppc()
        {
            regPC = (int)Popw();
        }
        private int Popw()
        {

            int t = _mem.Peekb(regSP) | (_mem.Peekb(regSP + 1) * 256);
            regSP = (regSP + 2 & 0xFFFF);
            return t;
        }
        public void Pushpc()
        {
            Pushw(regPC);
        }

        private void Pushw(int word)
        {
            regSP = (int)(regSP - 2 & 0xFFFF);
            _mem.Pokew(regSP, word);
        }
        private int Rl(int ans)
        {
          
            Boolean c;

            c = ((ans & 0x80L) != 0L);

            if (fC)
            {
                ans = (ans * 2) | 0x1;
            }
            else
            {
                ans = ans * 2;
            }
            ans = ans & 0xFF;

            fS = (ans & F_S) != 0L;
            // f3 = (ans & F_3) != 0L;
            // f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;

           return ans;
         
        }
        private void Rl_a()
        {
            int ans;
            bool c;

            ans = regA;
            c = ((ans & 0x80L) != 0L);

            if (fC)
            {
                ans = (ans * 2) | 0x1;
            }
            else
            {
                ans = ans * 2;
            }
            ans = ans & 0xFF;

            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fN = false;
            fH = false;
            fC = c;

            regA = (int)ans;
        }
        private int Rlc(int ans)
        {
            int rlcRet = default;
            Boolean c;

            c = ((ans & 0x80L) != 0L);

            if (c)
            {
                ans = ans * 2 | 0x1;
            }
            else
            {
                ans = ans * 2;
            }

            ans = ans & 0xFF;

            fS = (ans & F_S) != 0L;
            //f3 = (ans & F_3) != 0L;
            //f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;

            rlcRet = ans;
            return rlcRet;
        }
        private void Rlc_a()
        {
            Boolean c;

            c = (regA & 0x80L) != 0L;

            if (c)
            {
                regA = regA * 2 | 1;
            }
            else
            {
                regA = regA * 2;
            }
            regA = (int)(regA & 0xFFL);

            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fN = false;
            fH = false;
            fC = c;
        }

        private int Rr(int ans)
        {
         
            Boolean c;

            c = (ans & 0x1) != 0L;

            if (fC)
            {
                ans = (ans >> 1) | 0x80;
            }
            else
            {
                ans = (ans >> 1);
            }

            fS = (ans & F_S) != 0L;
            //f3 = (ans & F_3) != 0L;
            //f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;

           return ans;
        }
        private void Rr_a()
        {
            int ans;
            Boolean c;

            ans = regA;
            c = (ans & 0x1) != 0L;

            if (fC)
                // ans = glMemAddrDiv2(ans) Or &H80&
                ans = (ans >> 1) | 0x80;
            else
                // ans = glMemAddrDiv2(ans)
                ans = ans >> 1;

            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fN = false;
            fH = false;
            fC = c;

            regA = (int)ans;
        }

        private int Rrc(int ans)
        {

            Boolean c;

            c = (ans & 0x1) != 0;

            if (c)
                // ans = glMemAddrDiv2(ans) Or &H80&
                ans = (ans >> 1) | 0x80;
            else
                // ans = glMemAddrDiv2(ans)
                ans = (ans >> 1);

            fS = (ans & F_S) != 0L;
            //f3 = (ans & F_3) != 0L;
            //f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;

            return ans;
        }

        private void Rrc_a()
        {
            Boolean c;

            c = (regA & 1) != 0;

            if (c)
                // regA = glMemAddrDiv2(regA) Or &H80&
                regA = (regA >> 1) | 0x80;
            else
                // regA = glMemAddrDiv2(regA)
                regA = regA >> 1;

            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fN = false;
            fH = false;
            fC = c;
        }
        private void Sbc_a(int b)
        {
            int a;
            int wans;
            int ans;
            int c = 0;

            a = regA;

            if (fC)
                c = 1;

            wans = a - b - c;
            ans = wans & 0xFF;

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fC = (wans & 0x100L) != 0L;
            fPV = ((a ^ b) & (a ^ ans) & 0x80L) != 0L;
            fH = ((a & 0xFL) - (b & 0xFL) - c & F_H) != 0L;
            fN = true;

            regA = (int)ans;
        }
        private int Sbc16(int a, int b)
        {

            int c = 0;
            int lans;
            int ans;

            if (fC)
                c = 1;

            lans = a - b - c;
            ans = lans & 0xFFFF;

            fS = (ans & F_S * 256L) != 0L;
            f3 = (ans & F_3 * 256L) != 0L;
            f5 = (ans & F_5 * 256L) != 0L;
            fZ = ans == 0L;
            fC = (lans & 0x10000L) != 0L;
            fPV = ((a ^ b) & (a ^ ans) & 0x8000L) != 0L;
            fH = ((a & 0xFFFL) - (b & 0xFFFL) - c & 0x1000L) != 0L;
            fN = true;

            return ans;

        }

        private void Scf()
        {
            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fN = false;
            fH = false;
            fC = true;
        }
        public void SetAF(int v)
        {
            regA = (v & 0xFF00) >> 8;
            SetF((byte)(v & 0xFF));
        }
        public void SetBC(int nn)
        {
            regB = (nn & 0xFF00) >> 8;
            regC = nn & 0xFF;
        }

        public void Execute()
        {
            int local_tstates;
            int d;
            int lTemp;
            var xxx = default(long);

            local_tstates = -_zx81.glTstatesPerInterrupt;

            // // Yes, I appreciate that GOTO's and labels are a hideous blashphemy!
            // // However, this code is the fastest possible way of fetching and handling
            // // Z80 instructions I could come up with. There are only 8 compares per
            // // instruction fetch rather than between 1 and 255 as required in
            // // the previous version of vb81 with it's huge Case statement.
            // //
            // // I know it's slightly harder to follow the new code, but I think the
            // // speed increase justifies it. <CC>


            do           // main loop
            {

                if (regPC == 0)
                    _zx81.bBooting = true;

                //Display routine hack pause command
                if (regPC == 0x2A9)
                {
                    Outb(0, regA);
                    regPC = 0x229;
                }


                if (local_tstates >= 0L)
                {
                    // // Trigger an interrupt
                    Interrupt();
                    local_tstates = local_tstates - _zx81.glTstatesPerInterrupt;
                }
                // // REFRESH 1
                intRTemp = intRTemp + 1;

                xxx = Nxtpcb();
                switch (xxx)
                {

                    case 0:
                        {
                            // 000 NOP
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 1:
                        {
                            // 001 LD BC,nn
                            SetBC(Nxtpcw());
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 2:
                        {
                            // 002 LD (BC),A
                            _mem.Pokeb(GetBC(), regA);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 3:
                        {
                            // 003 INC BC
                            SetBC(Inc16(GetBC()));
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 4:
                        {
                            // 004 INC B
                            regB = (int)Inc8(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 5:
                        {
                            // 005 DEC B
                            regB = (int)Dec8(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 6:
                        {
                            // 006 LD B,n
                            regB = (int)Nxtpcb();
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 7:
                        {
                            // 007 RLCA
                            Rlc_a();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 8:
                        {
                            // 008 EX AF,AF'
                            Ex_af_af();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 9:
                        {
                            // 009 ADD HL,BC
                            regHL = (int)Add16(regHL, GetBC());
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 10:
                        {
                            // 010 LD A,(BC)
                            regA = (int)_mem.Peekb(GetBC());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 11:
                        {
                            // 011 DEC BC
                            SetBC(Dec16(GetBC()));
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 12:
                        {
                            // 012 INC C
                            regC = (int)Inc8(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 13:
                        {
                            // 013 DEC C
                            regC = (int)Dec8(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 14:
                        {
                            // 014 LD C,n
                            regC = (int)Nxtpcb();
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 15:
                        {
                            // 015 RRCA
                            Rrc_a();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 16:
                        {
                            // 016 DJNZ dis
                            lTemp = Qdec8(regB);

                            regB = (int)lTemp;
                            if (lTemp != 0L)
                            {
                                d = Nxtpcb();
                                if ((d & 128) == 128)
                                    d = -(256 - d);
                                regPC = (int)(regPC + d & 0xFFFF);
                                local_tstates = local_tstates + 13;
                            }
                            else
                            {
                                regPC = (int)Inc16(regPC);
                                local_tstates = local_tstates + 8;
                            }

                            break;
                        }
                    case 17:
                        {
                            // 017 LD DE,nn
                            regDE = (int)Nxtpcw();
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 18:
                        {
                            // 018 LD (DE),A
                            _mem.Pokeb(regDE, regA);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 19:
                        {
                            // 019 INC DE
                            regDE = (int)Inc16(regDE);
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 20:
                        {
                            // 020 INC D
                            SetD(Inc8(GetD()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 21:
                        {
                            // 021 DEC D
                            SetD(Dec8(GetD()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 22:
                        {
                            // 022 LD D,n
                            SetD(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 23:
                        {
                            // 023 ' RLA
                            Rl_a();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 24:
                        {
                            // 024 JR dis
                            d = Nxtpcb();
                            if ((d & 128) == 128)
                                d = -(256 - d);
                            regPC = (int)(regPC + d & 0xFFFF);
                            local_tstates = local_tstates + 12;
                            break;
                        }
                    case 25:
                        {
                            // 025 ADD HL,DE
                            regHL = (int)Add16(regHL, regDE);
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 26:
                        {
                            // 026 LD A,(DE)
                            regA = (int)_mem.Peekb(regDE);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 27:
                        {
                            // 027 DEC DE
                            regDE = (int)Dec16(regDE);
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 28:
                        {
                            // 028 INC E
                            SetE(Inc8(GetE()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 29:
                        {
                            // 029 DEC E
                            SetE(Dec8(GetE()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 30:
                        {
                            // 030 LD E,n
                            SetE(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 31:
                        {
                            // 031 RRA
                            Rr_a();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 32:
                        {
                            // 032 JR NZ dis
                            if (fZ == false)
                            {
                                d = Nxtpcb();
                                if ((d & 128) == 128)
                                    d = -(256 - d);
                                regPC = (int)(regPC + d & 0xFFFF);
                                local_tstates = local_tstates + 12;
                            }
                            else
                            {
                                regPC = (int)Inc16(regPC);
                                local_tstates = local_tstates + 7;
                            }

                            break;
                        }
                    case 33:
                        {
                            // 033 LD HL,nn
                            regHL = (int)Nxtpcw();
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 34:
                        {
                            // 034 LD (nn),HL
                            _mem.Pokew(Nxtpcw(), regHL);
                            local_tstates = local_tstates + 16;
                            break;
                        }
                    case 35:
                        {
                            // 035 INC HL
                            regHL = (int)Inc16(regHL);
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 36:
                        {
                            // 036 INC H
                            SetH(Inc8(GetH()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 37:
                        {
                            // 037 DEC H
                            SetH(Dec8(GetH()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 38:
                        {
                            // 038 LD H,n
                            SetH(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 39:
                        {
                            // 039 DAA
                            Daa_a();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 40:
                        {
                            // 040 JR Z dis
                            if (fZ == true)
                            {
                                d = Nxtpcb();
                                if ((d & 128) == 128)
                                    d = -(256 - d);
                                regPC = (regPC + d & 0xFFFF);
                                local_tstates = local_tstates + 12;
                            }
                            else
                            {
                                regPC = (int)Inc16(regPC);
                                local_tstates = local_tstates + 7;
                            }

                            break;
                        }
                    case 41:
                        {
                            // 041 ADD HL,HL
                            regHL = (int)Add16(regHL, regHL);
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 42:
                        {
                            // 042 LD HL,(nn)
                            regHL = _mem.Peekw(Nxtpcw());
                            local_tstates = local_tstates + 16;
                            break;
                        }
                    case 43:
                        {
                            // 043 DEC HL
                            regHL = Dec16(regHL);
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 44:
                        {
                            // 044 INC L
                            SetL(Inc8(GetL()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 45:
                        {
                            // 045 DEC L
                            SetL(Dec8(GetL()));
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 46:
                        {
                            // 046 LD L,n
                            SetL(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 47:
                        {
                            // 047 CPL
                            Cpl_a();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 48:
                        {
                            // 048 JR NC dis
                            if (fC == false)
                            {
                                d = Nxtpcb();
                                if ((d & 128) == 128)
                                    d = -(256 - d);
                                regPC = (regPC + d & 0xFFFF);
                                local_tstates = local_tstates + 12;
                            }
                            else
                            {
                                regPC = (int)Inc16(regPC);
                                local_tstates = local_tstates + 7;
                            }

                            break;
                        }
                    case 49:
                        {
                            // 049 LD SP,nn
                            regSP = (int)Nxtpcw();
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 50:
                        {
                            // 050 LD (nn),A
                            _mem.Pokeb(Nxtpcw(), regA);
                            local_tstates = local_tstates + 13;
                            break;
                        }
                    case 51:
                        {
                            // 051 INC SP
                            regSP = (int)Inc16(regSP);
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 52:
                        {
                            // 052 INC (HL)
                            _mem.Pokeb(regHL, Inc8(_mem.Peekb(regHL)));
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 53:
                        {
                            // 053 DEC (HL)
                            _mem.Pokeb(regHL, Dec8(_mem.Peekb(regHL)));
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 54:
                        {
                            // 054 LD (HL),n
                            _mem.Pokeb(regHL, Nxtpcb());
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 55:
                        {
                            // 055 SCF
                            Scf();
                            local_tstates = local_tstates + 4;
                            break;
                        }


                    case 56:
                        {
                            // 056 JR C dis
                            if (fC == true)
                            {
                                d = Nxtpcb();
                                if ((d & 128) == 128)
                                    d = -(256 - d);
                                regPC = (regPC + d & 0xFFFF);
                                local_tstates = local_tstates + 12;
                            }
                            else
                            {
                                regPC = (int)Inc16(regPC);
                                local_tstates = local_tstates + 7;
                            }

                            break;
                        }
                    case 57:
                        {
                            // 057 ADD HL,SP
                            regHL = (int)Add16(regHL, regSP);
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 58:
                        {
                            // 058 LD A,(nn)
                            regA = (int)_mem.Peekb(Nxtpcw());
                            local_tstates = local_tstates + 13;
                            break;
                        }
                    case 59:
                        {
                            // 059 DEC SP
                            regSP = (int)Dec16(regSP);
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 60:
                        {
                            // 060 INC A
                            regA = (int)Inc8(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 61:
                        {
                            // 061 DEC A
                            regA = (int)Dec8(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 62:
                        {
                            // 062 LD A,n
                            regA = (int)Nxtpcb();
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 63:
                        {
                            // 063 CCF
                            Ccf();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 64:
                        {
                            // LD B,B
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 65:
                        {
                            // 65 ' LD B,C
                            regB = regC;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 66:
                        {
                            // LD B,D
                            regB = (int)GetD();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 67:
                        {
                            // 67 ' LD B,E
                            regB = (int)GetE();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 68:
                        {
                            // LD B,H
                            regB = (int)GetH();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 69:
                        {
                            // 69 ' LD B,L
                            regB = (int)GetL();
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 70:
                        {
                            // LD B,(HL)
                            regB = (int)_mem.Peekb(regHL);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 71:
                        {
                            // 71 ' LD B,A
                            regB = regA;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 72:
                        {
                            // 72 ' LD C,B
                            regC = regB;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 73:
                        {
                            // 73 ' LD C,C
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 74:
                        {
                            // 74 ' LD C,D
                            regC = (int)GetD();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 75:
                        {
                            // 75 ' LD C,E
                            regC = (int)GetE();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 76:
                        {
                            // 76 ' LD C,H
                            regC = (int)GetH();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 77:
                        {
                            // 77 ' LD C,L
                            regC = (int)GetL();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 78:
                        {
                            // 78 ' LD C,(HL)
                            regC = (int)_mem.Peekb(regHL);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 79:
                        {
                            // 79 ' LD C,A
                            regC = regA;
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 80: // LD D,B
                        {
                            SetD(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 81: // LD D,C
                        {
                            SetD(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 82: // LD D,D
                        {
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 83: // LD D,E
                        {
                            SetD(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 84: // LD D,H
                        {
                            SetD(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 85: // LD D,L
                        {
                            SetD(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 86: // LD D,(HL)
                        {
                            SetD(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 87: // LD D,A
                        {
                            SetD(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    // // LD E,*
                    case 88: // LD E,B
                        {
                            SetE(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 89: // LD E,C
                        {
                            SetE(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 90: // LD E,D
                        {
                            SetE(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 91: // LD E,E
                        {
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 92: // LD E,H
                        {
                            SetE(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 93: // LD E,L
                        {
                            SetE(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 94: // LD E,(HL)
                        {
                            SetE(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 95: // LD E,A
                        {
                            SetE(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 96: // LD H,B
                        {
                            SetH(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 97: // LD H,C
                        {
                            SetH(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 98: // LD H,D
                        {
                            SetH(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 99: // LD H,E
                        {
                            SetH(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 100: // LD H,H
                        {
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 101: // LD H,L
                        {
                            SetH(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 102: // LD H,(HL)
                        {
                            SetH(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 103: // LD H,A
                        {
                            SetH(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    // // LD L,*
                    case 104: // LD L,B
                        {
                            SetL(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 105: // LD L,C
                        {
                            SetL(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 106: // LD L,D
                        {
                            SetL(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 107: // LD L,E
                        {
                            SetL(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 108: // LD L,H
                        {
                            SetL(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 109: // LD L,L
                        {
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 110: // LD L,(HL)
                        {
                            SetL(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 111: // LD L,A
                        {
                            SetL(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 112:
                        {
                            // 112 ' LD (HL),B
                            _mem.Pokeb(regHL, regB);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 113:
                        {
                            // 113 ' LD (HL),C
                            _mem.Pokeb(regHL, regC);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 114:
                        {
                            // 114 ' LD (HL),D
                            _mem.Pokeb(regHL, GetD());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 115:
                        {
                            // 115 ' LD (HL),E
                            _mem.Pokeb(regHL, GetE());
                            local_tstates = local_tstates + 7;
                            break;
                        }

                    case 116: // LD (HL),H
                        {
                            _mem.Pokeb(regHL, GetH());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 117: // LD (HL),L
                        {
                            _mem.Pokeb(regHL, GetL());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 118: // HALT
                        {
                            lTemp = (-local_tstates - 1) / 4 + 1;
                            local_tstates = local_tstates + lTemp * 4;
                            intRTemp = (int)(intRTemp + (lTemp - 1));
                            break;
                        }
                    case 119: // LD (HL),A
                        {
                            _mem.Pokeb(regHL, regA);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 120:
                        {
                            // 120 ' LD A,B
                            regA = regB;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 121:
                        {
                            // 121 ' LD A,C
                            regA = regC;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 122:
                        {
                            // 122 ' LD A,D
                            regA = (int)GetD();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 123:
                        {
                            // 123 ' LD A,E
                            regA = (int)GetE();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case var @case when @case == 124:
                        {
                            // 124 ' LD A,H
                            regA = (int)GetH();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case var case1 when case1 == 125:
                        {
                            // 125 ' LD A,L
                            regA = (int)GetL();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 126:
                        {
                            // 126 ' LD A,(HL)
                            regA = (int)_mem.Peekb(regHL);
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 127:
                        {
                            // 127 ' LD A,A
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    // // ADD A,*
                    case 128: // ADD A,B
                        {
                            Add_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 129: // ADD A,C
                        {
                            Add_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 130: // ADD A,D
                        {
                            Add_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 131: // ADD A,E
                        {
                            Add_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 132: // ADD A,H
                        {
                            Add_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 133: // ADD A,L
                        {
                            Add_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 134: // ADD A,(HL)
                        {
                            Add_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 135: // ADD A,A
                        {
                            Add_a(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 136: // ADC A,B
                        {
                            Adc_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 137: // ADC A,C
                        {
                            Adc_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 138: // ADC A,D
                        {
                            Adc_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 139: // ADC A,E
                        {
                            Adc_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 140: // ADC A,H
                        {
                            Adc_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 141: // ADC A,L
                        {
                            Adc_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 142: // ADC A,(HL)
                        {
                            Adc_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 143: // ADC A,A
                        {
                            Adc_a(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 144: // SUB B
                        {
                            Sub_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 145: // SUB C
                        {
                            Sub_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 146: // SUB D
                        {
                            Sub_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 147: // SUB E
                        {
                            Sub_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 148: // SUB H
                        {
                            Sub_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 149: // SUB L
                        {
                            Sub_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 150: // SUB (HL)
                        {
                            Sub_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 151: // SUB A
                        {
                            Sub_a(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 152: // SBC A,B
                        {
                            Sbc_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 153: // SBC A,C
                        {
                            Sbc_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 154: // SBC A,D
                        {
                            Sbc_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 155: // SBC A,E
                        {
                            Sbc_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 156: // SBC A,H
                        {
                            Sbc_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 157: // SBC A,L
                        {
                            Sbc_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 158: // SBC A,(HL)
                        {
                            Sbc_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 159: // SBC A,A
                        {
                            Sbc_a(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 160: // AND B
                        {
                            And_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 161: // AND C
                        {
                            And_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 162: // AND D
                        {
                            And_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 163: // AND E
                        {
                            And_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 164: // AND H
                        {
                            And_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 165: // AND L
                        {
                            And_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 166: // AND (HL)
                        {
                            And_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 167: // AND A
                        {
                            And_a(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 168: // XOR B
                        {
                            Xor_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 169: // XOR C
                        {
                            Xor_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 170: // XOR D
                        {
                            Xor_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 171: // XOR E
                        {
                            Xor_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 172: // XOR H
                        {
                            Xor_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 173: // XOR L
                        {
                            Xor_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 174: // XOR (HL)
                        {
                            Xor_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 175: // XOR A
                        {
                            regA = 0;
                            fS = false;
                            f3 = false;
                            f5 = false;
                            fH = false;
                            fPV = true;
                            fZ = true;
                            fN = false;
                            fC = false;
                            local_tstates = local_tstates + 4;
                            break;
                        }

                    case 176: // OR B
                        {
                            Or_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 177: // OR C
                        {
                            Or_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 178: // OR D'
                        {
                            Or_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 179: // OR E
                        {
                            Or_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 180: // OR H
                        {
                            Or_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 181: // OR L
                        {
                            Or_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 182: // OR (HL)
                        {
                            Or_a(_mem.Peekb(regHL));
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 183: // OR A
                        {
                            Or_a(regA);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    // // CP
                    case 184: // CP B
                        {
                            Cp_a(regB);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 185: // CP C
                        {
                            Cp_a(regC);
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 186: // CP D
                        {
                            Cp_a(GetD());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 187: // CP E
                        {
                            Cp_a(GetE());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 188: // CP H
                        {
                            Cp_a(GetH());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 189: // CP L
                        {
                            Cp_a(GetL());
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 190: // CP (HL)
                        {
                            Cp_a(_mem.Peekb(regHL));

                            if (regPC == 16803)
                            {
                                int asdassa = regA;
                                if (regA != 0)
                                {
                                    int asda = regA;
                                }
                            }


                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 191: // CP A
                        {
                            Cp_a(regA);
                            break;
                        }

                    case 192: // RET NZ
                        {
                            if (fZ == false)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 193: // POP BC
                        {
                            SetBC(Popw());
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 194: // JP NZ,nn
                        {
                            if (fZ == false)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 195: // JP nn
                        {
                            regPC = (int)_mem.Peekw(regPC);
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 196: // CALL NZ,nn
                        {
                            if (fZ == false)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 197: // PUSH BC
                        {
                            Pushw(GetBC());
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 198: // ADD A,n
                        {
                            Add_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 199: // RST 0
                        {
                            Pushpc();
                            regPC = 0;
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 200: // RET Z
                        {
                            if (fZ)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 201: // RET
                        {
                            Poppc();
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 202: // JP Z,nn
                        {
                            if (fZ)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 203: // Prefix CB
                        {
                            local_tstates = local_tstates + Execute_cb();
                            break;
                        }
                    case 204: // CALL Z,nn
                        {
                            if (fZ)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 205: // CALL nn
                        {
                            Pushw(regPC + 2);
                            regPC = (int)Nxtpcw();
                            local_tstates = local_tstates + 17;
                            break;
                        }
                    case 206: // ADC A,n
                        {
                            Adc_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 207: // RST 8
                        {
                            Pushpc();
                            regPC = 8;
                            local_tstates = local_tstates + 11;
                            break;
                        }

                    case 208: // RET NC
                        {
                            if (fC == false)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 209: // POP DE
                        {
                            regDE = (int)Popw();
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 210: // JP NC,nn
                        {
                            if (fC == false)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 211: // OUT (n),A
                        {
                            Outb(Nxtpcb(), regA);
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 212: // CALL NC,nn
                        {
                            if (fC == false)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 213: // PUSH DE
                        {
                            Pushw(regDE);
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 214: // SUB n
                        {
                            Sub_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 215: // RST 16
                        {
                            Pushpc();
                            regPC = 16;
                            local_tstates = local_tstates + 11;
                            break;
                        }

                    case 216: // RET C
                        {
                            if (fC)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 217: // EXX
                        {
                            Exx();
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 218: // JP C,nn
                        {
                            if (fC)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 219: // IN A,(n)
                        {
                            regA = Inb((regA * 256) | Nxtpcb());
                            local_tstates = local_tstates + 11;
                            break;
                        }

                    case 220: // CALL C,nn
                        {
                            if (fC)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 221: // prefix IX
                        {
                            regID = regIX;
                            local_tstates = local_tstates + Execute_id();
                            // ZX81 Specific
                            _zx81.HiResRoutine(regIX, regID);
                            //
                            regIX = regID;
                            break;
                        }
                    case 222: // SBC n
                        {
                            Sbc_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 223: // RST 24
                        {
                            Pushpc();
                            regPC = 24;
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 224:
                        {
                            // 224 ' RET PO
                            if (fPV == false)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 225:
                        {
                            // 225 ' POP HL
                            regHL = (int)Popw();
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 226:
                        {
                            // 226 JP PO,nn
                            if (fPV == false)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 227:
                        {
                            // 227 ' EX (SP),HL
                            lTemp = regHL;
                            regHL = (int)_mem.Peekw(regSP);
                            _mem.Pokew(regSP, lTemp);
                            local_tstates = local_tstates + 19;
                            break;
                        }
                    case 228:
                        {
                            // 228 ' CALL PO,nn
                            if (fPV == false)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 229:
                        {
                            // 229 ' PUSH HL
                            Pushw(regHL);
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 230:
                        {
                            // 230 ' AND n
                            And_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 231:
                        {
                            // 231 ' RST 32
                            Pushpc();
                            regPC = 32;
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 232:
                        {
                            // 232 ' RET PE
                            if (fPV)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 233:
                        {
                            // 233 ' JP HL
                            regPC = regHL;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 234:
                        {
                            // 234 ' JP PE,nn
                            if (fPV)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 235:
                        {
                            // 235 ' EX DE,HL
                            lTemp = regHL;
                            regHL = regDE;
                            regDE = (int)lTemp;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 236:
                        {
                            // 236 ' CALL PE,nn
                            if (fPV)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 237:
                        {
                            // 237 ' prefix ED
                            local_tstates = local_tstates + Execute_ed(local_tstates);
                            break;
                        }
                    case 238:
                        {
                            // 238 ' XOR n
                            Xor_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 239:
                        {
                            // 239 ' RST 40
                            Pushpc();
                            regPC = 40;
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 240:
                        {
                            // 240 RET P
                            if (fS == false)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 241:
                        {
                            // 241 POP AF
                            SetAF(Popw());
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 242:
                        {
                            // 242 JP P,nn
                            if (fS == false)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 243:
                        {
                            // 243 DI
                            intIFF1 = false;
                            intIFF2 = false;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 244:
                        {
                            // 244 CALL P,nn
                            if (fS == false)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 245:
                        {
                            // 245 PUSH AF
                            Pushw(GetAF());
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 246:
                        {
                            // 246 OR n
                            Or_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 247:
                        {
                            // 247 RST 48
                            Pushpc();
                            regPC = 48;
                            local_tstates = local_tstates + 11;
                            break;
                        }
                    case 248:
                        {
                            // 248 RET M
                            if (fS)
                            {
                                Poppc();
                                local_tstates = local_tstates + 11;
                            }
                            else
                            {
                                local_tstates = local_tstates + 5;
                            }

                            break;
                        }
                    case 249:
                        {
                            // 249 LD SP,HL
                            regSP = regHL;
                            local_tstates = local_tstates + 6;
                            break;
                        }
                    case 250:
                        {
                            // 250 JP M,nn
                            if (fS)
                            {
                                regPC = (int)Nxtpcw();
                            }
                            else
                            {
                                regPC = regPC + 2;
                            }
                            local_tstates = local_tstates + 10;
                            break;
                        }
                    case 251:
                        {
                            // 251 EI
                            intIFF1 = true;
                            intIFF2 = true;
                            local_tstates = local_tstates + 4;
                            break;
                        }
                    case 252:
                        {
                            // 252 CALL M,nn
                            if (fS)
                            {
                                Pushw(regPC + 2);
                                regPC = (int)Nxtpcw();
                                local_tstates = local_tstates + 17;
                            }
                            else
                            {
                                regPC = regPC + 2;
                                local_tstates = local_tstates + 10;
                            }

                            break;
                        }
                    case 253:
                        {
                            // 253 prefix IY
                            regID = regIY;
                            local_tstates = local_tstates + Execute_id();
                            regIY = regID;
                            break;
                        }
                    case 254:
                        {
                            // 254 CP n
                            Cp_a(Nxtpcb());
                            local_tstates = local_tstates + 7;
                            break;
                        }
                    case 255:
                        {
                            // 255 RST 56
                            Pushpc();
                            regPC = 56;
                            local_tstates = local_tstates + 11;
                            break;
                        }
                }
            }
            while (true);   // end of main loop


        }
        private int Qdec8(int a)
        {
            return a - 1 & 0xFF;
        }
        private int Execute_id()
        {

            int xxx;
            int lTemp;
            int op;

            intRTemp = intRTemp + 1;

            xxx = Nxtpcb();
            switch (xxx)
            {
                case >= 0 and <= 8: 
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 9: // ADD ID,BC
                    {
                        regID = (int)Add16(regID, GetBC());
                        return 15;

                    }
                case var case1 when 10L <= case1 && case1 <= 24:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 25: // ADD ID,DE
                    {
                        regID = (int)Add16(regID, regDE);
                        return 15;

                    }
                case var case2 when 26L <= case2 && case2 <= 31:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }

                case 32:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 33: // LD ID,nn
                    {
                        regID = (int)Nxtpcw();
                        return 14;

                    }
                case 34: // LD (nn),ID
                    {
                        _mem.Pokew(Nxtpcw(), regID);
                        return 20;

                    }
                case 35: // INC ID
                    {
                        regID = (int)Inc16(regID);
                        return 10;

                    }
                case 36: // INC IDH
                    {
                        SetIDH(Inc8(GetIDH()));
                        return 9;

                    }
                case 37: // DEC IDH
                    {
                        SetIDH(Dec8(GetIDH()));
                        return 9;

                    }
                case 38: // LD IDH,n
                    {
                        SetIDH(Nxtpcb());
                        return 11;

                    }
                case 39:
                case 40:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 41: // ADD ID,ID
                    {
                        lTemp = regID;
                        regID = (int)Add16(lTemp, lTemp);
                        return 15;

                    }
                case 42: // LD ID,(nn)
                    {
                        regID = (int)_mem.Peekw(Nxtpcw());
                        return 20;

                    }
                case 43: // DEC ID
                    {
                        regID = (int)Dec16(regID);
                        return 10;

                    }
                case 44: // INC IDL
                    {
                        SetIDL(Inc8(GetIDL()));
                        return 9;

                    }
                case 45: // DEC IDL
                    {
                        SetIDL(Dec8(GetIDL()));
                        return 9;

                    }
                case 46: // LD IDL,n
                    {
                        SetIDL(Nxtpcb());
                        return 11;

                    }
                case var case3 when 47L <= case3 && case3 <= 51:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 52: // INC (ID+d)
                    {
                        lTemp = Id_d();
                        _mem.Pokeb(lTemp, Inc8(_mem.Peekb(lTemp)));
                        return 23;

                    }
                case 53: // DEC (ID+d)
                    {
                        lTemp = Id_d();
                        _mem.Pokeb(lTemp, Dec8(_mem.Peekb(lTemp)));
                        return 23;

                    }
                case 54: // LD (ID+d),n
                    {
                        lTemp = Id_d();
                        _mem.Pokeb(lTemp, Nxtpcb());
                        return 19;

                    }
                case 55:
                case 56:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 57: // ADD ID,SP
                    {
                        regID = (int)Add16(regID, regSP);
                        return 15;

                    }
                case var case4 when 58L <= case4 && case4 <= 63:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }

                case var case5 when 64L <= case5 && case5 <= 67:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 68: // LD B,IDH
                    {
                        regB = (int)GetIDH();
                        return 9;

                    }
                case 69: // LD B,IDL
                    {
                        regB = (int)GetIDL();
                        return 9;

                    }
                case 70: // LD B,(ID+d)
                    {
                        regB = (int)_mem.Peekb(Id_d());
                        return 19;

                    }
                case var case6 when 71L <= case6 && case6 <= 75:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 76: // LD C,IDH
                    {
                        regC = (int)GetIDH();
                        return 9;

                    }
                case 77: // LD C,IDL
                    {
                        regC = (int)GetIDL();
                        return 9;

                    }
                case 78: // LD C,(ID+d)
                    {
                        regC = (int)_mem.Peekb(Id_d());
                        return 19;

                    }
                case var case7 when 79L <= case7 && case7 <= 83:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 84: // LD D,IDH
                    {
                        SetD(GetIDH());
                        return 9;

                    }
                case 85: // LD D,IDL
                    {
                        SetD(GetIDL());
                        return 9;

                    }
                case 86: // LD D,(ID+d)
                    {
                        SetD(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case8 when 87L <= case8 && case8 <= 91:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 92: // LD E,IDH
                    {
                        SetE(GetIDH());
                        return 9;

                    }
                case 93: // LD E,IDL
                    {
                        SetE(GetIDL());
                        return 9;

                    }
                case 94: // LD E,(ID+d)
                    {
                        SetE(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case 95:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 96: // LD IDH,B
                    {
                        SetIDH(regB);
                        return 9;

                    }
                case 97: // LD IDH,C
                    {
                        SetIDH(regC);
                        return 9;

                    }
                case 98: // LD IDH,D
                    {
                        SetIDH(GetD());
                        return 9;

                    }
                case 99: // LD IDH,E
                    {
                        SetIDH(GetE());
                        return 9;

                    }
                case 100: // LD IDH,IDH
                    {
                        return 9;

                    }
                case 101: // LD IDH,IDL
                    {
                        SetIDH(GetIDL());
                        return 9;

                    }
                case 102: // LD H,(ID+d)
                    {
                        SetH(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case 103: // LD IDH,A
                    {
                        SetIDH(regA);
                        return 9;

                    }
                case 104: // LD IDL,B
                    {
                        SetIDL(regB);
                        return 9;

                    }
                case 105: // LD IDL,C
                    {
                        SetIDL(regC);
                        return 9;

                    }
                case 106: // LD IDL,D
                    {
                        SetIDL(GetD());
                        return 9;

                    }
                case 107: // LD IDL,E
                    {
                        SetIDL(GetE());
                        return 9;

                    }
                case 108: // LD IDL,IDH
                    {
                        SetIDL(GetIDH());
                        return 9;

                    }
                case 109: // LD IDL,IDL
                    {
                        return 9;

                    }
                case 110: // LD L,(ID+d)
                    {
                        SetL(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case 111: // LD IDL,A
                    {
                        SetIDL(regA);
                        return 9;

                    }
                case 112: // LD (ID+d),B
                    {
                        _mem.Pokeb(Id_d(), regB);
                        return 19;

                    }
                case 113: // LD (ID+d),C
                    {
                        _mem.Pokeb(Id_d(), regC);
                        return 19;

                    }
                case 114: // LD (ID+d),D
                    {
                        _mem.Pokeb(Id_d(), GetD());
                        return 19;

                    }
                case 115: // LD (ID+d),E
                    {
                        _mem.Pokeb(Id_d(), GetE());
                        return 19;

                    }
                case 116: // LD (ID+d),H
                    {
                        _mem.Pokeb(Id_d(), GetH());
                        return 19;

                    }
                case 117: // LD (ID+d),L
                    {
                        _mem.Pokeb(Id_d(), GetL());
                        return 19;

                    }
                case 118: // UNKNOWN
                    {
                        Interaction.MsgBox("Unknown ID instruction " + xxx + " at " + regPC);
                        return 0;
                    }
                case 119: // LD (ID+d),A
                    {
                        _mem.Pokeb(Id_d(), regA);
                        return 19;

                    }
                case var case9 when 120L <= case9 && case9 <= 123:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 124: // LD A,IDH
                    {
                        regA = (int)GetIDH();
                        return 9;

                    }
                case 125: // LD A,IDL
                    {
                        regA = (int)GetIDL();
                        return 9;

                    }
                case 126: // LD A,(ID+d)
                    {
                        regA = (int)_mem.Peekb(Id_d());
                        return 19;

                    }
                case 127:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }

                case var case10 when 128L <= case10 && case10 <= 131:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 132: // ADD A,IDH
                    {
                        Add_a(GetIDH());
                        return 9;

                    }
                case 133: // ADD A,IDL
                    {
                        Add_a(GetIDL());
                        return 9;

                    }
                case 134: // ADD A,(ID+d)
                    {
                        Add_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case11 when 135L <= case11 && case11 <= 139:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 140: // ADC A,IDH
                    {
                        Adc_a(GetIDH());
                        return 9;

                    }
                case 141: // ADC A,IDL
                    {
                        Adc_a(GetIDL());
                        return 9;

                    }
                case 142: // ADC A,(ID+d)
                    {
                        Adc_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case12 when 143L <= case12 && case12 <= 147:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 148: // SUB IDH
                    {
                        Sub_a(GetIDH());
                        return 9;

                    }
                case 149: // SUB IDL
                    {
                        Sub_a(GetIDL());
                        return 9;

                    }
                case 150: // SUB (ID+d)
                    {
                        Sub_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case13 when 151L <= case13 && case13 <= 155:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 156: // SBC A,IDH
                    {
                        Sbc_a(GetIDH());
                        return 9;

                    }
                case 157: // SBC A,IDL
                    {
                        Sbc_a(GetIDL());
                        return 9;

                    }
                case 158: // SBC A,(ID+d)
                    {
                        Sbc_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case 159:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }

                case var case14 when 160L <= case14 && case14 <= 163:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 164: // AND IDH
                    {
                        And_a(GetIDH());
                        return 9;

                    }
                case 165: // AND IDL
                    {
                        And_a(GetIDL());
                        return 9;

                    }
                case 166: // AND (ID+d)
                    {
                        And_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case15 when 167L <= case15 && case15 <= 171:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 172: // XOR IDH
                    {
                        Xor_a(GetIDH());
                        return 9;

                    }
                case 173: // XOR IDL
                    {
                        Xor_a(GetIDL());
                        return 9;

                    }
                case 174: // XOR (ID+d)
                    {
                        Xor_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case16 when 175L <= case16 && case16 <= 179:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 180: // OR IDH
                    {
                        Or_a(GetIDH());
                        return 9;

                    }
                case 181: // OR IDL
                    {
                        Or_a(GetIDL());
                        return 9;

                    }
                case 182: // OR (ID+d)
                    {
                        Or_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case var case17 when 183L <= case17 && case17 <= 187:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 188: // CP IDH
                    {
                        Cp_a(GetIDH());
                        return 9;

                    }
                case 189: // CP IDL
                    {
                        Cp_a(GetIDL());
                        return 9;

                    }
                case 190: // CP (ID+d)
                    {
                        Cp_a(_mem.Peekb(Id_d()));
                        return 19;

                    }
                case 191:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }

                case var case18 when 192L <= case18 && case18 <= 202:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 203: // prefix CB
                    {
                        lTemp = Id_d();
                        op = Nxtpcb();
                        Execute_id_cb(op, lTemp);
                        if ((op & 0xC0L) == 0x40L)
                            return 20;
                        else
                            return 23;

                    }
                case var case19 when 204L <= case19 && case19 <= 224:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 225: // POP ID
                    {
                        regID = (int)Popw();
                        return 14;

                    }
                case 226:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 227: // EX (SP),ID
                    {
                        lTemp = regID;
                        regID = (int)_mem.Peekw(regSP);
                        _mem.Pokew(regSP, lTemp);
                        return 23;

                    }
                case 228:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 229: // PUSH ID
                    {
                        Pushw(regID);
                        return 15;

                    }
                case var case20 when 230L <= case20 && case20 <= 232:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 233: // JP ID
                    {
                        regPC = regID;
                        return 8;

                    }
                case var case21 when 234L <= case21 && case21 <= 248:
                    {
                        regPC = (int)Dec16(regPC);
                        // // REFRESH -1
                        intRTemp = intRTemp - 1;
                        return 4;

                    }
                case 249: // LD SP,ID
                    {
                        regSP = regID;
                        return 10;

                    }

                default:
                    {
                        Interaction.MsgBox("Unknown ID instruction " + xxx + " at " + regPC);
                        return 0;
                    }
            }


        }


        private void SetIDH(int byteval)
        {
            regID = (byteval * 256 & 0xFF00 | regID & 0xFF);
        }
        private void SetIDL(int byteval)
        {
            regID = (int)(regID & 0xFF00L | byteval & 0xFFL);
        }

        private int GetIDH()
        {
            return (regID >> 8) & 0xFF;
        }
        private int GetIDL()
        {

            return regID & 0xFF;

        }

        private int Inc16(int a)
        {
             return a + 1 & 0xFFFF;
        }
        public int Nxtpcw()
        {
            int nxtpcwRet = default;
            nxtpcwRet = _mem.Peekb(regPC) + _mem.Peekb(regPC + 1) * 256;
            regPC = regPC + 2;
            return nxtpcwRet;
        }

        public int Nxtpcb()
        {
            int nxtpcbRet = default;
            nxtpcbRet = _mem.Peekb(regPC);
            regPC = regPC + 1;
            return nxtpcbRet;
        }


        public void SetD(int l)
        {
            regDE =(l << 8) | (regDE & 0x00FF);
        }
        public void SetE(int l)
        {
            regDE = regDE & 0xFF00 | l;
        }

        public void SetF(byte b)
        {
            fS = (b & F_S) != 0;
            fZ = (b & F_Z) != 0;
            //     f5 = (b & F_5) != 0;
            fH = (b & F_H) != 0;
            //    f3 = (b & F_3) != 0;
            fPV = (b & F_PV) != 0;
            fN = (b & F_N) != 0;
            fC = (b & F_C) != 0;
        }


        public void SetH(int l)
        {
            regHL = (l * 256) | (regHL & 0xFF);
        }



        public void SetL(int l)
        {
            regHL = (regHL & 0xFF00) | l;
        }



        public int Sra(int ans)
        {
      
            Boolean c;
            c = (ans & 0x1) != 0L;
            ans = (ans >> 1) | (ans & 0x80);

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;
            return ans;
        }

        public int Srl(int ans)
        {
            Boolean c = (ans & 0x1) != 0L;
            ans = ans >> 1;

            fS = (ans & F_S) != 0L;
            // f3 = (ans & F_3) != 0L;
            // f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;

            return ans;

        }

        public int Sls(int ans)
        {
            Boolean c = (ans & 0x80) != 0;
            ans = (ans * 2 | 0x1) & 0xFF;
            fS = (ans & F_S) != 0;
            f3 = (ans & F_3) != 0;
            f5 = (ans & F_5) != 0;
            fZ = ans == 0L;
            fPV = Parity[ans];
            fH = false;
            fN = false;
            fC = c;
            return ans;
        }


        public void Sub_a(int b)
        {
            int a;
            int wans;
            int ans;

            a = regA;
            wans = a - b;
            ans = wans & 0xFF;

            fS = (ans & F_S) != 0L;
            f3 = (ans & F_3) != 0L;
            f5 = (ans & F_5) != 0L;
            fZ = ans == 0L;
            fC = (wans & 0x100L) != 0L;
            fPV = ((a ^ b) & (a ^ ans) & 0x80L) != 0L;
            fH = ((a & 0xFL) - (b & 0xFL) & F_H) != 0L;
            fN = true;
            regA = ans;
        }


        public void Xor_a(int b)
        {
            regA = (int)((regA ^ b) & 0xFFL);
            fS = (regA & F_S) != 0;
            f3 = (regA & F_3) != 0;
            f5 = (regA & F_5) != 0;
            fH = false;
            fPV = Parity[regA];
            fZ = regA == 0;
            fN = false;
            fC = false;
        }

        public void Z80Reset()
        {
            int iCounter;

            regPC = 0;
            regSP = 0;
            regA = 0;
            SetF(0);
            SetBC(0);
            regDE = 0;
            regHL = 0;

            Exx();
            Ex_af_af();

            regA = 0;
            SetF(0);
            SetBC(0);
            regDE = 0;
            regHL = 0;

            regIX = 0;
            regIY = 0;

            intR = 128;
            intRTemp = 128;

            intI = 0;
            intIFF1 = false;
            intIFF2 = false;
            intIM = 0;

            _zx81.bInputWait = false;
            _zx81.ShowDisplay(false);

            _zx81.lHiresLoc = 0;
            for (iCounter = 0; iCounter <= 767; iCounter++)
                _zx81.LastScreen[iCounter] = 0;
            for (iCounter = 0; iCounter <= 6143; iCounter++)
                _zx81.gcBufferBits[iCounter] = 0;
            _zx81.ClearScreen();

        }

        public void Outb(int port, int outbyte, int tstates = 0)
        {
            if (port == 253)
                // SLOW Mode
                SetAF_(GetAF_() | 32768);      //0b1000 0000 0000 0000
            else if (port == 254)
                // FAST Mode
                SetAF_(GetAF_() & (32767));    //0b0111 1111 1111 1111
            else if (port == 0)
            {
                _zx81.bInputWait = true;
                _zx81.bBooting = false;
                _zx81.ShowDisplay(true);
                if (_mem.Peekb(SysVars.KB_DEBOUNCE) == 255)
                {
                    _mem.Pokeb(SysVars.KB_DEBOUNCE, outbyte);
                }
            }
            else if (port == 1)
            {
                _zx81.bInputWait = false;
                _zx81.bBooting = false;
                if ((_mem.Peekb(SysVars.CDFLAG) & 128) == 128)
                    _zx81.ShowDisplay(true);
                else
                    _zx81.ShowDisplay(false);
            }
        }

        public int Inb(int port)
        {
            int res;
            res = 0xFF;
            if ((port & 1) == 0)
            {
                // port = glMemAddrDiv256(port And &HFF00&)
                port = (int)((port & 0xFF00L) >> 8);
                if ((port & 1) == 0)
                    res = res & _zx81.keyCAPS_V;
                if ((port & 2) == 0)
                    res = res & _zx81.keyA_G;
                if ((port & 4) == 0)
                    res = res & _zx81.keyQ_T;
                if ((port & 8) == 0)
                    res = res & _zx81.key1_5;
                if ((port & 16) == 0)
                    res = res & _zx81.key6_0;
                if ((port & 32) == 0)
                    res = res & _zx81.keyY_P;
                if ((port & 64) == 0)
                    res = res & _zx81.keyH_ENT;
                if ((port & 128) == 0)
                    res = res & _zx81.keyB_SPC;
                // // Bit7 of the port FE is always 0 on the zx81 (or so it appears)
                res = res & 127;
            }
            return res;
        }
    }





}


