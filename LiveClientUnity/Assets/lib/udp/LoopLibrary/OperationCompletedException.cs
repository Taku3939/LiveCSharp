using System;

namespace EventServerCore
{
    public class OperationCompletedException<T> : Exception
    {
        public readonly T Result;
        public OperationCompletedException(T result)
        {
            Result = result;
        }
    }
}
