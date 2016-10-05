using System;
using System.Runtime.Serialization;

namespace CompilersFinalProject.Compiler.Scanning
{
    [Serializable]
    internal class CompilationException : Exception
    {
        private string v;
        private object _currentLine;

        public CompilationException()
        {
        }

        public CompilationException(string message) : base(message)
        {
        }

        public CompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CompilationException(string v, object _currentLine)
        {
            this.v = v;
            this._currentLine = _currentLine;
        }

        protected CompilationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}