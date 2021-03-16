using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpRecognizer
{
    class Program
    {
        static void Main(string[] args)
        {

            //2+2
            //2+2+2
            //string exp = "(2*sin(x)+4)/fun(2)*(4+2)+245*x";

            var result = (new Tokenizer()).Split("2+2 -4 * 7 + (6+7 *8 / (3+4))^2");

            //foreach (var item in result) //"2+2.2^2- asdf+sin((2 +3),a, b, 3)-456 * 7 + (6+7821 *8 / (3+4))^2"))
            //{
            //    Console.WriteLine(item.Value + " " + item.Type.ToString());
            //}

            Console.WriteLine((new ContextParser()).Parse(result));

            //выделить блоки, разбить, парситьы
            //парсить выражения в скобках
        }
    }

    public struct Operation : IComparable<Operation>
    {
        public readonly string Type;
        public readonly int Priority;
        public readonly bool LeftSided;

        public Operation(string operation, int priority, bool leftsided = true)
        {
            Type = operation;
            Priority = priority;
            LeftSided = leftsided;
        }

        public bool Is(string type)
        {
            return type == Type;
        }

        public int CompareTo(Operation other)
        {
            return this.Priority - other.Priority;
        }
    }



    public enum TokenType { Number, Variable, Function, OpenBracket, CloseBracket, Operator, Comma, Point, WhiteSpace, None };

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

    public class Tokenizer
    {
        private StringBuilder buffer;

        public Tokenizer()
        {
            buffer = new StringBuilder();
        }

        public IEnumerable<Token> Split(string expression)
        {
            buffer.Clear();
            TokenType next = TokenType.None, prev = TokenType.None;

            foreach (var item in expression)
            {
                prev = next;
                next = DetermineType(item);
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

        private TokenType DetermineType(char ch)
        {
            if (char.IsLetter(ch))
                return TokenType.Variable;
            if (char.IsDigit(ch))
                return TokenType.Number;
            if (char.IsWhiteSpace(ch))
                return TokenType.WhiteSpace;
            if (ch == ',')
                return TokenType.Comma;
            if (ch == '.')
                return TokenType.Point;
            if (ch == '(')
                return TokenType.OpenBracket;
            if (ch == ')')
                return TokenType.CloseBracket;
            if (ContextParser.ContainsOperation(ch.ToString()))
                return TokenType.Operator;

            throw new Exception("Wrong character");
        }
    }




    public class ContextParser
    {
        private Stack<string> operationsStack;
        private static Dictionary<string, Operation> operations;
        private StringBuilder output;
        private StringBuilder buffer;

        static ContextParser()
        {
            operations = new Dictionary<string, Operation>()
            {
                {"+", new Operation("+", 2) },
                {"-", new Operation("-", 2) },
                {"/", new Operation("/", 4) },
                {"*", new Operation("*", 4) },
                {"^", new Operation("^", 6, false) }
            };
        }

        public ContextParser()
        {
            operationsStack = new Stack<string>();
            output = new StringBuilder();
            buffer = new StringBuilder();
        }

        public static bool ContainsOperation(string operation)
        {
            return operations.ContainsKey(operation);
        }

        public string Parse(IEnumerable<Token> tokens)
        {
            Reset();
            Process(tokens);
            Finish();
            return output.ToString();
        }

        private void Reset()
        {
            operationsStack.Clear();
            output.Clear();
        }

        private void Process(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case (TokenType.Number):
                    case (TokenType.Variable):
                        {
                            ToOutput(token.Value);
                            break;
                        }
                    case (TokenType.Function):
                    case (TokenType.OpenBracket):
                        {
                            operationsStack.Push(token.Value);
                            break;
                        }
                    case (TokenType.CloseBracket):
                        {
                            HandleCloseBracket();
                            break;
                        }
                    case (TokenType.Operator):
                        {
                            HandleOperation(token.Value);
                            break;
                        }
                    case (TokenType.Comma):
                        { // TODO
                            break;
                        }
                }
            }
        }

        private void Finish()
        {
            while (IsOperationExist(operationsStack))
            {
                ToOutput(operationsStack.Pop());
            }
        }

        private void HandleCloseBracket()
        {
            while (operationsStack.Peek() != "(") 
            {
                ToOutput(operationsStack.Pop());
            }
            operationsStack.Pop();
        }

        private void HandleOperation(string operation)
        {
            if (!IsOperationExist(operationsStack))
            {
                operationsStack.Push(operation);
                return;
            }
            if(operationsStack.Peek() == "(") 
            {

                operationsStack.Push(operation);
                return;
            }
            while (IsOperationExist(operationsStack) && (operations[operation].CompareTo(operations[operationsStack.Peek()]) < 0 ||
                    operations[operation].CompareTo(operations[operationsStack.Peek()]) == 0 && operations[operation].LeftSided))
            {
                output.Append(operationsStack.Pop());
                output.Append(' ');
            }

            operationsStack.Push(operation);
        }

        private bool IsOperationExist(Stack<string> operationsStack)
        {
            return operationsStack.Count != 0;
        }

        private void ToOutput(string value)
        {
            output.Append(value);
            output.Append(' ');
        }
    }
}