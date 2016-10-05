using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompilersFinalProject.Compiler.Scanning;

namespace CompilersFinalProject.Compiler
{
    public class Token
    {
        private TokenType _tokenType;
        private string _value;

        public Token(TokenType symbol, string value)
        {
            this._tokenType = symbol;
            this._value = value;
        }
    }
}
