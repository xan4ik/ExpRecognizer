using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static ConsoleApp6.ExpressionTreeCreator;

namespace ConsoleApp6
{
    class Program
    {
        static void Main(string[] args)
        {

            //2+2
            //2+2+2
            //string exp = "(2*sin(x)+4)/fun(2)*(4+2)+245*x";

            //"sin(max(2, 3 ) / 3 * Pi)"
            //"2+2 -4 * 7 + (6+7 *8 / (3+4))^2"
            //"2 + 5* PI * (sin(3+4* (1 -3))) /2^max(3,4)"

            var result = (new Tokenizer()).Split("2+2 -4 * 7 + (6+7 *8 / (3+4))^2");

            //foreach (var item in result) //"2+2.2^2- asdf+sin((2 +3),a, b, 3)-456 * 7 + (6+7821 *8 / (3+4))^2"))
            //{
            //    Console.WriteLine(item.Value + " " + item.Type.ToString());
            //}
            //Console.WriteLine();

            var tokens = (new TokenToQueryConverter()).Parse(result);
            foreach (var item in tokens)
            {
                Console.WriteLine(item.Value + " " + item.Type.ToString());
            }

            var exp = (new ExpressionTreeCreator()).CreateTree(tokens);
            Console.WriteLine(exp.Calculate());
            //выделить блоки, разбить, парситьы
            //парсить выражения в скобках
        }
    }

    public struct FunctionProfile : IComparable<FunctionProfile>
    {
        public readonly string Type;
        public readonly int Priority;
        public readonly bool LeftSided;
        public readonly int ArgumentCount;

        public FunctionProfile(string operation, int priority, int argc, bool leftsided = true)
        {
            Type = operation;
            Priority = priority;
            LeftSided = leftsided;
            ArgumentCount = argc;
        }

        public bool Is(string type)
        {
            return type == Type;
        }

        public int CompareTo(FunctionProfile other)
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

        private TokenType DetermineType(char element)
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
            if (Context.ContainsOperation(element.ToString()))
                return TokenType.Operator;

            throw new Exception("Wrong character");
        }
    }
    public class TokenToQueryConverter
    {
        private Stack<Token> operationsStack;
        private StringBuilder output;
        private StringBuilder buffer;


        public TokenToQueryConverter()
        {
            operationsStack = new Stack<Token>();
            output = new StringBuilder();
            buffer = new StringBuilder();
        }

        public IEnumerable<Token> Parse(IEnumerable<Token> tokens)
        {
            Reset();
            foreach (var item in Process(tokens))
            {
                yield return item;
            }
            foreach (var item in Finish())
            {
                yield return item;
            }
            //return output.ToString();
        }

        private void Reset()
        {
            operationsStack.Clear();
            output.Clear();
        }

        private IEnumerable<Token> Process(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case (TokenType.Number):
                    case (TokenType.Variable):
                        {
                            yield return token;
                            break;
                        }
                    case (TokenType.Function):
                    case (TokenType.OpenBracket):
                        {
                            operationsStack.Push(token);
                            break;
                        }
                    case (TokenType.CloseBracket):
                        {
                            foreach (var item in HandleCloseBracket())
                            {
                                yield return item;
                            }
                            break;
                        }
                    case (TokenType.Operator):
                        {
                            foreach (var item in HandleOperation(token))
                            {
                                yield return item;
                            }
                            break;
                        }
                }
            }
        }

        private IEnumerable<Token> Finish()
        {
            while (IsOperationExist(operationsStack))
            {
                yield return operationsStack.Pop();
            }
        }

        private IEnumerable<Token> HandleCloseBracket()
        {
            while (IsOperationExist(operationsStack) && operationsStack.Peek().Type != TokenType.OpenBracket)
            {
                yield return operationsStack.Pop();
            }
            operationsStack.Pop();
        }

        private IEnumerable<Token> HandleOperation(Token operation)
        {
            while (IsOperationExist(operationsStack) &&
                    (operationsStack.Peek().Type != TokenType.OpenBracket) &&
                    (Context.CompareOperations(operation.Value, operationsStack.Peek().Value) < 0 ||
                    (Context.CompareOperations(operation.Value, operationsStack.Peek().Value)) == 0 && Context.IsOperationLeftSided(operationsStack.Peek().Value)))
            {
                yield return operationsStack.Pop();
            }

            operationsStack.Push(operation);
        }

        private bool IsOperationExist(Stack<Token> operationsStack)
        {
            return operationsStack.Count != 0;
        }

        private void ToOutput(string value)
        {
            output.Append(value);
            output.Append(' ');
        }
    }



    public static class Context
    {
        private static Dictionary<string, FunctionProfile> operations;
        private static Dictionary<string, float> variables;
        private static Dictionary<string, FunctionExpression> functions;

        static Context()
        { //TODO - move argc to function

            functions = new Dictionary<string, FunctionExpression>()
            {
                {"+", new SumOperator() },
                {"-", new MinusOperator() },
                {"^", new PowOperator() },
                {"*", new MultiplyOperator() },
                {"/", new DevideOperator() },
                {"sin", new SinExpression() },
                {"max", new MaxExpression() },
            };

            operations = new Dictionary<string, FunctionProfile>()
            {
                {"+", new FunctionProfile("+", 2, 2) },
                {"-", new FunctionProfile("-", 2, 2) },
                {"/", new FunctionProfile("/", 4, 2) },
                {"*", new FunctionProfile("*", 4, 2) },
                {"^", new FunctionProfile("^", 6, 2, false) },
                {"sin", new FunctionProfile("sin", 10, 1, false) },
                {"max", new FunctionProfile("max", 10, 2, false) }
            };

            variables = new Dictionary<string, float>()
            {
                {"Pi", (float)Math.PI }
            };
        }

        public static FunctionExpression GetFunction(string name) 
        {
            return functions[name].Clone();   
        }

        public static int CompareOperations(string left, string right)
        {
            return operations[left].CompareTo(operations[right]);
        }

        public static bool ContainsOperation(string operation)
        {
            return operations.ContainsKey(operation);
        }

        public static bool IsOperationLeftSided(string operation)
        {
            return operations[operation].LeftSided;
        }

        public static int GetAgrumentsCount(string operation)
        {
            return operations[operation].ArgumentCount;
        }

        public static float GetVariableValue(string name)
        {
            return variables[name];
        }

        public static void AddVariable(string name, float value)
        {
            variables.Add(name, value);
        }

        public static void RemoveVariable(string name)
        {
            variables.Remove(name);
        }
    }

    public class ExpressionTreeCreator
    {
        private Stack<Expression> buffer;

        public ExpressionTreeCreator()
        {
            buffer = new Stack<Expression>();
        }

        public Expression CreateTree(IEnumerable<Token> polishEntry)
        {
            foreach (var item in polishEntry)
            {
                switch (item.Type)
                {
                    case (TokenType.Number):
                        buffer.Push(new NumberExpression(item.Value));
                        break;
                    case (TokenType.Variable):
                        buffer.Push(new VariableExpression(item.Value));
                        break;
                    case (TokenType.Operator): 
                    case (TokenType.Function):
                        {
                            var function = Context.GetFunction(item.Value);
                            for (int i = Context.GetAgrumentsCount(item.Value) - 1; i >= 0; i--)
                            {
                                function.SetArgument(i, buffer.Pop());
                            }
                            buffer.Push(function);
                            break;
                        }
                  
                }
            }
            return buffer.Pop();
        }


        public abstract class Expression
        {
            public abstract float Calculate();
        }

        public abstract class OperationExpression : Expression
        {
            protected readonly Expression left;
            protected readonly Expression right;

            public OperationExpression(Expression left, Expression right)
            {
                this.left = left;
                this.right = right;
            }

            public abstract OperationExpression CreateSameOperator(Expression left, Expression right);
        }


        public class PowOperator : FunctionExpression
        {
            public PowOperator() : base(2)
            { }

            public override float Calculate()
            {
                return (float)Math.Pow(GetArgument(0), GetArgument(1));
            }

            public override FunctionExpression Clone()
            {
                return new PowOperator();
            }
        }


        public class MultiplyOperator : FunctionExpression
        {
            public MultiplyOperator() : base(2)
            { }

            public override float Calculate()
            {
                return GetArgument(0) * GetArgument(1);
            }

            public override FunctionExpression Clone()
            {
                return new MultiplyOperator();
            }
        }

        public class DevideOperator : FunctionExpression
        {
            public DevideOperator() : base(2)
            { }

            public override float Calculate()
            {
                return GetArgument(0) / GetArgument(1);
            }

            public override FunctionExpression Clone()
            {
                return new DevideOperator();
            }
        }


        public class SumOperator : FunctionExpression
        {
            public SumOperator() : base(2)
            { }

            public override float Calculate()
            {
                return GetArgument(0) + GetArgument(1);
            }

            public override FunctionExpression Clone()
            {
                return new SumOperator();
            }
        }

        public class MinusOperator : FunctionExpression
        {
            public MinusOperator() : base(2)
            { }

            public override float Calculate()
            {
                return GetArgument(0) - GetArgument(1);
            }

            public override FunctionExpression Clone()
            {
                return new MinusOperator();
            }
        }




        public sealed class VariableExpression : Expression
        {
            private string variableName;
            public VariableExpression(string variableName)
            {
                this.variableName = variableName;
            }

            public override float Calculate()
            {
                return Context.GetVariableValue(variableName);
            }
        }
        public sealed class NumberExpression : Expression
        {
            private float value;

            public NumberExpression(float number)
            {
                value = number;
            }

            public NumberExpression(string number)
            {
                if (number.Contains('.'))
                {
                    number = number.Replace('.', ',');
                }
                value = float.Parse(number);
            }

            public override float Calculate()
            {
                return value;
            }
        }


        public abstract class FunctionExpression : Expression
        {
            private Expression[] arguments;

            protected FunctionExpression(Expression[] arguments)
            {
                this.arguments = arguments;
            }

            protected FunctionExpression(int argc)
            {
                this.arguments = new Expression[argc];
            }

            public void SetArgument(int index, Expression value)
            {
                arguments[index] = value;
            }

            public float GetArgument(int index)
            {
                return arguments[index].Calculate();
            }

            public abstract FunctionExpression Clone();
        }

        public sealed class SinExpression : FunctionExpression
        {
            public SinExpression(Expression expression) : base(1)
            {
                SetArgument(0, expression);
            }

            public SinExpression() : base(1)
            { }

            public override float Calculate()
            {
                return (float)Math.Sin(GetArgument(0));
            }

            public override FunctionExpression Clone()
            {
                return new SinExpression();
            }
        }

        public sealed class MaxExpression : FunctionExpression
        {
            public MaxExpression(Expression left, Expression right) : base(2)
            {
                SetArgument(0, left);
                SetArgument(1, right);
            }

            public MaxExpression() : base(2)
            { }

            public override float Calculate()
            {
                return Math.Max(GetArgument(0), GetArgument(1));
            }

            public override FunctionExpression Clone()
            {
                return new MaxExpression();
            }
        }

    }
}