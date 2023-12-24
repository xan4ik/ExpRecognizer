using ExpRecognizer.Exceptions;

namespace ExpRecognizer.Expressions.Values
{
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


    public sealed class Variable
    {
        public readonly string Name;

        private bool _isReadOnly;
        private float _value;

        public Variable(string name, float value, bool readOnly = false)
        {
            _isReadOnly = readOnly;
            _value = value;

            Name = name;
        }

        public bool IsConst
        {
            get
            {
                return _isReadOnly;
            }
        }

        public float GetValue()
        {
            return _value;
        }

        public void SetValue(float value)
        {
            if (_isReadOnly)
                throw new CalculationException("Can't change const value");

            _value = value;
        }
    }



}