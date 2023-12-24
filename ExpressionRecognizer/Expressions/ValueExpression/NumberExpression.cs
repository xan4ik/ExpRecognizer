namespace ExpRecognizer.Expressions.Values
{
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


}