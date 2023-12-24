using System;
using ExpRecognizer.Expressions.Functions.Names;

namespace ExpRecognizer.Expressions.Functions
{
    public class PowOperator : FunctionExpression
    {
        public PowOperator() : base( 
            new FunctionProfile(BasicMathOperationNameConstants.Pow, 6, 2, false)
        )
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


}