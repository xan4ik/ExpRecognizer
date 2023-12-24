using System;

namespace ExpRecognizer.Expressions.Functions
{
    public abstract class FunctionExpression : Expression
    {
        private Expression[] arguments;
        private FunctionProfile profile;

        protected FunctionExpression(FunctionProfile profile)
        {
            this.arguments = new Expression[profile.ArgumentCount];
            this.profile = profile; 
        }

        public FunctionProfile Profile
        {
            get
            {
                return profile;
            }
        }

        public void SetArgument(int index, Expression value)
        {
            try
            {
                arguments[index] = value;
            }
            catch (IndexOutOfRangeException exc) 
            {
                throw new ArgumentException($"Can't set argument at {index} position", exc);
            }
        }

        public float GetArgument(int index)
        {
            try
            {
                return arguments[index].Calculate();
            }
            catch (IndexOutOfRangeException exc) 
            {
                throw new ArgumentException($"Get method exception: {index} - index out of range", exc);
            }
        }

        public abstract FunctionExpression Clone();
    }


}