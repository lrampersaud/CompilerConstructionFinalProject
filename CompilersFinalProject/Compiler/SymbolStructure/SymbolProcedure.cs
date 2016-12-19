using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompilersFinalProject.Compiler.Scanning;

namespace CompilersFinalProject.Compiler.SymbolStructure
{
    public class SymbolProcedure: SymbolBase
    {
        public bool Seen { get; set; }
        public List<ProcedureArgument> Arguments { get; set; }
    }
}
