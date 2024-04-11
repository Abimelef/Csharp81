using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp81
{
    public class KeyPresses
    {

        //prefer to use byte type for these but bitwise OR, AND, NOT yield integers even
        //if both values are bytes so using int avoids having to out (byte) in front of 
        //each operation

        public int keyB_SPC;
        public int keyH_ENT;
        public int keyY_P;
        public int key6_0;
        public int key1_5;
        public int keyQ_T;
        public int keyA_G;
        public int keyCAPS_V;

        public void resetKeyboard()
        {
            keyB_SPC = 0xFF;
            keyH_ENT = 0xFF;
            keyY_P = 0xFF;
            key6_0 = 0xFF;
            key1_5 = 0xFF;
            keyQ_T = 0xFF;
            keyA_G = 0xFF;
            keyCAPS_V = 0xFF;
        }


    }
}
