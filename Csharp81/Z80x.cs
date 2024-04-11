using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp81
{
    internal class Z80x
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

            // Modified by Allan Macpherson Feb 2024 to run under VB.NET 6.0
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
        private KeyPresses _kb; //= new KeyPresses();
        private ZX81 _zx81;


        public int LoopCount = 0;

            public Z80x(Memory mem, KeyPresses kb, ZX81 z)
            {
                _mem = mem;
                _kb = kb;
                _zx81 = z;
                intIFF1 = true;
                intIFF2 = true;
                intIM = 2;
                InitParity();
            }

            private void InitParity()
            {
                int iCounter;
                byte j;
                Boolean p;

                for (iCounter = 0; iCounter <= 255; iCounter++)
                {
                    p = true;
                    for (j = 0; j <= 7; j++)
                    {
                        if ((iCounter & (int)(Math.Pow(2, j))) != 0)
                        {
                            p = !p;
                        }
                    }
                    Parity[iCounter] = p;
                }
            }

            public void setintIFF1(bool a)
            {
                intIFF1 = a;
            }

            public void setintIFF2(bool a)
            {
                intIFF2 = a;
            }

            public void setintIM(byte a)
            {
                intIM = a;
            }


            public int getintI()
            {
                return intI;
            }


            public void SetPC(int Address)
            {
                regPC = (int)Address;
            }

            private void adc_a(int b)
            {
                int wans;
                int ans;
                var c = default(long);

                if (fC)
                    c = 1;

                wans = (int)(regA + b + c);
                ans = (int)(wans & 0xFFL);

                fS = (ans & F_S) != 0L;
                f3 = (ans & F_3) != 0L;
                f5 = (ans & F_5) != 0L;
                fZ = ans == 0L;
                fC = (wans & 0x100L) != 0L;
                fPV = (((long)regA ^ ~b & 0xFFFF) & (regA ^ ans) & 0x80L) != 0L;

                fH = ((regA & 0xFL) + (b & 0xFL) + c & F_H) != 0L;
                fN = false;

                regA = (int)ans;
            }

            private void add_a(int b)
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
            private int adc16(int a, int b)
            {
                int adc16Ret = default;
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

                adc16Ret = ans;
                return adc16Ret;
            }
            private int add16(int a, int b)
            {
                int add16Ret = default;
                int lans;
                int ans;

                lans = a + b;
                ans = lans & 0xFFFF;

                f3 = (ans & F_3 * 256L) != 0L;
                f5 = (ans & F_5 * 256L) != 0L;
                fC = (lans & 0x10000L) != 0L;
                fH = ((a & 0xFFFL) + (b & 0xFFFL) & 0x1000L) != 0L;
                fN = false;

                add16Ret = ans;
                return add16Ret;
            }
            private void and_a(int b)
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
            private void bit(int b, int r)
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
            private int bitRes(int bit, int val)
            {
                int bitResRet = default;
                bitResRet = val & (~bit & 0xFFFF);
                return bitResRet;
            }
            public int bitSet(int bit, int val)
            {
                int bitSetRet = default;
                bitSetRet = val | bit;
                return bitSetRet;
            }

            private void ccf()
            {
                f3 = (regA & F_3) != 0;
                f5 = (regA & F_5) != 0;
                fH = fC;
                fN = false;
                fC = !fC;
            }


            public void initParity()
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


            public void cp_a(int b)
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

            private void cpl_a()
            {
                regA = (int)((regA ^ 0xFFL) & 0xFFL);

                f3 = (regA & F_3) != 0;
                f5 = (regA & F_5) != 0;
                fH = true;
                fN = true;
            }
            private void daa_a()
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
                    sub_a(incr);
                }
                else
                {
                    add_a(incr);
                }

                ans = regA;
                fC = carry;
                fPV = Parity[ans];
        }
            private int dec16(int a)
            {
                int dec16Ret = default;
                dec16Ret = a - 1 & 0xFFFF;
                return dec16Ret;
            }
            private void ex_af_af()
            {
                int t;

                t = getAF();
                setAF(getAF_());
                setAF_(t);
            }
            private int execute_cb()
            {
               
                int xxx;

             
                intRTemp = intRTemp + 1;

                xxx = nxtpcb();

                switch (xxx)
                {
                    case 0:
                        {
                            // 000 RLC B
                            regB = (int)rlc(regB);
                           return 8;
                            
                        }
                    case 1:
                        {
                            // 001 RLC C
                            regC = (int)rlc(regC);
                           return 8;
                            
                        }
                    case 2:
                        {
                            // 002 RLC D
                            setD(rlc(getD()));
                           return 8;
                            
                        }
                    case 3:
                        {
                            // 003 RLC E
                            setE(rlc(getE()));
                           return 8;
                            
                        }
                    case 4:
                        {
                            // 004 RLC H
                            setH(rlc(getH()));
                           return 8;
                            
                        }
                    case 5:
                        {
                            // 005 RLC L
                            setL(rlc(getL()));
                           return 8;
                            
                        }
                    case 6:
                        {
                            // 006 RLC (HL)
                            _mem.pokeb(regHL, rlc(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 7:
                        {
                            // 007 RLC A
                            regA = (int)rlc(regA);
                           return 8;
                            
                        }
                    case 8:
                        {
                            // 008 RRC B
                            regB = (int)rrc(regB);
                           return 8;
                            
                        }
                    case 9:
                        {
                            // 009 RRC C
                            regC = (int)rrc(regC);
                           return 8;
                            
                        }
                    case 10:
                        {
                            // 010 RRC D
                            setD(rrc(getD()));
                           return 8;
                            
                        }
                    case 11:
                        {
                            // 011 RRC E
                            setE(rrc(getE()));
                           return 8;
                            
                        }
                    case 12:
                        {
                            // 012 RRC H
                            setH(rrc(getH()));
                           return 8;
                            
                        }
                    case 13:
                        {
                            // 013 RRC L
                            setL(rrc(getL()));
                           return 8;
                            
                        }
                    case 14:
                        {
                            // 014 RRC (HL)
                            _mem.pokeb(regHL, rrc(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 15:
                        {
                            // 015 RRC A
                            regA = (int)rrc(regA);
                           return 8;
                            
                        }
                    case 16:
                        {
                            // 016 RL B
                            regB = (int)rl(regB);
                           return 8;
                            
                        }
                    case 17:
                        {
                            // 017 RL C
                            regC = (int)rl(regC);
                           return 8;
                            
                        }
                    case 18:
                        {
                            // 018 RL D
                            setD(rl(getD()));
                           return 8;
                            
                        }
                    case 19:
                        {
                            // 019 RL E
                            setE(rl(getE()));
                           return 8;
                            
                        }
                    case 20:
                        {
                            // 020 RL H
                            setH(rl(getH()));
                           return 8;
                            
                        }
                    case 21:
                        {
                            // 021 RL L
                            setL(rl(getL()));
                           return 8;
                            
                        }
                    case 22:
                        {
                            // 022 RL (HL)
                            _mem.pokeb(regHL, rl(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 23:
                        {
                            // 023 RL A
                            regA = (int)rl(regA);
                           return 8;
                            
                        }
                    case 24:
                        {
                            // 024 RR B
                            regB = (int)rr(regB);
                           return 8;
                            
                        }
                    case 25:
                        {
                            // 025 RR C
                            regC = (int)rr(regC);
                           return 8;
                            
                        }
                    case 26:
                        {
                            // 026 RR D
                            setD(rr(getD()));
                           return 8;
                            
                        }
                    case 27:
                        {
                            // 027 RR E
                            setE(rr(getE()));
                           return 8;
                            
                        }
                    case 28:
                        {
                            // 028 RR H
                            setH(rr(getH()));
                           return 8;
                            
                        }
                    case 29:
                        {
                            // 029 RR L
                            setL(rr(getL()));
                           return 8;
                            
                        }
                    case 30:
                        {
                            // 030 RR (HL)
                            _mem.pokeb(regHL, rr(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 31:
                        {
                            // 031 RR A
                            regA = (int)rr(regA);
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
                            setD(sla(getD()));
                           return 8;
                            
                        }
                    case 35: // SLA E
                        {
                            setE(sla(getE()));
                           return 8;
                            
                        }
                    case 36: // SLA H
                        {
                            setH(sla(getH()));
                           return 8;
                            
                        }
                    case 37: // SLA L
                        {
                            setL(sla(getL()));
                           return 8;
                            
                        }
                    case 38: // SLA (HL)
                        {
                            _mem.pokeb(regHL, sla(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 39: // SLA A
                        {
                            regA = (int)sla(regA);
                           return 8;
                            
                        }
                    case 40: // SRA B
                        {
                            regB = (int)sra(regB);
                           return 8;
                            
                        }
                    case 41: // SRA C
                        {
                            regC = (int)sra(regC);
                           return 8;
                            
                        }
                    case 42: // SRA D
                        {
                            setD(sra(getD()));
                           return 8;
                            
                        }
                    case 43: // SRA E
                        {
                            setE(sra(getE()));
                           return 8;
                            
                        }
                    case 44: // SRA H
                        {
                            setH(sra(getH()));
                           return 8;
                            
                        }
                    case 45:  // SRA L
                        {
                            setL(sra(getL()));
                           return 8;
                            
                        }
                    case 46: // SRA (HL)
                        {
                            _mem.pokeb(regHL, sra(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 47: // SRA A
                        {
                            regA = (int)sra(regA);
                           return 8;
                            
                        }
                    case 48: // SLS B
                        {
                            regB = (int)sls(regB);
                           return 8;
                            
                        }
                    case 49: // SLS C
                        {
                            regC = (int)sls(regC);
                           return 8;
                            
                        }
                    case 50: // SLS D
                        {
                            setD(sls(getD()));
                           return 8;
                            
                        }
                    case 51: // SLS E
                        {
                            setE(sls(getE()));
                           return 8;
                            
                        }
                    case 52: // SLS H
                        {
                            setH(sls(getH()));
                           return 8;
                            
                        }
                    case 53: // SLS L
                        {
                            setL(sls(getL()));
                           return 8;
                            
                        }
                    case 54: // SLS (HL)
                        {
                            _mem.pokeb(regHL, sls(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 55: // SLS A
                        {
                            regA = (int)sls(regA);
                           return 8;
                            
                        }
                    case 56: // SRL B
                        {
                            regB = (int)srl(regB);
                           return 8;
                            
                        }
                    case 57: // SRL C
                        {
                            regC = (int)srl(regC);
                           return 8;
                            
                        }
                    case 58: // SRL D
                        {
                            setD(srl(getD()));
                           return 8;
                            
                        }
                    case 59: // SRL E
                        {
                            setE(srl(getE()));
                           return 8;
                            
                        }
                    case 60: // SRL H
                        {
                            setH(srl(getH()));
                           return 8;
                            
                        }
                    case 61: // SRL L
                        {
                            setL(srl(getL()));
                           return 8;
                            
                        }
                    case 62: // SRL (HL)
                        {
                            _mem.pokeb(regHL, srl(_mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 63: // SRL A
                        {
                            regA = (int)srl(regA);
                           return 8;
                            
                        }
                    case 64:
                        {
                            // 064 BIT 0,B
                            bit(0x1, regB);
                           return 8;
                            
                        }
                    case 65:
                        {
                            // 065 ' BIT 0,C
                            bit(1, regC);
                           return 8;
                            
                        }
                    case 66:
                        {
                            // 066 BIT 0,D
                            bit(1, getD());
                           return 8;
                            
                        }
                    case 67:
                        {
                            // 067 BIT 0,E
                            bit(1, getE());
                           return 8;
                            
                        }
                    case 68:
                        {
                            // 068 BIT 0,H
                            bit(1, getH());
                           return 8;
                            
                        }
                    case 69:
                        {
                            // 069 BIT 0,L
                            bit(1, getL());
                           return 8;
                            
                        }
                    case 70:
                        {
                            // 070 BIT 0,(HL)
                            bit(1, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 71:
                        {
                            // 071 BIT 0,A
                            bit(1, regA);
                           return 8;
                            
                        }
                    case 72: // BIT 1,B
                        {
                            bit(2, regB);
                           return 8;
                            
                        }
                    case 73: // BIT 1,C
                        {
                            bit(2, regC);
                           return 8;
                            
                        }
                    case 74: // BIT 1,D
                        {
                            bit(2, getD());
                           return 8;
                            
                        }
                    case 75: // BIT 1,E
                        {
                            bit(2, getE());
                           return 8;
                            
                        }
                    case 76: // BIT 1,H
                        {
                            bit(2, getH());
                           return 8;
                            
                        }
                    case 77: // BIT 1,L
                        {
                            bit(2, getL());
                           return 8;
                            
                        }
                    case 78: // BIT 1,(HL)
                        {
                            bit(2, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 79: // BIT 1,A
                        {
                            bit(2, regA);
                           return 8;
                            
                        }
                    case 80: // BIT 2,B
                        {
                            bit(4, regB);
                           return 8;
                            
                        }
                    case 81: // BIT 2,C
                        {
                            bit(4, regC);
                           return 8;
                            
                        }
                    case 82: // BIT 2,D
                        {
                            bit(4, getD());
                           return 8;
                            
                        }
                    case 83: // BIT 2,E
                        {
                            bit(4, getE());
                           return 8;
                            
                        }
                    case 84: // BIT 2,H
                        {
                            bit(4, getH());
                           return 8;
                            
                        }
                    case 85: // BIT 2,L
                        {
                            bit(4, getL());
                           return 8;
                            
                        }
                    case 86: // BIT 2,(HL)
                        {
                            bit(4, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 87: // BIT 2,A
                        {
                            bit(4, regA);
                           return 8;
                            
                        }
                    case 88: // BIT 3,B
                        {
                            bit(8, regB);
                           return 8;
                            
                        }
                    case 89: // BIT 3,C
                        {
                            bit(8, regC);
                           return 8;
                            
                        }
                    case 90: // BIT 3,D
                        {
                            bit(8, getD());
                           return 8;
                            
                        }
                    case 91: // BIT 3,E
                        {
                            bit(8, getE());
                           return 8;
                            
                        }
                    case 92: // BIT 3,H
                        {
                            bit(8, getH());
                           return 8;
                            
                        }
                    case 93: // BIT 3,L
                        {
                            bit(8, getL());
                           return 8;
                            
                        }
                    case 94: // BIT 3,(HL)
                        {
                            bit(8, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 95: // BIT 3,A
                        {
                            bit(8, regA);
                           return 8;
                            
                        }
                    case 96: // BIT 4,B
                        {
                            bit(0x10, regB);
                           return 8;
                            
                        }
                    case 97: // BIT 4,C
                        {
                            bit(0x10, regC);
                           return 8;
                            
                        }
                    case 98: // BIT 4,D
                        {
                            bit(0x10, getD());
                           return 8;
                            
                        }
                    case 99: // BIT 4,E
                        {
                            bit(0x10, getE());
                           return 8;
                            
                        }
                    case 100: // BIT 4,H
                        {
                            bit(0x10, getH());
                           return 8;
                            
                        }
                    case 101: // BIT 4,L
                        {
                            bit(0x10, getL());
                           return 8;
                            
                        }
                    case 102: // BIT 4,(HL)
                        {
                            bit(0x10, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 103: // BIT 4,A
                        {
                            bit(0x10, regA);
                           return 8;
                            
                        }
                    case 104: // BIT 5,B
                        {
                            bit(0x20, regB);
                           return 8;
                            
                        }
                    case 105: // BIT 5,C
                        {
                            bit(0x20, regC);
                           return 8;
                            
                        }
                    case 106: // BIT 5,D
                        {
                            bit(0x20, getD());
                           return 8;
                            
                        }
                    case 107: // BIT 5,E
                        {
                            bit(0x20, getE());
                           return 8;
                            
                        }
                    case 108: // BIT 5,H
                        {
                            bit(0x20, getH());
                           return 8;
                            
                        }
                    case 109: // BIT 5,L
                        {
                            bit(0x20, getL());
                           return 8;
                            
                        }
                    case 110: // BIT 5,(HL)
                        {
                            bit(0x20, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 111: // BIT 5,A
                        {
                            bit(0x20, regA);
                           return 8;
                            
                        }
                    case 112:
                        {
                            // 112 BIT 6,B
                            bit(0x40, regB);
                           return 8;
                            
                        }
                    case 113:
                        {
                            // 113 BIT 6,C
                            bit(0x40, regC);
                           return 8;
                            
                        }
                    case 114:
                        {
                            // 114 BIT 6,D
                            bit(0x40, getD());
                           return 8;
                            
                        }
                    case 115:
                        {
                            // 115 BIT 6,E
                            bit(0x40, getE());
                           return 8;
                            
                        }
                    case 116:
                        {
                            // 116 BIT 6,H
                            bit(0x40, getH());
                           return 8;
                            
                        }
                    case 117:
                        {
                            // 117 BIT 6,L
                            bit(0x40, getL());
                           return 8;
                            
                        }
                    case 118:
                        {
                            // 118 BIT 6,(HL)
                            bit(0x40, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 119:
                        {
                            // 119 ' BIT 6,A
                            bit(0x40, regA);
                           return 8;
                            
                        }
                    case 120:
                        {
                            // 120 BIT 7,B
                            bit(0x80, regB);
                           return 8;
                            
                        }
                    case 121:
                        {
                            // 121 BIT 7,C
                            bit(0x80, regC);
                           return 8;
                            
                        }
                    case 122:
                        {
                            // 122 BIT 7,D
                            bit(0x80, getD());
                           return 8;
                            
                        }
                    case 123:
                        {
                            // 123 BIT 7,E
                            bit(0x80, getE());
                           return 8;
                            
                        }
                    case 124:
                        {
                            // 124 BIT 7,H
                            bit(0x80, getH());
                           return 8;
                            
                        }
                    case 125:
                        {
                            // 125 BIT 7,L
                            bit(0x80, getL());
                           return 8;
                            
                        }
                    case 126:
                        {
                            // 126 BIT 7,(HL)
                            bit(0x80, _mem.peekb(regHL));
                           return 12;
                            
                        }
                    case 127:
                        {
                            // 127 BIT 7,A
                            bit(0x80, regA);
                           return 8;
                            
                        }
                    case 128: // RES 0,B
                        {
                            regB = (int)bitRes(1, regB);
                           return 8;
                            
                        }
                    case 129: // RES 0,C
                        {
                            regC = (int)bitRes(1, regC);
                           return 8;
                            
                        }
                    case 130: // RES 0,D
                        {
                            setD(bitRes(1, getD()));
                           return 8;
                            
                        }
                    case 131: // RES 0,E
                        {
                            setE(bitRes(1, getE()));
                           return 8;
                            
                        }
                    case 132: // RES 0,H
                        {
                            setH(bitRes(1, getH()));
                           return 8;
                            
                        }
                    case 133: // RES 0,L
                        {
                            setL(bitRes(1, getL()));
                           return 8;
                            
                        }
                    case 134: // RES 0,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(0x1, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 135: // RES 0,A
                        {
                            regA = (int)bitRes(1, regA);
                           return 8;
                            
                        }
                    case 136: // RES 1,B
                        {
                            regB = (int)bitRes(2, regB);
                           return 8;
                            
                        }
                    case 137: // RES 1,C
                        {
                            regC = (int)bitRes(2, regC);
                           return 8;
                            
                        }
                    case 138: // RES 1,D
                        {
                            setD(bitRes(2, getD()));
                           return 8;
                            
                        }
                    case 139: // RES 1,E
                        {
                            setE(bitRes(2, getE()));
                           return 8;
                            
                        }
                    case 140: // RES 1,H
                        {
                            setH(bitRes(2, getH()));
                           return 8;
                            
                        }
                    case 141: // RES 1,L
                        {
                            setL(bitRes(2, getL()));
                           return 8;
                            
                        }
                    case 142: // RES 1,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(2, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 143: // RES 1,A
                        {
                            regA = (int)bitRes(2, regA);
                           return 8;
                            
                        }
                    case 144: // RES 2,B
                        {
                            regB = (int)bitRes(4, regB);
                           return 8;
                            
                        }
                    case 145: // RES 2,C
                        {
                            regC = (int)bitRes(4, regC);
                           return 8;
                            
                        }
                    case 146: // RES 2,D
                        {
                            setD(bitRes(4, getD()));
                           return 8;
                            
                        }
                    case 147: // RES 2,E
                        {
                            setE(bitRes(4, getE()));
                           return 8;
                            
                        }
                    case 148: // RES 2,H
                        {
                            setH(bitRes(4, getH()));
                           return 8;
                            
                        }
                    case 149: // RES 2,L
                        {
                            setL(bitRes(4, getL()));
                           return 8;
                            
                        }
                    case 150: // RES 2,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(4, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 151: // RES 2,A
                        {
                            regA = (int)bitRes(4, regA);
                           return 8;
                            
                        }
                    case 152: // RES 3,B
                        {
                            regB = (int)bitRes(8, regB);
                           return 8;
                            
                        }
                    case 153: // RES 3,C
                        {
                            regC = (int)bitRes(8, regC);
                           return 8;
                            
                        }
                    case 154: // RES 3,D
                        {
                            setD(bitRes(8, getD()));
                           return 8;
                            
                        }
                    case 155: // RES 3,E
                        {
                            setE(bitRes(8, getE()));
                           return 8;
                            
                        }
                    case 156: // RES 3,H
                        {
                            setH(bitRes(8, getH()));
                           return 8;
                            
                        }
                    case 157: // RES 3,L
                        {
                            setL(bitRes(8, getL()));
                           return 8;
                            
                        }
                    case 158: // RES 3,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(8, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 159: // RES 3,A
                        {
                            regA = (int)bitRes(8, regA);
                           return 8;
                            
                        }
                    case 160: // RES 4,B
                        {
                            regB = (int)bitRes(0x10, regB);
                           return 8;
                            
                        }
                    case 161: // RES 4,C
                        {
                            regC = (int)bitRes(0x10, regC);
                           return 8;
                            
                        }
                    case 162: // RES 4,D
                        {
                            setD(bitRes(0x10, getD()));
                           return 8;
                            
                        }
                    case 163: // RES 4,E
                        {
                            setE(bitRes(0x10, getE()));
                           return 8;
                            
                        }
                    case 164: // RES 4,H
                        {
                            setH(bitRes(0x10, getH()));
                           return 8;
                            
                        }
                    case 165: // RES 4,L
                        {
                            setL(bitRes(0x10, getL()));
                           return 8;
                            
                        }
                    case 166: // RES 4,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(0x10, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 167: // RES 4,A
                        {
                            regA = (int)bitRes(0x10, regA);
                           return 8;
                            
                        }
                    case 168:
                        {
                            // 168 RES 5,B
                            regB = (int)bitRes(0x20, regB);
                           return 8;
                            
                        }
                    case 169:
                        {
                            // 169 RES 5,C
                            regC = (int)bitRes(0x20, regC);
                           return 8;
                            
                        }
                    case 170:
                        {
                            // 170 RES 5,D
                            setD(bitRes(0x20, getD()));
                           return 8;
                            
                        }
                    case 171:
                        {
                            // 171 RES 5,E
                            setE(bitRes(0x20, getE()));
                           return 8;
                            
                        }
                    case 172: // RES 5,H
                        {
                            setH(bitRes(0x20, getH()));
                           return 8;
                            
                        }
                    case 173: // RES 5,L
                        {
                            setL(bitRes(0x20, getL()));
                           return 8;
                            
                        }
                    case 174: // RES 5,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(0x20, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 175: // RES 5,A
                        {
                            regA = (int)bitRes(0x20, regA);
                           return 8;
                            
                        }
                    case 176: // RES 6,B
                        {
                            regB = (int)bitRes(0x40, regB);
                           return 8;
                            
                        }
                    case 177: // RES 6,C
                        {
                            regC = (int)bitRes(0x40, regC);
                           return 8;
                            
                        }
                    case 178: // RES 6,D
                        {
                            setD(bitRes(0x40, getD()));
                           return 8;
                            
                        }
                    case 179: // RES 6,E
                        {
                            setE(bitRes(0x40, getE()));
                           return 8;
                            
                        }
                    case 180: // RES 6,H
                        {
                            setH(bitRes(0x40, getH()));
                           return 8;
                            
                        }
                    case 181: // RES 6,L
                        {
                            setL(bitRes(0x40, getL()));
                           return 8;
                            
                        }
                    case 182: // RES 6,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(0x40, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 183: // RES 6,A
                        {
                            regA = (int)bitRes(0x40, regA);
                           return 8;
                            
                        }
                    case 184: // RES 7,B
                        {
                            regB = (int)bitRes(0x80, regB);
                           return 8;
                            
                        }
                    case 185: // RES 7,C
                        {
                            regC = (int)bitRes(0x80, regC);
                           return 8;
                            
                        }
                    case 186: // RES 7,D
                        {
                            setD(bitRes(0x80, getD()));
                           return 8;
                            
                        }
                    case 187: // RES 7,E
                        {
                            setE(bitRes(0x80, getE()));
                           return 8;
                            
                        }
                    case 188: // RES 7,H
                        {
                            setH(bitRes(0x80, getH()));
                           return 8;
                            
                        }
                    case 189: // RES 7,L
                        {
                            setL(bitRes(0x80, getL()));
                           return 8;
                            
                        }
                    case 190: // RES 7,(HL)
                        {
                            _mem.pokeb(regHL, bitRes(0x80, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 191: // RES 7,A
                        {
                            regA = (int)bitRes(0x80, regA);
                           return 8;
                            
                        }
                    case 192: // SET 0,B
                        {
                            regB = (int)bitSet(1, regB);
                           return 8;
                            
                        }
                    case 193: // SET 0,C
                        {
                            regC = (int)bitSet(1, regC);
                           return 8;
                            
                        }
                    case 194: // SET 0,D
                        {
                            setD(bitSet(1, getD()));
                           return 8;
                            
                        }
                    case 195: // SET 0,E
                        {
                            setE(bitSet(1, getE()));
                           return 8;
                            
                        }
                    case 196: // SET 0,H
                        {
                            setH(bitSet(1, getH()));
                           return 8;
                            
                        }
                    case 197: // SET 0,L
                        {
                            setL(bitSet(1, getL()));
                           return 8;
                            
                        }
                    case 198: // SET 0,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(1, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 199: // SET 0,A
                        {
                            regA = (int)bitSet(1, regA);
                           return 8;
                            
                        }
                    case 200: // SET 1,B
                        {
                            regB = (int)bitSet(2, regB);
                           return 8;
                            
                        }
                    case 201: // SET 1,C
                        {
                            regC = (int)bitSet(2, regC);
                           return 8;
                            
                        }
                    case 202: // SET 1,D
                        {
                            setD(bitSet(2, getD()));
                           return 8;
                            
                        }
                    case 203: // SET 1,E
                        {
                            setE(bitSet(2, getE()));
                           return 8;
                            
                        }
                    case 204: // SET 1,H
                        {
                            setH(bitSet(2, getH()));
                           return 8;
                            
                        }
                    case 205: // SET 1,L
                        {
                            setL(bitSet(2, getL()));
                           return 8;
                            
                        }
                    case 206: // SET 1,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(2, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 207: // SET 1,A
                        {
                            regA = (int)bitSet(2, regA);
                           return 8;
                            
                        }
                    case 208: // SET 2,B
                        {
                            regB = (int)bitSet(4, regB);
                           return 8;
                            
                        }
                    case 209: // SET 2,C
                        {
                            regC = (int)bitSet(4, regC);
                           return 8;
                            
                        }
                    case 210: // SET 2,D
                        {
                            setD(bitSet(4, getD()));
                           return 8;
                            
                        }
                    case 211: // SET 2,E
                        {
                            setE(bitSet(4, getE()));
                           return 8;
                            
                        }
                    case 212: // SET 2,H
                        {
                            setH(bitSet(4, getH()));
                           return 8;
                            
                        }
                    case 213: // SET 2,L
                        {
                            setL(bitSet(4, getL()));
                           return 8;
                            
                        }
                    case 214: // SET 2,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(0x4, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 215: // SET 2,A
                        {
                            regA = (int)bitSet(4, regA);
                           return 8;
                            
                        }
                    case 216: // SET 3,B
                        {
                            regB = (int)bitSet(8, regB);
                           return 8;
                            
                        }
                    case 217: // SET 3,C
                        {
                            regC = (int)bitSet(8, regC);
                           return 8;
                            
                        }
                    case 218: // SET 3,D
                        {
                            setD(bitSet(8, getD()));
                           return 8;
                            
                        }
                    case 219: // SET 3,E
                        {
                            setE(bitSet(8, getE()));
                           return 8;
                            
                        }
                    case 220: // SET 3,H
                        {
                            setH(bitSet(8, getH()));
                           return 8;
                            
                        }
                    case 221: // SET 3,L
                        {
                            setL(bitSet(8, getL()));
                           return 8;
                            
                        }
                    case 222: // SET 3,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(0x8, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 223: // SET 3,A
                        {
                            regA = (int)bitSet(8, regA);
                           return 8;
                            
                        }
                    case 224: // SET 4,B
                        {
                            regB = (int)bitSet(0x10, regB);
                           return 8;
                            
                        }
                    case 225: // SET 4,C
                        {
                            regC = (int)bitSet(0x10, regC);
                           return 8;
                            
                        }
                    case 226: // SET 4,D
                        {
                            setD(bitSet(0x10, getD()));
                           return 8;
                            
                        }
                    case 227: // SET 4,E
                        {
                            setE(bitSet(0x10, getE()));
                           return 8;
                            
                        }
                    case 228: // SET 4,H
                        {
                            setH(bitSet(0x10, getH()));
                           return 8;
                            
                        }
                    case 229: // SET 4,L
                        {
                            setL(bitSet(0x10, getL()));
                           return 8;
                            
                        }
                    case 230: // SET 4,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(0x10, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 231: // SET 4,A
                        {
                            regA = (int)bitSet(0x10, regA);
                           return 8;
                            
                        }
                    case 232: // SET 5,B
                        {
                            regB = (int)bitSet(0x20, regB);
                           return 8;
                            
                        }
                    case 233: // SET 5,C
                        {
                            regC = (int)bitSet(0x20, regC);
                           return 8;
                            
                        }
                    case 234: // SET 5,D
                        {
                            setD(bitSet(0x20, getD()));
                           return 8;
                            
                        }
                    case 235: // SET 5,E
                        {
                            setE(bitSet(0x20, getE()));
                           return 8;
                            
                        }
                    case 236: // SET 5,H
                        {
                            setH(bitSet(0x20, getH()));
                           return 8;
                            
                        }
                    case 237: // SET 5,L
                        {
                            setL(bitSet(0x20, getL()));
                           return 8;
                            
                        }
                    case 238: // SET 5,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(0x20, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 239: // SET 5,A
                        {
                            regA = (int)bitSet(0x20, regA);
                           return 8;
                            
                        }
                    case 240: // SET 6,B
                        {
                            regB = (int)bitSet(0x40, regB);
                           return 8;
                            
                        }
                    case 241: // SET 6,C
                        {
                            regC = (int)bitSet(0x40, regC);
                           return 8;
                            
                        }
                    case 242: // SET 6,D
                        {
                            setD(bitSet(0x40, getD()));
                           return 8;
                            
                        }
                    case 243: // SET 6,E
                        {
                            setE(bitSet(0x40, getE()));
                           return 8;
                            
                        }
                    case 244: // SET 6,H
                        {
                            setH(bitSet(0x40, getH()));
                           return 8;
                            
                        }
                    case 245: // SET 6,L
                        {
                            setL(bitSet(0x40, getL()));
                           return 8;
                            
                        }
                    case 246: // SET 6,(HL)
                        {
                            _mem.pokeb(regHL, bitSet(0x40, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 247: // SET 6,A
                        {
                            regA = (int)bitSet(0x40, regA);
                           return 8;
                            
                        }
                    case 248: // SET 7,B
                        {
                            regB = (int)bitSet(0x80, regB);
                           return 8;
                            
                        }
                    case 249: // SET 7,C
                        {
                            regC = (int)bitSet(0x80, regC);
                           return 8;
                            
                        }
                    case 250: // SET 7,D
                        {
                            setD(bitSet(0x80, getD()));
                           return 8;
                            
                        }
                    case 251: // SET 7,E
                        {
                            setE(bitSet(0x80, getE()));
                           return 8;
                            
                        }
                    case 252:
                        {
                            // 252 SET 7,H
                            setH(bitSet(0x80, getH()));
                           return 8;
                            
                        }
                    case 253:
                        {
                            // 253 SET 7,L
                            setL(bitSet(0x80, getL()));
                           return 8;
                            
                        }
                    case 254:
                        {
                            // 254 SET 7,(HL)
                            _mem.pokeb(regHL, bitSet(0x80, _mem.peekb(regHL)));
                           return 15;
                            
                        }
                    case 255:
                        {
                            // 255 SET 7,A
                            regA = (int)bitSet(0x80, regA);
                           return 8;
                            
                        }
                }
                return 0;
            }



            public int sla(int ans)
            {
                int slaRet = default;
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

                slaRet = ans;
                return slaRet;
            }

            private int execute_ed(int local_tstates)
            {
             
                int xxx;
                int count;
                int dest;
                int @from;
                int TempLocal_tstates;
                Boolean c;
                int b;

               
                intRTemp = intRTemp + 1;

                xxx = nxtpcb();

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
                            regB = (int)in_bc();
                            return 12;
                          
                        }
                    case 65:
                        {
                            // 065 OUT (c),B
                            outb(getBC(), regB);
                            return 12;
                            
                        }
                    case 66:
                        {
                            // 066 SBC HL,BC
                            regHL = (int)sbc16(regHL, getBC());
                            return 15;
                            
                        }
                    case 67:
                        {
                            // 067 LD (nn),BC
                            _mem.pokew(nxtpcw(), getBC());
                            return 20;
                            
                        }
                    case 68:
                        {
                            // 068 NEG
                            neg_a();
                            return 8;
                            
                        }
                    case 69:
                        {
                            // 069 RETn
                            intIFF1 = intIFF2;
                            poppc();
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
                            regC = (int)in_bc();
                            return 12;
                            
                        }
                    case 73:
                        {
                            // 073 OUT (c),C
                            outb(getBC(), regC);
                            return 12;
                            
                        }
                    case 74:
                        {
                            // 074 ADC HL,BC
                            regHL = (int)adc16(regHL, getBC());
                            return 15;
                            
                        }
                    case 75:
                        {
                            // 075 LD BC,(nn)
                            setBC(_mem.peekw(nxtpcw()));
                            return 20;
                            
                        }
                    case 76:
                        {
                            // 076 NEG
                            neg_a();
                            return 8;
                            
                        }
                    case 77:
                        {
                            // 077 RETI
                            // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                            // //          copied to IFF1 for RETI - but in a real Z80 it is
                            intIFF1 = intIFF2;
                            poppc();
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
                            setD(in_bc());
                            return 12;
                            
                        }
                    case 81:
                        {
                            // 081 OUT (c),D
                            outb(getBC(), getD());
                            return 12;
                            
                        }
                    case 82:
                        {
                            // 082 SBC HL,DE
                            regHL = (int)sbc16(regHL, regDE);
                            return 15;
                            
                        }
                    case 83:
                        {
                            // 083 LD (nn),DE
                            _mem.pokew(nxtpcw(), regDE);
                            return 20;
                            
                        }
                    case 84:
                        {
                            // NEG
                            neg_a();
                            return 8;
                            
                        }
                    case 85:
                        {
                            // 85 RETn
                            intIFF1 = intIFF2;
                            poppc();
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
                            ld_a_i();
                            return 9;
                            
                        }
                    case 88:
                        {
                            // 088 IN E,(c)
                            setE(in_bc());
                            return 12;
                            
                        }
                    case 89:
                        {
                            // 089 OUT (c),E
                            outb(getBC(), getE());
                            return 12;
                            
                        }
                    case 90:
                        {
                            // 090 ADC HL,DE
                            regHL = (int)adc16(regHL, regDE);
                            return 15;
                            
                        }
                    case 91:
                        {
                            // 091 LD DE,(nn)
                            regDE = (int)_mem.peekw(nxtpcw());
                            return 20;
                            
                        }
                    case 92:
                        {
                            // NEG
                            neg_a();
                            return 8;
                            
                        }
                    case 93:
                        {
                            // 93 RETI
                            // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                            // //          copied to IFF1 for RETI - but in a real Z80 it is
                            intIFF1 = intIFF2;
                            poppc();
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
                            ld_a_r();
                            return 9;
                            
                        }
                    case 96: // IN H,(c)
                        {
                            setH(in_bc());
                            return 12;
                            
                        }
                    case 97: // OUT (c),H
                        {
                            outb(getBC(), getH());
                            return 12;
                            
                        }
                    case 98: // SBC HL,HL
                        {
                            regHL = (int)sbc16(regHL, regHL);
                            return 15;
                            
                        }
                    case 99: // LD (nn),HL
                        {
                            _mem.pokew(nxtpcw(), regHL);
                            return 20;
                            
                        }
                    case 100: // NEG
                        {
                            neg_a();
                            return 8;
                            
                        }
                    case 101: // RETn
                        {
                            intIFF1 = intIFF2;
                            poppc();
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
                            setL(in_bc());
                            return 12;
                            
                        }
                    case 105: // OUT (c),L
                        {
                            outb(getBC(), getL());
                            return 12;
                            
                        }
                    case 106: // ADC HL,HL
                        {
                            regHL = (int)adc16(regHL, regHL);
                            return 15;
                            
                        }
                    case 107: // LD HL,(nn)
                        {
                            regHL = (int)_mem.peekw(nxtpcw());
                            return 20;
                            
                        }
                    case 108: // NEG
                        {
                            neg_a();
                            return 8;
                            
                        }
                    case 109: // RETI
                        {
                            // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                            // //          copied to IFF1 for RETI - but in a real Z80 it is
                            intIFF1 = intIFF2;
                            poppc();
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
                            in_bc();
                            return 12;
                            
                        }
                    case 113: // OUT (c),0
                        {
                            outb(getBC(), 0);
                            return 12;
                            
                        }
                    case 114: // SBC HL,SP
                        {
                            regHL = (int)sbc16(regHL, regSP);
                            return 15;
                            
                        }
                    case 115: // LD (nn),SP
                        {
                            _mem.pokew(nxtpcw(), regSP);
                            return 20;
                            
                        }
                    case 116: // NEG
                        {
                            neg_a();
                            return 8;
                            
                        }
                    case 117: // RETn
                        {
                            intIFF1 = intIFF2;
                            poppc();
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
                            regA = (int)in_bc();
                            return 12;
                            
                        }
                    case 121: // OUT (c),A
                        {
                            outb(getBC(), regA);
                            return 12;
                            
                        }
                    case 122: // ADC HL,SP
                        {
                            regHL = (int)adc16(regHL, regSP);
                            return 15;
                            
                        }
                    case 123: // LD SP,(nn)
                        {
                            regSP = (int)_mem.peekw(nxtpcw());
                            return 20;
                            
                        }
                    case 124: // NEG
                        {
                            neg_a();
                            return 8;
                            
                        }
                    case 125: // RETI
                        {
                            // // TOCHECK: according to the official Z80 docs, IFF2 does not get
                            // //          copied to IFF1 for RETI - but in a real Z80 it is
                            intIFF1 = intIFF2;
                            poppc();
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
                            _mem.pokeb(regDE, _mem.peekb(regHL));

                            //f3 = Conversions.ToBoolean(F_3 & _mem.peekb(regHL) + regA); // // TOCHECK: Is this correct?
                            //f5 = Conversions.ToBoolean(2L & _mem.peekb(regHL) + regA);   // // TOCHECK: Is this correct?

                            regDE = (int)inc16(regDE);
                            regHL = (int)inc16(regHL);
                            setBC(dec16(getBC()));

                            fPV = getBC() != 0L;
                            fH = false;
                            fN = false;

                            return 16;
                            
                        }
                    case 161: // CPI
                        {
                            c = fC;

                            cp_a(_mem.peekb(regHL));
                            regHL = (int)inc16(regHL);
                            setBC(dec16(getBC()));

                            fPV = getBC() != 0L;
                            fC = c;

                            return 16;
                            
                        }
                    case 162: // INI
                        {
                        _mem.pokeb(regHL, inb(getBC()));
                        b = qdec8(regB);
                            regB = (int)b;
                            regHL = (int)inc16(regHL);

                            fZ = b == 0L;
                            fN = true;

                            return 16;
                            
                        }
                    case 163: // OUTI
                        {
                            b = qdec8(regB);
                            regB = (int)b;
                            outb(getBC(), _mem.peekb(regHL));
                            regHL = (int)inc16(regHL);

                            fZ = b == 0L;
                            fN = true;

                            return 16;
                            
                        }

                    // /* xxD */
                    case 168: // LDD
                        {
                            _mem.pokeb(regDE, _mem.peekb(regHL));

                            //f3 = Conversions.ToBoolean(F_3 & _mem.peekb(regHL) + regA); // // TOCHECK: Is this correct?
                            //f5 = Conversions.ToBoolean(2L & _mem.peekb(regHL) + regA);   // // TOCHECK: Is this correct?

                            regDE = (int)dec16(regDE);
                            regHL = (int)dec16(regHL);
                            setBC(dec16(getBC()));

                            fPV = getBC() != 0L;
                            fH = false;
                            fN = false;

                            return 16;
                            
                        }
                    case 169: // CPD
                        {
                            c = fC;

                            cp_a(_mem.peekb(regHL));
                            regHL = (int)dec16(regHL);
                            setBC(dec16(getBC()));

                            fPV = getBC() != 0L;
                            fC = c;

                            return 16;
                            
                        }
                    case 170: // IND
                        {
                            _mem.pokeb(regHL, inb(getBC()));
                            b = qdec8(regB);
                            regB = (int)b;
                            regHL = (int)dec16(regHL);

                            fZ = b == 0;
                            fN = true;

                            return 16;
                            
                        }
                    case 171: // OUTD
                        {
                            count = qdec8(regB);
                            regB = (int)count;
                            outb(getBC(), _mem.peekb(regHL));
                            regHL = (int)dec16(regHL);

                            fZ = count == 0L;
                            fN = true;

                            return 16;
                            
                        }

                    // // xxIR
                    case 176: // LDIR
                        {
                            TempLocal_tstates = 0;
                            count = getBC();
                            dest = regDE;
                            from = regHL;

                            // // REFRESH -2
                            intRTemp = intRTemp - 2;
                            do
                            {
                                _mem.pokeb(dest, _mem.peekb(from));
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
                            setBC(count);

                            return TempLocal_tstates;
                            
                        }
                    case 177: // CPIR
                        {
                            c = fC;

                            cp_a(_mem.peekb(regHL));
                            regHL = (int)inc16(regHL);
                            setBC(dec16(getBC()));

                            fC = c;
                            c = getBC() != 0;
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
                            _mem.pokeb(regHL, inb(getBC()));
                            b = qdec8(regB);
                            regB = (int)b;
                            regHL = (int)inc16(regHL);

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
                            b = qdec8(regB);
                            regB = (int)b;
                            outb(getBC(), _mem.peekb(regHL));
                            regHL = (int)inc16(regHL);

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
                            count = getBC();
                            dest = regDE;
                            from = regHL;

                            // // REFRESH -2
                            intRTemp = intRTemp - 2;
                            do
                            {
                                _mem.pokeb(dest, _mem.peekb(from));
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
                            setBC(count);

                            return TempLocal_tstates;
                            
                        }
                    case 185: // CPDR
                        {
                            c = fC;

                            cp_a(_mem.peekb(regHL));
                            regHL = (int)dec16(regHL);
                            setBC(dec16(getBC()));

                            fPV = getBC() != 0L;
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
                            _mem.pokeb(regHL, inb(getBC()));
                            b = qdec8(regB);
                            regB = (int)b;
                            regHL = (int)dec16(regHL);

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
                            b = qdec8(regB);
                            regB = (int)b;
                            outb(getBC(), _mem.peekb(regHL));
                            regHL = (int)dec16(regHL);

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
                t = _mem.peekb(regHL);
                q = t;

                t = (t * 16) | (ans & 0xF);
                ans = (ans & 0xF0) | (q / 16);
                _mem.pokeb(regHL, t & 0xFF);

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
                t = _mem.peekb(regHL);
                q = t;

                t = (t / 16) | (ans * 16);
                ans = (ans & 0xF0) | (q & 0xF);
                _mem.pokeb(regHL, t);

                fS = (ans & F_S) != 0L;
                f3 = (ans & F_3) != 0L;
                f5 = (ans & F_5) != 0L;
                fZ = ans == 0L;
                fPV = intIFF2;
                fH = false;
                fN = false;

                regA = (int)ans;
            }
            private void neg_a()
            {
                int t;

                t = regA;
                regA = 0;
                sub_a(t);
            }
            private void execute_id_cb(int op, int z)
            {
                switch (op)
                {
                    case 0: // RLC B
                        {
                            op = rlc(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 1: // RLC C
                        {
                            op = rlc(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 2: // RLC D
                        {
                            op = rlc(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 3: // RLC E
                        {
                            op = rlc(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 4: // RLC H
                        {
                            op = rlc(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 5: // RLC L
                        {
                            op = rlc(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 6: // RLC (HL)
                        {
                            _mem.pokeb(z, rlc(_mem.peekb(z)));
                            break;
                        }
                    case 7: // RLC A
                        {
                            op = rlc(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }

                    case 8: // RRC B
                        {
                            op = rrc(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 9: // RRC C
                        {
                            op = rrc(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 10: // RRC D
                        {
                            op = rrc(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 11: // RRC E
                        {
                            op = rrc(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 12: // RRC H
                        {
                            op = rrc(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 13: // RRC L
                        {
                            op = rrc(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 14: // RRC (HL)
                        {
                            _mem.pokeb(z, rrc(_mem.peekb(z)));
                            break;
                        }
                    case 15: // RRC A
                        {
                            op = rrc(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 16: // RL B
                        {
                            op = rl(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 17: // RL C
                        {
                            op = rl(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 18: // RL D
                        {
                            op = rl(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 19: // RL E
                        {
                            op = rl(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 20: // RL H
                        {
                            op = rl(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 21: // RL L
                        {
                            op = rl(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 22: // RL (HL)
                        {
                            _mem.pokeb(z, rl(_mem.peekb(z)));
                            break;
                        }
                    case 23: // RL A
                        {
                            op = rl(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 24: // RR B
                        {
                            op = rr(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 25: // RR C
                        {
                            op = rr(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 26: // RR D
                        {
                            op = rr(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 27: // RR E
                        {
                            op = rr(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 28: // RR H
                        {
                            op = rr(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 29: // RR L
                        {
                            op = rr(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 30: // RR (HL)
                        {
                            _mem.pokeb(z, rl(_mem.peekb(z)));
                            break;
                        }
                    case 31: // RR A
                        {
                            op = rr(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 32: // SLA B
                        {
                            op = sla(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 33: // SLA C
                        {
                            op = sla(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 34: // SLA D
                        {
                            op = sla(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 35: // SLA E
                        {
                            op = sla(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 36: // SLA H
                        {
                            op = sla(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 37: // SLA L
                        {
                            op = sla(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 38: // SLA (HL)
                        {
                            _mem.pokeb(z, sla(_mem.peekb(z)));
                            break;
                        }
                    case 39: // SLA A
                        {
                            op = sla(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 40: // SRA B
                        {
                            op = sra(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 41: // SRA C
                        {
                            op = sra(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 42: // SRA D
                        {
                            op = sra(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 43: // SRA E
                        {
                            op = sra(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 44: // SRA H
                        {
                            op = sra(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 45: // SRA L
                        {
                            op = sra(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 46: // SRA (HL)
                        {
                            _mem.pokeb(z, sra(_mem.peekb(z)));
                            break;
                        }
                    case 47: // SRA A
                        {
                            op = sra(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 48: // SLS B
                        {
                            op = sls(_mem.peekb(z));
                            regB = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 49: // SLS C
                        {
                            op = sls(_mem.peekb(z));
                            regC = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 50: // SLS D
                        {
                            op = sls(_mem.peekb(z));
                            setD(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 51: // SLS E
                        {
                            op = sls(_mem.peekb(z));
                            setE(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 52: // SLS H
                        {
                            op = sls(_mem.peekb(z));
                            setH(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 53: // SLS L
                        {
                            op = sls(_mem.peekb(z));
                            setL(op);
                            _mem.pokeb(z, op);
                            break;
                        }
                    case 54: // SLS (HL)
                        {
                            _mem.pokeb(z, sls(_mem.peekb(z)));
                            break;
                        }
                    case 55: // SLS A
                        {
                            op = sls(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }


                    case 62: // SRL (HL)
                        {
                            _mem.pokeb(z, srl(_mem.peekb(z)));
                            break;
                        }
                    case 63: // SRL A
                        {
                            op = srl(_mem.peekb(z));
                            regA = (int)op;
                            _mem.pokeb(z, op);
                            break;
                        }
                    case var @case when 64L <= @case && @case <= 71: // BIT 0,B
                        {
                            bit(0x1, _mem.peekb(z));
                            break;
                        }
                    case var case1 when 72L <= case1 && case1 <= 79: // BIT 1,B
                        {
                            bit(0x2, _mem.peekb(z));
                            break;
                        }
                    case var case2 when 80L <= case2 && case2 <= 87: // BIT 2,B
                        {
                            bit(0x4, _mem.peekb(z));
                            break;
                        }
                    case var case3 when 88L <= case3 && case3 <= 95: // BIT 3,B
                        {
                            bit(0x8, _mem.peekb(z));
                            break;
                        }

                    case var case4 when 96L <= case4 && case4 <= 103: // BIT 4,B
                        {
                            bit(0x10, _mem.peekb(z));
                            break;
                        }
                    case var case5 when 104L <= case5 && case5 <= 111: // BIT 5,B
                        {
                            bit(0x20, _mem.peekb(z));
                            break;
                        }
                    case var case6 when 112L <= case6 && case6 <= 119: // BIT 6,B
                        {
                            bit(0x40, _mem.peekb(z));
                            break;
                        }
                    case var case7 when 120L <= case7 && case7 <= 127: // BIT 7,B
                        {
                            bit(0x80, _mem.peekb(z));
                            break;
                        }
                    case 134: // RES 0,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x1, _mem.peekb(z)));
                            break;
                        }
                    case 142: // RES 1,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x2, _mem.peekb(z)));
                            break;
                        }
                    case 150: // RES 2,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x4, _mem.peekb(z)));
                            break;
                        }
                    case 158: // RES 3,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x8, _mem.peekb(z)));
                            break;
                        }
                    case 166: // RES 4,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x10, _mem.peekb(z)));
                            break;
                        }
                    case 172: // RES 5,H
                        {
                            setH(bitRes(0x20, _mem.peekb(z)));
                            _mem.pokeb(z, getH());
                            break;
                        }
                    case 174: // RES 5,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x20, _mem.peekb(z)));
                            break;
                        }
                    case 175: // RES 5,A
                        {
                            regA = (int)bitRes(0x20, _mem.peekb(z));
                            _mem.pokeb(z, regA);
                            break;
                        }
                    case 182: // RES 6,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x40, _mem.peekb(z)));
                            break;
                        }
                    case 190: // RES 7,(HL)
                        {
                            _mem.pokeb(z, bitRes(0x80, _mem.peekb(z)));
                            break;
                        }
                    case 198: // SET 0,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x1, _mem.peekb(z)));
                            break;
                        }
                    case 206: // SET 1,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x2, _mem.peekb(z)));
                            break;
                        }
                    case 214: // SET 2,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x4, _mem.peekb(z)));
                            break;
                        }
                    case 222: // SET 3,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x8, _mem.peekb(z)));
                            break;
                        }
                    case 230: // SET 4,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x10, _mem.peekb(z)));
                            break;
                        }
                    case 238: // SET 5,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x20, _mem.peekb(z)));
                            break;
                        }
                    case 246: // SET 6,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x40, _mem.peekb(z)));
                            break;
                        }
                    case 254: // SET 7,(HL)
                        {
                            _mem.pokeb(z, bitSet(0x80, _mem.peekb(z)));
                            break;
                        }
                    case 255: // SET 7,A
                        {
                            regA = (int)bitSet(0x80, _mem.peekb(z));
                            _mem.pokeb(z, regA);
                            break;
                        }

                    default:
                        {
                            Interaction.MsgBox("Invalid ID CB op=" + op + " z=" + z);
                            break;
                        }
                }
            }

            private void exx()
            {
                int t;

                t = regHL;
                regHL = regHL_;
                regHL_ = (int)t;

                t = regDE;
                regDE = regDE_;
                regDE_ = (int)t;

                t = getBC();
                setBC(regBC_);
                regBC_ = (int)t;
            }
            private int getAF()
            {
                int getAFRet = default;
                getAFRet = (regA * 256) | getF();
                return getAFRet;
            }

            public int getAF_()
            {
                int getAF_Ret = default;
                getAF_Ret = regAF_;
                return getAF_Ret;
            }

            public void setAF_(int a)
            {
                regAF_ = a;
            }


            private int getBC()
            {
                int getBCRet = default;
                getBCRet = (regB * 256) | regC;
                return getBCRet;
            }
            private int getD()
            {
            return regDE >> 8;
        }
            private int getE()
            {
            return regDE & 0xFF;
        }
   
            private int getF()
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


        private int getH()
        {
            // getH = glMemAddrDiv256(regHL)
            return regHL >> 8;
        }
        private int getL()
        {
            return regHL & 0xFF;
        }

        private int getR()
        {
            return intR;
        }
        private int id_d()
            {
              
                int d;

                d = nxtpcb();
                if ((d & 128) == 128)
                    d = -(256 - d);
                return (regID + d) & 0xFFFF;
         
            }
            private void ld_a_i()
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
            private void ld_a_r()
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
            private int in_bc()
            {
                int in_bcRet = default;
                int ans;

                ans = inb(getBC());

                fZ = ans == 0L;
                fS = (ans & F_S) != 0L;
                f3 = (ans & F_3) != 0L;
                f5 = (ans & F_5) != 0L;
                fPV = Parity[ans];
                fN = false;
                fH = false;

                in_bcRet = ans;
                return in_bcRet;
            }
            private int inc8(int ans)
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
            private int dec8(int ans)
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
            public void interrupt()
            {
                _zx81.HandleInterrupt(intI);
            }

            private Boolean interruptTriggered(int tstates)
            {
            return (tstates >= 0);
        }
            private void or_a(int b)
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
          
       

         
            public void poppc()
            {
                regPC = (int)popw();
            }
            private int popw()
            {
               
                int t = _mem.peekb(regSP) | (_mem.peekb(regSP + 1) * 256);
                regSP = (regSP + 2 & 0xFFFF);
                return t;
            }
            public void pushpc()
            {
                pushw(regPC);
            }

            private void pushw(int word)
            {
                regSP = (int)(regSP - 2 & 0xFFFF);
                _mem.pokew(regSP, word);
            }
            private int rl(int ans)
            {
                int rlRet = default;
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

                rlRet = ans;
                return rlRet;
            }
            private void rl_a()
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
            private int rlc(int ans)
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
            private void rlc_a()
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

            private int rr(int ans)
            {
                int rrRet = default;
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

                rrRet = ans;
                return rrRet;
            }
            private void rr_a()
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

            private int rrc(int ans)
            {
                int rrcRet = default;
                Boolean c;

                c =(ans & 0x1) != 0;

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

                rrcRet = ans;
                return rrcRet;
            }

            private void rrc_a()
            {
                Boolean c;

                c =(regA & 1) != 0;

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
            private void sbc_a(int b)
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
            private int sbc16(int a, int b)
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

            private void scf()
            {
                f3 = (regA & F_3) != 0;
                f5 = (regA & F_5) != 0;
                fN = false;
                fH = false;
                fC = true;
            }
            public void setAF(int v)
            {
            regA = (v & 0xFF00) >> 8;
            setF((byte)(v & 0xFF));
            }
            public void setBC(int nn)
            {
               regB = (nn & 0xFF00) >> 8;
                regC = nn & 0xFF;
            }

            public void execute()
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
                    if (local_tstates >= 0L)
                    {
                        // // Trigger an interrupt
                        interrupt();
                        local_tstates = local_tstates - _zx81.glTstatesPerInterrupt;
                    }
                    // // REFRESH 1
                    intRTemp = intRTemp + 1;


                    if (regPC == 0x4092)
                    {
                        LoopCount = 0;
                    }



                    if (regPC == 0x419A & xxx == 204L)   // 
                    {
                        LoopCount = 0;
                    }

                    if (regPC == 0x4281 & xxx == 62L)   // maze drawing time up
                    {
                        LoopCount = 0;
                    }


                    xxx = nxtpcb();






                    switch (xxx)
                    {

                        case 0:
                            {
                                // 000 NOP
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 1:
                            {
                                // 001 LD BC,nn
                                setBC(nxtpcw());
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 2:
                            {
                                // 002 LD (BC),A
                                _mem.pokeb(getBC(), regA);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 3:
                            {
                                // 003 INC BC
                                setBC(inc16(getBC()));
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 4:
                            {
                                // 004 INC B
                                regB = (int)inc8(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 5:
                            {
                                // 005 DEC B
                                regB = (int)dec8(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 6:
                            {
                                // 006 LD B,n
                                regB = (int)nxtpcb();
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 7:
                            {
                                // 007 RLCA
                                rlc_a();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 8:
                            {
                                // 008 EX AF,AF'
                                ex_af_af();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 9:
                            {
                                // 009 ADD HL,BC
                                regHL = (int)add16(regHL, getBC());
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 10:
                            {
                                // 010 LD A,(BC)
                                regA = (int)_mem.peekb(getBC());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 11:
                            {
                                // 011 DEC BC
                                setBC(dec16(getBC()));
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 12:
                            {
                                // 012 INC C
                                regC = (int)inc8(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 13:
                            {
                                // 013 DEC C
                                regC = (int)dec8(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 14:
                            {
                                // 014 LD C,n
                                regC = (int)nxtpcb();
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 15:
                            {
                                // 015 RRCA
                                rrc_a();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 16:
                            {
                                // 016 DJNZ dis
                                lTemp = qdec8(regB);

                                regB = (int)lTemp;
                                if (lTemp != 0L)
                                {
                                    d = nxtpcb();
                                    if ((d & 128)==128)
                                        d = -(256 - d);
                                    regPC = (int)(regPC + d & 0xFFFF);
                                    local_tstates = local_tstates + 13;
                                }
                                else
                                {
                                    regPC = (int)inc16(regPC);
                                    local_tstates = local_tstates + 8;
                                }

                                break;
                            }
                        case 17:
                            {
                                // 017 LD DE,nn
                                regDE = (int)nxtpcw();
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 18:
                            {
                                // 018 LD (DE),A
                                _mem.pokeb(regDE, regA);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 19:
                            {
                                // 019 INC DE
                                regDE = (int)inc16(regDE);
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 20:
                            {
                                // 020 INC D
                                setD(inc8(getD()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 21:
                            {
                                // 021 DEC D
                                setD(dec8(getD()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 22:
                            {
                                // 022 LD D,n
                                setD(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 23:
                            {
                                // 023 ' RLA
                                rl_a();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 24:
                            {
                                // 024 JR dis
                                d = nxtpcb();
                                if ((d & 128)==128)
                                    d = -(256 - d);
                                regPC = (int)(regPC + d & 0xFFFF);
                                local_tstates = local_tstates + 12;
                                break;
                            }
                        case 25:
                            {
                                // 025 ADD HL,DE
                                regHL = (int)add16(regHL, regDE);
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 26:
                            {
                                // 026 LD A,(DE)
                                regA = (int)_mem.peekb(regDE);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 27:
                            {
                                // 027 DEC DE
                                regDE = (int)dec16(regDE);
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 28:
                            {
                                // 028 INC E
                                setE(inc8(getE()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 29:
                            {
                                // 029 DEC E
                                setE(dec8(getE()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 30:
                            {
                                // 030 LD E,n
                                setE(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 31:
                            {
                                // 031 RRA
                                rr_a();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 32:
                            {
                                // 032 JR NZ dis
                                if (fZ == false)
                                {
                                    d = nxtpcb();
                                    if ((d & 128)==128)
                                        d = -(256 - d);
                                    regPC = (int)(regPC + d & 0xFFFF);
                                    local_tstates = local_tstates + 12;
                                }
                                else
                                {
                                    regPC = (int)inc16(regPC);
                                    local_tstates = local_tstates  + 7;
                                }

                                break;
                            }
                        case 33:
                            {
                                // 033 LD HL,nn
                                regHL = (int)nxtpcw();
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 34:
                            {
                                // 034 LD (nn),HL
                                _mem.pokew(nxtpcw(), regHL);
                                local_tstates = local_tstates + 16;
                                break;
                            }
                        case 35:
                            {
                                // 035 INC HL
                                regHL = (int)inc16(regHL);
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 36:
                            {
                                // 036 INC H
                                setH(inc8(getH()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 37:
                            {
                                // 037 DEC H
                                setH(dec8(getH()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 38:
                            {
                                // 038 LD H,n
                                setH(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 39:
                            {
                                // 039 DAA
                                daa_a();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 40:
                            {
                                // 040 JR Z dis
                                if (fZ == true)
                                {
                                    d = nxtpcb();
                                    if ((d & 128)==128)
                                        d = -(256 - d);
                                    regPC = (regPC + d & 0xFFFF);
                                    local_tstates = local_tstates + 12;
                                }
                                else
                                {
                                    regPC = (int)inc16(regPC);
                                    local_tstates = local_tstates  + 7;
                                }

                                break;
                            }
                        case 41:
                            {
                                // 041 ADD HL,HL
                                regHL = (int)add16(regHL, regHL);
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 42:
                            {
                                // 042 LD HL,(nn)
                                regHL = _mem.peekw(nxtpcw());
                                local_tstates = local_tstates + 16;
                                break;
                            }
                        case 43:
                            {
                                // 043 DEC HL
                                regHL = dec16(regHL);
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 44:
                            {
                                // 044 INC L
                                setL(inc8(getL()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 45:
                            {
                                // 045 DEC L
                                setL(dec8(getL()));
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 46:
                            {
                                // 046 LD L,n
                                setL(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 47:
                            {
                                // 047 CPL
                                cpl_a();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 48:
                            {
                                // 048 JR NC dis
                                if (fC == false)
                                {
                                    d = nxtpcb();
                                    if ((d & 128)==128)
                                        d = -(256 - d);
                                    regPC = (regPC + d & 0xFFFF);
                                    local_tstates = local_tstates + 12;
                                }
                                else
                                {
                                    regPC = (int)inc16(regPC);
                                    local_tstates = local_tstates  + 7;
                                }

                                break;
                            }
                        case 49:
                            {
                                // 049 LD SP,nn
                                regSP = (int)nxtpcw();
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 50:
                            {
                                // 050 LD (nn),A
                                _mem.pokeb(nxtpcw(), regA);
                                local_tstates = local_tstates + 13;
                                break;
                            }
                        case 51:
                            {
                                // 051 INC SP
                                regSP = (int)inc16(regSP);
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 52:
                            {
                                // 052 INC (HL)
                                _mem.pokeb(regHL, inc8(_mem.peekb(regHL)));
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 53:
                            {
                                // 053 DEC (HL)
                                _mem.pokeb(regHL, dec8(_mem.peekb(regHL)));
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 54:
                            {
                                // 054 LD (HL),n
                                _mem.pokeb(regHL, nxtpcb());
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 55:
                            {
                                // 055 SCF
                                scf();
                                local_tstates = local_tstates  + 4;
                                break;
                            }


                        case 56:
                            {
                                // 056 JR C dis
                                if (fC == true)
                                {
                                    d = nxtpcb();
                                    if ((d & 128)==128)
                                        d = -(256 - d);
                                    regPC = (regPC + d & 0xFFFF);
                                    local_tstates = local_tstates + 12;
                                }
                                else
                                {
                                    regPC = (int)inc16(regPC);
                                    local_tstates = local_tstates  + 7;
                                }

                                break;
                            }
                        case 57:
                            {
                                // 057 ADD HL,SP
                                regHL = (int)add16(regHL, regSP);
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 58:
                            {
                                // 058 LD A,(nn)
                                regA = (int)_mem.peekb(nxtpcw());
                                local_tstates = local_tstates + 13;
                                break;
                            }
                        case 59:
                            {
                                // 059 DEC SP
                                regSP = (int)dec16(regSP);
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 60:
                            {
                                // 060 INC A
                                regA = (int)inc8(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 61:
                            {
                                // 061 DEC A
                                regA = (int)dec8(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 62:
                            {
                                // 062 LD A,n
                                regA = (int)nxtpcb();
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 63:
                            {
                                // 063 CCF
                                ccf();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 64:
                            {
                                // LD B,B
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 65:
                            {
                                // 65 ' LD B,C
                                regB = regC;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 66:
                            {
                                // LD B,D
                                regB = (int)getD();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 67:
                            {
                                // 67 ' LD B,E
                                regB = (int)getE();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 68:
                            {
                                // LD B,H
                                regB = (int)getH();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 69:
                            {
                                // 69 ' LD B,L
                                regB = (int)getL();
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 70:
                            {
                                // LD B,(HL)
                                regB = (int)_mem.peekb(regHL);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 71:
                            {
                                // 71 ' LD B,A
                                regB = regA;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 72:
                            {
                                // 72 ' LD C,B
                                regC = regB;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 73:
                            {
                                // 73 ' LD C,C
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 74:
                            {
                                // 74 ' LD C,D
                                regC = (int)getD();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 75:
                            {
                                // 75 ' LD C,E
                                regC = (int)getE();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 76:
                            {
                                // 76 ' LD C,H
                                regC = (int)getH();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 77:
                            {
                                // 77 ' LD C,L
                                regC = (int)getL();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 78:
                            {
                                // 78 ' LD C,(HL)
                                regC = (int)_mem.peekb(regHL);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 79:
                            {
                                // 79 ' LD C,A
                                regC = regA;
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 80: // LD D,B
                            {
                                setD(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 81: // LD D,C
                            {
                                setD(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 82: // LD D,D
                            {
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 83: // LD D,E
                            {
                                setD(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 84: // LD D,H
                            {
                                setD(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 85: // LD D,L
                            {
                                setD(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 86: // LD D,(HL)
                            {
                                setD(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 87: // LD D,A
                            {
                                setD(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        // // LD E,*
                        case 88: // LD E,B
                            {
                                setE(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 89: // LD E,C
                            {
                                setE(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 90: // LD E,D
                            {
                                setE(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 91: // LD E,E
                            {
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 92: // LD E,H
                            {
                                setE(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 93: // LD E,L
                            {
                                setE(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 94: // LD E,(HL)
                            {
                                setE(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 95: // LD E,A
                            {
                                setE(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 96: // LD H,B
                            {
                                setH(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 97: // LD H,C
                            {
                                setH(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 98: // LD H,D
                            {
                                setH(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 99: // LD H,E
                            {
                                setH(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 100: // LD H,H
                            {
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 101: // LD H,L
                            {
                                setH(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 102: // LD H,(HL)
                            {
                                setH(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 103: // LD H,A
                            {
                                setH(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        // // LD L,*
                        case 104: // LD L,B
                            {
                                setL(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 105: // LD L,C
                            {
                                setL(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 106: // LD L,D
                            {
                                setL(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 107: // LD L,E
                            {
                                setL(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 108: // LD L,H
                            {
                                setL(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 109: // LD L,L
                            {
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 110: // LD L,(HL)
                            {
                                setL(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 111: // LD L,A
                            {
                                setL(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 112:
                            {
                                // 112 ' LD (HL),B
                                _mem.pokeb(regHL, regB);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 113:
                            {
                                // 113 ' LD (HL),C
                                _mem.pokeb(regHL, regC);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 114:
                            {
                                // 114 ' LD (HL),D
                                _mem.pokeb(regHL, getD());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 115:
                            {
                                // 115 ' LD (HL),E
                                _mem.pokeb(regHL, getE());
                                local_tstates = local_tstates  + 7;
                                break;
                            }

                        case 116: // LD (HL),H
                            {
                                _mem.pokeb(regHL, getH());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 117: // LD (HL),L
                            {
                                _mem.pokeb(regHL, getL());
                                local_tstates = local_tstates  + 7;
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
                                _mem.pokeb(regHL, regA);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 120:
                            {
                                // 120 ' LD A,B
                                regA = regB;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 121:
                            {
                                // 121 ' LD A,C
                                regA = regC;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 122:
                            {
                                // 122 ' LD A,D
                                regA = (int)getD();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 123:
                            {
                                // 123 ' LD A,E
                                regA = (int)getE();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case var @case when @case == 124:
                            {
                                // 124 ' LD A,H
                                regA = (int)getH();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case var case1 when case1 == 125:
                            {
                                // 125 ' LD A,L
                                regA = (int)getL();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 126:
                            {
                                // 126 ' LD A,(HL)
                                regA = (int)_mem.peekb(regHL);
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 127:
                            {
                                // 127 ' LD A,A
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        // // ADD A,*
                        case 128: // ADD A,B
                            {
                                add_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 129: // ADD A,C
                            {
                                add_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 130: // ADD A,D
                            {
                                add_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 131: // ADD A,E
                            {
                                add_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 132: // ADD A,H
                            {
                                add_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 133: // ADD A,L
                            {
                                add_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 134: // ADD A,(HL)
                            {
                                add_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 135: // ADD A,A
                            {
                                add_a(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 136: // ADC A,B
                            {
                                adc_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 137: // ADC A,C
                            {
                                adc_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 138: // ADC A,D
                            {
                                adc_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 139: // ADC A,E
                            {
                                adc_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 140: // ADC A,H
                            {
                                adc_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 141: // ADC A,L
                            {
                                adc_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 142: // ADC A,(HL)
                            {
                                adc_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 143: // ADC A,A
                            {
                                adc_a(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 144: // SUB B
                            {
                                sub_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 145: // SUB C
                            {
                                sub_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 146: // SUB D
                            {
                                sub_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 147: // SUB E
                            {
                                sub_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 148: // SUB H
                            {
                                sub_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 149: // SUB L
                            {
                                sub_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 150: // SUB (HL)
                            {
                                sub_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 151: // SUB A
                            {
                                sub_a(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 152: // SBC A,B
                            {
                                sbc_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 153: // SBC A,C
                            {
                                sbc_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 154: // SBC A,D
                            {
                                sbc_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 155: // SBC A,E
                            {
                                sbc_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 156: // SBC A,H
                            {
                                sbc_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 157: // SBC A,L
                            {
                                sbc_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 158: // SBC A,(HL)
                            {
                                sbc_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 159: // SBC A,A
                            {
                                sbc_a(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 160: // AND B
                            {
                                and_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 161: // AND C
                            {
                                and_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 162: // AND D
                            {
                                and_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 163: // AND E
                            {
                                and_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 164: // AND H
                            {
                                and_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 165: // AND L
                            {
                                and_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 166: // AND (HL)
                            {
                                and_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 167: // AND A
                            {
                                and_a(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 168: // XOR B
                            {
                                xor_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 169: // XOR C
                            {
                                xor_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 170: // XOR D
                            {
                                xor_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 171: // XOR E
                            {
                                xor_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 172: // XOR H
                            {
                                xor_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 173: // XOR L
                            {
                                xor_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 174: // XOR (HL)
                            {
                                xor_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
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
                                local_tstates = local_tstates  + 4;
                                break;
                            }

                        case 176: // OR B
                            {
                                or_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 177: // OR C
                            {
                                or_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 178: // OR D'
                            {
                                or_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 179: // OR E
                            {
                                or_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 180: // OR H
                            {
                                or_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 181: // OR L
                            {
                                or_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 182: // OR (HL)
                            {
                                or_a(_mem.peekb(regHL));
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 183: // OR A
                            {
                                or_a(regA);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        // // CP
                        case 184: // CP B
                            {
                                cp_a(regB);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 185: // CP C
                            {
                                cp_a(regC);
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 186: // CP D
                            {
                                cp_a(getD());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 187: // CP E
                            {
                                cp_a(getE());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 188: // CP H
                            {
                                cp_a(getH());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 189: // CP L
                            {
                                cp_a(getL());
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 190: // CP (HL)
                            {
                                cp_a(_mem.peekb(regHL));

                                if (regPC == 16803)
                                {
                                    int asdassa = regA;
                                    if (regA != 0)
                                    {
                                        int asda = regA;
                                    }
                                }


                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 191: // CP A
                            {
                                cp_a(regA);
                                break;
                            }

                        case 192: // RET NZ
                            {
                                if (fZ == false)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
                                }
                                else
                                {
                                    local_tstates = local_tstates + 5;
                                }

                                break;
                            }
                        case 193: // POP BC
                            {
                                setBC(popw());
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 194: // JP NZ,nn
                            {
                                if (fZ == false)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 195: // JP nn
                            {
                                regPC = (int)_mem.peekw(regPC);
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 196: // CALL NZ,nn
                            {
                                if (fZ == false)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 197: // PUSH BC
                            {
                                pushw(getBC());
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 198: // ADD A,n
                            {
                                add_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 199: // RST 0
                            {
                                pushpc();
                                regPC = 0;
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 200: // RET Z
                            {
                                if (fZ)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
                                }
                                else
                                {
                                    local_tstates = local_tstates + 5;
                                }

                                break;
                            }
                        case 201: // RET
                            {
                                poppc();
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 202: // JP Z,nn
                            {
                                if (fZ)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 203: // Prefix CB
                            {
                                local_tstates = local_tstates + execute_cb();
                                break;
                            }
                        case 204: // CALL Z,nn
                            {
                                if (fZ)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 205: // CALL nn
                            {
                                pushw(regPC + 2);
                                regPC = (int)nxtpcw();
                                local_tstates = local_tstates + 17;
                                break;
                            }
                        case 206: // ADC A,n
                            {
                                adc_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 207: // RST 8
                            {
                                pushpc();
                                regPC = 8;
                                local_tstates = local_tstates  + 11;
                                break;
                            }

                        case 208: // RET NC
                            {
                                if (fC == false)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
                                }
                                else
                                {
                                    local_tstates = local_tstates + 5;
                                }

                                break;
                            }
                        case 209: // POP DE
                            {
                                regDE = (int)popw();
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 210: // JP NC,nn
                            {
                                if (fC == false)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 211: // OUT (n),A
                            {
                                outb(nxtpcb(), regA);
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 212: // CALL NC,nn
                            {
                                if (fC == false)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 213: // PUSH DE
                            {
                                pushw(regDE);
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 214: // SUB n
                            {
                                sub_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 215: // RST 16
                            {
                                pushpc();
                                regPC = 16;
                                local_tstates = local_tstates  + 11;
                                break;
                            }

                        case 216: // RET C
                            {
                                if (fC)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
                                }
                                else
                                {
                                    local_tstates = local_tstates + 5;
                                }

                                break;
                            }
                        case 217: // EXX
                            {
                                exx();
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 218: // JP C,nn
                            {
                                if (fC)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 219: // IN A,(n)
                            {
                                regA = inb((regA * 256) | nxtpcb());
                                local_tstates = local_tstates  + 11;
                                break;
                            }

                        case 220: // CALL C,nn
                            {
                                if (fC)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 221: // prefix IX
                            {
                            regID = regIX;
                            local_tstates = local_tstates + execute_id();
                            // ZX81 Specific
                            _zx81.HiResRoutine(regIX, regID);
                            //
                            regIX = regID;
                            break;
                        }
                        case 222: // SBC n
                            {
                                sbc_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 223: // RST 24
                            {
                                pushpc();
                                regPC = 24;
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 224:
                            {
                                // 224 ' RET PO
                                if (fPV == false)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
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
                                regHL = (int)popw();
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 226:
                            {
                                // 226 JP PO,nn
                                if (fPV == false)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 227:
                            {
                                // 227 ' EX (SP),HL
                                lTemp = regHL;
                                regHL = (int)_mem.peekw(regSP);
                                _mem.pokew(regSP, lTemp);
                                local_tstates = local_tstates + 19;
                                break;
                            }
                        case 228:
                            {
                                // 228 ' CALL PO,nn
                                if (fPV == false)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 229:
                            {
                                // 229 ' PUSH HL
                                pushw(regHL);
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 230:
                            {
                                // 230 ' AND n
                                and_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 231:
                            {
                                // 231 ' RST 32
                                pushpc();
                                regPC = 32;
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 232:
                            {
                                // 232 ' RET PE
                                if (fPV)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
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
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 234:
                            {
                                // 234 ' JP PE,nn
                                if (fPV)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 235:
                            {
                                // 235 ' EX DE,HL
                                lTemp = regHL;
                                regHL = regDE;
                                regDE = (int)lTemp;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 236:
                            {
                                // 236 ' CALL PE,nn
                                if (fPV)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 237:
                            {
                                // 237 ' prefix ED
                                local_tstates = local_tstates + execute_ed(local_tstates);
                                break;
                            }
                        case 238:
                            {
                                // 238 ' XOR n
                                xor_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 239:
                            {
                                // 239 ' RST 40
                                pushpc();
                                regPC = 40;
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 240:
                            {
                                // 240 RET P
                                if (fS == false)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
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
                                setAF(popw());
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 242:
                            {
                                // 242 JP P,nn
                                if (fS == false)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 243:
                            {
                                // 243 DI
                                intIFF1 = false;
                                intIFF2 = false;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 244:
                            {
                                // 244 CALL P,nn
                                if (fS == false)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 245:
                            {
                                // 245 PUSH AF
                                pushw(getAF());
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 246:
                            {
                                // 246 OR n
                                or_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 247:
                            {
                                // 247 RST 48
                                pushpc();
                                regPC = 48;
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                        case 248:
                            {
                                // 248 RET M
                                if (fS)
                                {
                                    poppc();
                                    local_tstates = local_tstates  + 11;
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
                                local_tstates = local_tstates  + 6;
                                break;
                            }
                        case 250:
                            {
                                // 250 JP M,nn
                                if (fS)
                                {
                                    regPC = (int)nxtpcw();
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                }
                                local_tstates = local_tstates  + 10;
                                break;
                            }
                        case 251:
                            {
                                // 251 EI
                                intIFF1 = true;
                                intIFF2 = true;
                                local_tstates = local_tstates  + 4;
                                break;
                            }
                        case 252:
                            {
                                // 252 CALL M,nn
                                if (fS)
                                {
                                    pushw(regPC + 2);
                                    regPC = (int)nxtpcw();
                                    local_tstates = local_tstates + 17;
                                }
                                else
                                {
                                    regPC = regPC + 2;
                                    local_tstates = local_tstates  + 10;
                                }

                                break;
                            }
                        case 253:
                            {
                                // 253 prefix IY
                                regID = regIY;
                                local_tstates = local_tstates + execute_id();
                                regIY = regID;
                                break;
                            }
                        case 254:
                            {
                                // 254 CP n
                                cp_a(nxtpcb());
                                local_tstates = local_tstates  + 7;
                                break;
                            }
                        case 255:
                            {
                                // 255 RST 56
                                pushpc();
                                regPC = 56;
                                local_tstates = local_tstates  + 11;
                                break;
                            }
                    }
                }
                while (true);   // end of main loop


            }
            private int qdec8(int a)
            {
                int qdec8Ret = default;
                qdec8Ret = a - 1 & 0xFF;
                return qdec8Ret;
            }
            private int execute_id()
            {
               
                int xxx;
                int lTemp;
                int op;


                // // Yes, I appreciate that GOTO's and labels are a hideous blashphemy!
                // // However, this code is the fastest possible way of fetching and handling
                // // Z80 instructions I could come up with. There are only 8 compares per
                // // instruction fetch rather than between 1 and 255 as required in
                // // the previous version of vb81 with it's huge Case statement.
                // //
                // // I know it's slightly harder to follow the new code, but I think the
                // // speed increase justifies it. <CC>


                // // REFRESH 1
                intRTemp = intRTemp + 1;

                xxx = nxtpcb();


                switch (xxx)
                {
                    case var @case when 0L <= @case && @case <= 8:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 9: // ADD ID,BC
                        {
                            regID = (int)add16(regID, getBC());
                           return 15;
                            
                        }
                    case var case1 when 10L <= case1 && case1 <= 24:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 25: // ADD ID,DE
                        {
                            regID = (int)add16(regID, regDE);
                           return 15;
                            
                        }
                    case var case2 when 26L <= case2 && case2 <= 31:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }

                    case 32:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 33: // LD ID,nn
                        {
                            regID = (int)nxtpcw();
                           return 14;
                            
                        }
                    case 34: // LD (nn),ID
                        {
                            _mem.pokew(nxtpcw(), regID);
                           return 20;
                            
                        }
                    case 35: // INC ID
                        {
                            regID = (int)inc16(regID);
                           return 10;
                            
                        }
                    case 36: // INC IDH
                        {
                            setIDH(inc8(getIDH()));
                           return 9;
                            
                        }
                    case 37: // DEC IDH
                        {
                            setIDH(dec8(getIDH()));
                           return 9;
                            
                        }
                    case 38: // LD IDH,n
                        {
                            setIDH(nxtpcb());
                           return 11;
                            
                        }
                    case 39:
                    case 40:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 41: // ADD ID,ID
                        {
                            lTemp = regID;
                            regID = (int)add16(lTemp, lTemp);
                           return 15;
                            
                        }
                    case 42: // LD ID,(nn)
                        {
                            regID = (int)_mem.peekw(nxtpcw());
                           return 20;
                            
                        }
                    case 43: // DEC ID
                        {
                            regID = (int)dec16(regID);
                           return 10;
                            
                        }
                    case 44: // INC IDL
                        {
                            setIDL(inc8(getIDL()));
                           return 9;
                            
                        }
                    case 45: // DEC IDL
                        {
                            setIDL(dec8(getIDL()));
                           return 9;
                            
                        }
                    case 46: // LD IDL,n
                        {
                            setIDL(nxtpcb());
                           return 11;
                            
                        }
                    case var case3 when 47L <= case3 && case3 <= 51:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 52: // INC (ID+d)
                        {
                            lTemp = id_d();
                            _mem.pokeb(lTemp, inc8(_mem.peekb(lTemp)));
                           return 23;
                            
                        }
                    case 53: // DEC (ID+d)
                        {
                            lTemp = id_d();
                            _mem.pokeb(lTemp, dec8(_mem.peekb(lTemp)));
                           return 23;
                            
                        }
                    case 54: // LD (ID+d),n
                        {
                            lTemp = id_d();
                            _mem.pokeb(lTemp, nxtpcb());
                           return 19;
                            
                        }
                    case 55:
                    case 56:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 57: // ADD ID,SP
                        {
                            regID = (int)add16(regID, regSP);
                           return 15;
                            
                        }
                    case var case4 when 58L <= case4 && case4 <= 63:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }

                    case var case5 when 64L <= case5 && case5 <= 67:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 68: // LD B,IDH
                        {
                            regB = (int)getIDH();
                           return 9;
                            
                        }
                    case 69: // LD B,IDL
                        {
                            regB = (int)getIDL();
                           return 9;
                            
                        }
                    case 70: // LD B,(ID+d)
                        {
                            regB = (int)_mem.peekb(id_d());
                           return 19;
                            
                        }
                    case var case6 when 71L <= case6 && case6 <= 75:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 76: // LD C,IDH
                        {
                            regC = (int)getIDH();
                           return 9;
                            
                        }
                    case 77: // LD C,IDL
                        {
                            regC = (int)getIDL();
                           return 9;
                            
                        }
                    case 78: // LD C,(ID+d)
                        {
                            regC = (int)_mem.peekb(id_d());
                           return 19;
                            
                        }
                    case var case7 when 79L <= case7 && case7 <= 83:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 84: // LD D,IDH
                        {
                            setD(getIDH());
                           return 9;
                            
                        }
                    case 85: // LD D,IDL
                        {
                            setD(getIDL());
                           return 9;
                            
                        }
                    case 86: // LD D,(ID+d)
                        {
                            setD(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case8 when 87L <= case8 && case8 <= 91:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 92: // LD E,IDH
                        {
                            setE(getIDH());
                           return 9;
                            
                        }
                    case 93: // LD E,IDL
                        {
                            setE(getIDL());
                           return 9;
                            
                        }
                    case 94: // LD E,(ID+d)
                        {
                            setE(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case 95:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 96: // LD IDH,B
                        {
                            setIDH(regB);
                           return 9;
                            
                        }
                    case 97: // LD IDH,C
                        {
                            setIDH(regC);
                           return 9;
                            
                        }
                    case 98: // LD IDH,D
                        {
                            setIDH(getD());
                           return 9;
                            
                        }
                    case 99: // LD IDH,E
                        {
                            setIDH(getE());
                           return 9;
                            
                        }
                    case 100: // LD IDH,IDH
                        {
                           return 9;
                            
                        }
                    case 101: // LD IDH,IDL
                        {
                            setIDH(getIDL());
                           return 9;
                            
                        }
                    case 102: // LD H,(ID+d)
                        {
                            setH(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case 103: // LD IDH,A
                        {
                            setIDH(regA);
                           return 9;
                            
                        }
                    case 104: // LD IDL,B
                        {
                            setIDL(regB);
                           return 9;
                            
                        }
                    case 105: // LD IDL,C
                        {
                            setIDL(regC);
                           return 9;
                            
                        }
                    case 106: // LD IDL,D
                        {
                            setIDL(getD());
                           return 9;
                            
                        }
                    case 107: // LD IDL,E
                        {
                            setIDL(getE());
                           return 9;
                            
                        }
                    case 108: // LD IDL,IDH
                        {
                            setIDL(getIDH());
                           return 9;
                            
                        }
                    case 109: // LD IDL,IDL
                        {
                           return 9;
                            
                        }
                    case 110: // LD L,(ID+d)
                        {
                            setL(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case 111: // LD IDL,A
                        {
                            setIDL(regA);
                           return 9;
                            
                        }
                    case 112: // LD (ID+d),B
                        {
                            _mem.pokeb(id_d(), regB);
                           return 19;
                            
                        }
                    case 113: // LD (ID+d),C
                        {
                            _mem.pokeb(id_d(), regC);
                           return 19;
                            
                        }
                    case 114: // LD (ID+d),D
                        {
                            _mem.pokeb(id_d(), getD());
                           return 19;
                            
                        }
                    case 115: // LD (ID+d),E
                        {
                            _mem.pokeb(id_d(), getE());
                           return 19;
                            
                        }
                    case 116: // LD (ID+d),H
                        {
                            _mem.pokeb(id_d(), getH());
                           return 19;
                            
                        }
                    case 117: // LD (ID+d),L
                        {
                            _mem.pokeb(id_d(), getL());
                           return 19;
                            
                        }
                    case 118: // UNKNOWN
                        {
                            Interaction.MsgBox("Unknown ID instruction " + xxx + " at " + regPC);
                        return 0;
                        }
                    case 119: // LD (ID+d),A
                        {
                            _mem.pokeb(id_d(), regA);
                           return 19;
                            
                        }
                    case var case9 when 120L <= case9 && case9 <= 123:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 124: // LD A,IDH
                        {
                            regA = (int)getIDH();
                           return 9;
                            
                        }
                    case 125: // LD A,IDL
                        {
                            regA = (int)getIDL();
                           return 9;
                            
                        }
                    case 126: // LD A,(ID+d)
                        {
                            regA = (int)_mem.peekb(id_d());
                           return 19;
                            
                        }
                    case 127:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }

                    case var case10 when 128L <= case10 && case10 <= 131:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 132: // ADD A,IDH
                        {
                            add_a(getIDH());
                           return 9;
                            
                        }
                    case 133: // ADD A,IDL
                        {
                            add_a(getIDL());
                           return 9;
                            
                        }
                    case 134: // ADD A,(ID+d)
                        {
                            add_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case11 when 135L <= case11 && case11 <= 139:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 140: // ADC A,IDH
                        {
                            adc_a(getIDH());
                           return 9;
                            
                        }
                    case 141: // ADC A,IDL
                        {
                            adc_a(getIDL());
                           return 9;
                            
                        }
                    case 142: // ADC A,(ID+d)
                        {
                            adc_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case12 when 143L <= case12 && case12 <= 147:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 148: // SUB IDH
                        {
                            sub_a(getIDH());
                           return 9;
                            
                        }
                    case 149: // SUB IDL
                        {
                            sub_a(getIDL());
                           return 9;
                            
                        }
                    case 150: // SUB (ID+d)
                        {
                            sub_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case13 when 151L <= case13 && case13 <= 155:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 156: // SBC A,IDH
                        {
                            sbc_a(getIDH());
                           return 9;
                            
                        }
                    case 157: // SBC A,IDL
                        {
                            sbc_a(getIDL());
                           return 9;
                            
                        }
                    case 158: // SBC A,(ID+d)
                        {
                            sbc_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case 159:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }

                    case var case14 when 160L <= case14 && case14 <= 163:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 164: // AND IDH
                        {
                            and_a(getIDH());
                           return 9;
                            
                        }
                    case 165: // AND IDL
                        {
                            and_a(getIDL());
                           return 9;
                            
                        }
                    case 166: // AND (ID+d)
                        {
                            and_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case15 when 167L <= case15 && case15 <= 171:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 172: // XOR IDH
                        {
                            xor_a(getIDH());
                           return 9;
                            
                        }
                    case 173: // XOR IDL
                        {
                            xor_a(getIDL());
                           return 9;
                            
                        }
                    case 174: // XOR (ID+d)
                        {
                            xor_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case16 when 175L <= case16 && case16 <= 179:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 180: // OR IDH
                        {
                            or_a(getIDH());
                           return 9;
                            
                        }
                    case 181: // OR IDL
                        {
                            or_a(getIDL());
                           return 9;
                            
                        }
                    case 182: // OR (ID+d)
                        {
                            or_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case var case17 when 183L <= case17 && case17 <= 187:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 188: // CP IDH
                        {
                            cp_a(getIDH());
                           return 9;
                            
                        }
                    case 189: // CP IDL
                        {
                            cp_a(getIDL());
                           return 9;
                            
                        }
                    case 190: // CP (ID+d)
                        {
                            cp_a(_mem.peekb(id_d()));
                           return 19;
                            
                        }
                    case 191:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }

                    case var case18 when 192L <= case18 && case18 <= 202:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 203: // prefix CB
                        {
                            lTemp = id_d();
                            op = nxtpcb();
                            execute_id_cb(op, lTemp);
                            if ((op & 0xC0L) == 0x40L)
                               return 20;
                            else
                               return 23;
                            
                        }
                    case var case19 when 204L <= case19 && case19 <= 224:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 225: // POP ID
                        {
                            regID = (int)popw();
                           return 14;
                            
                        }
                    case 226:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 227: // EX (SP),ID
                        {
                            lTemp = regID;
                            regID = (int)_mem.peekw(regSP);
                            _mem.pokew(regSP, lTemp);
                           return 23;
                            
                        }
                    case 228:
                        {
                            regPC = (int)dec16(regPC);
                            // // REFRESH -1
                            intRTemp = intRTemp - 1;
                           return 4;
                            
                        }
                    case 229: // PUSH ID
                        {
                            pushw(regID);
                           return 15;
                            
                        }
                    case var case20 when 230L <= case20 && case20 <= 232:
                        {
                            regPC = (int)dec16(regPC);
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
                            regPC = (int)dec16(regPC);
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


            private void setIDH(int byteval)
            {
                regID = (byteval * 256 & 0xFF00 | regID & 0xFF);
            }
            private void setIDL(int byteval)
            {
                regID = (int)(regID & 0xFF00L | byteval & 0xFFL);
            }

            private int getIDH()
            {
            return (regID >> 8) & 0xFF;
        }
            private int getIDL()
            {
       
                return regID & 0xFF;

            }

            private int inc16(int a)
            {
                int inc16Ret = default;
                inc16Ret = a + 1 & 0xFFFF;
                return inc16Ret;
            }
            public int nxtpcw()
            {
                int nxtpcwRet = default;
                nxtpcwRet = _mem.peekb(regPC) + _mem.peekb(regPC + 1) * 256;
                regPC = regPC + 2;
                return nxtpcwRet;
            }

            public int nxtpcb()
            {
                int nxtpcbRet = default;



                nxtpcbRet = _mem.peekb(regPC);
                regPC = regPC + 1;
                return nxtpcbRet;
            }


         

            public void setD(int l)
            {
                regDE = (int)((long)Math.Round(l * 256.0d) | regDE & 0xFFL);
            }
            public void setE(int l)
            {
                regDE = (int)(regDE & 0xFF00 | l);
            }

            public void setF(byte b)
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

           
            public void setH(int l)
            {
                regHL = (l * 256) | (regHL & 0xFF);
            }



        public void setL(int l)
        {
            regHL = (regHL & 0xFF00) | l;
        }



        public int sra(int ans)
            {
                int sraRet = default;
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

            public int srl(int ans)
            {
                int srlRet = default;
                Boolean c;

                c = (ans & 0x1) != 0L;
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

            public int sls(int ans)
            {
                int slsRet = default;
                Boolean c;

                c = (ans & 0x80) != 0;
                ans = (ans * 2 | 0x1) & 0xFF;

                fS = (ans & F_S) != 0;
                f3 = (ans & F_3) != 0;
                f5 = (ans & F_5) != 0;
                fZ = ans == 0L;
                fPV = Parity[ans];
                fH = false;
                fN = false;
                fC = c;

                slsRet = ans;
                return slsRet;
            }


            public void sub_a(int b)
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

                regA = (int)ans;
            }


            public void xor_a(int b)
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
                setF(0);
                setBC(0);
                regDE = 0;
                regHL = 0;

                exx();
                ex_af_af();

                regA = 0;
                setF(0);
                setBC(0);
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
                _zx81.sLastScreen[iCounter] = 0;
            for (iCounter = 0; iCounter <= 6143; iCounter++)
                _zx81.gcBufferBits[iCounter] = 0;
            _zx81.ClearScreen();
            // frmMainWnd.Cls
        }

            public void outb(int port, int outbyte, int tstates = 0)
            {
                if (port == 253)
                    // SLOW Mode
                    setAF_(getAF_() | 32768);      //0b1000 0000 0000 0000
                else if (port == 254)
                    // FAST Mode
                    setAF_(getAF_() & (32767));    //0b0111 1111 1111 1111
                else if (port == 0)
                {
                    _zx81.bInputWait = true;
                    _zx81.bBooting = false;
                    _zx81.ShowDisplay(true);
                }
                else if (port == 1)
                {
                    _zx81.bInputWait = false;
                    _zx81.bBooting = false;
                    if ((_mem.peekb(16443) & 128) == 128)
                        _zx81.ShowDisplay(true);
                    else
                        _zx81.ShowDisplay(false);
                }
            }

            public int inb(int port)
            {

                int res;

                res = 0xFF;
                if ((port & 1) == 0)
                {
                    // port = glMemAddrDiv256(port And &HFF00&)
                    port = (int)((port & 0xFF00L) >> 8);
                    if ((port & 1) == 0)
                        res = res & _kb.keyCAPS_V;
                    if ((port & 2) == 0)
                        res = res & _kb.keyA_G;
                    if ((port & 4) == 0)
                        res = res & _kb.keyQ_T;
                    if ((port & 8) == 0)
                        res = res & _kb.key1_5;
                    if ((port & 16) == 0)
                        res = res & _kb.key6_0;
                    if ((port & 32) == 0)
                        res = res & _kb.keyY_P;
                    if ((port & 64) == 0)
                        res = res & _kb.keyH_ENT;
                    if ((port & 128) == 0)
                        res = res & _kb.keyB_SPC;
                    // // Bit7 of the port FE is always 0 on the zx81 (or so it appears)
                    res = res & 127;
                }
                return res;

            }



        }





}
    

