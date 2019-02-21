namespace Nullability
{
    class GenericType<T> where T : class
    {
        public T? Value { get; set; }

        // This feels reasonable
        public GenericType(T value) => Value = value;

        public GenericType()
        {
            // This silences the compiler, but it's not clear why.
            // default(string) is still null... and it's also the default
            // value for the field!
            Value = default(T);
        }
    }
}
