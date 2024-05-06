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

        public int Peekw(int addr)
        {
            return Peekb(addr) | (Peekb((addr + 1)) * 256);
        }

        public void PokebUnrestricted(int addr, int newByte)
        {
            mem[addr] = newByte;    
        }


        public void Pokeb(int addr, int newByte)
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

        public void Pokew(int addr, int word)
        {
            Pokeb(addr, (byte)(word & 0xFF));

            // pokeb((addr + 1) And &HFFFF, glMemAddrDiv256(word And &HFF00))
            Pokeb((addr + 1) & 0xFFFF, (byte)((word & 0xFF00) >> 8));
        }


        public int Peekb(int addr)
        {
            return mem[addr];
        }

    }





}
