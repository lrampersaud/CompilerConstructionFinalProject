using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CompilersFinalProject.Compiler.Scanning;
using CompilersFinalProject.Compiler.SymbolStructure;

namespace CompilersFinalProject.Compiler.Parsing
{

    public class Parser
    {
        public Scanner scanner { get; set; }
        private HashSet<SymbolBase> SymbolTable { get; set; }
        private int ip = 0;
        private int sp = 0;
        private int dp = 0;

        #region constants






        private const int MAXBREAK = 16;
        private const int NAMESIZE = 32;
        private const int PRESIZE = 32;
        private const int MAX_ARRAY = 100000;

        //Definision for that will help with the symbol tables.
        private const int MAX = 1000;
        private const int H = 255;

        private const int S_STACK = 50;

        #endregion


        #region data_structures_used

        char[] code = new char[MAX_ARRAY];
        char[] data = new char[MAX_ARRAY];


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

        int CUR_VALUE; // Token value if any
        string CUR_NAME = ""; // Token name in the case of scanner.CurrentToken.TokenTypeDefinition == TK_ID

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


        public Parser(string source)
        {
            scanner = new Scanner(source);
            ip = 0;
            sp = 0;
            dp = 0;
            pointer = 0;
            scanner.Advance();
        }

        public void ParseHeader()
        {
            Match(TokenTypeDefinition.TK_PROGRAM);
            Match(TokenTypeDefinition.TK_ID);

        }

        public void ParseExpressions()
        {
            int ty, val;
            char[] name;
            
            
            if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID)
            {
                ty = procIDs(name, tf);
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_INT ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_CHAR ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_FLOAT)
            {
                ty = procVAR();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GREATER ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GTEQ ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LESS ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LTEQ)
            {
                ty = procRELATION();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_INTLIT)
            {
                ty = procLIT();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_WHILE)
            {
                ty = procWHILE();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_DO)
            {
                ty = procDO();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_FOR)
            {
                ty = procFOR();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_IF)
            {
                procIF();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ELSE)
            {
                procELSE();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_SWITCH)
            {
                procSWITCH();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_OUT)
            {
                procOUT();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_COMMA)
            {
                ty = procRE_EXP(TRUE);
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LBRACE)
            {
                Match(TokenTypeDefinition.TK_LBRACE);
            }
            else
            {
                scanner.LogErrorToken(scanner.CurrentToken);
            }
        }


        public void Match(TokenTypeDefinition t)
        {
            if (t == scanner.CurrentToken.TokenTypeDefinition)
            {
                scanner.Advance();
            }
            else
            {
                scanner.LogErrorToken(new Token(TokenCategory.Literal, t, ""));
            }
        }


        public void Run()
        {
            ParseHeader();
        }








    }
}
