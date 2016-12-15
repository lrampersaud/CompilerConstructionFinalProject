using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CompilersFinalProject.Compiler.Scanning;
using CompilersFinalProject.Compiler.SymbolStructure;

namespace CompilersFinalProject.Compiler.SemanticAnalysis
{
    public class SemanticAnalyzer
    {
        private const int MAX_ARRAY = 100000;
        public Scanner scanner { get; set; }
        public HashSet<SymbolBase> SymbolTable { get; set; }
        public int dp { get; set; }
        public int ip { get; set; }
        public int sp { get; set; }

        public char[] code { get; set; }
        public char[] data { get; set; }
        public StackPointer[] stack { get; set; }



        public SemanticAnalyzer(Scanner scan)
        {
            scanner = scan;
            dp = 0;
            ip = 0;
            sp = 0;

            code = new char[MAX_ARRAY];
            data = new char[MAX_ARRAY];
            stack = new StackPointer[MAX_ARRAY];
        }


        public void VariableDeclaration()
        {
            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_A_VAR)
            {
                scanner.Match(TokenTypeDefinition.TK_A_VAR);

                //begin reading variable names
                List<SymbolVariable> variableTokens = new List<SymbolVariable>();
                while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID)
                {
                    variableTokens.Add(new SymbolVariable
                    {
                        Address = 0,
                        Name = scanner.CurrentToken.Value,
                        FlagTypeDefinition = FlagTypeDefinition.Register
                    });

                    if (SymbolTable.Any(p => p.Name == scanner.CurrentToken.Value))
                    {
                        scanner.LogErrorToken(new Token(TokenCategory.Identifier, TokenTypeDefinition.TK_MULTIPLE_DECLARATIONS, ""));
                    }

                    scanner.Match(TokenTypeDefinition.TK_ID);
                }

                scanner.Match(TokenTypeDefinition.TK_COLON);

                int size = 0;
                DataTypeDefinition currentDataType= DataTypeDefinition.TYPE_VOID;
                
                switch (scanner.CurrentToken.TokenTypeDefinition)
                {
                    case TokenTypeDefinition.TK_INT:
                    {
                        size = 4;
                        currentDataType = DataTypeDefinition.TYPE_INT;
                        scanner.Match(TokenTypeDefinition.TK_INT);
                        break;
                    }
                    case TokenTypeDefinition.TK_FLOAT:
                    {
                        size = 8;
                        currentDataType = DataTypeDefinition.TYPE_FLOAT;
                        scanner.Match(TokenTypeDefinition.TK_FLOAT);
                        break;
                    }
                    case TokenTypeDefinition.TK_CHAR:
                    {
                        size = 1;
                        currentDataType = DataTypeDefinition.TYPE_CHAR;
                        scanner.Match(TokenTypeDefinition.TK_CHAR);
                        break;
                    }
                    case TokenTypeDefinition.TK_BOOL:
                    {
                        size = 1;
                        currentDataType = DataTypeDefinition.TYPE_BOOL;
                        scanner.Match(TokenTypeDefinition.TK_BOOL);
                        break;
                    }
                    default:
                    {
                        scanner.LogErrorToken(scanner.CurrentToken);
                        break;
                    }
                }
                
                //compute addresses
                foreach (SymbolVariable t in variableTokens)
                {
                    t.Size = size;
                    t.Address = dp;
                    t.DataTypeDefinition = currentDataType;
                    dp = dp + size;
                    SymbolTable.Add(t);
                }
                
                scanner.Match(TokenTypeDefinition.TK_SEMI);
            }

        }




        //ARITHMETIC HEADACHE
        public TokenTypeValue F()
        {
            TokenTypeValue tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_VOID, null);
            switch (scanner.CurrentToken.TokenTypeDefinition)
            {
                case TokenTypeDefinition.TK_ID:
                {
                    if (SymbolTable.Any(p => p.Name == scanner.CurrentToken.Value))
                    {
                        SymbolVariable symVar = (SymbolVariable)SymbolTable.FirstOrDefault(p => p.Name == scanner.CurrentToken.Value);
                        tokenFound = new TokenTypeValue(symVar.DataTypeDefinition, symVar.Address);
                        tokenFound.isAddress = true;
                        gen4(symVar.Address);
                    }
                    break;
                }
                case TokenTypeDefinition.TK_INTLIT:
                {
                    gen4(Convert.ToInt32(scanner.CurrentToken.Value));
                    tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_INT, Convert.ToInt32(scanner.CurrentToken.Value));

                    break;
                }
                case TokenTypeDefinition.TK_REALLIT:
                {
                    gen8(Convert.ToInt32(scanner.CurrentToken.Value));
                    tokenFound =  new TokenTypeValue(DataTypeDefinition.TYPE_FLOAT, Convert.ToSingle(scanner.CurrentToken.Value));
                    break;
                }
                case TokenTypeDefinition.TK_BOOLLIT:
                {
                    gen1(Convert.ToBoolean(scanner.CurrentToken.Value) ? '1' : '0');
                    tokenFound =  new TokenTypeValue(DataTypeDefinition.TYPE_BOOL, Convert.ToBoolean(scanner.CurrentToken.Value));
                    break;
                }
                case TokenTypeDefinition.TK_CHARLIT:
                {
                    gen1(scanner.CurrentToken.Value[0]);
                    tokenFound =  new TokenTypeValue(DataTypeDefinition.TYPE_CHAR, scanner.CurrentToken.Value);
                    break;
                }
            }
            scanner.Advance();
            return tokenFound;
        }


        public TokenTypeValue T()
        {
            TokenTypeValue t1 = F();

            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_SLASH ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_STAR)
            {
                TokenTypeDefinition operation = scanner.CurrentToken.TokenTypeDefinition;
                scanner.Match(operation);

                TokenTypeValue t2 = F();

                t1 = Generate(t1, t2, operation);

            }

            return t1;
        }

        public TokenTypeValue E()
        {
            TokenTypeValue t1 = T();

            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_PLUS ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_MINUS)
            {
                TokenTypeDefinition operation = scanner.CurrentToken.TokenTypeDefinition;
                scanner.Match(operation);

                TokenTypeValue t2 = T();

                t1 = Generate(t1, t2, operation);

            }

            return t1;
        }


        public TokenTypeValue Generate(TokenTypeValue t1, TokenTypeValue t2, TokenTypeDefinition operation)
        {
            TokenTypeValue tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_VOID, null);
            if (operation == TokenTypeDefinition.TK_PLUS)
            {
                if (t1.DataType == t2.DataType)
                {
                    tokenFound.DataType = t1.DataType;
                    if (t1.DataType == DataTypeDefinition.TYPE_INT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_add);
                    }
                    else if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_fadd);
                    }
                }
                else
                {
                    tokenFound.DataType = DataTypeDefinition.TYPE_FLOAT;
                    if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fadd);
                    }
                    else
                    {
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_fadd);
                    }
                }
            }

            if (operation == TokenTypeDefinition.TK_MINUS)
            {
                if (t1.DataType == t2.DataType)
                {
                    tokenFound.DataType = t1.DataType;
                    if (t1.DataType == DataTypeDefinition.TYPE_INT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_sub);
                    }
                    else if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_sub);
                    }
                }
                else
                {
                    tokenFound.DataType = DataTypeDefinition.TYPE_FLOAT;
                    if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_sub);
                    }
                    else
                    {
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_sub);
                    }
                }
            }


            if (operation == TokenTypeDefinition.TK_STAR)
            {
                if (t1.DataType == t2.DataType)
                {
                    tokenFound.DataType = t1.DataType;
                    if (t1.DataType == DataTypeDefinition.TYPE_INT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_mul);
                    }
                    else if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_fmul);
                    }
                }
                else
                {
                    tokenFound.DataType = DataTypeDefinition.TYPE_FLOAT;
                    if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fmul);
                    }
                    else
                    {
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_fmul);
                    }
                }
            }

            if (operation == TokenTypeDefinition.TK_SLASH)
            {
                if (t1.DataType == t2.DataType)
                {
                    tokenFound.DataType = t1.DataType;
                    if (t1.DataType == DataTypeDefinition.TYPE_INT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_div);
                    }
                    else if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_fdiv);
                    }
                }
                else
                {
                    tokenFound.DataType = DataTypeDefinition.TYPE_FLOAT;
                    if (t1.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fdiv);
                    }
                    else
                    {
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_fdiv);
                    }
                }
            }


            return tokenFound;
        }


        void GenerateOperation(OperationTypeDefinition op)
        {
            if(ip<MAX_ARRAY)
            {
                switch (op)
                {
                    case OperationTypeDefinition.op_end:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_end;
                        break;
                    }
                    case OperationTypeDefinition.op_add:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_add;
                        break;
                    }
                    case OperationTypeDefinition.op_sub:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_sub;
                        break;
                    }
                    case OperationTypeDefinition.op_push:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_push;
                        break;
                    }
                    case OperationTypeDefinition.op_pushi:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_pushi;
                        break;
                    }
                    case OperationTypeDefinition.op_pop:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_pop;
                        break;
                    }
                    case OperationTypeDefinition.op_popi:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_popi;
                        break;
                    }
                    case OperationTypeDefinition.op_dup:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_dup;
                        break;
                    }
                    case OperationTypeDefinition.op_exch:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_exch;
                        break;
                    }
                    case OperationTypeDefinition.op_jmp:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_jmp;
                        break;
                    }
                    case OperationTypeDefinition.op_fjmp:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_fjmp;
                        break;
                    }
                    case OperationTypeDefinition.op_tjmp:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_tjmp;
                        break;
                    }
                    case OperationTypeDefinition.op_mul:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_mul;
                        break;
                    }
                    case OperationTypeDefinition.op_popa:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_popa;
                        break;
                    }
                    case OperationTypeDefinition.op_pusha:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_pusha;
                        break;
                    }
                    case OperationTypeDefinition.op_out:
                    {
                        code[ip] = (char)OperationTypeDefinition.op_out;
                        break;
                    }
                    default:
                    {
                        //printf("\nThis operation has not been emplemented yet. --> %d\n",op);
                        break;
                    }
                }
                ip++;
            }
            else
            {
                //printf("\nRun time error: Out of range Code\n");
                //exit(EXIT_FAILURE);
            }
        }


        private void gen1(char val)
        {
            code[ip] = val;
            ip +=1;
        }

        private void gen4(int val)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                code[ip++] = (char) b;
            }
        }



        private void gen8(double val)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                code[ip++] = (char) b;
            }
        }





    }
}