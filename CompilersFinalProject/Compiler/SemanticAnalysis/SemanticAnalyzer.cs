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
            SymbolTable = new HashSet<SymbolBase>();
        }


        public void VariableDeclaration()
        {
            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_A_VAR && scanner.ErrorLog.Count == 0)
            {
                scanner.Match(TokenTypeDefinition.TK_A_VAR);

                //begin reading variable names
                List<SymbolVariable> variableTokens = new List<SymbolVariable>();
                while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID && scanner.ErrorLog.Count == 0)
                {
                    variableTokens.Add(new SymbolVariable
                    {
                        Address = 0,
                        Name = scanner.CurrentToken.Value,
                        FlagTypeDefinition = FlagTypeDefinition.Register,
                        ChildSymbolTable=new HashSet<SymbolBase>()
                    });

                    if (SymbolTable.Any(p => p.Name == scanner.CurrentToken.Value))
                    {
                        scanner.LogErrorToken(new Token(TokenCategory.Identifier, TokenTypeDefinition.TK_MULTIPLE_DECLARATIONS, ""));
                    }

                    scanner.Match(TokenTypeDefinition.TK_ID);
                    if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_COMMA)
                    {
                        scanner.Match(TokenTypeDefinition.TK_COMMA);
                    }
                }

                scanner.Match(TokenTypeDefinition.TK_COLON);

                int size = 0;
                DataTypeDefinition currentDataType = DataTypeDefinition.TYPE_VOID;

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

        /// <summary>
        /// Store variables in data segment and initialize
        /// </summary>
        public void VariableDeclarationProcedure(SymbolProcedure symbolProcedure)
        {
            symbolProcedure.Arguments = new List<ProcedureArgument>();
            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID && scanner.ErrorLog.Count == 0)
            {
                //begin reading variable names
                List<SymbolVariable> variableTokens = new List<SymbolVariable>();
                while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_ID && scanner.ErrorLog.Count == 0)
                {
                    variableTokens.Add(new SymbolVariable
                    {
                        Address = 0,
                        Name = scanner.CurrentToken.Value,
                        FlagTypeDefinition = FlagTypeDefinition.Register,
                        ChildSymbolTable = new HashSet<SymbolBase>()
                    });

                    if (SymbolTable.Any(p => p.Name == scanner.CurrentToken.Value))
                    {
                        scanner.LogErrorToken(new Token(TokenCategory.Identifier, TokenTypeDefinition.TK_MULTIPLE_DECLARATIONS, ""));
                    }

                    scanner.Match(TokenTypeDefinition.TK_ID);
                    if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_COMMA)
                    {
                        scanner.Match(TokenTypeDefinition.TK_COMMA);
                    }
                }

                scanner.Match(TokenTypeDefinition.TK_COLON);

                int size = 0;
                DataTypeDefinition currentDataType = DataTypeDefinition.TYPE_VOID;

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

                //compute addresses amd imitialize from stack
                foreach (SymbolVariable t in variableTokens)
                {
                    t.Size = size;
                    t.Address = dp;
                    t.DataTypeDefinition = currentDataType;
                    dp = dp + size;
                    SymbolTable.Add(t);

                    switch (size)
                    {
                        case 1:
                            GenerateOperation(OperationTypeDefinition.op_store);
                            gen4(t.Address);
                            break;
                        case 4:
                            GenerateOperation(OperationTypeDefinition.op_storei);
                            gen4(t.Address);
                            break;
                        case 8:
                            GenerateOperation(OperationTypeDefinition.op_storef);
                            gen4(t.Address);
                            break;
                    }

                    symbolProcedure.Arguments.Add(new ProcedureArgument
                    {
                        DataType = currentDataType,
                        Name = t.Name
                    });

                }
                
            }

        }




        //ARITHMETIC HEADACHE

        public TokenTypeValue M()
        {
            TokenTypeValue t1 = F();

            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_EQUAL ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_NOTEQ ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GTEQ ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_GREATER ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LESS ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_LTEQ)
            {
                TokenTypeDefinition operation = scanner.CurrentToken.TokenTypeDefinition;
                scanner.Match(operation);

                TokenTypeValue t2 = F();

                t1 = Generate(t1, t2, operation);

            }

            return t1;
        }


        public TokenTypeValue F()
        {
            TokenTypeValue tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_VOID, null);
            switch (scanner.CurrentToken.TokenTypeDefinition)
            {
                case TokenTypeDefinition.TK_ID:
                    {
                        SymbolBase searchedSymVar = SymbolTable.FirstOrDefault(p => p.Name == scanner.CurrentToken.Value);
                        if (searchedSymVar != null)
                        {
                            //get the type of variable
                            if (searchedSymVar.GetType() == typeof(SymbolVariable))
                            {
                                SymbolVariable symVar = (SymbolVariable) searchedSymVar;
                                tokenFound = new TokenTypeValue(symVar.DataTypeDefinition, symVar.Address);
                                tokenFound.isAddress = true;
                                switch (symVar.DataTypeDefinition)
                                {
                                    case DataTypeDefinition.TYPE_BOOL:
                                    case DataTypeDefinition.TYPE_CHAR:
                                    {
                                        GenerateOperation(OperationTypeDefinition.op_fetch); //does a push of the value onto the stack
                                        gen4(symVar.Address);
                                        break;
                                    }
                                    case DataTypeDefinition.TYPE_INT:
                                    {
                                        GenerateOperation(OperationTypeDefinition.op_fetchi);
                                        gen4(symVar.Address);
                                        break;
                                    }
                                    case DataTypeDefinition.TYPE_FLOAT:
                                    {
                                        GenerateOperation(OperationTypeDefinition.op_fetchf);
                                        gen4(symVar.Address);
                                        break;
                                    }
                                }
                            }
                            else if (searchedSymVar.GetType() == typeof(SymbolProcedure))
                            {
                                //store where to return to in the stack before the parameters
                                GenerateOperation(OperationTypeDefinition.op_pushi);
                                int hole = ip;
                                gen4(0);

                                int count = 0;
                                SymbolProcedure symVar = (SymbolProcedure)searchedSymVar;
                                scanner.Match(TokenTypeDefinition.TK_ID);
                                scanner.Match(TokenTypeDefinition.TK_LBRACE);

                                while (scanner.CurrentToken.TokenTypeDefinition != TokenTypeDefinition.TK_RBRACE && scanner.ErrorLog.Count == 0)
                                {
                                    count++;
                                    switch (scanner.CurrentToken.TokenTypeDefinition)
                                    {
                                        case TokenTypeDefinition.TK_BOOLLIT:
                                        case TokenTypeDefinition.TK_CHARLIT:
                                        {
                                            GenerateOperation(OperationTypeDefinition.op_push); //does a push of the value onto the stack
                                            gen1(scanner.CurrentToken.Value[0]);
                                            break;
                                        }
                                        case TokenTypeDefinition.TK_INTLIT:
                                        {
                                            GenerateOperation(OperationTypeDefinition.op_pushi);
                                            gen4(Convert.ToInt32(scanner.CurrentToken.Value));
                                            break;
                                        }
                                        case TokenTypeDefinition.TK_REALLIT:
                                        {
                                            GenerateOperation(OperationTypeDefinition.op_pushf);
                                            gen8(Convert.ToSingle(scanner.CurrentToken.Value));
                                            break;
                                        }
                                    }
                                    scanner.Match(scanner.CurrentToken.TokenTypeDefinition);
                                    if (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_COMMA)
                                        scanner.Match(scanner.CurrentToken.TokenTypeDefinition);

                                }
                                gen_Address(ip, hole); //push where to return to the bottom of the parameters
                                scanner.Match(TokenTypeDefinition.TK_SEMI);

                                if (count != symVar.Arguments.Count)
                                {
                                    scanner.LogErrorToken(new Token(TokenCategory.Literal, TokenTypeDefinition.TK_INVALID_NUMBER_OF_ARGUMENTS, ""));
                                }

                            }
                        }
                        break;
                    }
                case TokenTypeDefinition.TK_INTLIT:
                    {
                        GenerateOperation(OperationTypeDefinition.op_pushi);
                        gen4(Convert.ToInt32(scanner.CurrentToken.Value));
                        tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_INT, Convert.ToInt32(scanner.CurrentToken.Value));

                        break;
                    }
                case TokenTypeDefinition.TK_REALLIT:
                    {
                        GenerateOperation(OperationTypeDefinition.op_pushf);
                        gen8(Convert.ToInt32(scanner.CurrentToken.Value));
                        tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_FLOAT, Convert.ToSingle(scanner.CurrentToken.Value));
                        break;
                    }
                case TokenTypeDefinition.TK_BOOLLIT:
                    {
                        GenerateOperation(OperationTypeDefinition.op_push);
                        gen1(Convert.ToBoolean(scanner.CurrentToken.Value) ? '1' : '0');
                        tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_BOOL, Convert.ToBoolean(scanner.CurrentToken.Value));
                        break;
                    }
                case TokenTypeDefinition.TK_CHARLIT:
                    {
                        GenerateOperation(OperationTypeDefinition.op_push);
                        gen1(scanner.CurrentToken.Value[0]);
                        tokenFound = new TokenTypeValue(DataTypeDefinition.TYPE_CHAR, scanner.CurrentToken.Value);
                        break;
                    }
                
            }
            scanner.Advance();
            return tokenFound;
        }

        public void Condition()
        {
            TokenTypeValue t = E();
            if(t.DataType != DataTypeDefinition.TYPE_BOOL)
                scanner.LogErrorToken(new Token(TokenCategory.Symbol, TokenTypeDefinition.TK_REPEAT, ""));
        }

        public TokenTypeValue T()
        {
            TokenTypeValue t1 = M();

            while (scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_SLASH ||
                   scanner.CurrentToken.TokenTypeDefinition == TokenTypeDefinition.TK_STAR)
            {
                TokenTypeDefinition operation = scanner.CurrentToken.TokenTypeDefinition;
                scanner.Match(operation);

                TokenTypeValue t2 = M();

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

            #region plus
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
            #endregion

            #region minus
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
            #endregion

            #region star_multiply
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
            #endregion

            #region divide_slash
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
            #endregion



            #region relational operators
            if (operation == TokenTypeDefinition.TK_EQUAL ||
                   operation == TokenTypeDefinition.TK_NOTEQ ||
                   operation == TokenTypeDefinition.TK_GTEQ ||
                   operation == TokenTypeDefinition.TK_GREATER ||
                   operation == TokenTypeDefinition.TK_LESS ||
                   operation == TokenTypeDefinition.TK_LTEQ)
            {
                tokenFound.DataType = DataTypeDefinition.TYPE_BOOL;

                if (t1.DataType != t2.DataType)
                {
                    if (t1.DataType == DataTypeDefinition.TYPE_FLOAT && t2.DataType == DataTypeDefinition.TYPE_INT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                    }
                    else if (t1.DataType == DataTypeDefinition.TYPE_INT && t2.DataType == DataTypeDefinition.TYPE_FLOAT)
                    {
                        GenerateOperation(OperationTypeDefinition.op_exch);
                        GenerateOperation(OperationTypeDefinition.op_fcon);
                        GenerateOperation(OperationTypeDefinition.op_exch);
                    }
                    else
                    {
                        scanner.LogErrorToken(new Token(TokenCategory.Identifier, TokenTypeDefinition.TK_DATATYPEMISMATCH, ""));
                    }
                }

                switch (operation)
                {
                    case TokenTypeDefinition.TK_EQUAL:
                    {
                        GenerateOperation(OperationTypeDefinition.op_eql);
                        break;
                    }
                    case TokenTypeDefinition.TK_NOTEQ:
                    {
                        GenerateOperation(OperationTypeDefinition.op_neq);
                        break;
                    }
                    case TokenTypeDefinition.TK_GTEQ:
                    {
                        GenerateOperation(OperationTypeDefinition.op_geq);
                        break;
                    }
                    case TokenTypeDefinition.TK_GREATER:
                    {
                        GenerateOperation(OperationTypeDefinition.op_gtr);
                        break;
                    }
                    case TokenTypeDefinition.TK_LESS:
                    {
                        GenerateOperation(OperationTypeDefinition.op_lss);
                        break;
                    }
                    case TokenTypeDefinition.TK_LTEQ:
                    {
                        GenerateOperation(OperationTypeDefinition.op_leq);
                        break;
                    }
                }

            }
            #endregion

            return tokenFound;
        }


        public void GenerateOperation(OperationTypeDefinition op)
        {
            if (ip < MAX_ARRAY)
            {
                code[ip] = Convert.ToChar(Convert.ToByte((int)op));
                ip++;
            }
            else
            {
                //printf("\nRun time error: Out of range Code\n");
                //exit(EXIT_FAILURE);
            }
        }


        public void gen1(char val)
        {
            code[ip] = val;
            ip += 1;
        }

        public void gen4(int val)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                code[ip++] = (char)b;
            }
        }



        public void gen8(double val)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                code[ip++] = (char)b;
            }
        }

        public void gen_Address(int val, int target)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                code[target++] = (char)b;
            }
           
        }

        public void save4(int val, int pos)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                data[pos++] = (char)b;
            }
        }

        public void save1(int val, int pos)
        {
            data[pos] = (char)val;
            
        }

        public void save8(double val, int pos)
        {
            var bytes = BitConverter.GetBytes(val);
            foreach (var b in bytes)
            {
                data[pos++] = (char)b;
            }
        }




    }
}