using System.Collections.Generic;

namespace CompilersFinalProject.Compiler.SymbolStructure
{
    public class SymbolBase
    {
        public string Name { get; set; }
        public TokenTypeDefinition Token { get; set; }
        public int Address { get; set; }
        public HashSet<SymbolBase> ChildSymbolTable { get; set; }
    }
}