using ExpRecognizer.Expressions.Functions.Names;

namespace ExpRecognizer.Expressions.Functions
{
    public class MinusOperator : FunctionExpression
    {
        public MinusOperator() : base(
            new FunctionProfile(BasicMathOperationNameConstants.Minus, 2, 2)
        )
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


}