using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompilersFinalProject.Compiler.Scanning;

namespace CompilersFinalProject.Compiler.Parsing
{

    public class Parser
    {
        #region constants

        private const int DIGIT = 1;
        private const int LETTER = 2;
        private const int SPACE = 3;
        private const int SYMBOL = 4;

        private const int MAXBREAK = 16;

        private const int NAMESIZE = 32;

        private const int PRESIZE = 32;

        private const int MAX_ARRAY = 100000;

        private const int TRUE = 1;
        private const int FALSE = 0;

        private const int TYPE_CHAR = 2;   //type char
        private const int TYPE_INT = 3; //type int
        private const int TYPE_FLOAT = 4;  //type float
        private const int TYPE_VOID = 5; //type void

        private const int op_push = 1;
        private const int op_pushi = 2;
        private const int op_pop = 3;
        private const int op_popi = 4;
        private const int op_add = 5;
        private const int op_sub = 6;
        private const int op_end = 7;
        private const int op_mul = 8;
        private const int op_div = 9;
        private const int op_and = 10;
        private const int op_or = 11;
        private const int op_xor = 12;
        private const int op_shl = 13;
        private const int op_shr = 14;
        private const int op_not = 15;
        private const int op_neg = 16;
        private const int op_fadd = 17;
        private const int op_fend = 18;
        private const int op_fmul = 19;
        private const int op_fdiv = 20;
        private const int op_fneg = 21;
        private const int op_fnot = 22;
        private const int op_fcon = 23;
        private const int op_icon = 24;
        private const int op_exch = 25;
        private const int op_eql = 26;
        private const int op_neq = 27;
        private const int op_lss = 28;
        private const int op_gtr = 29;
        private const int op_leq = 30;
        private const int op_geq = 31;
        private const int op_feql = 32;
        private const int op_fneq = 33;
        private const int op_flss = 34;
        private const int op_fgtr = 35;
        private const int op_fleg = 36;
        private const int op_fgeq = 37;
        private const int op_printint = 38;
        private const int op_stop = 39;
        private const int op_dup = 40;
        private const int op_jmp = 41;
        private const int op_fjmp = 42;
        private const int op_tjmp = 43;
        private const int op_popa = 44;
        private const int op_pusha = 45;
        private const int op_out = 46;
        private const int op_pushr = 47;


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



        int status = TRUE;
        int ARRAYI = 0;



        STACK[] stack = new STACK[MAX_ARRAY];

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

        int CUR_TOKEN; // Token type
        int CUR_VALUE; // Token value if any
        char[] CUR_NAME = new char[NAMESIZE]; // Token name in the case of CUR_TOKEN == TK_ID

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
            //int fd;
            //int fleng;

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
                errort(t);
            }
        }

        void errort(Token t)
        {
            int stop = 0;

            switch (t._tokenTypeDefinition)
            {
                case TokenTypeDefinition.TK_SLASH:
                    {
                        printf("Expected \"/\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_NOT:
                    {
                        printf("Expected \"!\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_MOD:
                    {
                        printf("Expected \"%\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_AMPER:
                    {
                        printf("Expected \"&\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_LPAREN:
                    {
                        printf("Expected \"(\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_RPAREN:
                    {
                        printf("Expected \")\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_STAR:
                    {
                        printf("Expected \"*\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_PLUS:
                    {
                        printf("Expected \"+\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_COMMA:
                    {
                        printf("Expected \",\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_MINUS:
                    {
                        printf("Expected \"-\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_COLOM:
                    {
                        printf("Expected \":\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_SEMI:
                    {
                        printf("Expected \";\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_LESS:
                    {
                        printf("Expected \"<\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_ASSIGN:
                    {
                        printf("Expected \"=\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_GREATER:
                    {
                        printf("Expected \">\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_QMARK:
                    {
                        printf("Expected \"?\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_LBRACK:
                    {
                        printf("Expected \"[\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_RBRACK:
                    {
                        printf("Expected \"]\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_CARET:
                    {
                        printf("Expected \"^\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_LBRACE:
                    {
                        printf("Expected \"{\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_RBRACE:
                    {
                        printf("Expected \"}\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                case TokenTypeDefinition.TK_PIPE:
                    {
                        printf("Expected \"|\" one line %d colume %d source file %s\n", CUR_LINE, CUR_COL, SOURCE[P_STACK]);
                        break;
                    }
                    //// left here need to work on this.
            }
            printf("error will matching TOKEN: %s with ID: %d", CUR_NAME, CUR_TOKEN);
            exit(EXIT_FAILURE);
        }

        void token()
        {
            int chart;
            initoken();
            chart = chartype();
            if (chart == LETTER)
            {
                strlit();
                CUR_TOKEN = keywordc(CUR_NAME);
                CUR_VALUE = NULL;
                fprintf(sc, "%d : ", CUR_TOKEN);
                fprintf(sc, "%s\n", CUR_NAME);
            }
            else if (chart == DIGIT)
            {
                CUR_VALUE = intlit();
                CUR_TOKEN = TK_INTLIT;
                fprintf(sc, "%d : ", CUR_TOKEN);
                fprintf(sc, "%d\n", CUR_VALUE);
            }
            else if (chart == SYMBOL)
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
            else if (chart == SPACE)
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

        int chartype()
        {
            if (BUFF == '\0')
            {
                return -1;
            }
            else if (97 <= BUFF && 122 >= BUFF)
            {
                return LETTER;
            }
            else if (65 <= BUFF && 90 >= BUFF)
            {
                return LETTER;
            }
            else if (BUFF == 95)
            {
                return LETTER;
            }
            else if (48 <= BUFF && 57 >= BUFF)
            {
                return DIGIT;
            }
            else if (BUFF <= 32)
            {
                return SPACE;
            }
            else
            {
                return SYMBOL;
            }
        }



    }

    struct VAR
    {
        int var; //TK_A_VAR, TK_AN_ARRAY, TK_A_LABEL
        int ty; //TYPE_INT, TYPE_CHAR, TYPE_FLOAT
        int high; //used when var == TK_AN_ARRAY else NULL
        int addr; //Address in data[] for TK_A_VAR, TK_ALABEL and address for the first element of the array in the case of TK_AN_ARRAY
    }


    struct STACK
    {
        int i;
        float f;
    }

}
