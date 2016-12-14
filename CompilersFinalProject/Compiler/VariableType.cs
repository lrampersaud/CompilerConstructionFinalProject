namespace CompilersFinalProject.Compiler
{
    public struct VariableType
    {
        int var; //TK_A_VAR, TK_AN_ARRAY, TK_A_LABEL
        int ty; //TYPE_INT, TYPE_CHAR, TYPE_FLOAT
        int high; //used when var == TK_AN_ARRAY else NULL
        int addr; //Address in data[] for TK_A_VAR, TK_ALABEL and address for the first element of the array in the case of TK_AN_ARRAY
    }
}