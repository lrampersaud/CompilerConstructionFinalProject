using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilersFinalProject.Compiler.Scanning
{
    //http://blogs.microsoft.co.il/sasha/2010/10/06/writing-a-compiler-in-c-lexical-analysis/
    public class Scanner
    {
        private string _source;
        private bool _done;
        private Token _currentToken;
        private int _currentLine;
        private int _currentCharacter;
        public bool IsAtEnd { get; private set; }

        public Scanner(string source)
        {
            this._source = source;
            this._currentLine = 0;
            this._currentCharacter = 0;
        }
        


        public void Advance()
        {
            EatWhitespace();

            if (IsAtEnd)
            {
                _done = true;
                return;
            }
            var nextChar = NextChar();
            if (Syntax.IsSymbol(nextChar))
            {
                //This token is going to be a symbol. There are
                //three special look-ahead cases for '<=', '>=', 
                //and '!='.
                if ((new[] { '<', '>', '!' }.Contains(nextChar)) && LookAhead() == '=')
                {
                    NextChar();//Eat the '='
                    _currentToken = new Token(TokenType.Symbol, nextChar + "=");
                }
                else
                {
                    _currentToken = new Token(TokenType.Symbol, nextChar.ToString());
                }
            }
            else if (Syntax.IsNumber(nextChar))
            {
                //This token is going to be an integer constant.
                var intConst = nextChar.ToString();
                intConst += EatWhile(Syntax.IsNumber);

                int result;
                if (!int.TryParse(intConst, out result))
                {
                    throw new CompilationException("Int const must be in range [0,2147483648), " +"but got: " + intConst, _currentLine);
                }

                _currentToken = new Token(TokenType.IntConst, intConst);
            }
            else if (Syntax.IsCharOrdinalStart(nextChar))
            {
                var marker = NextChar();
                if (marker == '\\')
                {
                    var code = EatWhile(Syntax.IsNumber);
                    if (code.Length != 3)
                    {
                        throw new CompilationException("Expected: \\nnn where n are decimal digits",_currentLine);
                    }
                    var value = int.Parse(code);
                    if (value >= 256)
                    {
                        throw new CompilationException("Character ordinal is out of range [0,255]",_currentLine);
                    }
                    _currentToken = new Token(TokenType.IntConst, value.ToString());
                }
                else
                {
                    _currentToken = new Token(TokenType.IntConst, ((int)marker).ToString());
                }
                NextChar();//Swallow the end of the character ordinal
            }
            else if (Syntax.IsStringConstantStart(nextChar))
            {
                //This token is going to be a string constant.
                var strConst = EatWhile(c => !Syntax.IsStringConstantStart(c));
                NextChar();//Swallow the end of the string constant
                _currentToken = new Token(TokenType.StrConst, strConst);
            }
            else if (Syntax.IsStartOfKeywordOrIdent(nextChar))
            {
                var keywordOrIdent = nextChar.ToString();
                keywordOrIdent += EatWhile(Syntax.IsPartOfKeywordOrIdent);
                _currentToken = Syntax.IsKeyword(keywordOrIdent) ? new Token(TokenType.Keyword, keywordOrIdent) : new Token(TokenType.Ident, keywordOrIdent);
            }
            else
            {
                throw new CompilationException("Unexpected character: " + nextChar, _currentLine);
            }
        }

        private string EatWhile(Func<char, bool> isCondition)
        {
            string added = "";
            while (isCondition(LookAhead()))
            {
                added = LookAhead().ToString();
                _currentCharacter++;
            }

            return added;
        }

        private char LookAhead()
        {
            if (_currentCharacter < _source.Length)
            {
                return _source[_currentCharacter];
            }
            else
            {
                return '@';
            }
        }

        private char NextChar()
        {
            if (_currentCharacter < _source.Length)
            {
                _currentCharacter++;
                return _source[_currentCharacter - 1];
            }
            else
            {
                IsAtEnd = true;
                return '@';
            }
        }

        private void EatWhitespace()
        {
            while (LookAhead() == ' ')
            {
                _currentCharacter++;
            }
        }
    }
}
