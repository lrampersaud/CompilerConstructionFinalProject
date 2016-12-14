using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CompilersFinalProject.Compiler.Scanning;

namespace CompilersFinalProject.Compiler.Parsing
{

    public class Parser
    {
        #region constants

        private List<string> ErrorLog { get; set; }

        private const int MAXBREAK = 16;
        private const int NAMESIZE = 32;
        private const int PRESIZE = 32;
        private const int MAX_ARRAY = 100000;

        //Definision for that will help with the symbol tables.
        private const int MAX = 1000;
        private const int H = 255;

        private const int S_STACK = 50;

        #endregion


        #region data structures used

        char[] code = new char[MAX_ARRAY];
        char[] data = new char[MAX_ARRAY];
        int ip = 0;
        int sp = 0;
        int dp = 0;

        short IFELSE = 0;
        short SWITCHCASE = 0;



        int status = (int) DataTypeDefinition.TRUE;
        int ARRAYI = 0;



        StackPointer[] stack = new StackPointer[MAX_ARRAY];

        int[] s_break = new int[MAXBREAK];
        int[] s_connt = new int[MAXBREAK];
        int pb = 0;
        int pc = 0;
        int bc = 1;




        int indexg = 0;
        char[,,] locale = new char[S_STACK, H, MAX];
        char[,] global = new char[H, MAX];



        //char TOKEN[MAX_T_SIZE];
        char BUFF; // buffer for the source char.

        TokenTypeDefinition CUR_TOKEN; // Token type
        int CUR_VALUE; // Token value if any
        string CUR_NAME = ""; // Token name in the case of CUR_TOKEN == TK_ID

        int[] scanp = new int[10000000]; // Pointer to the corrent position on the source file
        int pointer;

        int CUR_LINE; // Line location of the scanned char in the corrent source file
        int CUR_COL; // Culome location on the scanned char in the corrent source file
                     // CUR_LINE and CUR_COL are used for error reproting


        //Stack Structure for preprocessing (#include)
        int P_STACK = 0;
        int[] LINE = new int[PRESIZE];
        int[] COL = new int[PRESIZE];
        int[] POINTER = new int[PRESIZE];
        char[] SOURCE = new char[PRESIZE];

        char?[,] PREFILE = new char?[PRESIZE, NAMESIZE];
        #endregion


        public Scanner scanner { get; set; }
        public int IP { get; set; }
        private Token currenToken { get; set; }

        public Parser(string source)
        {
            scanner = new Scanner(source);
            IP = 0;
        }

        void init()
        {
            CUR_LINE = LINE[P_STACK];
            CUR_COL = COL[P_STACK];
            pointer = POINTER[P_STACK];
            BUFF = (char)0;
            do
            {
                scanp[pointer] = getc(fd);
            } while (scanp[pointer++] >= '\0');
            pointer = 0;
        }

        void initarray(char?[] t, int index)
        {
            int i;
            index = t.Length;
            for (i = 0; i < index; i++)
            {
                t[i] = null;
            }
        }

        //initpref
        void initpref()
        {
            int i = 0;
            for (i = 0; i < NAMESIZE; i++)
            {
                PREFILE[P_STACK, i] = null;
            }
        }

        //scanchar
        char scanchar()
        {
            if (scanp[pointer] == '\n')
            {
                CUR_LINE++;
                CUR_COL = 1;
            }
            else
            {
                CUR_COL++;
            }
            return (char)scanp[pointer++];
        }

        //initoken
        void initoken()
        {
            int i;
            CUR_TOKEN = 0;
            CUR_VALUE = 0;

            for (i = 0; i < NAMESIZE; i++)
            {
                CUR_NAME[i] = (char)0;
            }
        }
        void Match(Token t)
        {

            if (t._tokenTypeDefinition == currenToken._tokenTypeDefinition)
            {
                token();
            }
            else
            {
                LogErrorToken(t);
            }
        }

        void LogErrorToken(Token t)
        {
            int stop = 0;

            switch (t._tokenTypeDefinition)
            {
                case TokenTypeDefinition.TK_SLASH:
                    {
                        ErrorLog.Add($"Expected \"/\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_NOT:
                    {
                        ErrorLog.Add($"Expected \"!\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_MOD:
                    {
                        ErrorLog.Add($"Expected \"%\" one line {CUR_LINE}colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_AMPER:
                    {
                        ErrorLog.Add("Expected \"&\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_LPAREN:
                    {
                        ErrorLog.Add($"Expected \"(\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_RPAREN:
                    {
                        ErrorLog.Add($"Expected \")\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_STAR:
                    {
                        ErrorLog.Add($"Expected \"*\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_PLUS:
                    {
                        ErrorLog.Add($"Expected \"+\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_COMMA:
                    {
                        ErrorLog.Add($"Expected \",\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_MINUS:
                    {
                        ErrorLog.Add($"Expected \"-\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_COLOM:
                    {
                        ErrorLog.Add("Expected \":\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_SEMI:
                    {
                        ErrorLog.Add($"Expected \";\" one line {CUR_LINE} colume %d source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_LESS:
                    {
                        ErrorLog.Add($"Expected \"<\" one line {CUR_LINE}colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_ASSIGN:
                    {
                        ErrorLog.Add($"Expected \"=\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_GREATER:
                    {
                        ErrorLog.Add($"Expected \">\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_QMARK:
                    {
                        ErrorLog.Add($"Expected \"?\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_LBRACK:
                    {
                        ErrorLog.Add($"Expected \"[\" one line {CUR_LINE} colume {CUR_COL}source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_RBRACK:
                    {
                        ErrorLog.Add($"Expected \"]\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_CARET:
                    {
                        ErrorLog.Add($"Expected \"^\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_LBRACE:
                    {
                        ErrorLog.Add($"Expected \"{\" one line {CUR_LINE} colume {CUR_COL}source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_RBRACE:
                    {
                        ErrorLog.Add($"Expected \"}\" one line {CUR_LINE} colume {CUR_COL}source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                case TokenTypeDefinition.TK_PIPE:
                    {
                        ErrorLog.Add($"Expected \"|\" one line {CUR_LINE} colume {CUR_COL} source file {SOURCE[P_STACK]}\n");
                        break;
                    }
                    //// left here need to work on this.
            }
            ErrorLog.Add($"error will matching TOKEN: {CUR_NAME} with ID: {CUR_TOKEN}");
            Application.Exit();
        }

        void token()
        {
            CharacterTypeDefinition chart;
            initoken();
            chart = chartype();
            if (chart == CharacterTypeDefinition.LETTER)
            {
                strlit();
                CUR_TOKEN = Scanner.KeyWordID(CUR_NAME);
                CUR_VALUE = 0;
                fprintf(sc, "%d : ", CUR_TOKEN);
                fprintf(sc, "%s\n", CUR_NAME);
            }
            else if (chart == CharacterTypeDefinition.DIGIT)
            {
                CUR_VALUE = intlit();
                CUR_TOKEN = TokenTypeDefinition.TK_INTLIT;
                fprintf(sc, "%d : ", CUR_TOKEN);
                fprintf(sc, "%d\n", CUR_VALUE);
            }
            else if (chart == CharacterTypeDefinition.SYMBOL)
            {
                scansym();
                if (CUR_TOKEN == NULL)
                {
                    BUFF = scanchar();
                    token();
                    putback(1);
                }
                else
                {
                    fprintf(sc, "%d : ", CUR_TOKEN);
                    fprintf(sc, "%s\n", CUR_NAME);
                }
            }
            else if (chart == CharacterTypeDefinition.SPACE)
            {
                BUFF = scanchar();
                token();
                putback(1);
            }
            else
            {
                return;
            }
            BUFF = scanchar();
        }

        void gettoken()
        {
            if (BUFF != '\0')
            {
                token();
            }
        }

        //init_gettoken
        void init_gettoken()
        {
            init();
            BUFF = scanchar();
            gettoken();
        }

        protected CharacterTypeDefinition chartype()
        {
            if (BUFF == '\0')
            {
                return CharacterTypeDefinition.NULL;
            }
            else if (97 <= BUFF && 122 >= BUFF)
            {
                return CharacterTypeDefinition.LETTER;
            }
            else if (65 <= BUFF && 90 >= BUFF)
            {
                return CharacterTypeDefinition.LETTER;
            }
            else if (BUFF == 95)
            {
                return CharacterTypeDefinition.LETTER;
            }
            else if (48 <= BUFF && 57 >= BUFF)
            {
                return CharacterTypeDefinition.DIGIT;
            }
            else if (BUFF <= 32)
            {
                return CharacterTypeDefinition.SPACE;
            }
            else
            {
                return CharacterTypeDefinition.SYMBOL;
            }
        }

        



    }
}
