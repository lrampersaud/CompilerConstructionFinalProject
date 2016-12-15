using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CompilersFinalProject.Compiler.Scanning;
using CompilersFinalProject.Compiler.SemanticAnalysis;
using CompilersFinalProject.Compiler.SymbolStructure;

namespace CompilersFinalProject.Compiler.Parsing
{

    public class Parser
    {
        public Scanner scanner { get; set; }
        public SemanticAnalyzer semanticAnalyzer { get; set; }




        public Parser(string source)
        {
            scanner = new Scanner(source);


            scanner.Advance();
            semanticAnalyzer = new SemanticAnalyzer(scanner);
        }

        public void ParseHeader()
        {
            scanner.Match(TokenTypeDefinition.TK_PROGRAM);
            scanner.Match(TokenTypeDefinition.TK_ID);

        }

        public void ParseExpressions()
        {
            int ty, val;
            char[] name;
            
            
            if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID)
            {
                semanticAnalyzer.F();

                if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_EQUAL) //an assignment
                {

                    semanticAnalyzer.E();
                    scanner.Match(TokenTypeDefinition.TK_SEMI);
                }


                //ty = procIDs(name, tf);
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_A_VAR)
            {
                semanticAnalyzer.VariableDeclaration();
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
                scanner.Match(TokenTypeDefinition.TK_LBRACE);
            }
            else
            {
                scanner.LogErrorToken(scanner.CurrentToken);
            }
        }





        public void Run()
        {
            ParseHeader();

            while (!scanner._done)
            {
                ParseExpressions();
            }

        }








    }
}
