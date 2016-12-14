namespace CompilersFinalProject.Compiler.SymbolStructure
{
    public class SymbolVariable: SymbolBase
    {
        public DataTypeDefinition DataTypeDefinition { get; set; }
        public int Size { get; set; }
        public FlagTypeDefinition FlagTypeDefinition { get; set; }
    }
}