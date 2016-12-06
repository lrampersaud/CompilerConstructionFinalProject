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
        public TokenCategory _tokenCategory;
        public TokenTypeDefinition _tokenTypeDefinition;
        public string _value;

        public Token(TokenCategory symbol, TokenTypeDefinition tokenTypeDefinition, string value)
        {
            this._tokenCategory = symbol;
            this._tokenTypeDefinition = tokenTypeDefinition;
            this._value = value;
        }
    }
}
