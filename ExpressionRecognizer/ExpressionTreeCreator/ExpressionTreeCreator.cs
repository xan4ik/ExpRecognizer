using System;
using System.Collections.Generic;
using System.Linq;
using ExpRecognizer.EvaluationContext;
using ExpRecognizer.Exceptions;
using ExpRecognizer.Expressions;
using ExpRecognizer.Expressions.Values;
using ExpRecognizer.Tokenizer;

namespace ExpRecognizer.ExpressionTreeCreator
{
    public class ExpressionTreeCreator
    {
        public Expression CreateTree(IEnumerable<Token> polishEntry, Context context)
        {
            Stack<Expression> buffer = new Stack<Expression>();
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
                            buffer.Push(GetVariable(item.Value, context));
                            break;
                        }
                    case (TokenType.Operator):
                    case (TokenType.Function):
                        {
                            buffer.Push(GetFunction(item.Value, buffer ,context));
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

        private Expression GetVariable(string variableName, Context context)
        {
            try
            {
                return new VariableExpression(context.GetVariable(variableName));
            }
            catch (ArgumentException exc)
            {
                throw new TreeCreateException("Can't create variable node", exc);
            }
        }

        private Expression GetFunction(string functionName, Stack<Expression> buffer, Context context)
        {
            try
            {
                var function = context.CreateFunction(functionName);
                for (int i = function.Profile.ArgumentCount - 1; i >= 0; i--)
                {
                    function.SetArgument(i, buffer.Pop());
                }
                return function;
            }
            catch (ArgumentException exc)
            {
                throw new TreeCreateException("Can't create function node", exc);
            }
        }
    }


}