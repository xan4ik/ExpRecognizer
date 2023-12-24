using System;

namespace ExpRecognizer.Expressions.Functions
{
    public struct FunctionProfile : IComparable<FunctionProfile>
    {
        public readonly string FunctionName;
        public readonly int Priority;
        public readonly bool LeftSided;
        public readonly int ArgumentCount;

        public FunctionProfile(string operation, int priority, int argc, bool leftsided = true)
        {
            FunctionName = operation;
            Priority = priority;
            LeftSided = leftsided;
            ArgumentCount = argc;
        }

        public bool Is(string type)
        {
            return type == FunctionName;
        }

        public int CompareTo(FunctionProfile other)
        {
            return this.Priority - other.Priority;
        }
    }
}