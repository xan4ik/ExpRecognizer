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
            string exp = "(2*sin(x)+4)/fun(2)*(4+2)+245*x";

            Console.WriteLine((new ContextParser()).Parse("3+4*2/(1-5)^2^3"));

            //выделить блоки, разбить, парситьы
            //парсить выражения в скобках
        }
    }

    public struct Operation : IComparable<Operation>
    {
        public readonly char Type;
        public readonly int Priority;

        public Operation(char operation, int priority)
        {
            Type = operation;
            Priority = priority;
        }

        public bool Is(char type) 
        {
            return type == Type;
        }

        public int CompareTo(Operation other)
        {
            return this.Priority - other.Priority;
        }
    }

    public class ContextParser 
    {
        private Stack<char> operationsStack;
        private Dictionary<char, Operation> operations;
        private StringBuilder output;
        private StringBuilder buffer;

        public ContextParser()
        {
            operationsStack = new Stack<char>();
            output = new StringBuilder();
            buffer = new StringBuilder();
            operations = new Dictionary<char, Operation>()
            {
                {'+', new Operation('+', 2) },
                {'-', new Operation('-', 2) },
                {'/', new Operation('/', 4) },
                {'*', new Operation('*', 4) },
                {'^', new Operation('^', 6) }
            };
        }


        public string Parse(string exp) 
        {
            Reset();
            Process(exp);
            ClearBuffer();
            Finish();
            return output.ToString();
        }

        private void Reset() 
        {
            buffer.Clear();
            output.Clear();
        }

        private void Process(string expression) 
        {
            foreach (var symbol in expression)
            {
                if (IsPartOfNumber(symbol))
                {
                    buffer.Append(symbol);
                }
                else if (IsBinaryOperator(symbol))
                {
                    CompletePart();
                    HandleOperation(symbol);
                }
                else if (symbol == '(')
                {
                    operationsStack.Push(symbol);
                }
                else if (symbol == ')') 
                {
                    HandleCloseBracket();
                }
            }
        }

        private void Finish() 
        {
            while (IsOperationExist(operationsStack)) 
            {
                output.Append(operationsStack.Pop());
                output.Append(' ');
            }
        }

        private bool IsPartOfNumber(char symbol)
        {
            return symbol >= '0' && symbol <= '9' || symbol == ',';
        }

        private bool IsBinaryOperator(char symbol) 
        {
            return operations.Keys.Contains(symbol);
        }

        private void ClearBuffer() 
        {
            if (buffer.Length != 0)
            {
                output.Append(buffer.ToString());
                output.Append(' ');
            }
        }

        private void CompletePart()
        {
            if (buffer.Length != 0)
            {
                output.Append(buffer.ToString());
                output.Append(' ');
                buffer.Clear();
            }
        }

        private void HandleOperation(char operation) 
        {
            if (IsOperationExist(operationsStack) && operationsStack.Peek() != '(')
            {
                if ( operations[operation].CompareTo(operations[operationsStack.Peek()]) <= 0) // operation rather
                {
                    while (operations[operation].CompareTo(operations[operationsStack.Peek()]) <= 0) 
                    {
                        operationsStack.Pop();
                    }
                    operationsStack.Push(operation);
                }
                else // (operation.CompareTo(operationsStack.Peek()) == 0) 
                {
                    output.Append(operationsStack.Pop());
                    output.Append(' ');
                    operationsStack.Push(operation);
                }
            }
            else operationsStack.Push(operation);
        }

        private bool IsOperationExist(Stack<char> operations) 
        {
            return operations.Count != 0;
        }

        private void HandleCloseBracket() 
        {
            var current = operationsStack.Pop();
            while (current != '(') 
            {
                current = operationsStack.Pop();
            }
            operationsStack.Pop();
        }
    }


    public abstract class Expression 
    {
        public abstract float Calculate();
    }







    //public class Context 
    //{
    //    private StringBuilder builder;
    //    private string fullExpression;

    //    public string GetCurrentPart() 
    //    { }
    //}

    //public class ContextSpliter 
    //{
    //    private Expression<float> head;


    //}

    //public interface Expression<T>
    //{
    //    void Interpret(Context context);
    //    T Calculate();
    //}


    //public abstract class NonTerminalExpression<T> : Expression<T>
    //{
    //    protected Expression<T> left;
    //    protected Expression<T> right;

    //    public T Calculate() 
    //    {
    //        try
    //        {
    //            return Calculate(left.Calculate(), right.Calculate());
    //        }
    //        catch (Exception exc) 
    //        {
    //            //TODO: exc
    //            throw new Exception();
    //        }
    //    }


    //    public abstract void Interpret(Context context);

    //    protected abstract T Calculate(T valueLeft, T valueRight);
    //}

    //public class SumExpression : NonTerminalExpression<float>
    //{
    //    public override void Interpret(Context context)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override float Calculate(float valueLeft, float valueRight)
    //    {
    //        return valueLeft + valueRight;
    //    }
    //}


    //public class IntTerminalExpression : Expression<int>
    //{
    //    public int Calculate()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Interpret(Context context)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class FloatTerminalExpression: Expression<float>
    //{
    //    private float value;
    //    public void Interpret(Context context)
    //    {
            
    //    }

    //    public float Calculate()
    //    {
    //        return 0;
    //    }
    //}

}
