using System;
using System.Collections.Generic;
using ExpRecognizer.Expressions.Functions;
using ExpRecognizer.Expressions.Functions.Names;
using ExpRecognizer.Expressions.Values;

namespace ExpRecognizer.EvaluationContext
{
    public sealed class Context
    {
        private Dictionary<string, FunctionExpression> _functions;
        private Dictionary<string, Variable> _variables;

        public Context(Dictionary<string, FunctionExpression> functions, Dictionary<string, Variable> variables)
        {
            _functions = functions;
            _variables = variables;
        }

        public static Context CreateDefault()
        {
            var functions = new Dictionary<string, FunctionExpression>()
            {
                {BasicMathOperationNameConstants.Plus, new SumOperator() },
                {BasicMathOperationNameConstants.Minus, new MinusOperator() },
                {BasicMathOperationNameConstants.Pow, new PowOperator() },
                {BasicMathOperationNameConstants.Multiply, new MultiplyOperator() },
                {BasicMathOperationNameConstants.Devide, new DevideOperator() },
                {"max", new MaxExpression() }
            };

            var variables = new Dictionary<string, Variable>()
            {
                {"Pi", new Variable("Pi", (float)(Math.PI), true) }
            };

            return new Context(functions, variables);
        }

        public int CompareOperations(string left, string right)
        {
            try
            {
                var _leftFunction = _functions[left].Profile;
                var _rightFunction = _functions[right].Profile;

                return _leftFunction.CompareTo(_rightFunction);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(string.Format("One of function {0}\\{1} not exist", left, right));
            }
        }

        public bool ContainsOperation(string operation)
        {
            return _functions.ContainsKey(operation);
        }

        public bool ContainsVariable(string variable)
        {
            return _variables.ContainsKey(variable);
        }

        public FunctionExpression CreateFunction(string name)
        {
            if (!_functions.ContainsKey(name))
                throw new ArgumentException(string.Format("Function of {0} not exist", name));

            return _functions[name].Clone();
        }

        public Variable GetVariable(string name)
        {
            if (!_variables.ContainsKey(name))
                throw new ArgumentException(string.Format("Variable of {0} not exist", name));

            return _variables[name];
        }

        public void AddVariable(string name, Variable value)
        {
            if (_variables.ContainsKey(name))
                throw new ArgumentException(string.Format("Variable of {0} not exist", name));

            _variables.Add(name, value);
        }

        public void RemoveVariable(string name)
        {
            if (!_variables.ContainsKey(name))
                throw new ArgumentException(string.Format("Variable of {0} not exist", name));

            _variables.Remove(name);
        }

        public void AddFunction(FunctionExpression expression)
        {
            var profile = expression.Profile.FunctionName;
            if (_functions.ContainsKey(profile))
                throw new ArgumentException(string.Format("Function of {0} not exist", profile));

            _functions.Add(profile, expression);
        }

        public void RemoveFunction(string functionName)
        {
            if (!_functions.ContainsKey(functionName))
                throw new ArgumentException(string.Format("Function of {0} not exist", functionName));

            _functions.Remove(functionName);
        }

        internal FunctionExpression GetFunction(string functionName)
        {
            if (!_functions.ContainsKey(functionName))
                throw new ArgumentException(string.Format("Function of {0} not exist", functionName));

            return _functions[functionName];
        }
    }
}
