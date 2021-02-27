namespace LiveCoreLibrary
{
    public class Result<T>
    {
        public bool Success;
        public T Value;

        public Result(bool success, T value = default)
        {
            Success = success;
            Value = value;
        }
    }
}