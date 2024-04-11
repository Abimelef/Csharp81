using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp81
{
    public class Memory
    {
        private int[] mem;

        public Memory()
        {
            mem = new int[65536];
        }

        public int peekw(int addr)
        {
            return peekb(addr) | (peekb((addr + 1)) * 256);
        }

        public void pokebUnrestricted(int addr, int newByte)
        {
            mem[addr] = newByte;    
        }


        public void pokeb(int addr, int newByte)
        {
            if (addr >= 16384)
                // // RAM
                mem[addr] = (byte)(newByte);
            else if (Properties.Settings.Default.stgAllowWritesToRom)
            {
                // // Writing to the Shadow ROM area is permitted by the settings...
                if (addr >= 8192)
                    mem[addr] = (byte)(newByte);
            }
        }

        public void pokew(int addr, int word)
        {
            pokeb(addr, (byte)(word & 0xFF));

            // pokeb((addr + 1) And &HFFFF, glMemAddrDiv256(word And &HFF00))
            pokeb((addr + 1) & 0xFFFF, (byte)((word & 0xFF00) >> 8));
        }


        public int peekb(int addr)
        {
            return mem[addr];
        }

    }





}
