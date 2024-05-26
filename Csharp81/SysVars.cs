using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp81
{
    static class SysVars
    {
        public const int ERR_NR = 16384; // 1 less than the report code. Starts off at 255 (for - 1), so PEEK 16384,
                                         // if it works at all, gives 255. POKE 16384, n can be used to force an error
                                         // halt: 0 n 14 gives one of the usual reports, 15 n 34 or 99 n 127 gives a non-standard report,
                                         // and 35 n 98 is liable to mess up the display file.
        public const int FLAGS = 16385; // Various flags to control the BASIC system.
        public const int ERR_SP = 16386; // Address of first item on machine stack (after GOSUB returns).
        public const int RAMTOP = 16388; // Address of first byte above BASIC system area. You can poke this to make NEW reserve space
                                         // above that area (see chapter 26) or to fool CLS into setting up a minimal display file
                                         // (chapter 27). Poking RAMTOP has no effect until one of these two is executed.
        public const int MODE = 16390; // Specified K, L, F or G cursor.
        public const int PPC = 16391; // Line number of statement currently being executed. Poking this has no lasting effect
                                      // except in the last line of the program.
        public const int VERSN = 16393; // 0 Identifies ZX81 BASIC in saved programs.
        public const int E_PPC = 16394; // Number of current line (with program cursor).
        public const int D_FILE = 16396; // Bottom address of display file.
        public const int DF_CC = 16398; // Address of PRINT position in display file. Can be poked so that PRINT output is sent elsewhere.
        public const int VARS = 16400; // Start of BASIC variables (and 1 past end of display file).
        public const int DEST = 16402; // Address of variable in assignment.
        public const int E_LINE = 16404; // Start of line being typed and workspace.
        public const int CH_ADD = 16406; // Address of the next character to be interpreted: the character after the argument of PEEK,
                                         // or the NEWLINE at the end of a POKE statement.
        public const int X_PTR = 16408; // Address of the character preceding the "S" marker.
        public const int STKBOT = 16410; // Bottom of calculator stack.
        public const int STKEND = 16412; // End of calculator stack.
        public const int BERG = 16414; // Calculator's b register.
        public const int MEM = 16415; // Address of area used for calculator's memory. (Usually MEMBOT, but not always.)
        public const int DF_SZ = 16418; // The number of lines (including one blank line) in the lower part of the creen.
        public const int S_TOP = 16419; // The number of the top program line in automatic listings.
        public const int LAST_K = 16421; // Shows which keys pressed.
        public const int KB_DEBOUNCE = 16423; // Debounce status of keyboard.
        public const int MARGIN = 16424; // Number of blank lines above or below picture: 55 in Britain, 31 in America.
        public const int NXTLIN = 16425; // Address of next program line to be executed.
        public const int OLDPPC = 16427; // Line number of which CONT jumps.
        public const int FLAGX = 16429; // Various flags.
        public const int STRLEN = 16430; // Length of string type destination in assignment.
        public const int T_ADDR = 16432; // Address of next item in syntax table (very unlikely to be useful).
        public const int SEED = 16434; // The seed for RND. This is the variable that is set by RAND.
        public const int FRAMES = 16436; // Counts the frames displayed on the television. Bit 15 is 1. Bits 0 to 14 are decremented for each frame set to the television. This can be used for timing, but PAUSE also uses it. PAUSE resets to 0 bit 15, & puts in bits 0 to 14 the length of the pause. When these have been counted down to zero, the pause stops. If the pause stops because of a key depression, bit 15 is set to 1 again.
        public const int COORDS = 16438; // x-coordinate of last point PLOTted. 16439 holds y-coordinate of last point PLOTted.
        public const int PR_CC = 16440; // Less significant byte of address of next position for LPRINT to print as (in PRBUFF).
        public const int S_POSN = 16441; // Column number for PRINT position. 16442 holds Line number for PRINT position.
        public const int CDFLAG = 16443; // Various flags. Bit 7 is on (1) during compute & display mode.
        public const int PRBUFF = 16444; // Printer buffer (33rd character is NEWLINE).
        public const int MEMBOT = 16477; // Calculator's memory area; used to store numbers that cannot conveniently be put on the calculator stack.

    }
}
