using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilersFinalProject.Compiler.Scanning
{
    public static class Syntax
    {
        private static char[] Numbers = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private static char[] Symbols = { '<', '>', '=', '+', '-', '*', '/', '{', '}' };
        public static bool IsSymbol(char c)
        {
            return Symbols.Contains(c);
        }

        public static bool IsNumber(char c)
        {
            return Numbers.Contains(c);
        }

        public static bool IsCharOrdinalStart(char c)
        {
            return true;
        }

        public static bool IsStringConstantStart(char c)
        {
            return true;
        }

        public static bool IsStartOfKeywordOrIdent(char c)
        {
            return true;
        }

        public static bool IsPartOfKeywordOrIdent(char c)
        {
            return true;
        }

        public static bool IsKeyword(string c)
        {
            return true;
        }
    }
}
