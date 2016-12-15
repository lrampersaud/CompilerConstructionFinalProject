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
            if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID)
            {
                if (semanticAnalyzer.SymbolTable.Any(p => p.Name == scanner.CurrentToken.Value))
                {
                    SymbolVariable symVar = (SymbolVariable)semanticAnalyzer.SymbolTable.FirstOrDefault(p => p.Name == scanner.CurrentToken.Value);
                    scanner.Advance();
                    if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_EQUAL) //an assignment
                    {
                        //save whatever is at the top of the stack into this variable
                        switch (symVar.DataTypeDefinition)
                        {
                            case DataTypeDefinition.TYPE_BOOL:
                            case DataTypeDefinition.TYPE_CHAR:
                                {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_store); //does a push of the value onto the stack
                                    semanticAnalyzer.gen4(symVar.Address);
                                    break;
                                }
                            case DataTypeDefinition.TYPE_INT:
                                {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_storei);
                                    semanticAnalyzer.gen4(symVar.Address);
                                    break;
                                }
                            case DataTypeDefinition.TYPE_FLOAT:
                                {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_storef);
                                    semanticAnalyzer.gen4(symVar.Address);
                                    break;
                                }
                        }

                        semanticAnalyzer.E();
                        scanner.Match(TokenTypeDefinition.TK_SEMI);
                    }
                    else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GREATER ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GTEQ ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LESS ||
                     scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LTEQ)
                    {
                        //if it is not an assignment, then it is a comparison of some sort
                        switch (scanner.CurrentToken.TokenTypeDefinition)
                        {
                            case TokenTypeDefinition.TK_GREATER:
                            {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_gtr);
                                    break;
                            }
                            case TokenTypeDefinition.TK_GTEQ:
                                {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_geq);
                                    break;
                                }
                            case TokenTypeDefinition.TK_LESS:
                                {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_lss);
                                    break;
                                }
                            case TokenTypeDefinition.TK_LTEQ:
                                {
                                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_leq);
                                    break;
                                }
                        }
                        scanner.Advance();
                        semanticAnalyzer.E();
                        scanner.Match(TokenTypeDefinition.TK_SEMI);
                    }
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
                //if it is a literal, then add it to the stack and look for a comparison of some sort
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
