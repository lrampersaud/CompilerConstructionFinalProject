using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilersFinalProject.Compiler.Scanning
{
    public static class Syntax
    {
        public static bool IsSymbol(char c)
        {
            return (((int)c) >= 38 && ((int)c) <= 47) || (((int)c) == 33) || (((int)c) >= 58 && ((int)c) <= 63) || (((int)c) >= 91 && ((int)c) <= 94); 
        }

        public static bool IsNumber(char c)
        {
            return ((int) c) >= 48 && ((int) c) <= 57;
        }

        public static bool IsLiteralStart(char c)
        {
            return (c == '"') || (c == '\'');
        }

        public static bool IsLetter(char c)
        {
            return (((int)c) >= 97 && ((int)c) <= 122) || (((int)c) >= 65 && ((int)c) <= 90) || ((int)c) == 95;
        }
        public static bool IsCharacterWithinWord(char c, char startc)
        {
            return (((int)c) >= 32 && ((int)c) <= 126) && (c != startc);
        }
    }
}
