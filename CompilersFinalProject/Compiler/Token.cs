using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using CompilersFinalProject.Compiler.Scanning;
using Microsoft.Win32.SafeHandles;

namespace CompilersFinalProject.Compiler
{
    public class Token
    {
        public TokenCategory TokenCategory { get; set; }
        public TokenTypeDefinition TokenTypeDefinition { get; set; }
        public string Value { get; set; }

        public Token(TokenCategory symbol, TokenTypeDefinition tokenTypeDefinition, string value)
        {
            this.TokenCategory = symbol;
            this.TokenTypeDefinition = tokenTypeDefinition;
            this.Value = value;
        }

        public Token()
        {
            this.Value = "";
        }

        public Token Clone()
        {
            return new Token(this.TokenCategory, this.TokenTypeDefinition, this.Value);
        }

    }
}
