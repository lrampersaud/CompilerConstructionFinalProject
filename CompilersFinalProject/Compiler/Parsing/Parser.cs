using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public string VMCode { get; set; }
        public string Errors { get; set; }



        public Parser(string source)
        {
            scanner = new Scanner(source);
            VMCode = "";

            scanner.Advance();
            semanticAnalyzer = new SemanticAnalyzer(scanner);
        }

        public void ParseHeader()
        {
            scanner.Match(TokenTypeDefinition.TK_PROGRAM);
            scanner.Match(TokenTypeDefinition.TK_ID);
            scanner.Match(TokenTypeDefinition.TK_SEMI);
        }

        public void ParseExpressions()
        {
            if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID)
            {
                SymbolBase symBase = semanticAnalyzer.SymbolTable.FirstOrDefault(p => p.Name == scanner.CurrentToken.Value);
                if (symBase != null)
                {
                    if (typeof(SymbolVariable) == symBase.GetType())
                    {
                        SymbolVariable symVar = (SymbolVariable) symBase;

                        scanner.Match(TokenTypeDefinition.TK_ID);
                        if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_EQUAL) //an assignment
                        {
                            scanner.Match(TokenTypeDefinition.TK_EQUAL);

                            semanticAnalyzer.E();
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
                            scanner.Match(TokenTypeDefinition.TK_SEMI);
                        }
                        else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GREATER ||
                                 scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GTEQ ||
                                 scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LESS ||
                                 scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LTEQ)
                        {
                            scanner.Match(scanner.CurrentToken.TokenTypeDefinition);
                            semanticAnalyzer.E();
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
                        }
                    }
                    else if (typeof(SymbolProcedure) == symBase.GetType())
                    {
                        SymbolProcedure symVar = (SymbolProcedure)symBase;

                        scanner.Match(TokenTypeDefinition.TK_ID);
                        scanner.Match(TokenTypeDefinition.TK_LPAREN);

                        while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_RPAREN)
                        {
                            semanticAnalyzer.F();
                            if(scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_COMMA)
                                scanner.Match(TokenTypeDefinition.TK_COMMA);
                        }
                        scanner.Match(TokenTypeDefinition.TK_RPAREN);
                        scanner.Match(TokenTypeDefinition.TK_SEMI);

                        //push the return to position
                        semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_pushi);
                        semanticAnalyzer.gen4(semanticAnalyzer.ip);

                        //jump to procedure
                        semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jmp);
                        semanticAnalyzer.gen4(symVar.Address);

                    }
                }
                
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_A_VAR)
            {
                semanticAnalyzer.VariableDeclaration();
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_REPEAT)
            {
                scanner.Match(TokenTypeDefinition.TK_REPEAT);
                int target = semanticAnalyzer.ip;
                while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_UNTIL)
                {
                    ParseExpressions();
                }
                scanner.Match(TokenTypeDefinition.TK_UNTIL);
                semanticAnalyzer.Condition();
                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jfalse);
                semanticAnalyzer.gen4(target);
                scanner.Match(TokenTypeDefinition.TK_SEMI);
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_WHILE)
            {
                int target = semanticAnalyzer.ip;
                scanner.Match(TokenTypeDefinition.TK_WHILE);
                semanticAnalyzer.Condition();
                
                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jfalse);
                int hole = semanticAnalyzer.ip;
                semanticAnalyzer.gen4(0);
                scanner.Match(TokenTypeDefinition.TK_DO);
                scanner.Match(TokenTypeDefinition.TK_BEGIN);
                while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_END)
                {
                    ParseExpressions();
                }
                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jmp);
                semanticAnalyzer.gen4(target);
                semanticAnalyzer.gen_Address(semanticAnalyzer.ip, hole);

                scanner.Match(TokenTypeDefinition.TK_END);
                scanner.Match(TokenTypeDefinition.TK_SEMI);
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_IF)
            {
                scanner.Match(TokenTypeDefinition.TK_IF);
                semanticAnalyzer.Condition();
                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jfalse);
                int hole = semanticAnalyzer.ip;
                semanticAnalyzer.gen4(0);

                scanner.Match(TokenTypeDefinition.TK_THEN);
                bool isMany = scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_BEGIN;

                if (isMany)
                {
                    scanner.Match(TokenTypeDefinition.TK_BEGIN);
                    while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_END)
                    {
                        ParseExpressions();
                    }
                    scanner.Match(TokenTypeDefinition.TK_END);
                }
                else
                {
                    ParseExpressions();
                }

                if(scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ELSE)
                {
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jmp);
                    int save_ip = semanticAnalyzer.ip;
                    semanticAnalyzer.gen4(0);

                    semanticAnalyzer.gen_Address(semanticAnalyzer.ip, hole);


                    scanner.Match(TokenTypeDefinition.TK_ELSE);
                    
                    isMany = scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_BEGIN;
                    if (isMany)
                    {
                        scanner.Match(TokenTypeDefinition.TK_BEGIN);
                        while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_END)
                        {
                            ParseExpressions();
                        }
                        scanner.Match(TokenTypeDefinition.TK_END);
                    }
                    else
                    {
                        ParseExpressions();
                    }

                    semanticAnalyzer.gen_Address(semanticAnalyzer.ip, save_ip);
                }
                else
                {
                    semanticAnalyzer.gen_Address(semanticAnalyzer.ip, hole);
                }
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_FOR)
            {
                scanner.Match(TokenTypeDefinition.TK_FOR);

                SymbolVariable element = (SymbolVariable)semanticAnalyzer.SymbolTable.FirstOrDefault(p=>p.Name == scanner.CurrentToken.Value);

                scanner.Match(TokenTypeDefinition.TK_ID);
                scanner.Match(TokenTypeDefinition.TK_EQUAL);
                semanticAnalyzer.F();

                if(element != null && element.DataTypeDefinition== DataTypeDefinition.TYPE_INT)
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_storei);
                else if (element != null && element.DataTypeDefinition == DataTypeDefinition.TYPE_FLOAT)
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_storef);
                if (element != null) semanticAnalyzer.gen4(element.Address);

                scanner.Match(TokenTypeDefinition.TK_TO);
                semanticAnalyzer.F();


                scanner.Match(TokenTypeDefinition.TK_DO);
                scanner.Match(TokenTypeDefinition.TK_BEGIN);

                int beginFor = semanticAnalyzer.ip;

                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_dup);

                if (element != null && element.DataTypeDefinition == DataTypeDefinition.TYPE_INT)
                {
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_fetchi);
                    semanticAnalyzer.gen4(element.Address);
                }
                else if (element != null && element.DataTypeDefinition == DataTypeDefinition.TYPE_FLOAT)
                {
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_fetchf);
                    semanticAnalyzer.gen4(element.Address);
                }


                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_leq);

                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jfalse);
                int save_ip = semanticAnalyzer.ip;
                semanticAnalyzer.gen4(0);

                while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_END)
                {
                    ParseExpressions();
                }

                //increment counter
                if (element != null && element.DataTypeDefinition == DataTypeDefinition.TYPE_INT)
                {
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_fetchi);
                    semanticAnalyzer.gen4(element.Address);
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_pushi);
                    semanticAnalyzer.gen4(1);
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_add);
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_storei);
                    semanticAnalyzer.gen4(element.Address);
                }
                else if (element != null && element.DataTypeDefinition == DataTypeDefinition.TYPE_FLOAT)
                {
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_fetchf);
                    semanticAnalyzer.gen4(element.Address);
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_pushf);
                    semanticAnalyzer.gen8(1);
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_add);
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_storef);
                    semanticAnalyzer.gen4(element.Address);
                }

                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jmp);
                semanticAnalyzer.gen4(beginFor);

                scanner.Match(TokenTypeDefinition.TK_END);
                scanner.Match(TokenTypeDefinition.TK_SEMI);

                semanticAnalyzer.gen_Address(semanticAnalyzer.ip, save_ip);
                

            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_CASE)
            {
                scanner.Match(TokenTypeDefinition.TK_CASE);

                semanticAnalyzer.F();

                scanner.Match(TokenTypeDefinition.TK_OF);

                int prevaddress = -1;
                while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_END)
                {
                    int address = semanticAnalyzer.ip;

                    //fill the previous hole with my address
                    if (prevaddress != -1)
                    {
                        semanticAnalyzer.gen_Address(address, prevaddress);
                    }

                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_dup);

                    semanticAnalyzer.F();
                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_eql);

                    semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jfalse);
                    prevaddress = semanticAnalyzer.ip;
                    semanticAnalyzer.gen4(0);

                    
                    scanner.Match(TokenTypeDefinition.TK_COLON);
                    
                    ParseExpressions();
                }

                //remove the expression value from the stack
                semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_pop);
                if (prevaddress != -1)
                {
                    semanticAnalyzer.gen_Address(semanticAnalyzer.ip, prevaddress);
                }


                scanner.Match(TokenTypeDefinition.TK_END);
                scanner.Match(TokenTypeDefinition.TK_SEMI);

            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_PROCEDURE)
            {
               
                scanner.Match(TokenTypeDefinition.TK_PROCEDURE);
                if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID)
                {
                    SymbolProcedure symbolProc = new SymbolProcedure();
                    symbolProc.Seen = true;
                    symbolProc.Name = scanner.CurrentToken.Value;
                    symbolProc.Address = semanticAnalyzer.ip;
                    symbolProc.ChildSymbolTable = new HashSet<SymbolBase>();

                    semanticAnalyzer.SymbolTable.Add(symbolProc);

                    HashSet<SymbolBase> previousSymbolTable = new HashSet<SymbolBase>();
                    previousSymbolTable.UnionWith(semanticAnalyzer.SymbolTable.ToList());

                    scanner.Match(TokenTypeDefinition.TK_ID);
                    scanner.Match(TokenTypeDefinition.TK_LPAREN);

                    semanticAnalyzer.VariableDeclarationProcedure(symbolProc);
                    
                    scanner.Match(TokenTypeDefinition.TK_RPAREN);
                    scanner.Match(TokenTypeDefinition.TK_SEMI);

                    if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_A_VAR)
                    {
                        scanner.Match(TokenTypeDefinition.TK_A_VAR);
                        semanticAnalyzer.VariableDeclaration();
                    }

                    scanner.Match(TokenTypeDefinition.TK_BEGIN);

                    while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_END)
                    {
                        ParseExpressions();
                    }
                    scanner.Match(TokenTypeDefinition.TK_END);
                    scanner.Match(TokenTypeDefinition.TK_SEMI);
                }

                //jump back to who called me: the top of the stack should be the address who called me
               semanticAnalyzer.GenerateOperation(OperationTypeDefinition.op_jmps); //pop the stack and jump to the address

                if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_BEGIN)
                    scanner.Match(TokenTypeDefinition.TK_BEGIN);

            }


            //else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_OUT)
            //{
            //    procOUT();
            //}

            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LBRACE)
            {
                scanner.Match(TokenTypeDefinition.TK_LBRACE);
            }
            else if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_END)
            {
                scanner.Match(TokenTypeDefinition.TK_END);
            }
            else
            {
                scanner.LogErrorToken(scanner.CurrentToken);
                scanner.Advance();
            }
        }





        public void Run()
        {
            ParseHeader();

            while (!scanner._done)
            {
                ParseExpressions();
            }


            VMCode = VirtualMachine();
            foreach (var item in scanner.ErrorLog)
            {
                Errors += item;
                Errors += "\n";
            }
        }


        public string VirtualMachine()
        {
            string strCode = "";

            int cp = 0; //code pointer to the code file
            while (cp < semanticAnalyzer.ip)
            {
                int c = Convert.ToInt32(Convert.ToByte(semanticAnalyzer.code[cp]));

                cp++;
                switch (c)
                {
                    case (int)OperationTypeDefinition.op_push:
                    {
                        strCode += "push ";
                        strCode += semanticAnalyzer.code[cp];
                        
                        break;
                    }
                    case (int)OperationTypeDefinition.op_pushi:
                        {
                            strCode += "pushi ";
                            strCode += BitConverter.ToInt32((byte[]) semanticAnalyzer.code.Skip(cp).Take(4).Select(p=>(byte)p).ToArray(),0);
                            cp+=4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_pop:
                        {
                            strCode += "pop ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_popi:
                        {
                            strCode += "popi ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_add:
                        {
                            strCode += "add ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_end:
                        {
                            strCode += "end ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_mul:
                        {
                            strCode += "mul ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_div:
                        {
                            strCode += "div ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_and:
                        {
                            strCode += "and ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_or:
                        {
                            strCode += "or ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_xor:
                        {
                            strCode += "xor ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_shl:
                        {
                            strCode += "shl ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_shr:
                        {
                            strCode += "shr ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_not:
                        {
                            strCode += "not ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_neg:
                        {
                            strCode += "neg ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fadd:
                        {
                            strCode += "fadd ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fend:
                        {
                            strCode += "fend ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fmul:
                        {
                            strCode += "fmul ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fdiv:
                        {
                            strCode += "fdiv ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fneg:
                        {
                            strCode += "fneg ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fnot:
                        {
                            strCode += "fnot ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fcon:
                        {
                            strCode += "fcon ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_icon:
                        {
                            strCode += "icon ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_exch:
                        {
                            strCode += "exch ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_eql:
                        {
                            strCode += "eql ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_neq:
                        {
                            strCode += "neq ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_lss:
                        {
                            strCode += "lss ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_gtr:
                        {
                            strCode += "gtr ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_leq:
                        {
                            strCode += "leq ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_geq:
                        {
                            strCode += "neq ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_feql:
                        {
                            strCode += "feql ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fneq:
                        {
                            strCode += "fneq ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_flss:
                        {
                            strCode += "flss ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fgtr:
                        {
                            strCode += "fgtr ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fleg:
                        {
                            strCode += "fleg ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fgeq:
                        {
                            strCode += "fgeq ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_pr:
                        {
                            strCode += "pr ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_stop:
                        {
                            strCode += "stop ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_dup:
                        {
                            strCode += "dup ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_jmp:
                        {
                            strCode += "jmp ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_jmps:
                        {
                            strCode += "jmp ";
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fjmp:
                        {
                            strCode += "fjmp ";
                            strCode += BitConverter.ToSingle((byte[])semanticAnalyzer.code.Skip(cp).Take(8).Select(p => (byte)p).ToArray(), 0);
                            cp += 8;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_tjmp:
                        {
                            strCode += "tjmp ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_popa:
                        {
                            strCode += "popa ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_out:
                        {
                            strCode += "out ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_pushr:
                        {
                            strCode += "pushr ";
                            strCode += semanticAnalyzer.code[cp];
                            cp ++;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_pushf:
                        {
                            strCode += "pushf ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(8).Select(p => (byte)p).ToArray(), 0);
                            cp += 8;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_popf:
                        {
                            strCode += "popf ";
                            
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fetchi:
                        {
                            strCode += "fetchi ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fetchf:
                        {
                            strCode += "fetchf ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(8).Select(p => (byte)p).ToArray(), 0);
                            cp += 8;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_fetch:
                        {
                            strCode += "pushi ";
                            strCode += semanticAnalyzer.code[cp];
                            cp ++;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_store:
                        {
                            strCode += "store ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_storei:
                        {
                            strCode += "storei ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_storef:
                        {
                            strCode += "storef ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_jfalse:
                        {
                            strCode += "jfalse ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }
                    case (int)OperationTypeDefinition.op_jtrue:
                        {
                            strCode += "jtrue ";
                            strCode += BitConverter.ToInt32((byte[])semanticAnalyzer.code.Skip(cp).Take(4).Select(p => (byte)p).ToArray(), 0);
                            cp += 4;
                            break;
                        }

                }
                strCode += "\n";
            }
            return strCode;
        }








    }
}
