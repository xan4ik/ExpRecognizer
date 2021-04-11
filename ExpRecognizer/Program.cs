using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
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
            try
            {
                var tokens = (new PolishEntryConverter()).TryConvert(result);
                foreach (var item in tokens)
                {
                    Console.WriteLine(item.Value + " " + item.Type.ToString());
                }

                var exp = (new ExpressionTreeCreator()).CreateTree(tokens);
                Console.WriteLine(exp.Calculate());
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message) ;
            }

            
            //выделить блоки, разбить, парситьы
            //парсить выражения в скобках
        }
    }

    public struct FunctionProfile : IComparable<FunctionProfile>
    {
        public readonly string FunctionName;
        public readonly int Priority;
        public readonly bool LeftSided;
        public readonly int ArgumentCount;

        public FunctionProfile(string operation, int priority, int argc, bool leftsided = true)
        {
            FunctionName = operation;
            Priority = priority;
            LeftSided = leftsided;
            ArgumentCount = argc;
        }

        public bool Is(string type)
        {
            return type == FunctionName;
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
    public class PolishEntryConverter
    {
        private Stack<Token> operationsStack;

        public PolishEntryConverter()
        {
            operationsStack = new Stack<Token>();
        }


        public IEnumerable<Token> TryConvert(IEnumerable<Token> tokens)
        {
            Reset();
            return Process(tokens).Concat<Token>(Finish());
        }

        private void Reset()
        {
            operationsStack.Clear();
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
                            foreach (var item in ProcessCloseBracket())
                            {
                                yield return item;
                            }
                            break;
                        }
                    case (TokenType.Operator):
                        {
                            foreach (var item in ProcessOperation(token))
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

        private IEnumerable<Token> ProcessCloseBracket()
        {
            while (IsOperationExist(operationsStack) && operationsStack.Peek().Type != TokenType.OpenBracket)
            {
                yield return operationsStack.Pop();
            }
            operationsStack.Pop();    
        }

        private IEnumerable<Token> ProcessOperation(Token operation)
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
    }

    public class BadSyntaxException : Exception
    {
        public BadSyntaxException(string message) : base(message)
        { }

        public BadSyntaxException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public class ObjectNotExistException : Exception 
    {
        public ObjectNotExistException(string message) : base(message)
        { }
    }

    public class ObjectAlreadyExistException : Exception
    {
        public ObjectAlreadyExistException(string message) : base(message)
        { }
    }

    public class TreeCreateException : Exception 
    {
        public TreeCreateException(string message, Exception exception) : base(message, exception)
        { }
    }

    public class CalculationException : Exception 
    {
        public CalculationException(string message) : base(message)
        { }


        public CalculationException(string message, Exception exception) : base(message, exception)
        { }
    }

    public class Variable 
    {
        public readonly string Name;
        
        private bool isReadOnly;
        private float value;

        public Variable(string name, float value, bool readOnly = false)
        {
            this.value = value;
            this.isReadOnly = readOnly;

            Name = name;
        }

        public bool IsConst 
        {
            get 
            {
                return isReadOnly;
            }
        }

        public float GetValue() 
        {
            return value;
        }

        public void SetValue(float value) 
        {
            if (isReadOnly) 
            {
                throw new CalculationException("Can't change const value");            
            }

            this.value = value;
        }
    }

    public static class Context
    {
        private static Dictionary<string, Variable> variables;
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
                {"max", new MaxExpression() }
            };

            variables = new Dictionary<string, Variable>()
            {
                {"Pi", new Variable("Pi", (float)(22/7f), true) }
            };
        }

        public static FunctionExpression CreateFunction(string name)
        {
            if (functions.ContainsKey(name))
            {
                return functions[name].Clone();
            }
            throw new ObjectNotExistException(String.Format("Function of {0} not exist", name));
            
        }

        public static int CompareOperations(string left, string right)
        {
            try
            {
                var _leftFunction = functions[left].Profile;
                var _rightFunction = functions[right].Profile;

                return _leftFunction.CompareTo(_rightFunction);
            }
            catch (KeyNotFoundException)
            {
                throw new ObjectNotExistException(String.Format("One of function {0}\\{1} not exist", left, right));
            }
        }

        public static bool ContainsOperation(string operation)
        {
            return functions.ContainsKey(operation);
        }

        public static bool ContainsVariable(string variable)
        {
            return variables.ContainsKey(variable);
        }


        // TODO: remove this
        public static bool IsOperationLeftSided(string operation)
        {
            if (functions.ContainsKey(operation))
            {
                return functions[operation].Profile.LeftSided;
            }
            throw new ObjectNotExistException(String.Format("Function of {0} not exist", operation));
        }

        public static Variable GetVariable(string name)
        {    
            if (variables.ContainsKey(name))
            {
                return variables[name];
            }
            throw new ObjectNotExistException(String.Format("Variable of {0} not exist", name));
        } 

        public static void AddVariable(string name, Variable value)
        {
            if (!variables.ContainsKey(name))
            {
                variables.Add(name, value);
            }
            else throw new ObjectAlreadyExistException(String.Format("Variable of {0} not exist", name));   
        }

        public static void RemoveVariable(string name)
        {
            if (variables.ContainsKey(name))
            {
                variables.Remove(name);
            }
            else throw new ObjectAlreadyExistException(String.Format("Variable of {0} not exist", name));
        }

        public static void AddFunction(FunctionExpression expression) 
        {
            var profile = expression.Profile.FunctionName;
            if (!functions.ContainsKey(profile))
            {
                functions.Add(profile, expression);
            }
            else throw new ObjectAlreadyExistException(String.Format("Function of {0} not exist", profile));
        }

        public static void RemoveFunction(string functionName) 
        {
            if (functions.ContainsKey(functionName)) 
            {
                functions.Remove(functionName);
            }
            else throw new ObjectNotExistException(String.Format("Function of {0} not exist", functionName));
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
                        {
                            buffer.Push(GetNumber(item.Value));
                            break;
                        }
                    case (TokenType.Variable):
                        {
                            buffer.Push(GetVariable(item.Value));
                            break;
                        }
                    case (TokenType.Operator):
                    case (TokenType.Function):
                        {
                            buffer.Push(GetFunction(item.Value));
                            break;
                        }

                }
            }
            return buffer.Pop();
        }

        private Expression GetNumber(string value) 
        {
            try
            {
                if (value.Contains('.'))
                {
                    value = value.Replace('.', ',');
                }
                return new NumberExpression(float.Parse(value));
            }
            catch (Exception exc) 
            {
                throw new TreeCreateException("Can't create number node", exc);
            }
        }

        private Expression GetVariable(string variableName)
        {
            try
            {
                return new VariableExpression(Context.GetVariable(variableName));
            }
            catch (ObjectNotExistException exc)
            {
                throw new TreeCreateException("Can't create variable node", exc);
            }
        }

        private Expression GetFunction(string functionName)
        {
            try
            {
                var function = Context.CreateFunction(functionName);
                for (int i = function.Profile.ArgumentCount - 1; i >= 0; i--)
                {
                    function.SetArgument(i, buffer.Pop());
                }
                return function;
            }
            catch (ObjectNotExistException exc) 
            {
                throw new TreeCreateException("Can't create function node", exc);
            }
        }
    }

    public abstract class Expression
    {
        public float Calculate() 
        {
            try
            {
                return OnCalculate();
            }
            catch (Exception inner) 
            {
                throw new CalculationException("Error while calculating! ", inner);
            }
        }

        protected abstract float OnCalculate();
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
        public PowOperator() : base( new FunctionProfile("^", 6, 2, false))
        { }

        protected override float OnCalculate()
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
        public MultiplyOperator() : base(new FunctionProfile("*", 4, 2))
        { }

        protected override float OnCalculate()
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
        public DevideOperator() : base(new FunctionProfile("/", 4, 2))
        { }

        protected override float OnCalculate()
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
        public SumOperator() : base( new FunctionProfile("+", 2, 2) )
        { }

        protected override float OnCalculate()
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
        public MinusOperator() : base(new FunctionProfile("-", 2, 2))
        { }

        protected override float OnCalculate()
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
        private Variable variable;
        public VariableExpression(Variable variableName)
        {
            this.variable = variableName;
        }

        protected override float OnCalculate()
        {
            return variable.GetValue();
        }
    }

    public sealed class NumberExpression : Expression
    {
        private float value;

        public NumberExpression(float number)
        {
            value = number;
        }

        protected override float OnCalculate()
        {
            return value;
        }
    }

    public abstract class FunctionExpression : Expression
    {
        private Expression[] arguments;
        private FunctionProfile profile;

        protected FunctionExpression(FunctionProfile profile)
        {
            this.arguments = new Expression[profile.ArgumentCount];
            this.profile = profile; 
        }

        public FunctionProfile Profile
        {
            get
            {
                return profile;
            }
        }

        public void SetArgument(int index, Expression value)
        {
            try
            {
                arguments[index] = value;
            }
            catch (IndexOutOfRangeException exc) 
            {
                throw new ArgumentException(String.Format("Can't set argument at {0} position", index), exc);
            }
        }

        public float GetArgument(int index)
        {
            try
            {
                return arguments[index].Calculate();
            }
            catch (IndexOutOfRangeException exc) 
            {
                throw new ArgumentException(String.Format("Get method exception: {0} - index out of range", index), exc);
            }
        }

        public abstract FunctionExpression Clone();
    }

    public sealed class SinExpression : FunctionExpression
    {
        public SinExpression(Expression expression) : base(new FunctionProfile("sin", 10, 1, false))
        {
            SetArgument(0, expression);
        }

        public SinExpression() : base(new FunctionProfile("sin", 10, 1, false))
        { }

        protected override float OnCalculate()
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
        public MaxExpression(Expression left, Expression right) : base(new FunctionProfile("max", 10, 2, false))
        {
            SetArgument(0, left);
            SetArgument(1, right);
        }

        public MaxExpression() : base(new FunctionProfile("max", 10, 2, false))
        { }

        protected override float OnCalculate()
        {
            return Math.Max(GetArgument(0), GetArgument(1));
        }

        public override FunctionExpression Clone()
        {
            return new MaxExpression();
        }
    }


}