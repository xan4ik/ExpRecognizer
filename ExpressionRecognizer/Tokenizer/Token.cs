namespace ExpRecognizer.Tokenizer
{
    public enum TokenType
    {
        Number,
        Variable,
        Function, 
        OpenBracket,
        CloseBracket, 
        Operator, 
        Comma, 
        Point, 
        WhiteSpace, 
        None
    }

    public struct Token
    {
        public readonly TokenType Type;
        public readonly string Value;

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}