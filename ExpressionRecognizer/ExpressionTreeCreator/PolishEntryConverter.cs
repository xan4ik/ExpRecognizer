using System.Linq;
using System.Collections.Generic;
using ExpRecognizer.Tokenizer;
using ExpRecognizer.EvaluationContext;

namespace ExpRecognizer.ExpressionTreeCreator
{
    public class PolishEntryConverter
    {
        public IEnumerable<Token> TryConvert(IEnumerable<Token> tokens, Context context)
        {
            var count = tokens.Count();
            if(count == 0)  
                return Enumerable.Empty<Token>();
            
            var result = new List<Token>(count);
            var operationsStack = new Stack<Token>(count);
              
            Process(tokens, operationsStack, result, context);
            Finish(operationsStack, result);

            return result;
        }

        private void Process(IEnumerable<Token> tokens, Stack<Token> stack, List<Token> result, Context context)
        {
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case (TokenType.Number):
                    case (TokenType.Variable):
                        {
                            result.Add(token);
                            break;
                        }
                    case (TokenType.Function):
                    case (TokenType.OpenBracket):
                        {
                            stack.Push(token);
                            break;
                        }
                    case (TokenType.CloseBracket):
                        {
                            ProcessCloseBracket(stack, result);
                            break;
                        }
                    case (TokenType.Operator):
                        {
                            ProcessOperation(token, stack, result, context);
                            break;
                        }
                }
            }
        }

        private IEnumerable<Token> Finish(Stack<Token> stack, List<Token> result)
        {
            while (stack.Any())
            {
                yield return stack.Pop();
            }
        }

        private void ProcessCloseBracket(Stack<Token> stack,List<Token> result)
        {
            while (stack.Any() && stack.Peek().Type != TokenType.OpenBracket)
            {
                result.Add(stack.Pop());
            }
            stack.Pop(); // pop '('
        }

        private void ProcessOperation(Token operation, Stack<Token> stack, List<Token> result, Context context)
        {
            while (stack.Any())
            {
                var top = stack.Peek();
                if(top.Type == TokenType.OpenBracket || !IsPartOfCurrentOperation(operation, top, context))
                    break;
                
                result.Add(stack.Pop());
            }

            stack.Push(operation);
        }

        private bool IsPartOfCurrentOperation(Token operation, Token token, Context context)
        {
            var left = context.GetFunction(operation.Value).Profile;
            var right = context.GetFunction(token.Value).Profile;


            var compareResult = left.CompareTo(right);
            var isEqualAndLeftSided = compareResult == 0 && right.LeftSided;

            return compareResult < 0 || isEqualAndLeftSided;
        }
    }
}