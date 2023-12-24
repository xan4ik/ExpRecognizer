using ExpRecognizer.Expressions.Functions.Names;

namespace ExpRecognizer.Expressions.Functions
{
    public class DevideOperator : FunctionExpression
    {
        public DevideOperator() : base(new FunctionProfile(BasicMathOperationNameConstants.Devide, 4, 2))
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


}