using ExpRecognizer.Expressions.Functions.Names;

namespace ExpRecognizer.Expressions.Functions
{
    public class SumOperator : FunctionExpression
    {
        public SumOperator() : base( 
            new FunctionProfile(BasicMathOperationNameConstants.Plus, 2, 2) 
        )
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


}