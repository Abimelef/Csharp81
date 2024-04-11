using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Csharp81
{
    public class ZX81
    {

        public Stopwatch timer = new Stopwatch();
        public DateTime StartTime;

        public int glTstatesPerInterrupt;

        public bool bInputWait;
        public bool bBooting;
        public int glInterruptTimer;
        
        public int[] sLastScreen = new int[768];
        public int lHiresLoc; // // location in ZX81's memory of the hi-res displayfile
        public int[,] cLastHiResScreen; //= new int[192, 32];

        
        public Color backGroundColour;
        public Color foreGroundColour;
        public Color borderColour;
        public Color fastmodeColour;

        public bool bHideTB;

        public bool bHideInFastMode;
        public bool bDisplayIsShowing;

        // Public sTapeDir As String

        // // Other settings
        public bool bAllowWritesToROM;

        // // Global picDisplay variable to speed things up

        public Graphics picDisplayGraphics;
        public Bitmap displayBitmap;
        public byte[] gcBufferBits = new byte[6145];

        public int DisplaySize;

       

        private Memory _mem;
        private Form _form;
        private KeyPresses _kb;
        public PictureBox _disp;

        public string sTapeDir;

        const int TimeDelay = 15;  // time delay factor to make emulation run at ZX81 speed
                                   // // PERFORM Z80 hardware functions (mimicking the NMI on a real ZX81)

        public ZX81(Memory mem, PictureBox picDisplay, KeyPresses keys,Form form)
        {
            _kb = keys;
            _disp = picDisplay;
            _mem = mem;
            _form = form;

            backGroundColour = Properties.Settings.Default.stgBackColour;  // Item(Color.FromArgb(200, 200, 200)  'pale grey
            foreGroundColour = Properties.Settings.Default.stgForeColour;  // Color.FromArgb(0, 0, 0)  'black
            borderColour = Properties.Settings.Default.stgBorderColour;  // Color.FromArgb(200, 200, 200)  'pale grey '&HC0C0C0
            fastmodeColour = Properties.Settings.Default.stgFastModeColour; // Color.FromArgb(128, 128, 128)  'pale grey &H808080

          //  DisplaySize = Properties.Settings.Default.stgDisplaySize;
            //SetDisplaySize(DisplaySize);
            bHideTB = Properties.Settings.Default.stgHideTB;
            bAllowWritesToROM = Properties.Settings.Default.stgAllowWritesToRom;
            bHideInFastMode = Properties.Settings.Default.stgHideScreenInFastMode;

            // Set up bitmap used for display
            displayBitmap = new Bitmap(256, 192, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            ColorPalette colours = displayBitmap.Palette;
            colours.Entries[0] = backGroundColour;
            colours.Entries[1] = foreGroundColour;
            displayBitmap.Palette = colours;
            form.BackColor = borderColour;

            // gpicDisplay = frmMainWnd.picDisplay
            // gpicDisplayGraphics = frmMainWnd.picDisplay.CreateGraphics()
         
            picDisplayGraphics =_disp.CreateGraphics();
            picDisplayGraphics.Clear(backGroundColour);

            glTstatesPerInterrupt = 160000;
            bInputWait = false;
            bBooting = false;

            sTapeDir = Properties.Settings.Default.stgTapeDir;

            cLastHiResScreen = new int[192, 32];

            LoadROM();
            InstallROMHacks();
            CopyROM();
            _kb.resetKeyboard();

            timer.Start();
            StartTime = DateTime.Now;

        }

        public void CopyROM()
        {
            int iCounter;

            for (iCounter = 0; iCounter <= 8191; iCounter++)
                _mem.pokebUnrestricted(iCounter + 8192,_mem.peekb(iCounter));
        }



        private void SetBitMapFromByteArray(Bitmap bmp3, byte[] pixelArray)
        {
            BitmapData bmpData = bmp3.LockBits(new Rectangle(0, 0, bmp3.Width, bmp3.Height), ImageLockMode.WriteOnly, bmp3.PixelFormat);
            // // Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(pixelArray, 0, bmpData.Scan0, pixelArray.Length);
            bmp3.UnlockBits(bmpData);

            _disp.Image = displayBitmap;
            // frmMainWnd.Refresh() not needed if doevents is used to update screen and read keyboard
        }


        public bool doKey(bool down, KeyEventArgs e)
        {
            bool doKeyRet = default;
            bool CAPS;
            int ascii;

            CAPS = e.Shift;

            if (CAPS)
            {
                _kb.keyCAPS_V = (_kb.keyCAPS_V & 0b11111110);
            }
            else
            {
                _kb.keyCAPS_V = (_kb.keyCAPS_V | 0b00000001);
            }
            ascii = e.KeyValue;

            switch (ascii)
            {
                case 8: // Backspace
                    {
                        if (down)
                        {
                            _kb.key6_0 = (_kb.key6_0 & 0b11111110);
                            _kb.keyCAPS_V = (_kb.keyCAPS_V & 0b11111110);
                        }
                        else
                        {
                            _kb.key6_0 = _kb.key6_0 | 0b00000001;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 16: // SHIFT
                    {
                        if (down)
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        else
                            _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                        break;
                    }
                case 65: // A
                    {
                        if (down)
                            _kb.keyA_G = _kb.keyA_G & 0b11111110;
                        else
                            _kb.keyA_G = _kb.keyA_G | 0b00000001;
                        break;
                    }
                case 66: // B
                    {
                        if (down)
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11101111;
                        else
                            _kb.keyB_SPC = _kb.keyB_SPC | 0b00010000;
                        break;
                    }
                case 67: // C
                    {
                        if (down)
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11110111;
                        else
                            _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00001000;
                        break;
                    }
                case 68: // D
                    {
                        if (down)
                            _kb.keyA_G = _kb.keyA_G & 0b11111011;
                        else
                            _kb.keyA_G = _kb.keyA_G | 0b00000100;
                        break;
                    }
                case 69: // E
                    {
                        if (down)
                            _kb.keyQ_T = _kb.keyQ_T & 0b11111011;
                        else
                            _kb.keyQ_T = _kb.keyQ_T | 0b00000100;
                        break;
                    }
                case 70: // F
                    {
                        if (down)
                            _kb.keyA_G = _kb.keyA_G & 0b11110111;
                        else
                            _kb.keyA_G = _kb.keyA_G | 0b00001000;
                        break;
                    }
                case 71: // G
                    {
                        if (down)
                            _kb.keyA_G = _kb.keyA_G & 0b11101111;
                        else
                            _kb.keyA_G = _kb.keyA_G | 0b00010000;
                        break;
                    }
                case 72: // H
                    {
                        if (down)
                            _kb.keyH_ENT = _kb.keyH_ENT & 0b11101111;
                        else
                            _kb.keyH_ENT = _kb.keyH_ENT | 0b00010000;
                        break;
                    }
                case 73: // I
                    {
                        if (down)
                            _kb.keyY_P = _kb.keyY_P & 0b11111011;
                        else
                            _kb.keyY_P = _kb.keyH_ENT | 0b00000100;
                        break;
                    }
                case 74: // J
                    {
                        if (down)
                            _kb.keyH_ENT = _kb.keyH_ENT & 0b11110111;
                        else
                            _kb.keyH_ENT = _kb.keyH_ENT |0b00001000;
                        break;
                    }
                case 75: // K
                    {
                        if (down)
                            _kb.keyH_ENT = _kb.keyH_ENT & 0b11111011;
                        else
                            _kb.keyH_ENT = _kb.keyH_ENT | 0b00000100;
                        break;
                    }
                case 76: // L
                    {
                        if (down)
                            _kb.keyH_ENT = _kb.keyH_ENT & 0b11111101;
                        else
                            _kb.keyH_ENT = _kb.keyH_ENT | 0b00000010;
                        break;
                    }
                case 77: // M
                    {
                        if (down)
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11111011;
                        else
                            _kb.keyB_SPC = _kb.keyB_SPC | 0b00000100;
                        break;
                    }
                case 78: // N
                    {
                        if (down)
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11110111;
                        else
                            _kb.keyB_SPC = _kb.keyB_SPC | 0b00001000;
                        break;
                    }
                case 79: // O
                    {
                        if (down)
                            _kb.keyY_P = _kb.keyY_P & 0b11111101;
                        else
                            _kb.keyY_P = _kb.keyY_P | 0b00000010;
                        break;
                    }
                case 80: // P
                    {
                        if (down)
                            _kb.keyY_P = _kb.keyY_P & 0b11111110;
                        else
                            _kb.keyY_P = _kb.keyY_P | 0b00000001;
                        break;
                    }
                case 81: // Q
                    {
                        if (down)
                            _kb.keyQ_T = _kb.keyQ_T & 0b11111110;
                        else
                            _kb.keyQ_T = _kb.keyQ_T | 0b00000001;
                        break;
                    }
                case 82: // R
                    {
                        if (down)
                            _kb.keyQ_T = _kb.keyQ_T & 0b11110111;
                        else
                            _kb.keyQ_T = _kb.keyQ_T |0b00001000;
                        break;
                    }
                case 83: // S
                    {
                        if (down)
                            _kb.keyA_G = _kb.keyA_G & 0b11111101;
                        else
                            _kb.keyA_G = _kb.keyA_G | 0b00000010;
                        break;
                    }
                case 84: // T
                    {
                        if (down)
                            _kb.keyQ_T = _kb.keyQ_T & 0b11101111;
                        else
                            _kb.keyQ_T = _kb.keyQ_T | 0b00010000;
                        break;
                    }
                case 85: // U
                    {
                        if (down)
                            _kb.keyY_P = _kb.keyY_P & 0b11110111;
                        else
                            _kb.keyY_P = _kb.keyY_P | 0b00001000;
                        break;
                    }
                case 86: // V
                    {
                        if (down)
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11101111;
                        else
                            _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00010000;
                        break;
                    }
                case 87: // W
                    {
                        if (down)
                            _kb.keyQ_T = _kb.keyQ_T & 0b11111101;
                        else
                            _kb.keyQ_T = _kb.keyQ_T | 0b00000010;
                        break;
                    }
                case 88: // X
                    {
                        if (down)
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111011;
                        else
                            _kb.keyCAPS_V = _kb.keyCAPS_V |0b00000100;
                        break;
                    }
                case 89: // Y
                    {
                        if (down)
                            _kb.keyY_P = _kb.keyY_P & 0b11101111;
                        else
                            _kb.keyY_P = _kb.keyY_P | 0b00010000;
                        break;
                    }
                case 90: // Z
                    {
                        if (down)
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111101;
                        else
                            _kb.keyCAPS_V = _kb.keyCAPS_V |0b00000010;
                        break;
                    }
                case 48: // 0
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11111110;
                        else
                            _kb.key6_0 = _kb.key6_0 | 0b00000001;
                        break;
                    }
                case 49: // 1
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11111110;
                        else
                            _kb.key1_5 = _kb.key1_5 | 0b00000001;
                        break;
                    }
                case 50: // 2
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11111101;
                        else
                            _kb.key1_5 = _kb.key1_5 |0b00000010;
                        break;
                    }
                case 51: // 3
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11111011;
                        else
                            _kb.key1_5 = _kb.key1_5 |0b00000100;
                        break;
                    }
                case 52: // 4
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11110111;
                        else
                            _kb.key1_5 = _kb.key1_5 |0b00001000;
                        break;
                    }
                case 53: // 5
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11101111;
                        else
                            _kb.key1_5 = _kb.key1_5 | 0b00010000;
                        break;
                    }
                case 54: // 6
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11101111;
                        else
                            _kb.key6_0 = _kb.key6_0 | 0b00010000;
                        break;
                    }
                case 55: // 7
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11110111;
                        else
                            _kb.key6_0 = _kb.key6_0 |0b00001000;
                        break;
                    }
                case 56: // 8
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11111011;
                        else
                            _kb.key6_0 = _kb.key6_0 |0b00000100;
                        break;
                    }
                case 57: // 9
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11111101;
                        else
                            _kb.key6_0 = _kb.key6_0 |0b00000010;
                        break;
                    }
                case 96: // _kb.keypad 0
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11111110;
                        else
                            _kb.key6_0 = _kb.key6_0 | 0b00000001;
                        break;
                    }
                case 97: // _kb.keypad 1
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11111110;
                        else
                            _kb.key1_5 = _kb.key1_5 | 0b00000001;
                        break;
                    }
                case 98: // _kb.keypad 2
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11111101;
                        else
                            _kb.key1_5 = _kb.key1_5 |0b00000010;
                        break;
                    }
                case 99: // _kb.keypad 3
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11111011;
                        else
                            _kb.key1_5 = _kb.key1_5 |0b00000100;
                        break;
                    }
                case 100: // _kb.keypad 4
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11110111;
                        else
                            _kb.key1_5 = _kb.key1_5 |0b00001000;
                        break;
                    }
                case 101: // _kb.keypad 5
                    {
                        if (down)
                            _kb.key1_5 = _kb.key1_5 & 0b11101111;
                        else
                            _kb.key1_5 = _kb.key1_5 | 0b00010000;
                        break;
                    }
                case 102: // _kb.keypad 6
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11101111;
                        else
                            _kb.key6_0 = _kb.key6_0 | 0b00010000;
                        break;
                    }
                case 103: // _kb.keypad 7
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11110111;
                        else
                            _kb.key6_0 = _kb.key6_0 |0b00001000;
                        break;
                    }
                case 104: // _kb.keypad 8
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11111011;
                        else
                            _kb.key6_0 = _kb.key6_0 |0b00000100;
                        break;
                    }
                case 105: // _kb.keypad 9
                    {
                        if (down)
                            _kb.key6_0 = _kb.key6_0 & 0b11111101;
                        else
                            _kb.key6_0 = _kb.key6_0 |0b00000010;
                        break;
                    }
                case 106: // _kb.keypad *
                    {
                        if (down)
                        {
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11101111;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.keyB_SPC = _kb.keyB_SPC | 0b00010000;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 107: // _kb.keypad +
                    {
                        if (down)
                        {
                            _kb.keyH_ENT = _kb.keyH_ENT & 0b11111011;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.keyH_ENT = _kb.keyH_ENT |0b00000100;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 109: // _kb.keypad -
                    {
                        if (down)
                        {
                            _kb.keyH_ENT = _kb.keyH_ENT & 0b11110111;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.keyH_ENT = _kb.keyH_ENT |0b00001000;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 110: // _kb.keypad .
                    {
                        if (down)
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11111101;
                        else
                            _kb.keyB_SPC = _kb.keyB_SPC |0b00000010;
                        break;
                    }
                case 111: // _kb.keypad /
                    {
                        if (down)
                        {
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11101110;
                        }
                        else if (!CAPS)
                        {
                            _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00010001;
                        }
                        else
                        {
                            _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00010000;
                        }

                        break;
                    }
                case 37: // Left
                    {
                        if (down)
                        {
                            _kb.key1_5 = _kb.key1_5 & 0b11101111;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.key1_5 = _kb.key1_5 | 0b00010000;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 38: // Up
                    {
                        if (down)
                        {
                            _kb.key6_0 = _kb.key6_0 & 0b11110111;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.key6_0 = _kb.key6_0 | 0b00001000;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 39: // Right
                    {
                        if (down)
                        {
                            _kb.key6_0 = _kb.key6_0 & 0b11111011;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.key6_0 = _kb.key6_0 | 0b00000100;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 40: // Down
                    {
                        if (down)
                        {
                            _kb.key6_0 = _kb.key6_0 & 0b11101111;
                            _kb.keyCAPS_V = _kb.keyCAPS_V & 0b11111110;
                        }
                        else
                        {
                            _kb.key6_0 = _kb.key6_0 | 0b00010000;
                            if (!CAPS)
                            {
                                _kb.keyCAPS_V = _kb.keyCAPS_V | 0b00000001;
                            }
                        }

                        break;
                    }
                case 13: // RETURN
                    {
                        if (!down)
                 //           _kb.keyH_ENT = _kb.keyH_ENT & 0b11111110;
                 //       else
                        _kb.keyH_ENT = _kb.keyH_ENT | 0b00000001;
                        break;
                    }
                case 32: // SPACE BAR
                    {
                        if (down)
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11111110;
                        else
                            _kb.keyB_SPC = _kb.keyB_SPC | 0b00000001;
                        break;
                    }
                case 190: // .
                    {
                        if (down)
                            _kb.keyB_SPC = _kb.keyB_SPC & 0b11111101;
                        else
                            _kb.keyB_SPC = _kb.keyB_SPC | 0b00000010;
                        break;
                    }

                default:
                    {
                        doKeyRet = false;
                        break;
                    }
            }

            doKeyRet = true;
            return doKeyRet;
        }

        public void doEnterKey(bool ShiftPressed)
        {
            if (ShiftPressed)
                {
                    _kb.keyCAPS_V = (_kb.keyCAPS_V & 0b11111110);
                }
                else
                {
                    _kb.keyCAPS_V = (_kb.keyCAPS_V | 0b00000001);
                }
            _kb.keyH_ENT = _kb.keyH_ENT & 0b11111110;

        }

       
        public  int inb(int port)
        {
            int inbRet = default;
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

            // If res <> 255 Then Stop
        
            inbRet = res;
            return inbRet;
        }
        public void TapeLoad(int FileNamePtr)
        {
            string sFileName = "";
            byte[] loadFile;
            int i;

            if (FileNamePtr >= 32768)
            {
                // // LOAD ""
                return;
            }
            else
            {
                // // LOAD "filename"
                FileNamePtr = FileNamePtr & 32767;
                // // FileNamePtr points to the filename in the ZX81 character set
                if (_mem.peekb(FileNamePtr) == 227)
                {
                    Environment.Exit(0);
                }

                while (_mem.peekb(FileNamePtr) < 127)
                {
                    sFileName = sFileName + ZXCharToASCII((byte)(_mem.peekb(FileNamePtr)));
                    FileNamePtr = FileNamePtr + 1;
                }
                sFileName = Strings.Trim(sFileName + ZXCharToASCII((byte)(_mem.peekb(FileNamePtr)))) + ".P";
            }

            if (string.IsNullOrEmpty(FileSystem.Dir(Properties.Settings.Default.stgTapeDir + "/" + sFileName)))
                return;

            loadFile = File.ReadAllBytes(Properties.Settings.Default.stgTapeDir + "/" + sFileName);

            var loopTo = loadFile.Length - 1;
            for (i = 0; i <= loopTo; i++)
                _mem.pokeb(0x4009 + i, loadFile[i]);

            bInputWait = true;
            ShowDisplay(true);

            

        }
        public void TapeSave(int FileNamePtr)
        {
            string sFileName = "";
            int i;
            int saveFileSize = _mem.peekw(16404) - 0x4009;

      
            var saveFile = new byte[saveFileSize + 1];

            if (FileNamePtr >= 32768)
            {
                // // SAVE ""
                return;
            }
            else
            {
                // // SAVE "filename"
                FileNamePtr = FileNamePtr & 32767;
                // // FileNamePtr points to the filename in the ZX81 character set
                while (_mem.peekb(FileNamePtr) < 127)
                {
                    sFileName = sFileName + ZXCharToASCII((byte)(_mem.peekb(FileNamePtr)));
                    FileNamePtr = FileNamePtr + 1;
                }
                sFileName = Strings.Trim(sFileName + ZXCharToASCII((byte)(_mem.peekb(FileNamePtr)))) + ".P";
            }



            var loopTo = _mem.peekw(16404);
            for (i = 0x4009; i <= loopTo; i++)
                saveFile[i - 0x4009] = (byte)(_mem.peekb(i));

            Information.Err().Clear();

            File.WriteAllBytes(Properties.Settings.Default.stgTapeDir + @"\" + sFileName, saveFile);


            if (Information.Err().Number != 0)
            {
                Interaction.MsgBox("Error: Unable to write to output file:" + Constants.vbCrLf + Constants.vbCrLf + "   " + FileSystem.CurDir() + @"\" + sFileName, Constants.vbOKOnly | Constants.vbExclamation, "vb81");
            }


            bInputWait = true;
            ShowDisplay(true);
        }

            private string ZXCharToASCII(byte cZX)
            {
                string ZXCharToASCIIRet = "";
                if ((cZX & 128) == 128)
                    cZX = (byte)(cZX & 63);

                switch (cZX)
                {
                    case 0: // // SPACE
                        {
                            ZXCharToASCIIRet = " ";
                            break;
                        }
                    case var @case when (byte)11 <= @case && @case <= (byte)13:
                        {
                            // 11 = 34   "
                            // 12 = 35  £/#
                            // 13 = 36  $
                            ZXCharToASCIIRet =  Convert.ToString(Strings.Chr(cZX + 23));
                            break;
                        }
                    case var case1 when (byte)14 <= case1 && case1 <= (byte)21:
                        {
                            // 14 = :
                            // 15 = ?
                            // 16 = (
                            // 17 = )
                            // 18 = >
                            // 19 = <
                            // 20 = =
                            // 21 = +
                            ZXCharToASCIIRet = "_";
                            break;
                        }
                    case 22:
                        {
                            // 22 = -
                            ZXCharToASCIIRet = "-";
                            break;
                        }
                    case var case2 when (byte)23 <= case2 && case2 <= (byte)26:
                        {
                            // 23 = *
                            // 24 = /
                            // 25 = ;
                            // 26 = ,
                            ZXCharToASCIIRet = "_";
                            break;
                        }
                    case 27:
                        {
                            // 27 = .
                            ZXCharToASCIIRet = ".";
                            break;
                        }
                    case var case3 when (byte)28 <= case3 && case3 <= (byte)37:
                        {
                            ZXCharToASCIIRet = Convert.ToString(Strings.Chr(cZX + 20));
                            break;
                        }
                    case var case4 when (byte)38 <= case4 && case4 <= (byte)63:
                        {
                            ZXCharToASCIIRet =Convert.ToString(Strings.Chr(cZX + 27));
                            break;
                        }

                    default:
                        {
                            ZXCharToASCIIRet = "_";
                            break;
                        }
                }

                return ZXCharToASCIIRet;
            }

        public void ShowDisplay(bool bShowDisplay) // , bPaintDisplay As Boolean)
        {
            // bShowDisplay=TRUE (ShowDisplay)
            // bShowDisplay=FALSE (HideDisplay)
            // bDisplayShown flips each time 

            if (bDisplayIsShowing != bShowDisplay)
            {
                if (bShowDisplay)
                {
                    _form.BackColor = borderColour;
                }
                // If bPaintDisplay Then screenPaint()
                else
                {
                    _form.BackColor = fastmodeColour;
                }
                _disp.Visible = bShowDisplay;
                _form.Refresh();
                bDisplayIsShowing = bShowDisplay;
            }


        }

        public void HiresScreenPaint(int _intI)
        {
            int x;
            int y;
            int c;

            int lTopMost;
            int lBottomMost;
            int lLeftMost;
            int lRightMost;
            bool bUpdated = false;

            lTopMost = 191;
            lBottomMost = 0;
            lLeftMost = 31;
            lRightMost = 0;

            _disp.Visible = false;

            for (y = 0; y <= 191; y++)
            {
                for (x = 0; x <= 31; x++)
                {
                    c = _mem.peekb(lHiresLoc + x + y * 33) & (int)(0xBFL); // strip bit 6

                    if (c != cLastHiResScreen[y, x])
                    {
                        if (y < lTopMost)
                            lTopMost = y;
                        if (y > lBottomMost)
                            lBottomMost = y;
                        if (x < lLeftMost)
                            lLeftMost = x;
                        if (x > lRightMost)
                            lRightMost = x;
                        bUpdated = true;
                        if ((c & 128) != 0)
                        {
                            // // Inverse video
                            gcBufferBits[y * 32 + x] = (byte)(_mem.peekb(_intI * 256 + (c & 63) * 8) ^ 255);
                        }
                        else
                        {
                            // // Normal video
                            gcBufferBits[y * 32 + x] = (byte)(_mem.peekb(_intI * 256 + c * 8));
                        }
                        cLastHiResScreen[y, x] = c;
                    }
                }
            }
            lLeftMost = lLeftMost * 8;
            lRightMost = lRightMost * 8;
            if (bUpdated)
                SetBitMapFromByteArray(displayBitmap, gcBufferBits); // StretchDIBits frmMainWnd.picDisplay.hdc, lLeftMost * glDisplayXMultiplier, (lBottomMost + 1) * glDisplayYMultiplier - 1, (lRightMost - lLeftMost + 8) * glDisplayXMultiplier, -(lBottomMost - lTopMost + 1) * glDisplayYMultiplier, lLeftMost, lTopMost, (lRightMost - lLeftMost) + 8, lBottomMost - lTopMost + 1, gcBufferBits(0), bmiBuffer, DIB_RGB_COLORS, SRCCOPY

            _disp.Visible = true;
        }

        public void screenPaint(int _intI)
        {
            int ptr;
            int x;
            int y;
            int c;
            int lChar;
            int lRow;

            int lTopMost;
            int lBottomMost;
            int lLeftMost;
            int lRightMost;
            bool bUpdated = false;

            lTopMost = 191;
            lBottomMost = 0;
            lLeftMost = 31;
            lRightMost = 0;

            ptr = _mem.peekw(16396) + 1; // mem[16396] + 256 * mem[16397] + 1; // // D_FILE (+1 to skip the initial HALT)

            // // don't bother if D_FILE is uninitialised
            if (ptr == 0)
                return;

            // frmMainWnd.picDisplay.Visible = False


            // // Build the video memory map in sScreen
            for (y = 0; y <= 23; y++)
            {
                for (x = 0; x <= 31; x++)
                {
                    if (_mem.peekb(ptr) == 0x76L)
                    {
                        lChar = y * 32 + x;
                        c = 0; // // space
                        ptr = ptr - 1;
                    }
                    else
                    {
                        lChar = y * 32 + x;
                        c = (int)(_mem.peekb(ptr) & 0xBFL);
                    } // strip bit 6

                    if (c != sLastScreen[lChar])
                    {
                        // // If this character has changed since we last painted
                        // // the screen, repaint it
                        if (y * 8 < lTopMost)
                            lTopMost = y * 8;
                        if (y * 8 + 7 > lBottomMost)
                            lBottomMost = y * 8 + 7;
                        if (x < lLeftMost)
                            lLeftMost = x;
                        if (x > lRightMost)
                            lRightMost = x;
                        bUpdated = true;
                        if ((c & 128)==128)
                        {
                            for (lRow = 0; lRow <= 7; lRow++)
                                gcBufferBits[y * 256 + x + lRow * 32] = (byte)(_mem.peekb(_intI * 256 + (c & 63) * 8 + lRow) ^ 255);
                        }
                        else
                        {
                            for (lRow = 0; lRow <= 7; lRow++)
                                gcBufferBits[y * 256 + x + lRow * 32] = (byte)(_mem.peekb(_intI * 256 + c * 8 + lRow));
                        }
                        sLastScreen[lChar] = c;
                    }

                    ptr = ptr + 1;
                }
                ptr = ptr + 1;
            }

            lLeftMost = lLeftMost * 8;
            lRightMost = lRightMost * 8;
            if (bUpdated)
            {
                SetBitMapFromByteArray(displayBitmap, gcBufferBits);  // StretchDIBits frmMainWnd.picDisplay.hdc, lLeftMost * glDisplayXMultiplier, (lBottomMost + 1) * glDisplayYMultiplier - 1, (lRightMost - lLeftMost + 8) * glDisplayXMultiplier, -(lBottomMost - lTopMost + 1) * glDisplayYMultiplier, lLeftMost, lTopMost, (lRightMost - lLeftMost) + 8, lBottomMost - lTopMost + 1, gcBufferBits(0), bmiBuffer, DIB_RGB_COLORS, SRCCOPY
            }

            // frmMainWnd.picDisplay.Visible = True
        }

        public void ReInitHiresScreen()
        {
            int y;
            int x;

            for (y = 0; y <= 191; y++)
            {
                for (x = 0; x <= 31; x++)
                {
                    cLastHiResScreen[y, x] = 0;
                    gcBufferBits[y * 32 + x] = 0;
                }
            }
        }
        public int SearchHiresScreen()
        {
            int SearchHiresScreenRet = 0;
            int lCounter;
            int v;
            bool FoundHiRes = false;
            int lCounter2;

            for (lCounter = 32767 - 6336; lCounter >= 8192; lCounter -= 1)
            {
                v = _mem.peekb(lCounter + 32);
                if ((v & 64) != 0)
                {
                    FoundHiRes = true;
                    for (lCounter2 = 0; lCounter2 <= 191; lCounter2++)
                    {
                        if ((_mem.peekb(lCounter + 33 * lCounter2) & 64 | _mem.peekb(lCounter + 32 + 33 * lCounter2)) != v)
                        {
                            FoundHiRes = false;
                            break;
                        }
                    }
                }
                if (FoundHiRes)
                {
                    SearchHiresScreenRet = lCounter;
                    break;
                }
            }

            return SearchHiresScreenRet;
        }

        public void InstallROMHacks()
        {
            // // Patch the 'SAVE' ROM routine with an illegal operation, which we
            // // trap in the Z80 emulation so the file can be saved to disk instead of
            // // the ROM trying to output it to TAPE via the ULA
            _mem.pokebUnrestricted(0x2FC,0xED); // // ED FD    ; the illegal Z80 op we use for SAVE
            _mem.pokebUnrestricted(0x2FD,0xFD); // //
            _mem.pokebUnrestricted(0x2FE,0xC3); // // JP 0207h ); this is where the ZX81 ROM jumps to after
            _mem.pokebUnrestricted(0x2FF,0x7);  // //          ); saving a program to tape
            _mem.pokebUnrestricted(0x300,0x2);  // //

            // // Patch the 'LOAD' ROM routine with an illegal op, which is trapped
            // // just like the SAVE patch above.
            _mem.pokebUnrestricted(0x347,0xEB); // // EX DE,HL ); points HL to the name of the program to
                               // //          ); load (in the ZX character set)
            _mem.pokebUnrestricted(0x348,0xED); // // ED FC    ); the illegal Z80 op we use for LOAD
            _mem.pokebUnrestricted(0x349,0xFC); // //
            _mem.pokebUnrestricted(0x34A,0xC3); // // JP 0207h ); this is where the ZX81 ROM jumps to after
            _mem.pokebUnrestricted(0x34B,0x7);  // //          ); loading a program from tape
            _mem.pokebUnrestricted(0x34C,0x2);  // //

            // // Execute 'out (0),a' when waiting for input, and
            // // 'out (1),a when finished waiting (used to paint
            // // the screen during idle periods when in fast mode)
            _mem.pokebUnrestricted(0x4CA,0xD3); // //       OUT (0),A  ); Tell the emulator we are waiting
            _mem.pokebUnrestricted(0x4CB,0);    // //                  ); for keyboard input
            _mem.pokebUnrestricted(0x4CC,0xCB); // // loop: BIT 0,(HL) ); Loop until our NMI emulation sets
            _mem.pokebUnrestricted(0x4CD,0x46); // //                  ); bit 0 of (CDFLAGS), which indicates
            _mem.pokebUnrestricted(0x4CE,0x28); // //       JR Z,loop  ); that a key has been pressed
            _mem.pokebUnrestricted(0x4CF,0xFC); // //
            _mem.pokebUnrestricted(0x4D0,0xD3); // //       OUT (1),A  ); Tell the emulator that we are no
            _mem.pokebUnrestricted(0x4D1,1);    // //                  ); longer waiting for keyboard input
            _mem.pokebUnrestricted(0x4D2,0);    // //       NOP        ); NOP an old byte (filler)

            // // This is required for correct implementation of the PAUSE command, as the
            // // PAUSE code in a real ZX81 is tied in with the FRAMES counter in the FAST
            // // mode display loop which we emulate outside of the routines in the ROM.
            _mem.pokebUnrestricted(0x2A9,0xD3); // //   OUT (0),A  ); see above
            _mem.pokebUnrestricted(0x2AA,0);
            _mem.pokebUnrestricted(0x2AB,118);  // //   HALT       ); ensures the bit of display code that
                               // //              ); we loop through only gets executed
                               // //              ); once per interrupt (as it would in a
                               // //              ); real ZX81)
            _mem.pokebUnrestricted(0x2AC,205);  // //   CALL 0229h ); jump out of the display routine
            _mem.pokebUnrestricted(0x2AD,0x29); // //              ); early to avoid the nasty code that
            _mem.pokebUnrestricted(0x2AE,2);    // //              ); jumps right into the DFILE!
            _mem.pokebUnrestricted(0x2AF,0xD3); // //  OUT (1),A   ); see above
            _mem.pokebUnrestricted(0x2B0,1);
            _mem.pokebUnrestricted(0x2B1,201);  // //  RET         ); Return to PAUSE command routine
        }
        public void LoadROM()
        {
            byte[] sROM;
            int iCounter;

            if (FileSystem.Dir("zx81.rom")=="")
            {
                Interaction.MsgBox("Error: Could not find required ROM image '" + FileSystem.CurDir() + @"\" + "zx81.rom'.", Constants.vbOKOnly | Constants.vbCritical, "vb81");
                Application.Exit();
                return;
            }
            sROM = File.ReadAllBytes("zx81.rom");


            int loopTo = sROM.Length - 1;
            for (iCounter = 0; iCounter <= loopTo; iCounter++)
                _mem.pokebUnrestricted(iCounter,sROM[iCounter]);
        }

        public void HandleInterrupt(int _intI)
        {
            int lSleep;

            if ((_mem.peekb(16443) & 128) == 128)
            {
                // // SLOW mode
                if (lHiresLoc == 0)

                    screenPaint(_intI);
                else
                    HiresScreenPaint(_intI);

                ShowDisplay(true);
            }
            else if (bInputWait)
            {
                // // FAST mode, but waiting for a keypress
                if (lHiresLoc == 0)
                    screenPaint(_intI);
                else
                    HiresScreenPaint(_intI);

                ShowDisplay(true);
            }
            else
            {
                // // FAST mode, executing code (hide display if necessary)
                if (bHideInFastMode)
                {

                    ShowDisplay(false);
                }
                else
                {
                    ShowDisplay(true);
                }
                
            }
           
            // read keyboard and refresh frmMainWnd etc


            Application.DoEvents();

            update_kybd();
            // // Interrupts should occur every 1/50th of a second (20ms)...
            // // In the (rather unlikely!) event that the emulation is
            // // running too quick, slow things down a bit

            // // Definitely needed in 2024 - Allan Macpherson
            // // Otherwise on a fairly up to date PC
            // // emulation runs muxh faster than an actual ZX81!


            TimeSpan interval = DateTime.Now - StartTime;

            int t = (int)(interval.TotalMilliseconds);

            

            if (t < TimeDelay)
            {
                lSleep = TimeDelay - t; // glInterruptTimer - timer.Elapsed.Milliseconds 'timeGetTime()
                System.Threading.Thread.Sleep(lSleep);
            }
          StartTime = DateTime.Now;
        }

        public void update_kybd()
        {
            int f;
            int oldLastK1;
            int oldLastK2;
            var LastK1 = default(int);
            var LastK2 = default(int);


            oldLastK1 = _mem.peekb(16421);
            oldLastK2 = _mem.peekb(16422);

            if ((_mem.peekb(16443) & 128) == 0)
            {
                // // FAST mode is 3.25MHz
                glTstatesPerInterrupt = 65000;
            }
            else
            {
                // // SLOW mode is 0.8MHz
                glTstatesPerInterrupt = 16000;

                // // Decrement FRAMES when in SLOW mode
                f = ((_mem.peekb(16436) | (_mem.peekb(16437)) * 256)) & 32767;
                if (f > 0)
                    f = f - 1;
                else
                    f = 32767;
                _mem.pokew(16436, 
                    ((_mem.peekb(16436) | (_mem.peekb(16437) * 256)) & 32768) | f
                    );
            }


            if (bBooting)
                return;

            if (oldLastK1 == 0 & oldLastK2 == 0)
            {
                _mem.pokeb(16421,255);
                _mem.pokeb(16422,255);
                return;
            }

            // A
            if ((_kb.keyA_G & 0x1) == 0)
            {
                LastK1 = 253;
                LastK2 = 253;
            }

            // S
            if ((_kb.keyA_G & 0x2) == 0)
            {
                LastK1 = 253;
                LastK2 = 251;
            }

            // D
            if ((_kb.keyA_G & 0x4) == 0)
            {
                LastK1 = 253;
                LastK2 = 247;
            }

            // F
            if ((_kb.keyA_G & 0x8) == 0)
            {
                LastK1 = 253;
                LastK2 = 239;
            }

            // G
            if ((_kb.keyA_G & 0x10) == 0)
            {
                LastK1 = 253;
                LastK2 = 223;
            }

            // H
            if ((_kb.keyH_ENT & 0x10) == 0)
            {
                LastK1 = 191;
                LastK2 = 223;
            }

            // J
            if ((_kb.keyH_ENT & 0x8) == 0)
            {
                LastK1 = 191;
                LastK2 = 239;
            }

            // K
            if ((_kb.keyH_ENT & 0x4) == 0)
            {
                LastK1 = 191;
                LastK2 = 247;
            }

            // L
            if ((_kb.keyH_ENT & 0x2) == 0)
            {
                LastK1 = 191;
                LastK2 = 251;
            }

            // ENTER
            if ((_kb.keyH_ENT & 0x1) == 0)
            {
                LastK1 = 191;
                LastK2 = 253;
            }

            // 1
            if ((_kb.key1_5 & 0x1) == 0)
            {
                LastK1 = 247;
                LastK2 = 253;
            }
            // 2
            if ((_kb.key1_5 & 0x2) == 0)
            {
                LastK1 = 247;
                LastK2 = 251;
            }
            // 3
            if ((_kb.key1_5 & 0x4) == 0)
            {
                LastK1 = 247;
                LastK2 = 247;
            }
            // 4
            if ((_kb.key1_5 & 0x8) == 0)
            {
                LastK1 = 247;
                LastK2 = 239;
            }
            // 5
            if ((_kb.key1_5 & 0x10) == 0)
            {
                LastK1 = 247;
                LastK2 = 223;
            }
            // 6
            if ((_kb.key6_0 & 0x10) == 0)
            {
                LastK1 = 239;
                LastK2 = 223;
            }
            // 7
            if ((_kb.key6_0 & 0x8) == 0)
            {
                LastK1 = 239;
                LastK2 = 239;
            }
            // 8
            if ((_kb.key6_0 & 0x4) == 0)
            {
                LastK1 = 239;
                LastK2 = 247;
            }
            // 9
            if ((_kb.key6_0 & 0x2) == 0)
            {
                LastK1 = 239;
                LastK2 = 251;
            }
            // 0
            if ((_kb.key6_0 & 0x1) == 0)
            {
                LastK1 = 239;
                LastK2 = 253;
            }

            // Q
            if ((_kb.keyQ_T & 0x1) == 0)
            {
                LastK1 = 251;
                LastK2 = 253;
            }
            // W
            if ((_kb.keyQ_T & 0x2) == 0)
            {
                LastK1 = 251;
                LastK2 = 251;
            }
            // E
            if ((_kb.keyQ_T & 0x4) == 0)
            {
                LastK1 = 251;
                LastK2 = 247;
            }
            // R
            if ((_kb.keyQ_T & 0x8) == 0)
            {
                LastK1 = 251;
                LastK2 = 239;
            }
            // T
            if ((_kb.keyQ_T & 0x10) == 0)
            {
                LastK1 = 251;
                LastK2 = 223;
            }
            // Y
            if ((_kb.keyY_P & 0x10) == 0)
            {
                LastK1 = 223;
                LastK2 = 223;
            }
            // U
            if ((_kb.keyY_P & 0x8) == 0)
            {
                LastK1 = 223;
                LastK2 = 239;
            }
            // I
            if ((_kb.keyY_P & 0x4) == 0)
            {
                LastK1 = 223;
                LastK2 = 247;
            }
            // O
            if ((_kb.keyY_P & 0x2) == 0)
            {
                LastK1 = 223;
                LastK2 = 251;
            }
            // P

            // If IntCount = 1000 Then
            // LastK1 = 223
            // LastK2 = 253
            // End If

            if ((_kb.keyY_P & 0x1) == 0)
            {
                LastK1 = 223;
                LastK2 = 253;
            }

            // Z
            if ((_kb.keyCAPS_V & 0x2) == 0)
            {
                LastK1 = 254;
                LastK2 = 251;
            }
            // X
            if ((_kb.keyCAPS_V & 0x4) == 0)
            {
                LastK1 = 254;
                LastK2 = 247;
            }
            // C
            if ((_kb.keyCAPS_V & 0x8) == 0)
            {
                LastK1 = 254;
                LastK2 = 239;
            }
            // V
            if ((_kb.keyCAPS_V & 0x10) == 0)
            {
                LastK1 = 254;
                LastK2 = 223;
            }
            // B
            if ((_kb.keyB_SPC & 0x10) == 0)
            {
                LastK1 = 127;
                LastK2 = 223;
            }
            // N
            if ((_kb.keyB_SPC & 0x8) == 0)
            {
                LastK1 = 127;
                LastK2 = 239;
            }
            // M
            if ((_kb.keyB_SPC & 0x4) == 0)
            {
                LastK1 = 127;
                LastK2 = 247;
            }
            // .
            if ((_kb.keyB_SPC & 0x2) == 0)
            {
                LastK1 = 127;
                LastK2 = 251;
            }
            // SPACE
            if ((_kb.keyB_SPC & 0x1) == 0)
            {
                LastK1 = 127;
                LastK2 = 253;
            }

            // SHIFT
            if ((_kb.keyCAPS_V & 0x1) == 0)
            {
                LastK2 = LastK2 & 254;
            }

            if (LastK1 == 0)
            {
                _mem.pokeb(16421,255);
                if ((_kb.keyCAPS_V & 0x1) == 0)
                {
                    _mem.pokeb(16422,254);
                }
                else
                {
                    _mem.pokeb(16422,255);
                }
                return;
            }

            _mem.pokeb(16421,(byte)LastK1);
            _mem.pokeb(16422,(byte)LastK2);

            if (bInputWait == false)
                return;

            // // Set FRAMES
            _mem.pokeb(16424,55);

            int k1 = _mem.peekb(16421);
            int k2 = _mem.peekb(16422);

            if (_mem.peekb(0x4027) == 255)
            {
                _mem.pokeb(0x4027, 15);
            }


            else if (
                     ((k1 != oldLastK1) | ((k2 & 0xFE) != (oldLastK2 & 0xFE)))
                     & (k1 != 255 & k2 != 255)
                    )
            {
                _mem.pokeb(16443,(byte)(_mem.peekb(16443) | 1));
            }
        }

        public void ClearScreen()
        {
            picDisplayGraphics.Clear(_disp.BackColor);
        }


        public void HiResRoutine(int _regIX,int _regID)
        {
            int lTemp;
            if ((_regIX != _regID) & (_regID > 8191))
            {
                // // IX has changed - looks like this is a hi-res graphics routine
                // // 1. Search for hires screen and store it's location in lHiresLoc
                lHiresLoc = SearchHiresScreen();
                if (lHiresLoc > 0)
                {
                    ReInitHiresScreen();
                    ClearScreen();
                }
            }
            else if (lHiresLoc > 0)
            {
                lHiresLoc = 0;
                for (lTemp = 0; lTemp <= 767; lTemp++)
                    sLastScreen[lTemp] = 0;
                for (lTemp = 0; lTemp <= 6143; lTemp++)
                    gcBufferBits[lTemp] = 0;
                ClearScreen();
            }
        }
       

    }




}

