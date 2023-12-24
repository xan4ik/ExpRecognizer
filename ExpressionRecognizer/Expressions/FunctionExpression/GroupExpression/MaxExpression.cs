using System;
using ExpRecognizer.Expressions.Functions.Names;

namespace ExpRecognizer.Expressions.Functions
{
    public sealed class MaxExpression : FunctionExpression
    {
        public MaxExpression(Expression left, Expression right) : base(
            new FunctionProfile(GroupOperationNameConstants.Max, 10, 2, false)
           )
        {
            SetArgument(0, left);
            SetArgument(1, right);
        }

        public MaxExpression() : base(
            new FunctionProfile(GroupOperationNameConstants.Max, 10, 2, false)
        )
        { }

        protected override float OnCalculate()
        {
            return Math.Max(GetArgument(0), GetArgument(1));
        }

        public override FunctionExpression Clone()
        {
            return new MaxExpression();
        }
    }


}