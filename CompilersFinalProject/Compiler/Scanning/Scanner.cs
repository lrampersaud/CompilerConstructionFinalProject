using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilersFinalProject.Compiler.Scanning
{
    //http://blogs.microsoft.co.il/sasha/2010/10/06/writing-a-compiler-in-c-lexical-analysis/
    public class Scanner
    {
        private string _source;
        public bool _done { get; private set; }
        private Token _currentToken;
        private int _currentLine;
        private int _currentCharacter;
        private int _currentErrorCharacter;
        private List<string> ErrorLog { get; set; }
        public bool IsAtEnd { get; private set; }

        public Scanner(string source)
        {
            this._source = source;
            this._currentLine = 1;
            this._currentCharacter = 0;
            ErrorLog = new List<string>();
        }

        public Token CurrentToken => _currentToken;


        public Token Advance()
        {
            EatWhitespace();

            if (IsAtEnd)
            {
                _done = true;
                return null;
            }
            var nextChar = NextChar();
            if (Syntax.IsSymbol(nextChar))
            {
                _currentToken = ScanSymbol(nextChar);
            }
            else if (Syntax.IsLiteralStart(nextChar))
            {
                var strConst = EatWhile(c => Syntax.IsCharacterWithinWord(c, nextChar));
                NextChar();  //Remove last ' or "
                _currentToken = new Token(TokenCategory.Literal, TokenTypeDefinition.TK_CHARLIT, strConst);
            }
            else if (Syntax.IsNumber(nextChar))  //can be integer or floating value
            {
                var intConst = nextChar.ToString();
                intConst += EatWhile(Syntax.IsNumber);

                if (LookAhead() != '.')
                {
                    int result;
                    if (!int.TryParse(intConst, out result))
                    {
                        throw new CompilationException("Int const must be in range [0,2147483648), " + "but got: " + intConst, _currentLine);
                    }
                    _currentToken = new Token(TokenCategory.Digit, TokenTypeDefinition.TK_INTLIT, intConst);
                }
                else
                {
                    var floatConstant = intConst + nextChar.ToString();
                    floatConstant += EatWhile(Syntax.IsNumber);

                    float result;
                    if (!float.TryParse(intConst, out result))
                    {
                        throw new CompilationException("Float const must be in range [1.17549E-38,3.40282E38), " + "but got: " + floatConstant, _currentLine);
                    }
                    _currentToken = new Token(TokenCategory.Digit, TokenTypeDefinition.TK_REALLIT, floatConstant);
                }
            }
            else if (Syntax.IsLetter(nextChar))
            {
                var strConst = nextChar + EatWhile(c => Syntax.IsLetter(c));
                TokenTypeDefinition keywordIdentifier = KeyWordId(strConst);

                if (keywordIdentifier != TokenTypeDefinition.TK_BOOLLIT)
                    _currentToken =
                        new Token(
                            (keywordIdentifier == TokenTypeDefinition.TK_ID)
                                ? TokenCategory.Identifier
                                : TokenCategory.Keyword, keywordIdentifier, strConst);
                else
                    _currentToken = new Token(TokenCategory.Literal, keywordIdentifier, strConst);
            }
            else
            {
                LogErrorToken(_currentToken);
                //throw new CompilationException("Unexpected character: " + nextChar, _currentLine);
            }

            return _currentToken;
        }

        private string EatWhile(Func<char, bool> isCondition)
        {
            string added = "";
            while (isCondition(LookAhead()))
            {
                added += LookAhead().ToString();
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
                return '\0';
            }
        }

        private char NextChar()
        {
            if (_currentCharacter < _source.Length)
            {
                if (_source[_currentCharacter] == '\n')
                {
                    this._currentLine++;
                    this._currentErrorCharacter = 0;
                }
                
                return _source[_currentCharacter++];
            }
            else
            {
                IsAtEnd = true;
                return '\0';
            }
        }

        private void EatWhitespace()
        {
            while (_source.Length > _currentCharacter && (int)LookAhead() <= 32)
            {
                if (_source[_currentCharacter] == '\n')
                {
                    this._currentLine++;
                    this._currentErrorCharacter = 0;
                }
                _currentCharacter++;
            }

            if (_source.Length <= _currentCharacter)
            {
                IsAtEnd = true;
            }
        }


        private Token ScanSymbol(char sym_char)
        {
            char[] _sym_Name = new char[255];
            _sym_Name[0] = sym_char;
            char _sym_char = sym_char;
            TokenTypeDefinition _sym_Token = TokenTypeDefinition.TK_EMPTY;
            int stop = 0;

            switch (_sym_char)
            {
                case '/':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '/')
                        {
                            while (_sym_char != '\n')
                            {
                                _sym_char = NextChar();
                            }
                        }
                        else if (_sym_char == '*')
                        {
                            do
                            {
                                _sym_char = NextChar();
                                if (_sym_char == '\0')
                                {
                                    throw new Exception("End of file reached.");
                                }
                                else if (_sym_char == '*')
                                {
                                    _sym_char = NextChar();
                                    if (_sym_char == '/')
                                    {
                                        stop = 1;
                                    }
                                }
                            } while (stop == 0);
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_Token = TokenTypeDefinition.TK_ASDIV;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_SLASH;
                        }
                        break;
                    }
                case '!':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_NOTEQ;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_NOT;
                        }
                        break;
                    }
                case '%':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASMOD;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_MOD;
                        }
                        break;
                    }
                case '&':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '&')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_LOGAND;
                            _sym_Name[1] = _sym_char;
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASAND;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_AMPER;
                        }
                        break;
                    }
                case '(':
                    {
                        _sym_Token = TokenTypeDefinition.TK_LPAREN;
                        break;
                    }
                case ')':
                    {
                        _sym_Token = TokenTypeDefinition.TK_RPAREN;
                        break;
                    }
                case '*':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASMUL;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_STAR;
                        }
                        break;
                    }
                case '+':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '+')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_INCR;
                            _sym_Name[1] = _sym_char;
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASPLUS;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_PLUS;
                        }
                        break;
                    }
                case ',':
                    {
                        _sym_Token = TokenTypeDefinition.TK_COMMA;
                        break;
                    }
                case '-':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '-')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_DECR;
                            _sym_Name[1] = _sym_char;
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASMINUS;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_MINUS;
                        }
                        break;
                    }
                case ':':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_EQUAL;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                            _sym_Token = TokenTypeDefinition.TK_COLON;
                        break;
                    }
                case ';':
                    {
                        _sym_Token = TokenTypeDefinition.TK_SEMI;
                        break;
                    }
                case '<':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '<')
                        {
                            _sym_char = NextChar();
                            _sym_Name[1] = _sym_char;
                            _sym_char = LookAhead();
                            if (_sym_char == '=')
                            {
                                _sym_char = NextChar();
                                _sym_Token = TokenTypeDefinition.TK_ASLSHIFT;
                                _sym_Name[2] = _sym_char;
                            }
                            else
                            {
                                _sym_Token = TokenTypeDefinition.TK_LSHIFT;
                            }
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_LTEQ;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_LESS;
                        }
                        break;
                    }
                case '=':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_EQUAL;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_ASSIGN;
                        }
                        break;
                    }
                case '>':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '>')
                        {
                            _sym_char = NextChar();
                            _sym_Name[1] = _sym_char;
                            _sym_char = LookAhead();
                            if (_sym_char == '=')
                            {
                                _sym_char = NextChar();
                                _sym_Name[2] = _sym_char;
                                _sym_Token = TokenTypeDefinition.TK_ASRSHIFT;
                            }
                            else
                            {
                                _sym_Token = TokenTypeDefinition.TK_RSHIFT;
                            }
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_GTEQ;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_GREATER;
                        }
                        break;
                    }
                case '?':
                    {
                        _sym_Token = TokenTypeDefinition.TK_QMARK;
                        break;
                    }
                case '[':
                    {
                        _sym_Token = TokenTypeDefinition.TK_LBRACK;
                        break;
                    }
                case ']':
                    {
                        _sym_Token = TokenTypeDefinition.TK_RBRACK;
                        break;
                    }
                case '^':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASXOR;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_CARET;
                        }
                        break;
                    }
                case '{':
                    {
                        _sym_Token = TokenTypeDefinition.TK_LBRACE;
                        break;
                    }
                case '}':
                    {
                        _sym_Token = TokenTypeDefinition.TK_RBRACE;
                        break;
                    }
                case '|':
                    {
                        _sym_char = LookAhead();
                        if (_sym_char == '|')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_LOGOR;
                            _sym_Name[1] = _sym_char;
                        }
                        else if (_sym_char == '=')
                        {
                            _sym_char = NextChar();
                            _sym_Token = TokenTypeDefinition.TK_ASOR;
                            _sym_Name[1] = _sym_char;
                        }
                        else
                        {
                            _sym_Token = TokenTypeDefinition.TK_PIPE;
                        }
                        break;
                    }
            }

            return new Token(TokenCategory.Symbol, _sym_Token, _sym_Name.ToString());


        }

        public static TokenTypeDefinition KeyWordId(string c)
        {
            
            switch (c[0])
            {
                case 'a':
                    {
                        if (c == "auto")
                        {
                            return TokenTypeDefinition.TK_AUTO;
                        }
                        break;
                    }
                case 'b':
                    {
                        if (c =="break")
                        {
                            return TokenTypeDefinition.TK_BREAK;
                        }
                        if (c == "bool")
                        {
                            return TokenTypeDefinition.TK_BOOL;
                        }
                        if (c == "begin")
                        {
                            return TokenTypeDefinition.TK_BEGIN;
                        }
                        break;
                    }
                case 'c':
                    {
                        if (c == "case")
                        {
                            return TokenTypeDefinition.TK_CASE;
                        }
                        if (c== "char")
                        {
                            return TokenTypeDefinition.TK_CHAR;
                        }
                        if (c== "const") 
                        {
                            return TokenTypeDefinition.TK_CONST;
                        }
                        if (c == "continue")
                        {
                            return TokenTypeDefinition.TK_CONTINUE;
                        }
                        break;
                    }
                case 'd':
                    {
                        if (c == "do")
                        {
                            return TokenTypeDefinition.TK_DO;
                        }
                        if (c == "default")
                        {
                            return TokenTypeDefinition.TK_DEFAULT;
                        }
                        if (c == "double")
                        {
                            return TokenTypeDefinition.TK_DOUBLE;
                        }
                        break;
                    }
                case 'e':
                    {
                        if (c == "else")
                        {
                            return TokenTypeDefinition.TK_ELSE;
                        }
                        if (c == "enum")
                        {
                            return TokenTypeDefinition.TK_ENUM;
                        }
                        if (c == "extern")
                        {
                            return TokenTypeDefinition.TK_EXTERN;
                        }
                        if (c == "extern")
                        {
                            return TokenTypeDefinition.TK_EXTERN;
                        }
                        if (c == "end")
                        {
                            return TokenTypeDefinition.TK_END;
                        }
                        break;
                    }
                case 'f':
                    {
                        if (c == "for")
                        {
                            return TokenTypeDefinition.TK_FOR;
                        }
                        if (c == "float")
                        {
                            return TokenTypeDefinition.TK_FLOAT;
                        }
                        if (c == "false")
                        {
                            return TokenTypeDefinition.TK_BOOLLIT;
                        }
                        break;
                    }

                case 'i':
                    {
                        if (c == "if")
                        {
                            return TokenTypeDefinition.TK_IF;
                        }
                        if (c == "int")
                        {
                            return TokenTypeDefinition.TK_INT;
                        }
                        if (c == "integer")
                        {
                            return TokenTypeDefinition.TK_INT;
                        }
                        break;
                    }
                case 'l':
                    {
                        if (c == "long")
                        {
                            return TokenTypeDefinition.TK_LONG;
                        }
                        break;
                    }
                case 'm':
                    {
                        if (c == "main")
                        {
                            return TokenTypeDefinition.TK_MAIN;
                        }
                        break;
                    }
                case 'p':
                {
                    if (c == "program")
                    {
                        return TokenTypeDefinition.TK_PROGRAM;
                    }
                        if (c == "procedure")
                        {
                            return TokenTypeDefinition.TK_PROCEDURE;
                        }
                        break;
                }
                case 'r':
                    {
                        if (c == "return")
                        {
                            return TokenTypeDefinition.TK_RETURN;
                        }
                        if (c == "register")
                        {
                            return TokenTypeDefinition.TK_REGISTER;
                        }
                        if (c == "repeat")
                        {
                            return TokenTypeDefinition.TK_REPEAT;
                        }
                        break;
                    }
                case 's':
                    {
                        if (c == "short")
                        {
                            return TokenTypeDefinition.TK_SHORT;
                        }
                        if (c == "signed")
                        {
                            return TokenTypeDefinition.TK_SIGNED;
                        }
                        if (c == "sizeof")
                        {
                            return TokenTypeDefinition.TK_SIZEOF;
                        }
                        if (c == "static")
                        {
                            return TokenTypeDefinition.TK_STATIC;
                        }
                        if (c == "struct")
                        {
                            return TokenTypeDefinition.TK_STRUCT;
                        }
                        if (c == "switch")
                        {
                            return TokenTypeDefinition.TK_SWITCH;
                        }
                        break;
                    }
                case 't':
                    {
                        if (c == "typedef")
                        {
                            return TokenTypeDefinition.TK_TYPEDEF;
                        }
                        if (c == "true")
                        {
                            return TokenTypeDefinition.TK_BOOLLIT;
                        }
                        if (c == "then")
                        {
                            return TokenTypeDefinition.TK_THEN;
                        }
                        if (c == "to")
                        {
                            return TokenTypeDefinition.TK_TO;
                        }
                        break;
                    }
                case 'u':
                {
                        if (c == "until")
                        {
                            return TokenTypeDefinition.TK_UNTIL;
                        }
                        break;
                }
                case 'v':
                    {
                        if (c == "void")
                        {
                            return TokenTypeDefinition.TK_VOID;
                        }
                        if (c == "volatile")
                        {
                            return TokenTypeDefinition.TK_VOLATILE;
                        }
                        if (c == "var")
                        {
                            return TokenTypeDefinition.TK_A_VAR;
                        }
                        break;
                    }
                case 'w':
                    {
                        if (c == "while")
                        {
                            return TokenTypeDefinition.TK_WHILE;
                        }
                        break;
                    }
                case '_':
                    {
                        if (c == "__argc")
                        {
                            return TokenTypeDefinition.TK_ARGC;
                        }
                        break;
                    }
                case 'o':
                    {
                        if (c == "out")
                        {
                            return TokenTypeDefinition.TK_OUT;
                        }
                        if (c == "of")
                        {
                            return TokenTypeDefinition.TK_OF;
                        }
                        break;
                    }
            }
            return TokenTypeDefinition.TK_ID;
        }


        public void Match(TokenTypeDefinition t)
        {
            if (t == this.CurrentToken.TokenTypeDefinition)
            {
                Advance();
            }
            else
            {
                this.LogErrorToken(new Token(TokenCategory.Literal, t, ""));
            }
        }
        
        
        public void LogErrorToken(Token t)
        {
            switch (t.TokenTypeDefinition)
            {
                case TokenTypeDefinition.TK_SLASH:
                {
                    ErrorLog.Add($"Expected \"/\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_NOT:
                {
                    ErrorLog.Add($"Expected \"!\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_MOD:
                {
                    ErrorLog.Add($"Expected \"%\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_AMPER:
                {
                    ErrorLog.Add("Expected \"&\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_LPAREN:
                {
                    ErrorLog.Add($"Expected \"(\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_RPAREN:
                {
                    ErrorLog.Add($"Expected \")\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_STAR:
                {
                    ErrorLog.Add($"Expected \"*\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_PLUS:
                {
                    ErrorLog.Add($"Expected \"+\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_COMMA:
                {
                    ErrorLog.Add($"Expected \",\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_MINUS:
                {
                    ErrorLog.Add($"Expected \"-\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_COLON:
                {
                    ErrorLog.Add($"Expected \":\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_SEMI:
                {
                    ErrorLog.Add($"Expected \";\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_LESS:
                {
                    ErrorLog.Add($"Expected \"<\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_ASSIGN:
                {
                    ErrorLog.Add($"Expected \"=\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_GREATER:
                {
                    ErrorLog.Add($"Expected \">\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_QMARK:
                {
                    ErrorLog.Add($"Expected \"?\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_LBRACK:
                {
                    ErrorLog.Add($"Expected \"[\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_RBRACK:
                {
                    ErrorLog.Add($"Expected \"]\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_CARET:
                {
                    ErrorLog.Add($"Expected \"^\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_LBRACE:
                {
                    ErrorLog.Add("Expected \"{\" on line " + _currentLine + " column " + _currentErrorCharacter);
                    break;
                }
                case TokenTypeDefinition.TK_RBRACE:
                {
                    ErrorLog.Add("Expected \"}\" on line " + _currentLine + " column " + _currentErrorCharacter);
                    break;
                }
                case TokenTypeDefinition.TK_PIPE:
                {
                    ErrorLog.Add($"Expected \"|\" on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                case TokenTypeDefinition.TK_MULTIPLE_DECLARATIONS:
                {
                    ErrorLog.Add($"Multiple Declarations found on line {_currentLine} column {_currentErrorCharacter}");
                    break;
                }
                //// left here need to work on this.
            }
            ErrorLog.Add($"Error matching TOKEN: {_currentToken.Value} with ID: {_currentToken.TokenTypeDefinition}");
        }

    }
}
