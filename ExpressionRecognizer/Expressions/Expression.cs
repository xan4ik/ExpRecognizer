using System;
using ExpRecognizer.Exceptions;

namespace ExpRecognizer.Expressions
{
    public abstract class Expression
    {
        public float Calculate() 
        {
            try
            {
                return OnCalculate();
            }
            catch (Exception inner) 
            {
                throw new CalculationException("Error while calculating! ", inner);
            }
        }

        protected abstract float OnCalculate();
    }


}