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
        public TokenCategory TokenCategory { get; }
        public TokenTypeDefinition TokenTypeDefinition { get; };
        public string Value { get; }

        public Token(TokenCategory symbol, TokenTypeDefinition tokenTypeDefinition, string value)
        {
            this.TokenCategory = symbol;
            this.TokenTypeDefinition = tokenTypeDefinition;
            this.Value = value;
        }

    }
}
