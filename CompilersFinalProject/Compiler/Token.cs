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
        private TokenCategory _tokenCategory;
        private TokenTypeDefinition _tokenTypeDefinition;
        private string _value;

        public Token(TokenCategory symbol, TokenTypeDefinition tokenTypeDefinition, string value)
        {
            this._tokenCategory = symbol;
            this._tokenTypeDefinition = tokenTypeDefinition;
            this._value = value;
        }
    }
}
