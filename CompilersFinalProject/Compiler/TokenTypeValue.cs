namespace CompilersFinalProject.Compiler
{
    public class TokenTypeValue
    {
        public object Value { get; set; }
        public DataTypeDefinition DataType { get; set; }
        public bool isAddress { get; set; }

        public TokenTypeValue(DataTypeDefinition dataTypeDefinition, object value)
        {
            DataType = dataTypeDefinition;
            Value = value;
            isAddress = false;
        }
    }
}