using ExpRecognizer.Expressions.Functions.Names;

namespace ExpRecognizer.Expressions.Functions
{
    public class MultiplyOperator : FunctionExpression
    {
        public MultiplyOperator() : base(
            new FunctionProfile(BasicMathOperationNameConstants.Multiply, 4, 2)
        )
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


}