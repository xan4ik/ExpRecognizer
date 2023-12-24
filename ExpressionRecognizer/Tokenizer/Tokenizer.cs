using System.Text;
using System.Collections.Generic;
using ExpRecognizer.EvaluationContext;
using ExpRecognizer.Exceptions;

namespace ExpRecognizer.Tokenizer
{
    public class Tokenizer
    {
        public IEnumerable<Token> Tokenize(string expression, Context context)
        {
            var buffer = new StringBuilder(expression.Length);
            TokenType next = TokenType.None, prev = TokenType.None;

            foreach (var item in expression)
            {
                prev = next;
                next = DetermineType(item, context);

                if (next == TokenType.OpenBracket && prev == TokenType.OpenBracket ||
                    next == TokenType.CloseBracket && prev == TokenType.CloseBracket)
                {
                    yield return new Token(prev, buffer.ToString());
                    buffer.Clear();
                }
                else if (next != prev)
                {
                    if (next == TokenType.OpenBracket && prev == TokenType.Variable)
                    {
                        yield return new Token(TokenType.Function, buffer.ToString());
                        buffer.Clear();
                    }
                    else if (next == TokenType.Point && prev == TokenType.Number)
                    {
                        next = TokenType.Number;
                    }
                    else
                    {
                        yield return new Token(prev, buffer.ToString());
                        buffer.Clear();
                    }
                }
                buffer.Append(item);
            }
            if (buffer.Length != 0)
            {
                yield return new Token(next, buffer.ToString());
            }
        }

        private TokenType DetermineType(char element, Context context)
        {
            if (char.IsLetter(element))
                return TokenType.Variable;
            if (char.IsDigit(element))
                return TokenType.Number;
            if (char.IsWhiteSpace(element))
                return TokenType.WhiteSpace;
            if (element == ',')
                return TokenType.Comma;
            if (element == '.')
                return TokenType.Point;
            if (element == '(')
                return TokenType.OpenBracket;
            if (element == ')')
                return TokenType.CloseBracket;
            if (context.ContainsOperation(element.ToString()))
                return TokenType.Operator;

            throw new BadSyntaxException($"Unknown character {element}");
        }
    }


}